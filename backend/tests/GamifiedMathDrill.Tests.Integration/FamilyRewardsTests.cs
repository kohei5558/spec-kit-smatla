using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 景品の家庭ごとの管理（007）の統合テスト
/// </summary>
public class FamilyRewardsTests : IClassFixture<TestWebApplicationFactory>
{
    private const int StarterRewardCount = 20;
    private readonly TestWebApplicationFactory _factory;

    public FamilyRewardsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region US1: 保護者は自分の家庭の景品だけを管理する

    [Fact]
    public async Task Parent_SeesAndManagesOnlyOwnFamilyRewards()
    {
        var client = _factory.CreateClient();
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "景品A");
        var b = await FamilyTestHelper.CreateFamilyAsync(client, "景品B");
        var rewardOfA = await FamilyTestHelper.CreateRewardAsync(client, a.ParentJwt, "A家だけの景品");
        var rewardOfB = await FamilyTestHelper.CreateRewardAsync(client, b.ParentJwt, "B家だけの景品");

        var idsForA = await GetRewardIdsAsync(client, a.ParentJwt);
        Assert.Contains(rewardOfA, idsForA);
        Assert.DoesNotContain(rewardOfB, idsForA);

        Assert.Equal(HttpStatusCode.NotFound, (await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/rewards/{rewardOfB}", a.ParentJwt)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/rewards/{rewardOfB}", a.ParentJwt, UpdateForm("書き換え", 1))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await FamilyTestHelper.SendAsync(client, HttpMethod.Delete, $"/api/rewards/{rewardOfB}", a.ParentJwt)).StatusCode);

        // B の景品は変更・削除されていない
        var rewardB = await GetRewardAsync(client, b.ParentJwt, rewardOfB);
        Assert.Equal("B家だけの景品", rewardB.Name);

        // 自分の景品は更新・削除できる
        var currentA = await GetRewardAsync(client, a.ParentJwt, rewardOfA);
        Assert.Equal(HttpStatusCode.OK, (await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/rewards/{rewardOfA}", a.ParentJwt, UpdateForm("A家の景品（更新）", 70, currentA.Stock, currentA.RowVersion))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await FamilyTestHelper.SendAsync(client, HttpMethod.Delete, $"/api/rewards/{rewardOfA}", a.ParentJwt)).StatusCode);
    }

    #endregion

    #region US2: 子供は自分の家庭の景品とだけ交換できる

    [Fact]
    public async Task Child_SeesAndExchangesOnlyOwnFamilyRewards()
    {
        var client = _factory.CreateClient();
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "子供景品A");
        var b = await FamilyTestHelper.CreateFamilyAsync(client, "子供景品B");
        await FamilyTestHelper.SetPointsAsync(_factory.Services, a.StudentId, 500);
        var rewardOfA = await FamilyTestHelper.CreateRewardAsync(client, a.ParentJwt, "A家のおかし");
        var rewardOfB = await FamilyTestHelper.CreateRewardAsync(client, b.ParentJwt, "B家のおかし", stock: 5);

        var idsForChildA = await GetRewardIdsAsync(client, a.ChildJwt);
        Assert.Contains(rewardOfA, idsForChildA);
        Assert.DoesNotContain(rewardOfB, idsForChildA);
        Assert.Equal(HttpStatusCode.NotFound, (await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/rewards/{rewardOfB}", a.ChildJwt)).StatusCode);

        var exchange = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/rewards/{rewardOfB}/exchange", a.ChildJwt,
            new ExchangeRewardRequestDto { StudentId = a.StudentId });
        Assert.Equal(HttpStatusCode.NotFound, exchange.StatusCode);

        var request = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/exchange-requests", a.ChildJwt,
            new CreateExchangeRequestRequest { RewardId = rewardOfB });
        Assert.Equal(HttpStatusCode.NotFound, request.StatusCode);

        // ポイントも B の在庫も変わっていない
        Assert.Equal(500, await GetPointsAsync(client, a.ChildJwt, a.StudentId));
        Assert.Equal(5, (await GetRewardAsync(client, b.ParentJwt, rewardOfB)).Stock);

        // 自分の家庭の景品には申請できる
        var ownRequest = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/exchange-requests", a.ChildJwt,
            new CreateExchangeRequestRequest { RewardId = rewardOfA });
        Assert.True(ownRequest.IsSuccessStatusCode, await ownRequest.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SameStarterReward_ExchangeDoesNotChangeOtherFamilysStock()
    {
        var client = _factory.CreateClient();
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "在庫A");
        var b = await FamilyTestHelper.CreateFamilyAsync(client, "在庫B");
        await FamilyTestHelper.SetPointsAsync(_factory.Services, a.StudentId, 500);
        var umaiboA = await FindRewardByNameAsync(client, a.ParentJwt, "うまい棒 チーズ味");
        var umaiboB = await FindRewardByNameAsync(client, b.ParentJwt, "うまい棒 チーズ味");
        Assert.NotEqual(umaiboA.Id, umaiboB.Id);

        var request = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/exchange-requests", a.ChildJwt,
            new CreateExchangeRequestRequest { RewardId = umaiboA.Id });
        Assert.True(request.IsSuccessStatusCode, await request.Content.ReadAsStringAsync());

        Assert.Equal(umaiboA.Stock - 1, (await GetRewardAsync(client, a.ParentJwt, umaiboA.Id)).Stock);
        Assert.Equal(umaiboB.Stock, (await GetRewardAsync(client, b.ParentJwt, umaiboB.Id)).Stock);
    }

    #endregion

    #region US3: 新しい家庭には初期景品が用意される

    [Fact]
    public async Task NewParent_GetsStarterRewards_IndependentPerFamily()
    {
        var client = _factory.CreateClient();
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "初期A");
        var b = await FamilyTestHelper.CreateFamilyAsync(client, "初期B");

        var rewardsA = await GetRewardsAsync(client, a.ParentJwt);
        var rewardsB = await GetRewardsAsync(client, b.ParentJwt);
        Assert.Equal(StarterRewardCount, rewardsA.Count);
        Assert.Equal(StarterRewardCount, rewardsB.Count);
        Assert.Empty(rewardsA.Select(r => r.Id).Intersect(rewardsB.Select(r => r.Id)));

        // A が必要ポイントを変えても B は変わらない
        var targetA = rewardsA.Single(r => r.Name == "チロルチョコ");
        var targetB = rewardsB.Single(r => r.Name == "チロルチョコ");
        var update = await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/rewards/{targetA.Id}", a.ParentJwt,
            UpdateForm("チロルチョコ", 999, targetA.Stock, targetA.RowVersion));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(999, (await GetRewardAsync(client, a.ParentJwt, targetA.Id)).RequiredPoints);
        Assert.Equal(targetB.RequiredPoints, (await GetRewardAsync(client, b.ParentJwt, targetB.Id)).RequiredPoints);
    }

    #endregion

    #region Helpers

    private static MultipartFormDataContent UpdateForm(string name, int requiredPoints, int? stock = 10, byte[]? rowVersion = null) => new()
    {
        { new StringContent(Convert.ToBase64String(rowVersion ?? new byte[8])), "RowVersion" },
        { new StringContent(name), "Name" },
        { new StringContent("更新テスト"), "Description" },
        { new StringContent(requiredPoints.ToString()), "RequiredPoints" },
        { new StringContent("Snack"), "Category" },
        { new StringContent("true"), "IsPhysical" },
        { new StringContent((stock ?? 10).ToString()), "Stock" },
        { new StringContent("true"), "IsActive" }
    };

    private static async Task<List<RewardDto>> GetRewardsAsync(HttpClient client, string jwt)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, "/api/rewards", jwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<RewardDto>>())!;
    }

    private static async Task<List<int>> GetRewardIdsAsync(HttpClient client, string jwt)
    {
        return (await GetRewardsAsync(client, jwt)).Select(r => r.Id).ToList();
    }

    private static async Task<RewardDto> GetRewardAsync(HttpClient client, string jwt, int id)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/rewards/{id}", jwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RewardDto>())!;
    }

    private static async Task<RewardDto> FindRewardByNameAsync(HttpClient client, string jwt, string name)
    {
        return (await GetRewardsAsync(client, jwt)).Single(r => r.Name == name);
    }

    private static async Task<int> GetPointsAsync(HttpClient client, string jwt, int studentId)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/students/{studentId}", jwt);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        return json.GetProperty("data").GetProperty("totalPoints").GetInt32();
    }

    #endregion
}
