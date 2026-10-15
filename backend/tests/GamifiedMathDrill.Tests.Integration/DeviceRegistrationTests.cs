using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 子供用端末の登録（006）の統合テスト
/// 家庭ごとに保護者を新規登録し、テスト間で端末数や子供の一覧が干渉しないようにする
/// </summary>
public class DeviceRegistrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public DeviceRegistrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region US1: 端末登録

    [Fact]
    public async Task RegisterDevice_AsParent_ReturnsTokenAndDevice()
    {
        var client = _factory.CreateClient();
        var parent = await CreateParentAsync(client);

        var response = await SendAsync(client, HttpMethod.Post, "/api/devices", parent,
            body: new RegisterDeviceRequest { Name = "リビングのタブレット" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RegisterDeviceResponse>();
        Assert.NotNull(result);
        Assert.True(result.Token.Length >= 43, "256bit以上のトークンであること");
        Assert.Equal("リビングのタブレット", result.Device.Name);
        Assert.True(result.Device.Id > 0);
    }

    [Fact]
    public async Task RegisterDevice_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/devices", new RegisterDeviceRequest { Name = "端末" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RegisterDevice_AsChild_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        var parent = await CreateParentAsync(client);
        var child = await CreateChildAsync(client, parent, "端末登録禁止テスト", "1357");
        var deviceToken = await AuthenticationHelper.RegisterDeviceAsync(client, parent);
        var childJwt = await LoginChildAsync(client, deviceToken, child.Id, "1357");

        var response = await SendAsync(client, HttpMethod.Post, "/api/devices", childJwt,
            body: new RegisterDeviceRequest { Name = "子供が登録" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234567890123456789012345678901")] // 31文字
    public async Task RegisterDevice_WithInvalidName_ReturnsBadRequest(string name)
    {
        var client = _factory.CreateClient();
        var parent = await CreateParentAsync(client);

        var response = await SendAsync(client, HttpMethod.Post, "/api/devices", parent,
            body: new RegisterDeviceRequest { Name = name });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterDevice_EleventhDevice_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var parent = await CreateParentAsync(client);
        for (int i = 1; i <= 10; i++)
        {
            await AuthenticationHelper.RegisterDeviceAsync(client, parent, $"端末{i}");
        }

        var response = await SendAsync(client, HttpMethod.Post, "/api/devices", parent,
            body: new RegisterDeviceRequest { Name = "端末11" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("10台", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetChildren_WithDeviceToken_ReturnsOnlyOwnActiveChildren()
    {
        var client = _factory.CreateClient();
        var parentA = await CreateParentAsync(client);
        var parentB = await CreateParentAsync(client);
        var activeChild = await CreateChildAsync(client, parentA, "A家の子", "2468");
        var suspendedChild = await CreateChildAsync(client, parentA, "A家の停止中の子", "1111");
        var otherFamilyChild = await CreateChildAsync(client, parentB, "B家の子", "2222");
        var suspend = await SendAsync(client, HttpMethod.Post, $"/api/child-accounts/{suspendedChild.Id}/suspend", parentA);
        suspend.EnsureSuccessStatusCode();
        var deviceToken = await AuthenticationHelper.RegisterDeviceAsync(client, parentA);

        var response = await SendAsync(client, HttpMethod.Get, "/api/devices/current/children", deviceToken: deviceToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var children = await response.Content.ReadFromJsonAsync<List<ChildAccountDto>>();
        var ids = children!.Select(c => c.Id).ToList();
        Assert.Equal(new[] { activeChild.Id }, ids);
        Assert.DoesNotContain(otherFamilyChild.Id, ids);
    }

    [Fact]
    public async Task ChildLogin_WithOwnDeviceToken_Succeeds()
    {
        var client = _factory.CreateClient();
        var parent = await CreateParentAsync(client);
        var child = await CreateChildAsync(client, parent, "ログイン成功の子", "8642");
        var deviceToken = await AuthenticationHelper.RegisterDeviceAsync(client, parent);

        var childJwt = await LoginChildAsync(client, deviceToken, child.Id, "8642");

        Assert.False(string.IsNullOrEmpty(childJwt));
    }

    #endregion

    #region US2: 未登録の端末では子供の情報が見えない

    [Theory]
    [InlineData(null)]
    [InlineData("invalid-token")]
    public async Task GetChildren_WithoutValidDeviceToken_ReturnsUnauthorized(string? deviceToken)
    {
        var client = _factory.CreateClient();
        var parent = await CreateParentAsync(client);
        await CreateChildAsync(client, parent, "見えてはいけない子", "3579");

        var response = await SendAsync(client, HttpMethod.Get, "/api/devices/current/children", deviceToken: deviceToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain("見えてはいけない子", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ChildLogin_WithoutDeviceToken_ReturnsUnauthorized_AndDoesNotCountPinFailure()
    {
        var client = _factory.CreateClient();
        var parent = await CreateParentAsync(client);
        var child = await CreateChildAsync(client, parent, "トークンなしの子", "4826");

        // 端末トークンなしで間違ったPINを3回送っても、ロックアウトの回数に数えない
        for (int i = 0; i < 3; i++)
        {
            var noTokenResponse = await PostChildLoginAsync(client, deviceToken: null, child.Id, "0000");
            Assert.Equal(HttpStatusCode.Unauthorized, noTokenResponse.StatusCode);
        }

        var deviceToken = await AuthenticationHelper.RegisterDeviceAsync(client, parent);
        var response = await PostChildLoginAsync(client, deviceToken, child.Id, "4826");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ChildLogin_WithOtherFamilyDeviceToken_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var parentA = await CreateParentAsync(client);
        var parentB = await CreateParentAsync(client);
        var childOfA = await CreateChildAsync(client, parentA, "A家の子ログイン", "9753");
        var deviceTokenOfB = await AuthenticationHelper.RegisterDeviceAsync(client, parentB);

        // 存在しない子供と同じ応答にして、他家庭の子供の存在を推測させない
        var response = await PostChildLoginAsync(client, deviceTokenOfB, childOfA.Id, "9753");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PublicChildAccountsEndpoint_IsRemoved()
    {
        var client = _factory.CreateClient();
        var parent = await CreateParentAsync(client);
        await CreateChildAsync(client, parent, "公開APIで見えてはいけない子", "6420");

        // 廃止後は GET /api/child-accounts/{id} に該当し、認証なしでは 401 になる
        var response = await client.GetAsync("/api/child-accounts/public");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.DoesNotContain("公開APIで見えてはいけない子", await response.Content.ReadAsStringAsync());
    }

    #endregion

    #region US3: 登録済み端末の確認・解除

    [Fact]
    public async Task ListDevices_ReturnsOnlyOwnDevicesWithoutToken()
    {
        var client = _factory.CreateClient();
        var parentA = await CreateParentAsync(client);
        var parentB = await CreateParentAsync(client);
        var tokenA1 = await AuthenticationHelper.RegisterDeviceAsync(client, parentA, "A端末1");
        await AuthenticationHelper.RegisterDeviceAsync(client, parentA, "A端末2");
        await AuthenticationHelper.RegisterDeviceAsync(client, parentB, "B端末");

        var response = await SendAsync(client, HttpMethod.Get, "/api/devices", parentA);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(tokenA1, json);
        Assert.DoesNotContain("token", json, StringComparison.OrdinalIgnoreCase);
        var devices = await response.Content.ReadFromJsonAsync<List<RegisteredDeviceDto>>();
        Assert.Equal(new[] { "A端末1", "A端末2" }, devices!.Select(d => d.Name));
    }

    [Fact]
    public async Task RevokeDevice_InvalidatesToken()
    {
        var client = _factory.CreateClient();
        var parent = await CreateParentAsync(client);
        var deviceToken = await AuthenticationHelper.RegisterDeviceAsync(client, parent, "解除する端末");
        var devices = await GetDevicesAsync(client, parent);

        var revoke = await SendAsync(client, HttpMethod.Delete, $"/api/devices/{devices.Single().Id}", parent);

        Assert.Equal(HttpStatusCode.NoContent, revoke.StatusCode);
        Assert.Empty(await GetDevicesAsync(client, parent));
        var children = await SendAsync(client, HttpMethod.Get, "/api/devices/current/children", deviceToken: deviceToken);
        Assert.Equal(HttpStatusCode.Unauthorized, children.StatusCode);
    }

    [Fact]
    public async Task RevokeDevice_OfOtherParent_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var parentA = await CreateParentAsync(client);
        var parentB = await CreateParentAsync(client);
        var deviceTokenOfA = await AuthenticationHelper.RegisterDeviceAsync(client, parentA);
        var deviceIdOfA = (await GetDevicesAsync(client, parentA)).Single().Id;

        var revoke = await SendAsync(client, HttpMethod.Delete, $"/api/devices/{deviceIdOfA}", parentB);

        Assert.Equal(HttpStatusCode.NotFound, revoke.StatusCode);
        var children = await SendAsync(client, HttpMethod.Get, "/api/devices/current/children", deviceToken: deviceTokenOfA);
        Assert.Equal(HttpStatusCode.OK, children.StatusCode);
    }

    #endregion

    #region Helpers

    private static async Task<string> CreateParentAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = $"device_parent_{Guid.NewGuid():N}@example.com",
            DisplayName = "端末テスト保護者",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!"
        });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        return result!.Token!;
    }

    private static async Task<ChildAccountDto> CreateChildAsync(HttpClient client, string parentJwt, string name, string pin)
    {
        var response = await SendAsync(client, HttpMethod.Post, "/api/child-accounts", parentJwt,
            body: new ChildAccountCreateDto { Name = name, GradeLevel = 3, PresetAvatarId = 1, PIN = pin });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ChildAccountDto>())!;
    }

    private static async Task<List<RegisteredDeviceDto>> GetDevicesAsync(HttpClient client, string parentJwt)
    {
        var response = await SendAsync(client, HttpMethod.Get, "/api/devices", parentJwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<RegisteredDeviceDto>>())!;
    }

    private static Task<HttpResponseMessage> PostChildLoginAsync(HttpClient client, string? deviceToken, string childId, string pin)
    {
        return SendAsync(client, HttpMethod.Post, "/api/auth/child/login", deviceToken: deviceToken,
            body: new ChildLoginRequest { ChildAccountId = childId, PIN = pin });
    }

    private static async Task<string> LoginChildAsync(HttpClient client, string deviceToken, string childId, string pin)
    {
        var response = await PostChildLoginAsync(client, deviceToken, childId, pin);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token!;
    }

    private static Task<HttpResponseMessage> SendAsync(
        HttpClient client, HttpMethod method, string url, string? jwt = null, string? deviceToken = null, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        if (jwt != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }
        if (deviceToken != null)
        {
            request.Headers.Add(AuthenticationHelper.DeviceTokenHeader, deviceToken);
        }
        if (body != null)
        {
            request.Content = JsonContent.Create(body, body.GetType());
        }
        return client.SendAsync(request);
    }

    #endregion
}
