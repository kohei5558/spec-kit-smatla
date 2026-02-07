using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// ログイン機能の統合テスト (User Story 1)
/// </summary>
public class LoginTests : AuthenticatedTestBase
{
    private const string ParentEmail = "parent@example.com";
    private const string ParentPassword = "Parent123!";

    public LoginTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_WithValidCredentials_WithoutRememberMe_ReturnsSuccess()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = ParentEmail,
            Password = ParentPassword,
            RememberMe = false
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.True(loginResponse.Success);
        Assert.NotEmpty(loginResponse.Token!);
        Assert.Equal("Parent", loginResponse.Role);
        Assert.NotNull(loginResponse.ExpiresAt);

        // Verify session expiry time (~60 minutes)
        var expiryTime = loginResponse.ExpiresAt.Value - DateTime.UtcNow;
        Assert.True(expiryTime.TotalMinutes >= 59 && expiryTime.TotalMinutes <= 61,
            $"Session expiry should be ~60 minutes, but was {expiryTime.TotalMinutes} minutes");
    }

    [Fact]
    public async Task Login_WithValidCredentials_WithRememberMe_ReturnsSuccess()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = ParentEmail,
            Password = ParentPassword,
            RememberMe = true
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.True(loginResponse.Success);
        Assert.NotEmpty(loginResponse.Token!);
        Assert.Equal("Parent", loginResponse.Role);
        Assert.NotNull(loginResponse.ExpiresAt);

        // Verify remember me expiry time (~30 days = 43200 minutes)
        var expiryTime = loginResponse.ExpiresAt.Value - DateTime.UtcNow;
        Assert.True(expiryTime.TotalMinutes >= 43190 && expiryTime.TotalMinutes <= 43210,
            $"Remember Me expiry should be ~43200 minutes (30 days), but was {expiryTime.TotalMinutes} minutes");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = ParentEmail,
            Password = "WrongPassword123!",
            RememberMe = false
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.False(loginResponse.Success);
        Assert.Null(loginResponse.Token);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!",
            RememberMe = false
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.False(loginResponse.Success);
        Assert.Null(loginResponse.Token);
    }

    [Fact]
    public async Task Login_TokenExpiry_DiffersBetweenSessionAndRememberMe()
    {
        // Arrange & Act - Session login
        var sessionRequest = new LoginRequest
        {
            Email = ParentEmail,
            Password = ParentPassword,
            RememberMe = false
        };
        var sessionResponse = await Client.PostAsJsonAsync("/api/auth/login", sessionRequest);
        var sessionLogin = await sessionResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Arrange & Act - Remember me login
        var rememberRequest = new LoginRequest
        {
            Email = ParentEmail,
            Password = ParentPassword,
            RememberMe = true
        };
        var rememberResponse = await Client.PostAsJsonAsync("/api/auth/login", rememberRequest);
        var rememberLogin = await rememberResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Assert
        Assert.NotNull(sessionLogin);
        Assert.NotNull(rememberLogin);
        Assert.NotNull(sessionLogin.ExpiresAt);
        Assert.NotNull(rememberLogin.ExpiresAt);

        var sessionExpiry = sessionLogin.ExpiresAt.Value - DateTime.UtcNow;
        var rememberExpiry = rememberLogin.ExpiresAt.Value - DateTime.UtcNow;

        // Session should be ~60 minutes, RememberMe should be ~30 days
        Assert.True(sessionExpiry.TotalMinutes >= 59 && sessionExpiry.TotalMinutes <= 61);
        Assert.True(rememberExpiry.TotalMinutes >= 43190 && rememberExpiry.TotalMinutes <= 43210);

        // RememberMe expiry should be significantly longer than session expiry
        Assert.True(rememberExpiry > sessionExpiry);
    }
}
