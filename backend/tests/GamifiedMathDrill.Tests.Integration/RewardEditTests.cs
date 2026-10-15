using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 本物の SQLite で動かす。InMemory DB は RowVersion などの値を DB 側で作る・作らないの違いを再現しないため
/// </summary>
public class SqliteWebApplicationFactory : TestWebApplicationFactory
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        _connection.Open();
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

/// <summary>
/// 既存の景品の編集（画像の変更を含む）。以前は RowVersion が DB に保存されず、編集が必ず 400 になっていた
/// </summary>
public class RewardEditTests : IClassFixture<SqliteWebApplicationFactory>
{
    private static readonly byte[] Png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    private readonly SqliteWebApplicationFactory _factory;

    public RewardEditTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task StarterReward_CanChangeImage()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "初期景品の画像変更");
        var reward = (await GetRewardsAsync(client, family.ParentJwt)).First(r => r.ImageUrl!.StartsWith("/images/rewards/"));
        Assert.NotNull(reward.RowVersion);

        var response = await UpdateAsync(client, family.ParentJwt, reward, reward.RowVersion, Png);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<RewardDto>())!;
        Assert.StartsWith("/api/reward-images/", updated.ImageUrl);
        Assert.Equal(HttpStatusCode.OK, (await _factory.CreateClient().GetAsync(updated.ImageUrl)).StatusCode);
    }

    [Fact]
    public async Task UploadedReward_CanChangeImage_AndOldImageIsDeleted()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "追加景品の画像変更");
        var created = await CreateWithImageAsync(client, family.ParentJwt);
        var reward = await GetRewardAsync(client, family.ParentJwt, created.Id);

        var response = await UpdateAsync(client, family.ParentJwt, reward, reward.RowVersion, Png);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = (await response.Content.ReadFromJsonAsync<RewardDto>())!;
        Assert.NotEqual(created.ImageUrl, updated.ImageUrl);
        Assert.Equal(HttpStatusCode.NotFound, (await _factory.CreateClient().GetAsync(created.ImageUrl)).StatusCode);
    }

    [Fact]
    public async Task Reward_CanBeEditedWithoutImage_TwiceInARow()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "画像なしの編集");
        var id = await FamilyTestHelper.CreateRewardAsync(client, family.ParentJwt, $"編集する景品{Guid.NewGuid():N}");

        var first = await GetRewardAsync(client, family.ParentJwt, id);
        Assert.Equal(HttpStatusCode.OK, (await UpdateAsync(client, family.ParentJwt, first, first.RowVersion, null, "1回目")).StatusCode);

        // 保存し直した最新の値を使えば、続けて編集できる
        var second = await GetRewardAsync(client, family.ParentJwt, id);
        Assert.Equal(HttpStatusCode.OK, (await UpdateAsync(client, family.ParentJwt, second, second.RowVersion, null, "2回目")).StatusCode);
        Assert.Equal("2回目", (await GetRewardAsync(client, family.ParentJwt, id)).Name);
    }

    [Fact]
    public async Task Reward_EditedWithOldVersion_IsConflict()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "同時編集");
        var id = await FamilyTestHelper.CreateRewardAsync(client, family.ParentJwt, $"同時に編集する景品{Guid.NewGuid():N}");
        var opened = await GetRewardAsync(client, family.ParentJwt, id);

        // 別の画面で先に保存された
        Assert.Equal(HttpStatusCode.OK, (await UpdateAsync(client, family.ParentJwt, opened, opened.RowVersion, null, "先に保存")).StatusCode);

        var response = await UpdateAsync(client, family.ParentJwt, opened, opened.RowVersion, null, "あとから保存");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("先に保存", (await GetRewardAsync(client, family.ParentJwt, id)).Name);
    }

    private static async Task<List<RewardDto>> GetRewardsAsync(HttpClient client, string jwt)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, "/api/rewards", jwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<RewardDto>>())!;
    }

    private static async Task<RewardDto> GetRewardAsync(HttpClient client, string jwt, int id)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/rewards/{id}", jwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RewardDto>())!;
    }

    private static async Task<RewardDto> CreateWithImageAsync(HttpClient client, string jwt)
    {
        var file = new ByteArrayContent(Png);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var form = new MultipartFormDataContent
        {
            { new StringContent($"画像つき景品{Guid.NewGuid():N}"), "Name" },
            { new StringContent("説明"), "Description" },
            { new StringContent("10"), "RequiredPoints" },
            { new StringContent("Toy"), "Category" },
            { new StringContent("true"), "IsPhysical" },
            { file, "Image", "photo.png" }
        };
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/rewards", jwt, form);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<RewardDto>())!;
    }

    // 画面（RewardApiClient.UpdateRewardAsync）と同じ形で送る
    private static Task<HttpResponseMessage> UpdateAsync(HttpClient client, string jwt, RewardDto reward, byte[]? rowVersion, byte[]? image, string? name = null)
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(name ?? reward.Name), "Name" },
            { new StringContent(reward.Description ?? ""), "Description" },
            { new StringContent(reward.RequiredPoints.ToString()), "RequiredPoints" },
            { new StringContent(((int)reward.Category).ToString()), "Category" },
            { new StringContent(reward.IsPhysical.ToString()), "IsPhysical" },
            { new StringContent(reward.IsActive.ToString()), "IsActive" }
        };
        if (reward.Stock.HasValue)
        {
            form.Add(new StringContent(reward.Stock.Value.ToString()), "Stock");
        }
        if (rowVersion != null)
        {
            form.Add(new StringContent(Convert.ToBase64String(rowVersion)), "RowVersion");
        }
        if (image != null)
        {
            var file = new ByteArrayContent(image);
            file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(file, "Image", "photo.png");
        }
        return FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/rewards/{reward.Id}", jwt, form);
    }
}
