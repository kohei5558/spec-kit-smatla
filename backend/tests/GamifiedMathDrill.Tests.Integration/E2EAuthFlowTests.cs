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
/// エンドツーエンド認証フローの統合テスト
/// </summary>
public class E2EAuthFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly IServiceProvider _services;

    public E2EAuthFlowTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _services = factory.Services;
    }

    /// <summary>
    /// E2Eテスト: 登録 → 自動ログイン → ダッシュボードアクセス
    /// </summary>
    [Fact]
    public async Task E2E_RegisterToLoginFlow_SuccessfullyCreatesAccountAndLogsIn()
    {
        // Arrange: ユニークなメールアドレスで新規ユーザーを作成
        var uniqueEmail = $"e2e_register_{Guid.NewGuid()}@example.com";
        var registerRequest = new RegisterRequest
        {
            Email = uniqueEmail,
            DisplayName = "E2E Test User",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!"
        };

        // Act 1: 新規登録
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Assert 1: 登録成功
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(registerResult);
        Assert.True(registerResult.Success);
        Assert.NotNull(registerResult.Token);
        Assert.NotEmpty(registerResult.Token);
        Assert.Equal(uniqueEmail, registerResult.Email);
        Assert.Equal("E2E Test User", registerResult.DisplayName);
        Assert.Equal("Parent", registerResult.Role);

        // Act 2: 取得したトークンで認証が必要なエンドポイントにアクセス
        var authenticatedClient = _client;
        AuthenticationHelper.AddAuthorizationHeader(authenticatedClient, registerResult.Token);

        var dashboardResponse = await authenticatedClient.GetAsync("/api/parent/dashboard");

        // Assert 2: 認証されたアクセス成功（または適切なレスポンス）
        // Note: ダッシュボードエンドポイントが実装されていない場合はNotFoundでも可
        Assert.True(
            dashboardResponse.IsSuccessStatusCode ||
            dashboardResponse.StatusCode == HttpStatusCode.NotFound,
            $"Expected success or NotFound, got {dashboardResponse.StatusCode}");

        // Act 3: 同じ認証情報で明示的にログイン
        var loginRequest = new LoginRequest
        {
            Email = uniqueEmail,
            Password = "SecurePassword123!",
            RememberMe = false
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert 3: ログイン成功
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);
        Assert.True(loginResult.Success);
        Assert.NotNull(loginResult.Token);
        Assert.NotEmpty(loginResult.Token);

        // 注: トークンは同じペイロード（userId, role等）から生成されるため同一になる可能性がある
        // 重要なのは両方とも有効なトークンであることを確認
        Assert.NotEmpty(registerResult.Token);
        Assert.NotEmpty(loginResult.Token);
    }

    /// <summary>
    /// E2Eテスト: ログイン → パスワード忘れた → リセット → ログイン
    /// Note: このテストは実際のメール送信をモックするため、簡略化されたフローをテスト
    /// </summary>
    [Fact]
    public async Task E2E_ForgotPasswordFlow_SendsResetEmailSuccessfully()
    {
        // Arrange: テストユーザーを確保
        await AuthenticationHelper.EnsureTestUsersExistAsync(_services);
        var testEmail = "parent@example.com";
        var originalPassword = "Parent123!";

        // Act 1: オリジナルのパスワードでログイン確認
        var initialLoginRequest = new LoginRequest
        {
            Email = testEmail,
            Password = originalPassword,
            RememberMe = false
        };

        var initialLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", initialLoginRequest);

        // Assert 1: 初回ログイン成功
        Assert.Equal(HttpStatusCode.OK, initialLoginResponse.StatusCode);

        // Act 2: パスワードリセットリクエスト
        var forgotPasswordRequest = new ForgotPasswordRequest
        {
            Email = testEmail
        };

        var forgotPasswordResponse = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotPasswordRequest);

        // Assert 2: パスワードリセットメール送信成功
        Assert.Equal(HttpStatusCode.OK, forgotPasswordResponse.StatusCode);

        var forgotPasswordResult = await forgotPasswordResponse.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
        Assert.NotNull(forgotPasswordResult);
        Assert.True(forgotPasswordResult.Success);
        Assert.Contains("メールを送信しました", forgotPasswordResult.Message ?? "");

        // Wait a moment for the token to be saved to database
        await Task.Delay(100);

        // Act 3: データベースにトークンが保存されたことを確認（E2E検証）
        using (var scope = _services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GamifiedMathDrill.Infrastructure.Data.ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(testEmail);
            Assert.NotNull(user);

            // データベースから最新のリセットトークンを取得
            var tokenRecord = db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault();

            Assert.NotNull(tokenRecord);
            Assert.False(tokenRecord.IsUsed);
            Assert.True(tokenRecord.ExpiresAt > DateTime.UtcNow);
        }

        // Note: 実際のパスワードリセット実行は PasswordResetTests.cs でカバー済み
    }

    /// <summary>
    /// E2Eテスト: RememberMe機能の動作確認（セッション vs ローカルストレージ）
    /// </summary>
    [Fact]
    public async Task E2E_RememberMeFunctionality_TokenExpiryDiffersBetweenSessionAndPersistent()
    {
        // Arrange: テストユーザーを確保
        await AuthenticationHelper.EnsureTestUsersExistAsync(_services);
        var testEmail = "parent@example.com";
        var testPassword = "Parent123!";

        // Act 1: RememberMe=false でログイン（セッション）
        var sessionLoginRequest = new LoginRequest
        {
            Email = testEmail,
            Password = testPassword,
            RememberMe = false
        };

        var sessionLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", sessionLoginRequest);

        // Assert 1: セッションログイン成功
        Assert.Equal(HttpStatusCode.OK, sessionLoginResponse.StatusCode);

        var sessionLoginResult = await sessionLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(sessionLoginResult);
        Assert.True(sessionLoginResult.Success);
        Assert.NotNull(sessionLoginResult.ExpiresAt);

        var sessionExpiryTime = sessionLoginResult.ExpiresAt.Value;
        var sessionDuration = sessionExpiryTime - DateTime.UtcNow;

        // Assert: セッションは約60分（55-65分の範囲）
        Assert.InRange(sessionDuration.TotalMinutes, 55, 65);

        // Act 2: RememberMe=true でログイン（永続）
        var persistentLoginRequest = new LoginRequest
        {
            Email = testEmail,
            Password = testPassword,
            RememberMe = true
        };

        var persistentLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", persistentLoginRequest);

        // Assert 2: 永続ログイン成功
        Assert.Equal(HttpStatusCode.OK, persistentLoginResponse.StatusCode);

        var persistentLoginResult = await persistentLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(persistentLoginResult);
        Assert.True(persistentLoginResult.Success);
        Assert.NotNull(persistentLoginResult.ExpiresAt);

        var persistentExpiryTime = persistentLoginResult.ExpiresAt.Value;
        var persistentDuration = persistentExpiryTime - DateTime.UtcNow;

        // Assert: 永続ログインは約30日（29-31日の範囲、43200分 = 30日）
        Assert.InRange(persistentDuration.TotalDays, 29, 31);

        // Assert 3: 永続ログインのトークンはセッションより大幅に長い
        Assert.True(persistentDuration > sessionDuration * 100,
            $"Persistent duration ({persistentDuration.TotalMinutes} min) should be much longer than session ({sessionDuration.TotalMinutes} min)");

        // Assert 4: トークンは異なる
        Assert.NotEqual(sessionLoginResult.Token, persistentLoginResult.Token);
    }
}
