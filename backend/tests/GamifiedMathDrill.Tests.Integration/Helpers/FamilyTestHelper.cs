using System.Net.Http.Headers;
using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace GamifiedMathDrill.Tests.Integration.Helpers;

/// <summary>
/// 家庭（保護者・登録端末・子供）単位でテストデータを作るヘルパー。
/// 家庭ごとに新しい保護者を登録するため、テスト間で子供の数・端末の数・景品が干渉しない
/// </summary>
public static class FamilyTestHelper
{
    public record Family(string ParentJwt, string DeviceToken, string ChildJwt, int StudentId, string ChildId);

    public record Child(string Jwt, int StudentId, string ChildId);

    /// <summary>
    /// 保護者を新規登録し、端末を登録して、子供1人を作成・ログインさせる
    /// </summary>
    public static async Task<Family> CreateFamilyAsync(HttpClient client, string label, string? childName = null)
    {
        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"family_{Guid.NewGuid():N}@example.com",
            DisplayName = $"{label}保護者",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!"
        });
        register.EnsureSuccessStatusCode();
        var parentJwt = (await register.Content.ReadFromJsonAsync<RegisterResponse>())!.Token!;
        var deviceToken = await AuthenticationHelper.RegisterDeviceAsync(client, parentJwt);
        var child = await AddChildAsync(client, parentJwt, deviceToken, childName ?? $"{label}の子", "7410");
        return new Family(parentJwt, deviceToken, child.Jwt, child.StudentId, child.ChildId);
    }

    /// <summary>
    /// 家庭に子供を追加し、登録端末からログインさせる
    /// </summary>
    public static async Task<Child> AddChildAsync(HttpClient client, string parentJwt, string deviceToken, string name, string pin, int gradeLevel = 3)
    {
        var create = await SendAsync(client, HttpMethod.Post, "/api/child-accounts", parentJwt,
            new ChildAccountCreateDto { Name = name, GradeLevel = gradeLevel, PresetAvatarId = 1, PIN = pin });
        create.EnsureSuccessStatusCode();
        var childId = (await create.Content.ReadFromJsonAsync<ChildAccountDto>())!.Id;

        using var login = new HttpRequestMessage(HttpMethod.Post, "/api/auth/child/login")
        {
            Content = JsonContent.Create(new ChildLoginRequest { ChildAccountId = childId, PIN = pin })
        };
        login.Headers.Add(AuthenticationHelper.DeviceTokenHeader, deviceToken);
        var response = await client.SendAsync(login);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<LoginResponse>())!;
        return new Child(result.Token!, result.StudentId!.Value, childId);
    }

    /// <summary>
    /// 保護者として景品を登録し、その ID を返す
    /// </summary>
    public static async Task<int> CreateRewardAsync(HttpClient client, string parentJwt, string name = "テスト景品", int requiredPoints = 50, int stock = 10)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(name), "Name" },
            { new StringContent("テスト"), "Description" },
            { new StringContent(requiredPoints.ToString()), "RequiredPoints" },
            { new StringContent("Snack"), "Category" },
            { new StringContent("true"), "IsPhysical" },
            { new StringContent(stock.ToString()), "Stock" }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/rewards") { Content = form };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", parentJwt);
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RewardDto>())!.Id;
    }

    /// <summary>
    /// 学習者の所持ポイントを直接設定する
    /// </summary>
    public static async Task SetPointsAsync(IServiceProvider services, int studentId, int points)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var student = await db.Students.FindAsync(studentId);
        student!.TotalPoints = points;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// JWT（任意）と JSON ボディ（任意）を付けてリクエストを送る
    /// </summary>
    public static Task<HttpResponseMessage> SendAsync(HttpClient client, HttpMethod method, string url, string? jwt, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (jwt != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }
        if (body is HttpContent content)
        {
            request.Content = content;
        }
        else if (body != null)
        {
            request.Content = JsonContent.Create(body, body.GetType());
        }
        return client.SendAsync(request);
    }
}
