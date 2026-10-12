using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 本番（家族で使う公開、012）の設定: 招待コード・CORS
/// </summary>
public class PublicDeploymentFactory : TestWebApplicationFactory
{
    public const string InviteCode = "family-2026";
    public const string FrontendOrigin = "https://math-drill.pages.dev";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("Registration:InviteCode", InviteCode);
        builder.UseSetting("Cors:AllowedOrigins:0", FrontendOrigin);
    }
}

public class InviteCodeAndCorsTests : IClassFixture<PublicDeploymentFactory>
{
    private readonly PublicDeploymentFactory _factory;

    public InviteCodeAndCorsTests(PublicDeploymentFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("wrong-code")]
    [InlineData("FAMILY-2026")]
    public async Task Register_WithoutCorrectInviteCode_IsRejected(string? inviteCode)
    {
        var response = await RegisterAsync(inviteCode);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("招待コード", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Register_WithInviteCode_Succeeds_AndGetsStarterRewards()
    {
        var response = await RegisterAsync(PublicDeploymentFactory.InviteCode);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Cors_AllowsConfiguredFrontendOnly()
    {
        var client = _factory.CreateClient();

        Assert.Equal(PublicDeploymentFactory.FrontendOrigin, await PreflightAsync(client, PublicDeploymentFactory.FrontendOrigin));
        Assert.Null(await PreflightAsync(client, "https://evil.example.com"));
    }

    private async Task<HttpResponseMessage> RegisterAsync(string? inviteCode)
    {
        var client = _factory.CreateClient();
        return await client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = $"invite_{Guid.NewGuid():N}@example.com",
            DisplayName = "招待テスト",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            InviteCode = inviteCode
        });
    }

    internal static async Task<string?> PreflightAsync(HttpClient client, string origin)
    {
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/auth/login");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        var response = await client.SendAsync(request);
        return response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values) ? values.Single() : null;
    }
}

/// <summary>
/// 招待コード・CORS の設定がないとき（開発）は今までどおり
/// </summary>
public class DefaultDeploymentSettingsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public DefaultDeploymentSettingsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WithoutInviteCodeSetting_NeedsNoCode()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"open_{Guid.NewGuid():N}@example.com",
            DisplayName = "招待なし",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Cors_AllowsLocalFrontendByDefault()
    {
        Assert.Equal("http://localhost:5071", await InviteCodeAndCorsTests.PreflightAsync(_factory.CreateClient(), "http://localhost:5071"));
    }

    [Fact]
    public async Task HealthCheck_IsAnonymous()
    {
        var response = await _factory.CreateClient().GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RewardImage_IsStoredInDatabase_AndServedAnonymously()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "画像DB");
        var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

        var created = await CreateRewardAsync(client, family.ParentJwt, png, "image/png");
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var reward = await created.Content.ReadFromJsonAsync<JsonElement>();
        var imageUrl = reward.GetProperty("imageUrl").GetString()!;
        Assert.StartsWith("/api/reward-images/", imageUrl);

        // ログインなし（<img> から）で取得できる
        var image = await _factory.CreateClient().GetAsync(imageUrl);
        Assert.Equal(HttpStatusCode.OK, image.StatusCode);
        Assert.Equal("image/png", image.Content.Headers.ContentType!.MediaType);
        Assert.Equal(png, await image.Content.ReadAsByteArrayAsync());

        // 景品を消すと画像も消える
        var rewardId = reward.GetProperty("id").GetInt32();
        (await FamilyTestHelper.SendAsync(client, HttpMethod.Delete, $"/api/rewards/{rewardId}", family.ParentJwt)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NotFound, (await _factory.CreateClient().GetAsync(imageUrl)).StatusCode);
    }

    [Fact]
    public async Task RewardImage_OverOneMegabyte_IsRejected()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "画像上限");

        var response = await CreateRewardAsync(client, family.ParentJwt, new byte[1024 * 1024 + 1], "image/jpeg");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("ファイルサイズが大きすぎます", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task RewardImage_Unknown_IsNotFound()
    {
        var response = await _factory.CreateClient().GetAsync($"/api/reward-images/{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static Task<HttpResponseMessage> CreateRewardAsync(HttpClient client, string jwt, byte[] image, string contentType)
    {
        var file = new ByteArrayContent(image);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        var form = new MultipartFormDataContent
        {
            { new StringContent($"画像つき景品{Guid.NewGuid():N}"), "Name" },
            { new StringContent("説明"), "Description" },
            { new StringContent("10"), "RequiredPoints" },
            { new StringContent("Toy"), "Category" },
            { new StringContent("true"), "IsPhysical" },
            { new StringContent("5"), "Stock" },
            { file, "Image", "photo.png" }
        };
        return FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/rewards", jwt, form);
    }
}
