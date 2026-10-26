using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Tests.Integration.Helpers;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 交換・取り消し・却下で在庫が変わったら、景品の RowVersion も新しくする。
/// 保護者がその前に開いた編集画面で保存しても、在庫を古い数で上書きしない（409）
/// </summary>
public class RewardStockConcurrencyTests : IClassFixture<SqliteWebApplicationFactory>
{
    private readonly SqliteWebApplicationFactory _factory;

    public RewardStockConcurrencyTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ExchangeRequest_AfterParentOpenedEdit_ParentSaveIsConflict()
    {
        var (client, family, rewardId) = await SetUpAsync("交換申請と編集");
        var opened = await GetRewardAsync(client, family.ParentJwt, rewardId);

        await CreateRequestAsync(client, family, rewardId);

        Assert.Equal(HttpStatusCode.Conflict, (await SaveAsync(client, family.ParentJwt, opened)).StatusCode);
        Assert.Equal(opened.Stock - 1, (await GetRewardAsync(client, family.ParentJwt, rewardId)).Stock);
    }

    [Fact]
    public async Task CancelRequest_AfterParentOpenedEdit_ParentSaveIsConflict()
    {
        var (client, family, rewardId) = await SetUpAsync("取り消しと編集");
        var requestId = await CreateRequestAsync(client, family, rewardId);
        var opened = await GetRewardAsync(client, family.ParentJwt, rewardId);

        (await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/exchange-requests/{requestId}/cancel", family.ChildJwt)).EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Conflict, (await SaveAsync(client, family.ParentJwt, opened)).StatusCode);
        Assert.Equal(opened.Stock + 1, (await GetRewardAsync(client, family.ParentJwt, rewardId)).Stock);
    }

    [Fact]
    public async Task DirectExchange_AfterParentOpenedEdit_ParentSaveIsConflict()
    {
        var (client, family, rewardId) = await SetUpAsync("交換と編集");
        var opened = await GetRewardAsync(client, family.ParentJwt, rewardId);

        var exchange = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/rewards/{rewardId}/exchange", family.ChildJwt,
            new ExchangeRewardRequestDto { StudentId = family.StudentId });
        Assert.True(exchange.IsSuccessStatusCode, await exchange.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Conflict, (await SaveAsync(client, family.ParentJwt, opened)).StatusCode);
        Assert.Equal(opened.Stock - 1, (await GetRewardAsync(client, family.ParentJwt, rewardId)).Stock);
    }

    [Fact]
    public async Task ParentEdit_AfterReopening_Succeeds()
    {
        var (client, family, rewardId) = await SetUpAsync("開き直して編集");
        await CreateRequestAsync(client, family, rewardId);

        var reopened = await GetRewardAsync(client, family.ParentJwt, rewardId);

        Assert.Equal(HttpStatusCode.OK, (await SaveAsync(client, family.ParentJwt, reopened)).StatusCode);
    }

    private async Task<(HttpClient, FamilyTestHelper.Family, int)> SetUpAsync(string label)
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, label);
        await FamilyTestHelper.SetPointsAsync(_factory.Services, family.StudentId, 500);
        var rewardId = await FamilyTestHelper.CreateRewardAsync(client, family.ParentJwt, $"在庫の景品{Guid.NewGuid():N}", requiredPoints: 10, stock: 5);
        return (client, family, rewardId);
    }

    private static async Task<int> CreateRequestAsync(HttpClient client, FamilyTestHelper.Family family, int rewardId)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/exchange-requests", family.ChildJwt,
            new CreateExchangeRequestRequest { RewardId = rewardId });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ExchangeRequestDto>())!.Id;
    }

    private static async Task<RewardDto> GetRewardAsync(HttpClient client, string jwt, int id)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/rewards/{id}", jwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RewardDto>())!;
    }

    // 画面を開いたときの内容のまま保存する（在庫も開いたときの数）
    private static Task<HttpResponseMessage> SaveAsync(HttpClient client, string jwt, RewardDto opened)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(opened.Name), "Name" },
            { new StringContent(opened.Description ?? ""), "Description" },
            { new StringContent(opened.RequiredPoints.ToString()), "RequiredPoints" },
            { new StringContent(((int)opened.Category).ToString()), "Category" },
            { new StringContent(opened.IsPhysical.ToString()), "IsPhysical" },
            { new StringContent(opened.IsActive.ToString()), "IsActive" },
            { new StringContent(opened.Stock!.Value.ToString()), "Stock" },
            { new StringContent(Convert.ToBase64String(opened.RowVersion!)), "RowVersion" }
        };
        return FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/rewards/{opened.Id}", jwt, form);
    }
}
