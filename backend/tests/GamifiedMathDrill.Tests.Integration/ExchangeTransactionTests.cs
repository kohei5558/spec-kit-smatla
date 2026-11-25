using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 決めた保存だけを わざと失敗させる（DB との やり取りが その瞬間に 途切れた ことに する）
/// </summary>
public class FailingSaveInterceptor : SaveChangesInterceptor
{
    public Func<ChangeTracker, bool>? FailWhen { get; set; }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null && FailWhen?.Invoke(eventData.Context.ChangeTracker) == true)
        {
            throw new DbUpdateException("テスト用: DB への保存に失敗");
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

public class FailingSaveWebApplicationFactory : SqliteWebApplicationFactory
{
    public FailingSaveInterceptor Interceptor { get; } = new();

    protected override void ConfigureDbContext(DbContextOptionsBuilder options)
    {
        base.ConfigureDbContext(options);
        options.AddInterceptors(Interceptor);
    }
}

/// <summary>
/// 景品の交換まわりで、2つ目以降の保存が失敗したら、前の保存も取り消す（#31）。本物の SQLite で確かめる
/// </summary>
public class ExchangeTransactionTests : IClassFixture<FailingSaveWebApplicationFactory>
{
    private const int Stock = 5;
    private const int Points = 500;
    private const int RequiredPoints = 10;

    private readonly FailingSaveWebApplicationFactory _factory;

    public ExchangeTransactionTests(FailingSaveWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateRequest_FailsToSaveRequest_StockIsNotReduced()
    {
        var (client, family, rewardId) = await SetUpAsync("申請の途中で失敗");

        var response = await FailWhileAsync(t => Has<ExchangeRequest>(t, EntityState.Added),
            () => FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/exchange-requests", family.ChildJwt, new CreateExchangeRequestRequest { RewardId = rewardId }));

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(Stock, await StockAsync(rewardId));
        Assert.Empty(await RequestsAsync(family.StudentId));
    }

    [Fact]
    public async Task CancelRequest_FailsToSaveStatus_StockIsNotReturned()
    {
        var (client, family, rewardId) = await SetUpAsync("取り消しの途中で失敗");
        var requestId = await CreateRequestAsync(client, family, rewardId);

        var response = await FailWhileAsync(t => Has<ExchangeRequest>(t, EntityState.Modified),
            () => FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/exchange-requests/{requestId}/cancel", family.ChildJwt));

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(Stock - 1, await StockAsync(rewardId));
        Assert.Equal(ExchangeStatus.Pending, (await RequestsAsync(family.StudentId)).Single().Status);
    }

    [Fact]
    public async Task RejectRequest_FailsToSaveStatus_StockIsNotReturned()
    {
        var (client, family, rewardId) = await SetUpAsync("却下の途中で失敗");
        var requestId = await CreateRequestAsync(client, family, rewardId);

        var response = await FailWhileAsync(t => Has<ExchangeRequest>(t, EntityState.Modified),
            () => FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/exchange-requests/{requestId}/reject", family.ParentJwt, new { Reason = "テスト" }));

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(Stock - 1, await StockAsync(rewardId));
        Assert.Equal(ExchangeStatus.Pending, (await RequestsAsync(family.StudentId)).Single().Status);
    }

    [Fact]
    public async Task ApproveRequest_FailsToSaveAcquiredReward_PointsAreNotSpent()
    {
        var (client, family, rewardId) = await SetUpAsync("承認の途中で失敗");
        var requestId = await CreateRequestAsync(client, family, rewardId);

        var response = await FailWhileAsync(t => Has<AcquiredReward>(t, EntityState.Added),
            () => FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/exchange-requests/{requestId}/approve", family.ParentJwt, new { ParentNote = "テスト" }));

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(Points, await PointsAsync(family.StudentId));
        Assert.Equal(ExchangeStatus.Pending, (await RequestsAsync(family.StudentId)).Single().Status);
        Assert.Equal(0, await AcquiredCountAsync(family.StudentId));
    }

    [Fact]
    public async Task DirectExchange_FailsToSaveAcquiredReward_PointsAreKept()
    {
        // 直接の交換は 実物でない景品（申請なしで すぐ交換、ポイントを減らす → 獲得記録を作る）
        var (client, family, rewardId) = await SetUpAsync("交換の途中で失敗", physical: false);

        var response = await FailWhileAsync(t => Has<AcquiredReward>(t, EntityState.Added),
            () => FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/rewards/{rewardId}/exchange", family.ChildJwt, new ExchangeRewardRequestDto { StudentId = family.StudentId }));

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(Points, await PointsAsync(family.StudentId));
        Assert.Equal(0, await AcquiredCountAsync(family.StudentId));
    }

    [Fact]
    public async Task WithoutFailure_EverythingIsSaved()
    {
        // 失敗しないときは、今までどおり 全部 保存される
        var (client, family, rewardId) = await SetUpAsync("失敗しない申請と承認");
        var requestId = await CreateRequestAsync(client, family, rewardId);
        (await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/exchange-requests/{requestId}/approve", family.ParentJwt, new { ParentNote = "OK" })).EnsureSuccessStatusCode();
        Assert.Equal(Stock - 1, await StockAsync(rewardId));
        Assert.Equal(Points - RequiredPoints, await PointsAsync(family.StudentId));

        var virtualRewardId = await CreateRewardAsync(client, family, physical: false);
        var exchange = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/rewards/{virtualRewardId}/exchange", family.ChildJwt, new ExchangeRewardRequestDto { StudentId = family.StudentId });
        Assert.True(exchange.IsSuccessStatusCode, await exchange.Content.ReadAsStringAsync());
        Assert.Equal(Points - RequiredPoints * 2, await PointsAsync(family.StudentId));
        Assert.Equal(2, await AcquiredCountAsync(family.StudentId));
    }

    private static bool Has<T>(ChangeTracker tracker, EntityState state) where T : class =>
        tracker.Entries<T>().Any(e => e.State == state);

    private async Task<HttpResponseMessage> FailWhileAsync(Func<ChangeTracker, bool> failWhen, Func<Task<HttpResponseMessage>> action)
    {
        _factory.Interceptor.FailWhen = failWhen;
        try
        {
            return await action();
        }
        finally
        {
            _factory.Interceptor.FailWhen = null;
        }
    }

    private async Task<(HttpClient, FamilyTestHelper.Family, int)> SetUpAsync(string label, bool physical = true)
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, label);
        await FamilyTestHelper.SetPointsAsync(_factory.Services, family.StudentId, Points);
        return (client, family, await CreateRewardAsync(client, family, physical));
    }

    // 実物でない景品は 画面から作れる形が無いので、実物で作ってから DB で 直す
    private async Task<int> CreateRewardAsync(HttpClient client, FamilyTestHelper.Family family, bool physical)
    {
        var rewardId = await FamilyTestHelper.CreateRewardAsync(client, family.ParentJwt, $"景品{Guid.NewGuid():N}", requiredPoints: RequiredPoints, stock: Stock);
        if (!physical)
        {
            await QueryAsync(async db =>
            {
                var reward = await db.Rewards.SingleAsync(r => r.Id == rewardId);
                reward.IsPhysical = false;
                reward.Stock = null;
                return await db.SaveChangesAsync();
            });
        }
        return rewardId;
    }

    private static async Task<int> CreateRequestAsync(HttpClient client, FamilyTestHelper.Family family, int rewardId)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/exchange-requests", family.ChildJwt,
            new CreateExchangeRequestRequest { RewardId = rewardId });
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<ExchangeRequestDto>())!.Id;
    }

    private async Task<T> QueryAsync<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        using var scope = _factory.Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    private Task<int?> StockAsync(int rewardId) => QueryAsync(db => db.Rewards.AsNoTracking().Where(r => r.Id == rewardId).Select(r => r.Stock).SingleAsync());

    private Task<int> PointsAsync(int studentId) => QueryAsync(db => db.Students.AsNoTracking().Where(s => s.Id == studentId).Select(s => s.TotalPoints).SingleAsync());

    private Task<List<ExchangeRequest>> RequestsAsync(int studentId) => QueryAsync(db => db.ExchangeRequests.AsNoTracking().Where(r => r.StudentId == studentId).ToListAsync());

    private Task<int> AcquiredCountAsync(int studentId) => QueryAsync(db => db.AcquiredRewards.CountAsync(a => a.StudentId == studentId));
}
