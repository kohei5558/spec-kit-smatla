using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Infrastructure.Identity;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 保護者アカウント登録の統合テスト
/// </summary>
public class AuthRegisterTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _services;

    public AuthRegisterTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _services = factory.Services;
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccessAndToken()
    {
        // Arrange
        var uniqueEmail = $"newparent_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest
        {
            Email = uniqueEmail,
            DisplayName = "New Parent",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEmpty(result.Token!);
        Assert.Equal(uniqueEmail, result.Email);
        Assert.Equal("New Parent", result.DisplayName);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange - 既存ユーザーを確保
        await AuthenticationHelper.EnsureTestUsersExistAsync(_services);

        var request = new RegisterRequest
        {
            Email = "parent@example.com", // 既存のメールアドレス
            DisplayName = "Duplicate Parent",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("既に使用されています", result.Message ?? "");
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var uniqueEmail = $"weakpass_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest
        {
            Email = uniqueEmail,
            DisplayName = "Weak Password User",
            Password = "weak", // 弱いパスワード
            ConfirmPassword = "weak"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("パスワード", result.Message ?? "");
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ReturnsBadRequest()
    {
        // Arrange
        var uniqueEmail = $"mismatch_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest
        {
            Email = uniqueEmail,
            DisplayName = "Mismatch User",
            Password = "SecurePassword123!",
            ConfirmPassword = "DifferentPassword123!" // 確認パスワードが不一致
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Contains("一致しません", result.Message ?? "");
    }

    [Fact]
    public async Task Register_AutoLogin_ReturnsValidJwtToken()
    {
        // Arrange
        var uniqueEmail = $"autologin_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest
        {
            Email = uniqueEmail,
            DisplayName = "Auto Login User",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token!);

        // トークンを使って認証が必要なエンドポイントにアクセスできるか確認
        // 既存の_clientにAuthorizationヘッダーを追加
        AuthenticationHelper.AddAuthorizationHeader(_client, result.Token!);

        var testResponse = await _client.GetAsync("/api/parent/dashboard");
        Assert.True(testResponse.IsSuccessStatusCode || testResponse.StatusCode == HttpStatusCode.NotFound);
    }
}
