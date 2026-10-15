using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// デイリーチャレンジ（010）: 子供ごと・レベルに合わせた問題・1日1回のテスト
/// </summary>
public class DailyChallengeTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public DailyChallengeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Today_IsPerChild_AndMatchesLevel()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "チャレンジ学年");
        var grade1 = await FamilyTestHelper.AddChildAsync(client, family.ParentJwt, family.DeviceToken, "チャレンジ1年生", "8520", gradeLevel: 1);

        var grade3Challenge = await GetTodayAsync(client, family.ChildJwt, family.StudentId);
        var grade1Challenge = await GetTodayAsync(client, grade1.Jwt, grade1.StudentId);

        // 3年生はレベル5（難易度5〜6）なので難易度6、1年生はレベル1（難易度1〜2）なので難易度2
        Assert.Equal(6, grade3Challenge.GetProperty("difficulty").GetInt32());
        Assert.Equal(2, grade1Challenge.GetProperty("difficulty").GetInt32());
        Assert.NotEqual(grade3Challenge.GetProperty("id").GetInt32(), grade1Challenge.GetProperty("id").GetInt32());
        Assert.Equal(30, grade3Challenge.GetProperty("bonusPoints").GetInt32());
        Assert.False(grade3Challenge.GetProperty("isAnswered").GetBoolean());
        Assert.Equal(JsonValueKind.Null, grade3Challenge.GetProperty("correctAnswer").ValueKind);

        // 同じ日にもう一度開いても同じチャレンジ
        var again = await GetTodayAsync(client, family.ChildJwt, family.StudentId);
        Assert.Equal(grade3Challenge.GetProperty("id").GetInt32(), again.GetProperty("id").GetInt32());

        // あまりのあるわり算は出ない
        var problem = await GetProblemAsync(grade3Challenge.GetProperty("id").GetInt32());
        Assert.NotEqual(CalculationType.DivisionWithRemainder, problem.CalculationType);
    }

    [Fact]
    public async Task CorrectAnswer_GivesBonusOnce_AndKeepsStreak()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "チャレンジ正解");
        await FamilyTestHelper.SetPointsAsync(_factory.Services, family.StudentId, 100);
        await SetStreakAsync(family.StudentId, 3);
        var challengeId = (await GetTodayAsync(client, family.ChildJwt, family.StudentId)).GetProperty("id").GetInt32();
        var correct = (await GetProblemAsync(challengeId)).CorrectAnswer;

        var first = await AnswerAsync(client, family.ChildJwt, challengeId, family.StudentId, correct);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var result = await first.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(30, result.GetProperty("bonusPoints").GetInt32());
        Assert.Equal(correct, result.GetProperty("correctAnswer").GetInt32());

        // 2回目は受け付けない（ポイントも増えない）
        var second = await AnswerAsync(client, family.ChildJwt, challengeId, family.StudentId, correct);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var student = await GetStudentAsync(family.StudentId);
        Assert.Equal(130, student.TotalPoints);
        Assert.Equal(3, student.CorrectAnswers); // 連続正解数はチャレンジでは変わらない

        // 学習記録には1件だけ残る
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var problemId = (await db.DailyChallenges.FindAsync(challengeId))!.ProblemId;
            Assert.Equal(1, await db.LearningRecords.CountAsync(r => r.StudentId == family.StudentId && r.ProblemId == problemId));
        }

        // 今日のチャレンジは「答えた・正解」になり、正解も見られる
        var today = await GetTodayAsync(client, family.ChildJwt, family.StudentId);
        Assert.True(today.GetProperty("isAnswered").GetBoolean());
        Assert.True(today.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(correct, today.GetProperty("studentAnswer").GetInt32());
        Assert.Equal(correct, today.GetProperty("correctAnswer").GetInt32());
    }

    [Fact]
    public async Task WrongAnswer_GivesNoBonus_AndCannotRetry()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "チャレンジ不正解");
        await FamilyTestHelper.SetPointsAsync(_factory.Services, family.StudentId, 100);
        await SetStreakAsync(family.StudentId, 3);
        var challengeId = (await GetTodayAsync(client, family.ChildJwt, family.StudentId)).GetProperty("id").GetInt32();
        var correct = (await GetProblemAsync(challengeId)).CorrectAnswer;

        var wrong = await AnswerAsync(client, family.ChildJwt, challengeId, family.StudentId, correct + 1);
        var result = await wrong.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(result.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(0, result.GetProperty("bonusPoints").GetInt32());

        var retry = await AnswerAsync(client, family.ChildJwt, challengeId, family.StudentId, correct);
        Assert.Equal(HttpStatusCode.Conflict, retry.StatusCode);

        var student = await GetStudentAsync(family.StudentId);
        Assert.Equal(100, student.TotalPoints);
        Assert.Equal(3, student.CorrectAnswers); // 不正解でも連続正解数は変わらない
    }

    [Fact]
    public async Task Parent_CanView_ButCannotAnswer()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "チャレンジ保護者");

        var challengeId = (await GetTodayAsync(client, family.ParentJwt, family.StudentId)).GetProperty("id").GetInt32();
        var response = await AnswerAsync(client, family.ParentJwt, challengeId, family.StudentId, 1);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OtherFamilysChallenge_IsNotFound()
    {
        var client = _factory.CreateClient();
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "チャレンジ分離A");
        var b = await FamilyTestHelper.CreateFamilyAsync(client, "チャレンジ分離B");
        var bChallengeId = (await GetTodayAsync(client, b.ChildJwt, b.StudentId)).GetProperty("id").GetInt32();

        // 他の家庭の子供のチャレンジは見られない
        var view = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/dailychallenges/today?studentId={b.StudentId}", a.ChildJwt);
        Assert.Equal(HttpStatusCode.NotFound, view.StatusCode);

        // 他の子供のチャレンジに、自分の学習者IDで答えることもできない
        var answer = await AnswerAsync(client, a.ChildJwt, bChallengeId, a.StudentId, 1);
        Assert.Equal(HttpStatusCode.NotFound, answer.StatusCode);
    }

    #region Helpers

    private static async Task<JsonElement> GetTodayAsync(HttpClient client, string jwt, int studentId)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/dailychallenges/today?studentId={studentId}", jwt);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static Task<HttpResponseMessage> AnswerAsync(HttpClient client, string jwt, int challengeId, int studentId, int answer) =>
        FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/dailychallenges/{challengeId}/answer", jwt,
            new { studentId, answer });

    private async Task<Problem> GetProblemAsync(int challengeId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var challenge = await db.DailyChallenges.Include(c => c.Problem).FirstAsync(c => c.Id == challengeId);
        return challenge.Problem;
    }

    private async Task<Student> GetStudentAsync(int studentId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return (await db.Students.AsNoTracking().FirstAsync(s => s.Id == studentId));
    }

    private async Task SetStreakAsync(int studentId, int streak)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var student = await db.Students.FirstAsync(s => s.Id == studentId);
        student.CorrectAnswers = streak;
        await db.SaveChangesAsync();
    }

    #endregion
}
