using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.DTOs;
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
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    /// <summary>
    /// テストユーザーをシード（存在しない場合のみ作成）
    /// </summary>
    public static async Task EnsureTestUsersExistAsync(IServiceProvider services)
    {
        await _semaphore.WaitAsync();
        try
        {
            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 保護者ユーザーの存在確認と作成
            var parentEmail = "parent@example.com";
            var existingParent = await userManager.FindByEmailAsync(parentEmail);

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

                var result = await userManager.CreateAsync(newParent, "Parent123!");
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create parent user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // 子供ユーザーの存在確認と作成
            var parentUser = await userManager.FindByEmailAsync(parentEmail)
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

                var result = await userManager.CreateAsync(child);
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create child user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                existingChild = child;
            }

            // Studentエントリを作成（存在しない場合）または既存のものをリセット
            // NOTE: 毎回チェックして、ポイントをリセットする
            var student = db.Students.FirstOrDefault(s => s.Name == "太郎");
            if (student == null)
            {
                student = new Student
                {
                    Name = "太郎",
                    TotalPoints = 0 // テスト用の初期ポイント（各テストで必要に応じて設定）
                };
                db.Students.Add(student);
            }
            else
            {
                // 既存のStudentをリセット
                student.TotalPoints = 0;
            }
            await db.SaveChangesAsync();

            // 子供アカウントと学習者を紐付ける（学習者へのアクセスは名前ではなくこの紐付けで判定される）
            if (existingChild.StudentId != student.Id)
            {
                existingChild.StudentId = student.Id;
                await userManager.UpdateAsync(existingChild);
            }
        }
        finally
        {
            _semaphore.Release();
        }
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
            Password = "Parent123!",
            RememberMe = false
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
            ChildAccountId = childId,
            PIN = pin
        };

        var response = await client.PostAsJsonAsync("/api/auth/child/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse?.Token ?? throw new InvalidOperationException("Failed to get JWT token");
    }

    /// <summary>
    /// 新しい家庭（保護者）を登録して子供アカウントを1人作り、その学習者を返す。
    /// クライアントはその保護者でログインした状態になる（保護者は自分の子供の学習者にだけアクセスできるため）
    /// </summary>
    public static async Task<TestStudentDto> CreateStudentInNewFamilyAsync(HttpClient client, string childName)
    {
        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"student_family_{Guid.NewGuid():N}@example.com",
            DisplayName = "テスト保護者",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!"
        });
        register.EnsureSuccessStatusCode();
        var parentJwt = (await register.Content.ReadFromJsonAsync<RegisterResponse>())!.Token!;
        AddAuthorizationHeader(client, parentJwt);

        var create = await client.PostAsJsonAsync("/api/child-accounts", new ChildAccountCreateDto
        {
            Name = childName,
            GradeLevel = 3,
            PresetAvatarId = 1,
            PIN = "1593"
        });
        create.EnsureSuccessStatusCode();
        var child = (await create.Content.ReadFromJsonAsync<ChildAccountDto>())!;
        return new TestStudentDto { Id = child.StudentId, Name = child.Name, Grade = child.GradeLevel };
    }

    /// <summary>
    /// 端末トークンを送るHTTPヘッダー名
    /// </summary>
    public const string DeviceTokenHeader = "X-Device-Token";

    /// <summary>
    /// 保護者のJWTで端末を子供用に登録し、端末トークンを返す
    /// </summary>
    public static async Task<string> RegisterDeviceAsync(HttpClient client, string parentJwt, string name = "テスト端末")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/devices")
        {
            Content = JsonContent.Create(new RegisterDeviceRequest { Name = name })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", parentJwt);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RegisterDeviceResponse>();
        return result?.Token ?? throw new InvalidOperationException("Failed to get device token");
    }

    /// <summary>
    /// テスト用保護者（parent@example.com）の登録端末トークンをクライアントに設定する。
    /// 端末の上限に達しないよう、トークンはファクトリー（テストクラス）単位で使い回す
    /// </summary>
    public static async Task UseParentDeviceAsync(HttpClient client, TestWebApplicationFactory factory)
    {
        if (factory.ParentDeviceToken == null)
        {
            var parentJwt = await LoginAsParentAsync(client, factory.Services);
            factory.ParentDeviceToken = await RegisterDeviceAsync(client, parentJwt);
        }

        SetDeviceToken(client, factory.ParentDeviceToken);
    }

    /// <summary>
    /// HTTPクライアントに端末トークンを設定（null で削除）
    /// </summary>
    public static void SetDeviceToken(HttpClient client, string? token)
    {
        client.DefaultRequestHeaders.Remove(DeviceTokenHeader);
        if (token != null)
        {
            client.DefaultRequestHeaders.Add(DeviceTokenHeader, token);
        }
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

    /// <summary>
    /// テスト用のStudentIDを取得
    /// </summary>
    public static int GetTestStudentId(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var child = db.Users.FirstOrDefault(u => u.DisplayName == "太郎" && u.Role == UserRole.Child);
        return child?.StudentId ?? throw new Exception("Test student not found");
    }
}
