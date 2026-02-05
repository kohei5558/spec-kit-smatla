using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GamifiedMathDrill.Tests.Integration.Helpers;

/// <summary>
/// テスト用の認証ヘルパー
/// </summary>
public static class AuthenticationHelper
{
    private static bool _usersSeeded = false;
    private static readonly object _lock = new object();

    /// <summary>
    /// テストユーザーをシード（初回のみ実行）
    /// </summary>
    public static Task EnsureTestUsersExistAsync(IServiceProvider services)
    {
        if (_usersSeeded) return Task.CompletedTask;

        lock (_lock)
        {
            if (_usersSeeded) return Task.CompletedTask;

            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 保護者ユーザーを作成
            var parentEmail = "parent@example.com";
            var existingParent = userManager.FindByEmailAsync(parentEmail).Result;
            
            if (existingParent == null)
            {
                var newParent = new ApplicationUser
                {
                    UserName = parentEmail,
                    Email = parentEmail,
                    DisplayName = "Test Parent",
                    Role = UserRole.Parent,
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(newParent, "Parent123!").Result;
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create parent user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // 子供ユーザーを作成
            var parentUser = userManager.FindByEmailAsync(parentEmail).Result 
                ?? throw new Exception("Parent user not found after creation");

            var existingChild = userManager.Users.FirstOrDefault(u => u.DisplayName == "太郎" && u.Role == UserRole.Child);
            
            if (existingChild == null)
            {
                var child = new ApplicationUser
                {
                    UserName = $"child_taro_{Guid.NewGuid()}",
                    Email = $"taro_{Guid.NewGuid()}@test.local",
                    DisplayName = "太郎",
                    Role = UserRole.Child,
                    ParentId = parentUser.Id,
                    PIN = userManager.PasswordHasher.HashPassword(null!, "1234"),
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(child).Result;
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create child user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                
                existingChild = child;
            }
            
            // Studentエントリを作成（存在しない場合）
            var student = db.Students.FirstOrDefault(s => s.Name == "太郎");
            if (student == null)
            {
                student = new Student
                {
                    Name = "太郎",
                    TotalPoints = 100 // テスト用に初期ポイントを設定
                };
                db.Students.Add(student);
                db.SaveChangesAsync().Wait();
            }

            _usersSeeded = true;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 保護者としてログインし、JWTトークンを取得
    /// </summary>
    public static async Task<string> LoginAsParentAsync(HttpClient client, IServiceProvider? services = null)
    {
        if (services != null)
        {
            await EnsureTestUsersExistAsync(services);
        }

        var loginRequest = new LoginRequest
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Login failed: {response.StatusCode} - {errorContent}");
        }

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse?.Token ?? throw new InvalidOperationException("Failed to get JWT token");
    }

    /// <summary>
    /// 子供としてログインし、JWTトークンを取得
    /// </summary>
    public static async Task<string> LoginAsChildAsync(HttpClient client, string childId, string pin, IServiceProvider? services = null)
    {
        if (services != null)
        {
            await EnsureTestUsersExistAsync(services);
        }

        var loginRequest = new ChildLoginRequest
        {
            ChildId = childId,
            PIN = pin
        };

        var response = await client.PostAsJsonAsync("/api/auth/child-login", loginRequest);
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse?.Token ?? throw new InvalidOperationException("Failed to get JWT token");
    }

    /// <summary>
    /// HTTPクライアントに認証ヘッダーを追加
    /// </summary>
    public static void AddAuthorizationHeader(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// テスト用の子供ユーザーIDを取得
    /// </summary>
    public static string GetTestChildId(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var child = userManager.Users.FirstOrDefault(u => u.DisplayName == "太郎" && u.Role == UserRole.Child);
        return child?.Id ?? throw new Exception("Test child user not found");
    }
}
