using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Models.Responses;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Infrastructure.Identity;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// パスワードリセット機能の統合テスト (User Story 2)
/// </summary>
public class PasswordResetTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private const string TestEmail = "parent@example.com";

    public PasswordResetTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // テストユーザーをシード
        AuthenticationHelper.EnsureTestUsersExistAsync(factory.Services).Wait();
    }

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            Email = TestEmail
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_ReturnsSuccess()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            Email = "nonexistent@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", request);

        // Assert
        // セキュリティ上、存在しないメールでも成功を返す
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ValidateResetToken_WithValidToken_ReturnsTrue()
    {
        // Arrange - パスワードリセットトークンを生成
        var token = await GenerateResetTokenAsync(TestEmail);

        // Act
        var response = await _client.GetAsync($"/api/auth/validate-reset-token?token={Uri.EscapeDataString(token)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ValidateResetToken_WithExpiredToken_ReturnsFalse()
    {
        // Arrange - 期限切れトークンを生成
        var expiredToken = await GenerateExpiredResetTokenAsync(TestEmail);

        // Act
        var response = await _client.GetAsync($"/api/auth/validate-reset-token?token={Uri.EscapeDataString(expiredToken)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        Assert.NotNull(result);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ReturnsSuccess()
    {
        // Arrange - パスワードリセットトークンを生成
        var token = await GenerateResetTokenAsync(TestEmail);

        var request = new ResetPasswordRequest
        {
            Token = token,
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);

        // 新しいパスワードでログインできることを確認
        var loginRequest = new LoginRequest
        {
            Email = TestEmail,
            Password = "NewPassword123!",
            RememberMe = false
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithUsedToken_ReturnsFail()
    {
        // Arrange - パスワードリセットトークンを生成し、一度使用する
        var token = await GenerateResetTokenAsync(TestEmail);

        var firstRequest = new ResetPasswordRequest
        {
            Token = token,
            NewPassword = "FirstPassword123!",
            ConfirmPassword = "FirstPassword123!"
        };

        await _client.PostAsJsonAsync("/api/auth/reset-password", firstRequest);

        // Act - 同じトークンで再度リセットを試みる
        var secondRequest = new ResetPasswordRequest
        {
            Token = token,
            NewPassword = "SecondPassword123!",
            ConfirmPassword = "SecondPassword123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", secondRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
    }

    /// <summary>
    /// テスト用のリセットトークンを生成
    /// </summary>
    private async Task<string> GenerateResetTokenAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new Exception($"User with email {email} not found");
        }

        // トークン生成（実際のSendPasswordResetEmailAsyncロジックを模倣）
        var token = Guid.NewGuid().ToString("N");
        var tokenHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));

        var resetToken = new GamifiedMathDrill.Core.Models.PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsUsed = false,
            IpAddress = "127.0.0.1"
        };

        db.PasswordResetTokens.Add(resetToken);
        await db.SaveChangesAsync();

        return token;
    }

    /// <summary>
    /// テスト用の期限切れリセットトークンを生成
    /// </summary>
    private async Task<string> GenerateExpiredResetTokenAsync(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new Exception($"User with email {email} not found");
        }

        var token = Guid.NewGuid().ToString("N");
        var tokenHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));

        var resetToken = new GamifiedMathDrill.Core.Models.PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // 既に期限切れ
            IsUsed = false,
            IpAddress = "127.0.0.1"
        };

        db.PasswordResetTokens.Add(resetToken);
        await db.SaveChangesAsync();

        return token;
    }
}
