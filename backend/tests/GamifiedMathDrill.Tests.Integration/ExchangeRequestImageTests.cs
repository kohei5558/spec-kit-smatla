using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Tests.Integration.Helpers;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 交換申請の景品画像は、どの API でも相対パスのまま返す（画面の ImageUrls.Resolve で解決する、012）。
/// 以前は詳細・承認・却下だけ API のアドレスを付けていたため、画面側にある初期景品の画像が表示されなかった
/// </summary>
public class ExchangeRequestImageTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ExchangeRequestImageTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Detail_ReturnsRelativeImageUrl()
    {
        var (client, family, reward) = await SetUpAsync("交換申請の詳細の画像");
        var id = await CreateRequestAsync(client, family, reward);

        var detail = await ReadAsync(await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/exchange-requests/{id}", family.ParentJwt));

        Assert.Equal(reward.ImageUrl, detail.RewardImageUrl);
    }

    [Fact]
    public async Task Approve_ReturnsRelativeImageUrl()
    {
        var (client, family, reward) = await SetUpAsync("交換申請の承認の画像");
        var id = await CreateRequestAsync(client, family, reward);

        var approved = await ReadAsync(await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/exchange-requests/{id}/approve", family.ParentJwt,
            new ApproveRequestRequest()));

        Assert.Equal(reward.ImageUrl, approved.RewardImageUrl);
    }

    [Fact]
    public async Task Reject_ReturnsRelativeImageUrl()
    {
        var (client, family, reward) = await SetUpAsync("交換申請の却下の画像");
        var id = await CreateRequestAsync(client, family, reward);

        var rejected = await ReadAsync(await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/exchange-requests/{id}/reject", family.ParentJwt,
            new RejectRequestRequest { Reason = "また今度" }));

        Assert.Equal(reward.ImageUrl, rejected.RewardImageUrl);
    }

    private async Task<(HttpClient, FamilyTestHelper.Family, RewardDto)> SetUpAsync(string label)
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, label);
        await FamilyTestHelper.SetPointsAsync(_factory.Services, family.StudentId, 500);

        var rewards = await (await FamilyTestHelper.SendAsync(client, HttpMethod.Get, "/api/rewards", family.ParentJwt)).Content.ReadFromJsonAsync<List<RewardDto>>();
        var starter = rewards!.First(r => r.ImageUrl != null && r.ImageUrl.StartsWith("/images/rewards/"));
        return (client, family, starter);
    }

    private static async Task<int> CreateRequestAsync(HttpClient client, FamilyTestHelper.Family family, RewardDto reward)
    {
        var created = await ReadAsync(await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/exchange-requests", family.ChildJwt,
            new CreateExchangeRequestRequest { RewardId = reward.Id }));
        Assert.Equal(reward.ImageUrl, created.RewardImageUrl);
        return created.Id;
    }

    private static async Task<ExchangeRequestDto> ReadAsync(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ExchangeRequestDto>())!;
    }
}
