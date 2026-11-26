using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 問題やデイリーチャレンジに答えたとき、2つ目以降の保存が失敗したら、前の保存も取り消す（#31 と同じ形）。本物の SQLite で確かめる
/// </summary>
public class AnswerTransactionTests : IClassFixture<FailingSaveWebApplicationFactory>
{
    private const int Points = 100;

    private readonly FailingSaveWebApplicationFactory _factory;

    public AnswerTransactionTests(FailingSaveWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProblemAnswer_FailsToSavePoints_NoLearningRecordIsLeft()
    {
        // 学習記録を作る → ポイント・連続正解数を保存する。2つ目が失敗したら 学習記録も残さない
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "回答の途中で失敗");
        await FamilyTestHelper.SetPointsAsync(_factory.Services, family.StudentId, Points);
        var problem = await NextProblemAsync(client, family);

        var response = await FailWhileAsync(t => Has<Student>(t, EntityState.Modified),
            () => FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problem.Id}/answer?studentId={family.StudentId}", family.ChildJwt,
                new { answer = problem.CorrectAnswer, remainder = problem.CorrectRemainder }));

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(Points, await PointsAsync(family.StudentId));
        Assert.Equal(0, await RecordCountAsync(family.StudentId));
    }

    [Fact]
    public async Task DailyChallenge_FailsToSaveRecord_CanAnswerAgain()
    {
        // 回答済みにする → 学習記録を作る → ボーナスを足す。途中で失敗したら 回答済みにもしない（もう一度 答えられる）
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "チャレンジの途中で失敗");
        await FamilyTestHelper.SetPointsAsync(_factory.Services, family.StudentId, Points);
        var challengeId = (await GetTodayAsync(client, family)).GetProperty("id").GetInt32();
        var correct = await ChallengeAnswerAsync(challengeId);

        var failed = await FailWhileAsync(t => Has<LearningRecord>(t, EntityState.Added),
            () => AnswerChallengeAsync(client, family, challengeId, correct));

        Assert.False(failed.IsSuccessStatusCode);
        Assert.Equal(Points, await PointsAsync(family.StudentId));
        Assert.Equal(0, await RecordCountAsync(family.StudentId));
        Assert.False((await GetTodayAsync(client, family)).GetProperty("isAnswered").GetBoolean());

        // もう一度 答えると、今度は ボーナスがもらえる
        var again = await AnswerChallengeAsync(client, family, challengeId, correct);
        Assert.True(again.IsSuccessStatusCode, await again.Content.ReadAsStringAsync());
        Assert.True(await PointsAsync(family.StudentId) > Points);
        Assert.Equal(1, await RecordCountAsync(family.StudentId));
    }

    [Fact]
    public async Task WithoutFailure_AnswerIsSaved()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "失敗しない回答");
        await FamilyTestHelper.SetPointsAsync(_factory.Services, family.StudentId, Points);
        var problem = await NextProblemAsync(client, family);

        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problem.Id}/answer?studentId={family.StudentId}", family.ChildJwt,
            new { answer = problem.CorrectAnswer, remainder = problem.CorrectRemainder });

        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        Assert.Equal(Points + 10, await PointsAsync(family.StudentId));
        Assert.Equal(1, await RecordCountAsync(family.StudentId));
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

    private async Task<Problem> NextProblemAsync(HttpClient client, FamilyTestHelper.Family family)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}&category=Addition", family.ChildJwt);
        response.EnsureSuccessStatusCode();
        var id = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("id").GetInt32();
        return await QueryAsync(db => db.Problems.AsNoTracking().SingleAsync(p => p.Id == id));
    }

    private static async Task<JsonElement> GetTodayAsync(HttpClient client, FamilyTestHelper.Family family)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/dailychallenges/today?studentId={family.StudentId}", family.ChildJwt);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static Task<HttpResponseMessage> AnswerChallengeAsync(HttpClient client, FamilyTestHelper.Family family, int challengeId, int answer) =>
        FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/dailychallenges/{challengeId}/answer", family.ChildJwt, new { studentId = family.StudentId, answer });

    private Task<int> ChallengeAnswerAsync(int challengeId) =>
        QueryAsync(db => db.DailyChallenges.Where(c => c.Id == challengeId).Select(c => c.Problem.CorrectAnswer).SingleAsync());

    private async Task<T> QueryAsync<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        using var scope = _factory.Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    private Task<int> PointsAsync(int studentId) => QueryAsync(db => db.Students.AsNoTracking().Where(s => s.Id == studentId).Select(s => s.TotalPoints).SingleAsync());

    private Task<int> RecordCountAsync(int studentId) => QueryAsync(db => db.LearningRecords.CountAsync(r => r.StudentId == studentId));
}
