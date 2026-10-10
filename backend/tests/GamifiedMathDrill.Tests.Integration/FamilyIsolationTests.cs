using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Models.DTOs;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 家庭間のデータ分離と認証必須化の統合テスト。
/// 他の家庭の学習者IDを指定しても、存在しないのと同じ 404 になること、未ログインでは 401 になることを確認する
/// </summary>
public class FamilyIsolationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public FamilyIsolationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/api/learning-records?studentId=1")]
    [InlineData("/api/learning-records/statistics?studentId=1")]
    [InlineData("/api/dailychallenges/today")]
    [InlineData("/api/preset-avatars")]
    public async Task Api_WithoutLogin_ReturnsUnauthorized(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DailyChallengeAnswer_WithoutLogin_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var challengeId = await AddChallengeAsync(null);

        var response = await client.PostAsJsonAsync($"/api/dailychallenges/{challengeId}/answer",
            new DailyChallengeAnswerRequestDto { StudentId = 1, Answer = 0 });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Child_CannotAccessOtherFamilysStudent()
    {
        var client = _factory.CreateClient();
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "分離A");
        var b = await FamilyTestHelper.CreateFamilyAsync(client, "分離B");
        var challengeId = await AddChallengeAsync(b.StudentId);
        var problemId = GetAnyProblemId();

        var asChildA = (HttpMethod method, string url, object? body) => FamilyTestHelper.SendAsync(client, method, url, a.ChildJwt, body);

        Assert.Equal(HttpStatusCode.NotFound, (await asChildA(HttpMethod.Get, $"/api/problems/next?studentId={b.StudentId}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await asChildA(HttpMethod.Post, $"/api/problems/{problemId}/answer?studentId={b.StudentId}", new SubmitAnswerDto { Answer = 1 })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await asChildA(HttpMethod.Get, $"/api/learning-records?studentId={b.StudentId}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await asChildA(HttpMethod.Get, $"/api/learning-records/statistics?studentId={b.StudentId}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await asChildA(HttpMethod.Get, $"/api/students/{b.StudentId}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await asChildA(HttpMethod.Patch, $"/api/students/{b.StudentId}/login", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await asChildA(HttpMethod.Get, $"/api/rewards/acquired?studentId={b.StudentId}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await asChildA(HttpMethod.Post, $"/api/dailychallenges/{challengeId}/answer", new DailyChallengeAnswerRequestDto { StudentId = b.StudentId, Answer = 0 })).StatusCode);

        // 自分の学習者IDなら使える
        Assert.Equal(HttpStatusCode.OK, (await asChildA(HttpMethod.Get, $"/api/students/{a.StudentId}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await asChildA(HttpMethod.Get, $"/api/learning-records?studentId={a.StudentId}", null)).StatusCode);
    }

    [Fact]
    public async Task Child_CannotAccessSiblingsStudent()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "兄弟A");
        var sibling = await FamilyTestHelper.AddChildAsync(client, family.ParentJwt, family.DeviceToken, "兄弟A_弟", "8520");

        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/learning-records?studentId={sibling.StudentId}", family.ChildJwt);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Parent_CanAccessOwnChildren_ButNotOtherFamily()
    {
        var client = _factory.CreateClient();
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "保護者分離A");
        var b = await FamilyTestHelper.CreateFamilyAsync(client, "保護者分離B");

        var own = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/learning-records/statistics?studentId={a.StudentId}", a.ParentJwt);
        var other = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/learning-records/statistics?studentId={b.StudentId}", a.ParentJwt);
        var otherStudent = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/students/{b.StudentId}", a.ParentJwt);

        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, other.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, otherStudent.StatusCode);
    }

    [Fact]
    public async Task ExchangeRequests_AreIsolatedBetweenFamilies_EvenWithSameChildName()
    {
        var client = _factory.CreateClient();
        // 別の家庭に同じ名前の子供がいる
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "同名", childName: "同名テストの子");
        var b = await FamilyTestHelper.CreateFamilyAsync(client, "同名", childName: "同名テストの子");
        await GivePointsAsync(a.StudentId, 500);
        await GivePointsAsync(b.StudentId, 500);
        var rewardOfB = await FamilyTestHelper.CreateRewardAsync(client, b.ParentJwt, "分離テスト景品");

        // B の子供の申請は B の学習者に紐付く（名前で A の子供と取り違えない）
        var createB = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/exchange-requests", b.ChildJwt, new CreateExchangeRequestRequest { RewardId = rewardOfB });
        Assert.True(createB.IsSuccessStatusCode, await createB.Content.ReadAsStringAsync());
        var requestB = (await createB.Content.ReadFromJsonAsync<ExchangeRequestDto>())!;
        Assert.Equal(b.StudentId, requestB.StudentId);

        var myA = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, "/api/exchange-requests/my", a.ChildJwt);
        Assert.DoesNotContain(requestB.Id, (await myA.Content.ReadFromJsonAsync<List<ExchangeRequestDto>>())!.Select(r => r.Id));

        // A の保護者には B の申請が見えず、承認・却下・参照もできない
        var allForA = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, "/api/exchange-requests", a.ParentJwt);
        Assert.Equal(HttpStatusCode.OK, allForA.StatusCode);
        Assert.DoesNotContain(requestB.Id, (await allForA.Content.ReadFromJsonAsync<List<ExchangeRequestDto>>())!.Select(r => r.Id));
        Assert.Equal(HttpStatusCode.NotFound, (await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/exchange-requests/{requestB.Id}", a.ParentJwt)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/exchange-requests/{requestB.Id}", a.ChildJwt)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/exchange-requests/{requestB.Id}/approve", a.ParentJwt, new ApproveRequestRequest())).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/exchange-requests/{requestB.Id}/reject", a.ParentJwt, new RejectRequestRequest { Reason = "テスト" })).StatusCode);

        // B の保護者は自分の子供の申請を見られる
        var allForB = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, "/api/exchange-requests", b.ParentJwt);
        Assert.Contains(requestB.Id, (await allForB.Content.ReadFromJsonAsync<List<ExchangeRequestDto>>())!.Select(r => r.Id));
    }

    [Fact]
    public async Task RewardExchange_ForOtherStudent_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "景品交換A");
        var b = await FamilyTestHelper.CreateFamilyAsync(client, "景品交換B");
        await GivePointsAsync(b.StudentId, 500);
        var rewardId = await FamilyTestHelper.CreateRewardAsync(client, a.ParentJwt, "分離テスト景品");

        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/rewards/{rewardId}/exchange", a.ChildJwt,
            new ExchangeRewardRequestDto { StudentId = b.StudentId });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreatingStandaloneStudent_IsNotAllowed()
    {
        var client = _factory.CreateClient();
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "単独学習者");

        // 家庭に属さない学習者を作るAPIは廃止（学習者は子供アカウントと一緒に作られる）
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/students", a.ParentJwt, new CreateStudentDto { Name = "誰の子でもない" });

        Assert.False(response.IsSuccessStatusCode);
    }

    #region Helpers

    private Task GivePointsAsync(int studentId, int points) => FamilyTestHelper.SetPointsAsync(_factory.Services, studentId, points);

    /// <summary>
    /// 学習者のチャレンジを直接作る（学習者を指定しない場合は既存のいずれかの学習者）
    /// </summary>
    private async Task<int> AddChallengeAsync(int? studentId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var challenge = new DailyChallenge
        {
            StudentId = studentId ?? db.Students.First().Id,
            ProblemId = db.Problems.First().Id,
            TargetDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-100)
        };
        db.DailyChallenges.Add(challenge);
        await db.SaveChangesAsync();
        return challenge.Id;
    }

    private int GetAnyProblemId()
    {
        using var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Problems.First().Id;
    }

    #endregion
}
