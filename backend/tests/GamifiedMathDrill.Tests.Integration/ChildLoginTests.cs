using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 子供ログイン機能の統合テスト (Phase 3, US1)
/// </summary>
public class ChildLoginTests : AuthenticatedTestBase
{
    public ChildLoginTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ChildLogin_WithValidPin_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "ログインテスト太郎",
            GradeLevel = 4,
            PresetAvatarId = 1,
            PIN = "9876"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var childAccount = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(childAccount);

        // ログアウト（子供として再ログイン）
        Client.DefaultRequestHeaders.Authorization = null;

        // 子供ログインリクエスト
        var loginRequest = new
        {
            ChildId = childAccount.Id,
            PIN = "9876"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.True(loginResponse.Success);
        Assert.NotEmpty(loginResponse.Token!);
        Assert.Equal("Child", loginResponse.Role);
        Assert.NotNull(loginResponse.ExpiresAt);
    }

    [Fact]
    public async Task ChildLogin_WithInvalidPin_ReturnsUnauthorized()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "無効PIN太郎",
            GradeLevel = 3,
            PresetAvatarId = 2,
            PIN = "1234"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var childAccount = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(childAccount);

        // ログアウト
        Client.DefaultRequestHeaders.Authorization = null;

        // 間違ったPINでログイン
        var loginRequest = new
        {
            ChildId = childAccount.Id,
            PIN = "9999" // 間違ったPIN
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChildLogin_WithSuspendedAccount_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "停止テスト太郎",
            GradeLevel = 2,
            PresetAvatarId = 3,
            PIN = "5555"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var childAccount = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(childAccount);

        // アカウントを停止
        var suspendResponse = await Client.PostAsync($"/api/child-accounts/{childAccount.Id}/suspend", null);
        Assert.True(suspendResponse.IsSuccessStatusCode);

        // ログアウト
        Client.DefaultRequestHeaders.Authorization = null;

        // 停止中のアカウントでログイン
        var loginRequest = new
        {
            ChildId = childAccount.Id,
            PIN = "5555"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("使用できません", errorContent);
    }

    [Fact]
    public async Task ChildLogin_WithNonExistentAccount_ReturnsNotFound()
    {
        // Arrange
        var loginRequest = new
        {
            ChildId = "non-existent-id",
            PIN = "1234"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChildLogin_VerifyTokenContainsChildRole()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "ロール確認太郎",
            GradeLevel = 5,
            PresetAvatarId = 4,
            PIN = "7777"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var childAccount = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(childAccount);

        // ログアウト
        Client.DefaultRequestHeaders.Authorization = null;

        // 子供でログイン
        var loginRequest = new
        {
            ChildId = childAccount.Id,
            PIN = "7777"
        };

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);

        // トークンを使用して認証が必要なエンドポイントにアクセス
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);

        // 子供ロールでアクセス可能なエンドポイントを呼び出し
        var testResponse = await Client.GetAsync("/api/problems/category/addition?difficulty=1");

        // Assert - 子供ロールで問題取得が可能
        Assert.True(testResponse.IsSuccessStatusCode || testResponse.StatusCode == HttpStatusCode.NotFound,
            "Child should be able to access problems endpoint");
    }

    private class LoginResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
