using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 子供アカウントの作成・編集フォームまわり（005 の T064・T065）
/// </summary>
public class ChildAccountFormTests : IClassFixture<TestWebApplicationFactory>
{
    private const string FrontendOrigin = "http://localhost:5071";

    private readonly TestWebApplicationFactory _factory;

    public ChildAccountFormTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// 兄弟と同じ PIN でも作成はでき、警告は ASCII のコードで返す。
    /// 以前は日本語をヘッダーに入れていたため、本物のサーバー（Kestrel）では作成後に 400 になっていた
    /// </summary>
    [Fact]
    public async Task Create_WithSiblingsPin_SucceedsWithAsciiWarning()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "同じPINで作成"); // 1人目の PIN は 7410

        var response = await CreateAsync(client, family.ParentJwt, "2人目", pin: "7410");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("duplicate-pin", Assert.Single(response.Headers.GetValues("X-Pin-Warning")));
    }

    [Fact]
    public async Task Create_WithDifferentPin_HasNoWarning()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "違うPINで作成");

        var response = await CreateAsync(client, family.ParentJwt, "2人目", pin: "8520");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("X-Pin-Warning"));
    }

    [Fact]
    public async Task Update_ToSiblingsPin_SucceedsWithAsciiWarning()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "同じPINに変更");
        var second = await (await CreateAsync(client, family.ParentJwt, "2人目", pin: "8520")).Content.ReadFromJsonAsync<ChildAccountDto>();

        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/child-accounts/{second!.Id}", family.ParentJwt,
            new ChildAccountUpdateDto { Name = "2人目", GradeLevel = 3, PresetAvatarId = 1, NewPIN = "7410" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("duplicate-pin", Assert.Single(response.Headers.GetValues("X-Pin-Warning")));
    }

    /// <summary>
    /// 画面は別のアドレスから呼ぶため、ブラウザが警告のヘッダーを読めるよう CORS で公開する
    /// </summary>
    [Fact]
    public async Task PinWarningHeader_IsExposedToFrontend()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "警告ヘッダーの公開");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/child-accounts")
        {
            Content = JsonContent.Create(new ChildAccountCreateDto { Name = "2人目", GradeLevel = 3, PresetAvatarId = 1, PIN = "7410" })
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", family.ParentJwt);
        request.Headers.Add("Origin", FrontendOrigin);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("X-Pin-Warning", string.Join(",", response.Headers.GetValues("Access-Control-Expose-Headers")), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// アバターを選ばなかったときは、最初のアバターにする（T065）
    /// </summary>
    [Fact]
    public async Task Create_WithoutAvatar_UsesFirstAvatar()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "アバターなし");

        var response = await CreateAsync(client, family.ParentJwt, "2人目", pin: "8520", presetAvatarId: 0);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("/avatars/cat.png", (await response.Content.ReadFromJsonAsync<ChildAccountDto>())!.AvatarUrl);
    }

    [Fact]
    public async Task Create_WithUnknownAvatar_IsBadRequest()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "存在しないアバター");

        var response = await CreateAsync(client, family.ParentJwt, "2人目", pin: "8520", presetAvatarId: 999);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static Task<HttpResponseMessage> CreateAsync(HttpClient client, string jwt, string name, string pin, int presetAvatarId = 1) =>
        FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/child-accounts", jwt,
            new ChildAccountCreateDto { Name = name, GradeLevel = 3, PresetAvatarId = presetAvatarId, PIN = pin });
}
