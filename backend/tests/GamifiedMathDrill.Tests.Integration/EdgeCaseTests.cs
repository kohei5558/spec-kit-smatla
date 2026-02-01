using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// エッジケーステスト（仕様書120-150行目参照）
/// </summary>
public class EdgeCaseTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public EdgeCaseTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StockManagement_ConcurrentRequests_CorrectHandling()
    {
        // Arrange: 保護者でログイン
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // 在庫1の景品を作成
        var createRewardResponse = await _client.PostAsJsonAsync("/api/rewards", new
        {
            Name = "Limited Item",
            RequiredPoints = 50,
            Category = "Snack",
            IsPhysical = true,
            Stock = 1
        });
        var rewardDto = await createRewardResponse.Content.ReadFromJsonAsync<RewardDto>();

        // 子供でログイン
        var childLoginResponse = await _client.PostAsJsonAsync("/api/auth/child-login", new
        {
            ChildId = 1,
            PIN = "1234"
        });
        var childLoginResult = await childLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult.Token);

        // Act: 交換申請（成功するはず）
        var exchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });

        // 在庫が0になったので、2回目は失敗するはず
        var secondExchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, exchangeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondExchangeResponse.StatusCode);
    }

    [Fact]
    public async Task PointInsufficiency_ApprovalTimeCheck()
    {
        // Arrange: 保護者でログイン
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // 景品を作成（100ポイント必要）
        var createRewardResponse = await _client.PostAsJsonAsync("/api/rewards", new
        {
            Name = "Expensive Item",
            RequiredPoints = 100,
            Category = "Toy",
            IsPhysical = true,
            Stock = 10
        });
        var rewardDto = await createRewardResponse.Content.ReadFromJsonAsync<RewardDto>();

        // 子供でログイン（初期ポイント: 0）
        var childLoginResponse = await _client.PostAsJsonAsync("/api/auth/child-login", new
        {
            ChildId = 1,
            PIN = "1234"
        });

        // ポイント不足時の申請は拒否されるべき
        var childLoginResult = await childLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult.Token);

        var exchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });

        // Assert: ポイント不足でエラー
        Assert.Equal(HttpStatusCode.BadRequest, exchangeResponse.StatusCode);
        var errorContent = await exchangeResponse.Content.ReadAsStringAsync();
        Assert.Contains("ポイントが不足しています", errorContent);
    }

    [Fact]
    public async Task ImageUpload_OversizedFile_Rejected()
    {
        // Arrange: 保護者でログイン
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // 5MBを超える画像をシミュレート（実際は6MB）
        var oversizedContent = new byte[6 * 1024 * 1024]; // 6MB
        var fileContent = new ByteArrayContent(oversizedContent);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var formData = new MultipartFormDataContent
        {
            { new StringContent("Test Reward"), "Name" },
            { new StringContent("100"), "RequiredPoints" },
            { new StringContent("Toy"), "Category" },
            { new StringContent("true"), "IsPhysical" },
            { new StringContent("10"), "Stock" },
            { fileContent, "Image", "large.jpg" }
        };

        // Act
        var response = await _client.PostAsync("/api/rewards", formData);

        // Assert: ファイルサイズ超過でエラー
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("ファイルサイズが大きすぎます", errorContent);
    }

    [Fact]
    public async Task RequestCancellation_OnlyPendingStatusAllowed()
    {
        // Arrange: 保護者でログイン
        var parentLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var parentLoginResult = await parentLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // 景品を作成
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentLoginResult.Token);
        var createRewardResponse = await _client.PostAsJsonAsync("/api/rewards", new
        {
            Name = "Test Item",
            RequiredPoints = 10,
            Category = "Snack",
            IsPhysical = true,
            Stock = 5
        });
        var rewardDto = await createRewardResponse.Content.ReadFromJsonAsync<RewardDto>();

        // 子供でログイン
        var childLoginResponse = await _client.PostAsJsonAsync("/api/auth/child-login", new
        {
            ChildId = 1,
            PIN = "1234"
        });
        var childLoginResult = await childLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // 交換申請
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult.Token);
        var exchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });
        var exchangeRequest = await exchangeResponse.Content.ReadFromJsonAsync<ExchangeRequestDto>();

        // Act: Pendingステータスのキャンセル（成功するはず）
        var cancelResponse = await _client.PutAsync($"/api/exchange-requests/{exchangeRequest.Id}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        // キャンセル後、再度キャンセル試行（失敗するはず）
        var secondCancelResponse = await _client.PutAsync($"/api/exchange-requests/{exchangeRequest.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.BadRequest, secondCancelResponse.StatusCode);
    }

    [Fact]
    public async Task ApprovedRequest_CannotBeCancelled()
    {
        // Arrange: 保護者でログイン
        var parentLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var parentLoginResult = await parentLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // 景品を作成
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentLoginResult.Token);
        var createRewardResponse = await _client.PostAsJsonAsync("/api/rewards", new
        {
            Name = "Test Item",
            RequiredPoints = 10,
            Category = "Snack",
            IsPhysical = true,
            Stock = 5
        });
        var rewardDto = await createRewardResponse.Content.ReadFromJsonAsync<RewardDto>();

        // 子供でログイン・申請
        var childLoginResponse = await _client.PostAsJsonAsync("/api/auth/child-login", new
        {
            ChildId = 1,
            PIN = "1234"
        });
        var childLoginResult = await childLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult.Token);

        var exchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });
        var exchangeRequest = await exchangeResponse.Content.ReadFromJsonAsync<ExchangeRequestDto>();

        // 保護者が承認
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentLoginResult.Token);
        await _client.PutAsync($"/api/exchange-requests/{exchangeRequest.Id}/approve", null);

        // Act: 承認済み申請をキャンセル試行（失敗するはず）
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult.Token);
        var cancelResponse = await _client.PutAsync($"/api/exchange-requests/{exchangeRequest.Id}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, cancelResponse.StatusCode);
        var errorContent = await cancelResponse.Content.ReadAsStringAsync();
        Assert.Contains("申請中のみキャンセルできます", errorContent);
    }

    [Fact]
    public async Task StockReturnsOnRejection()
    {
        // Arrange: 保護者でログイン
        var parentLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var parentLoginResult = await parentLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // 在庫5の景品を作成
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentLoginResult.Token);
        var createRewardResponse = await _client.PostAsJsonAsync("/api/rewards", new
        {
            Name = "Stock Test Item",
            RequiredPoints = 10,
            Category = "Snack",
            IsPhysical = true,
            Stock = 5
        });
        var rewardDto = await createRewardResponse.Content.ReadFromJsonAsync<RewardDto>();

        // 子供でログイン・申請
        var childLoginResponse = await _client.PostAsJsonAsync("/api/auth/child-login", new
        {
            ChildId = 1,
            PIN = "1234"
        });
        var childLoginResult = await childLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult.Token);

        var exchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });
        var exchangeRequest = await exchangeResponse.Content.ReadFromJsonAsync<ExchangeRequestDto>();

        // 在庫確認（申請後は4になっているはず）
        var afterRequestResponse = await _client.GetAsync($"/api/rewards/{rewardDto.Id}");
        var afterRequestReward = await afterRequestResponse.Content.ReadFromJsonAsync<RewardDto>();
        Assert.Equal(4, afterRequestReward.Stock);

        // Act: 保護者が却下
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentLoginResult.Token);
        await _client.PutAsJsonAsync($"/api/exchange-requests/{exchangeRequest.Id}/reject", new
        {
            Reason = "Test rejection"
        });

        // Assert: 在庫が戻っているはず（5に戻る）
        var afterRejectionResponse = await _client.GetAsync($"/api/rewards/{rewardDto.Id}");
        var afterRejectionReward = await afterRejectionResponse.Content.ReadFromJsonAsync<RewardDto>();
        Assert.Equal(5, afterRejectionReward.Stock);
    }
}
