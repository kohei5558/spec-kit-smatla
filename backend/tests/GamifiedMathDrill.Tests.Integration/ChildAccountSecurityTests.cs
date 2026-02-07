using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Core.DTOs;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// PINロックアウト機能の統合テスト (Phase 3, US1)
/// </summary>
public class ChildAccountSecurityTests : AuthenticatedTestBase
{
    public ChildAccountSecurityTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ChildLogin_ThreeFailedAttempts_LocksAccount()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "ロックアウトテスト太郎",
            GradeLevel = 3,
            PresetAvatarId = 1,
            PIN = "1234"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var childAccount = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(childAccount);

        // ログアウト
        Client.DefaultRequestHeaders.Authorization = null;

        var loginRequest = new
        {
            ChildAccountId = childAccount.Id,
            PIN = "9999" // 間違ったPIN
        };

        // Act - 3回失敗させる
        var firstAttempt = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);
        var secondAttempt = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);
        var thirdAttempt = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);

        // Assert - 最初の3回はUnauthorized
        Assert.Equal(HttpStatusCode.Unauthorized, firstAttempt.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, secondAttempt.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, thirdAttempt.StatusCode);

        // 4回目はTooManyRequests（ロックアウト）
        var fourthAttempt = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);
        Assert.Equal(HttpStatusCode.TooManyRequests, fourthAttempt.StatusCode);

        var errorContent = await fourthAttempt.Content.ReadAsStringAsync();
        Assert.Contains("ロック", errorContent);
    }

    [Fact]
    public async Task ChildLogin_CorrectPinAfterFailures_ResetsCounter()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "リセットテスト花子",
            GradeLevel = 4,
            PresetAvatarId = 2,
            PIN = "5678"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var childAccount = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(childAccount);

        // ログアウト
        Client.DefaultRequestHeaders.Authorization = null;

        // 2回失敗
        var wrongPinRequest = new
        {
            ChildAccountId = childAccount.Id,
            PIN = "0000"
        };

        await Client.PostAsJsonAsync("/api/auth/child/login", wrongPinRequest);
        await Client.PostAsJsonAsync("/api/auth/child/login", wrongPinRequest);

        // 正しいPINでログイン成功
        var correctPinRequest = new
        {
            ChildAccountId = childAccount.Id,
            PIN = "5678"
        };

        var successResponse = await Client.PostAsJsonAsync("/api/auth/child/login", correctPinRequest);

        // Assert - 正しいPINで成功
        Assert.Equal(HttpStatusCode.OK, successResponse.StatusCode);

        // ログアウト
        Client.DefaultRequestHeaders.Authorization = null;

        // 再度2回失敗しても、カウンターがリセットされているのでロックされない
        await Client.PostAsJsonAsync("/api/auth/child/login", wrongPinRequest);
        var secondFailure = await Client.PostAsJsonAsync("/api/auth/child/login", wrongPinRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, secondFailure.StatusCode);
        Assert.NotEqual(HttpStatusCode.TooManyRequests, secondFailure.StatusCode);
    }

    [Fact(Skip = "5分間のロックアウト解除テストは実行時間が長いためスキップ")]
    public async Task ChildLogin_LockoutExpires_AfterFiveMinutes()
    {
        // このテストは実際には5分待つ必要があるため、手動テストまたは
        // モック時刻を使用した単体テストで確認する
        
        // Arrange
        await AuthenticateAsParentAsync();

        var createRequest = new ChildAccountCreateDto
        {
            Name = "タイムアウトテスト",
            GradeLevel = 2,
            PresetAvatarId = 3,
            PIN = "4321"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var childAccount = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(childAccount);

        // ログアウト
        Client.DefaultRequestHeaders.Authorization = null;

        // 3回失敗してロック
        var wrongRequest = new { ChildAccountId = childAccount.Id, PIN = "0000" };
        await Client.PostAsJsonAsync("/api/auth/child/login", wrongRequest);
        await Client.PostAsJsonAsync("/api/auth/child/login", wrongRequest);
        await Client.PostAsJsonAsync("/api/auth/child/login", wrongRequest);

        // ロック確認
        var lockedResponse = await Client.PostAsJsonAsync("/api/auth/child/login", wrongRequest);
        Assert.Equal(HttpStatusCode.TooManyRequests, lockedResponse.StatusCode);

        // 5分待機（実際のテストではスキップ）
        // await Task.Delay(TimeSpan.FromMinutes(5));

        // 正しいPINで再試行（5分後は成功するはず）
        // var correctRequest = new { ChildAccountId = childAccount.Id, PIN = "4321" };
        // var unlockedResponse = await Client.PostAsJsonAsync("/api/auth/child/login", correctRequest);
        // Assert.Equal(HttpStatusCode.OK, unlockedResponse.StatusCode);
    }

    [Fact]
    public async Task ChildLogin_DifferentAccounts_HaveIndependentLockouts()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 2つの子供アカウントを作成
        var child1Request = new ChildAccountCreateDto
        {
            Name = "独立テスト1",
            GradeLevel = 1,
            PresetAvatarId = 1,
            PIN = "1111"
        };

        var child2Request = new ChildAccountCreateDto
        {
            Name = "独立テスト2",
            GradeLevel = 2,
            PresetAvatarId = 2,
            PIN = "2222"
        };

        var child1Response = await Client.PostAsJsonAsync("/api/child-accounts", child1Request);
        var child1 = await child1Response.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(child1);

        var child2Response = await Client.PostAsJsonAsync("/api/child-accounts", child2Request);
        var child2 = await child2Response.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(child2);

        // ログアウト
        Client.DefaultRequestHeaders.Authorization = null;

        // child1で3回失敗
        var child1WrongRequest = new { ChildAccountId = child1.Id, PIN = "0000" };
        await Client.PostAsJsonAsync("/api/auth/child/login", child1WrongRequest);
        await Client.PostAsJsonAsync("/api/auth/child/login", child1WrongRequest);
        await Client.PostAsJsonAsync("/api/auth/child/login", child1WrongRequest);

        // child1はロックされる
        var child1LockedResponse = await Client.PostAsJsonAsync("/api/auth/child/login", child1WrongRequest);
        Assert.Equal(HttpStatusCode.TooManyRequests, child1LockedResponse.StatusCode);

        // child2は正常にログインできる
        var child2CorrectRequest = new { ChildAccountId = child2.Id, PIN = "2222" };
        var child2LoginResponse = await Client.PostAsJsonAsync("/api/auth/child/login", child2CorrectRequest);

        // Assert - child2はロックされていない
        Assert.Equal(HttpStatusCode.OK, child2LoginResponse.StatusCode);
    }

    [Fact]
    public async Task ChildLogin_SuspendedAccount_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "停止テスト太郎",
            GradeLevel = 4,
            PresetAvatarId = 2,
            PIN = "5555"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var childAccount = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(childAccount);

        // アカウントを停止
        var suspendResponse = await Client.PostAsync($"/api/child-accounts/{childAccount.Id}/suspend", null);
        Assert.Equal(HttpStatusCode.NoContent, suspendResponse.StatusCode);

        // ログアウト
        Client.DefaultRequestHeaders.Authorization = null;

        // Act: 停止中のアカウントでログイン試行
        var loginRequest = new
        {
            ChildAccountId = childAccount.Id,
            PIN = "5555" // 正しいPIN
        };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);

        // Assert: 停止中のため拒否される
        Assert.Equal(HttpStatusCode.Forbidden, loginResponse.StatusCode);
    }

    [Fact]
    public async Task ChildLogin_DeletedAccount_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "削除テスト子",
            GradeLevel = 3,
            PresetAvatarId = 5,
            PIN = "6666"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var childAccount = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(childAccount);
        var childId = childAccount.Id;

        // アカウントを削除
        var deleteResponse = await Client.DeleteAsync($"/api/child-accounts/{childId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // ログアウト
        Client.DefaultRequestHeaders.Authorization = null;

        // Act: 削除済みアカウントでログイン試行
        var loginRequest = new
        {
            ChildAccountId = childId,
            PIN = "6666" // 正しいPIN（だが削除済み）
        };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);

        // Assert: 削除済みのため認証失敗（NotFound）
        Assert.Equal(HttpStatusCode.NotFound, loginResponse.StatusCode);
    }
}

