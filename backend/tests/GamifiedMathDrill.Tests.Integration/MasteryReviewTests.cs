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
/// マスターした問題は出さない・まちがえた問題を復習で出す（030）
/// </summary>
public class MasteryReviewTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public MasteryReviewTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MasteredProblem_IsNotGivenAgain()
    {
        // レベル10（むずかしさ10 だけ）で、むずかしさ10の 読みの問題を 2問だけ のこし、1問を 画面から 3回つづけて 正解する
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "マスターした問題");
        await StudentLevelTestHelper.SetAsync(_factory.Services, family.StudentId, 10, 0, CalculationType.KanjiReading);
        var ids = await ProblemIdsAsync(CalculationType.KanjiReading, 10);
        var (mastered, other) = (ids[0], ids[1]);
        foreach (var id in ids.Skip(2))
        {
            await AddRecordsAsync(family.StudentId, id, true, true, true);
        }
        var problem = await GetProblemAsync(mastered);
        await AnswerAsync(client, family, problem, correct: true);
        await AnswerAsync(client, family, problem, correct: true);
        // 2回の正解では まだ マスターではない（のこりの2問の どちらも 出る）
        var before = new HashSet<int>();
        for (var i = 0; i < 20; i++)
        {
            before.Add((await NextAsync(client, family, "category=KanjiReading")).GetProperty("id").GetInt32());
        }
        Assert.Equal(new[] { mastered, other }.OrderBy(x => x), before.OrderBy(x => x));

        await AnswerAsync(client, family, problem, correct: true);
        for (var i = 0; i < 20; i++)
        {
            Assert.Equal(other, (await NextAsync(client, family, "category=KanjiReading")).GetProperty("id").GetInt32());
        }
    }

    [Fact]
    public async Task WrongProblem_ComesBackAsReview()
    {
        // レベル1の子に、むずかしさ10の 読みの問題を まちがえた記録を つくる。ふつうの出題では むずかしさ1〜2 なので、出たら 復習
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "復習の問題");
        var hard = await FirstProblemAsync(CalculationType.KanjiReading, 10);
        await AddRecordsAsync(family.StudentId, hard.Id, false);

        var reviews = new List<JsonElement>();
        for (var i = 0; i < 30; i++)
        {
            var data = await NextAsync(client, family, "category=KanjiReading");
            if (data.GetProperty("id").GetInt32() == hard.Id)
            {
                reviews.Add(data);
            }
            else
            {
                Assert.False(data.GetProperty("isReview").GetBoolean());
            }
        }
        // 3問に1問の くじなので、30回で 1回も 出ない ことは まず ない（(2/3)^30 ≒ 0.0005%）
        Assert.NotEmpty(reviews);
        Assert.All(reviews, r => Assert.True(r.GetProperty("isReview").GetBoolean()));

        // 正解すれば もう 復習には 出ない（むずかしさ10 なので ふつうの出題でも 出ない）
        await AnswerAsync(client, family, hard, correct: true);
        for (var i = 0; i < 15; i++)
        {
            Assert.NotEqual(hard.Id, (await NextAsync(client, family, "category=KanjiReading")).GetProperty("id").GetInt32());
        }
    }

    [Fact]
    public async Task AllMasteredAtThisDifficulty_GoesLowerFirst_ThenHigher()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "下から上へ");

        // レベル2（むずかしさ2〜3）で 2と3を 全部マスター → 下の むずかしさ1 から
        await StudentLevelTestHelper.SetAsync(_factory.Services, family.StudentId, 2, 0, CalculationType.Time);
        await MasterAllAsync(family.StudentId, CalculationType.Time, 2, 3);
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(1, (await NextAsync(client, family, "category=Time")).GetProperty("difficultyLevel").GetInt32());
        }

        // 1も 全部マスター → 上の むずかしさ4 から
        await MasterAllAsync(family.StudentId, CalculationType.Time, 1);
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(4, (await NextAsync(client, family, "category=Time")).GetProperty("difficultyLevel").GetInt32());
        }
    }

    [Fact]
    public async Task WholeTypeMastered_SaysSo_ButOtherTypesStillCome()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "全部マスター");
        await MasterAllAsync(family.StudentId, CalculationType.Time, Enumerable.Range(1, 10).ToArray());

        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}&category=Time", family.ChildJwt);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("AllMastered", body.GetProperty("errors").EnumerateArray().Select(e => e.GetString()));

        // さんすうの「すべて」は、ほかの種類から出る
        for (var i = 0; i < 10; i++)
        {
            Assert.NotEqual("Time", (await NextAsync(client, family, "subject=Math")).GetProperty("calculationTypeText").GetString());
        }
    }

    private static async Task<JsonElement> NextAsync(HttpClient client, FamilyTestHelper.Family family, string query)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}&{query}", family.ChildJwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    private static async Task AnswerAsync(HttpClient client, FamilyTestHelper.Family family, Problem problem, bool correct)
    {
        object body = problem.CorrectText != null
            ? new { answer = 0, text = correct ? problem.CorrectText : "ん" }
            : new { answer = correct ? problem.CorrectAnswer : problem.CorrectAnswer + 1, remainder = problem.CorrectRemainder };
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problem.Id}/answer?studentId={family.StudentId}", family.ChildJwt, body);
        response.EnsureSuccessStatusCode();
        var result = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
        Assert.Equal(correct, result.GetProperty("isCorrect").GetBoolean());
    }

    private async Task<Problem> GetProblemAsync(int id)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Problems.AsNoTracking().FirstAsync(p => p.Id == id);
    }

    private async Task<List<int>> ProblemIdsAsync(CalculationType type, int difficulty)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Problems.Where(p => p.IsActive && p.CalculationType == type && p.DifficultyLevel == difficulty).OrderBy(p => p.Id).Select(p => p.Id).ToListAsync();
    }

    private async Task<Problem> FirstProblemAsync(CalculationType type, int difficulty)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Problems.AsNoTracking().Where(p => p.IsActive && p.CalculationType == type && p.DifficultyLevel == difficulty).OrderBy(p => p.Id).FirstAsync();
    }

    // その むずかしさの 問題を 全部、3回つづけて 正解した 記録を つくる
    private async Task MasterAllAsync(int studentId, CalculationType type, params int[] difficulties)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var ids = await db.Problems.Where(p => p.IsActive && p.CalculationType == type && difficulties.Contains(p.DifficultyLevel)).Select(p => p.Id).ToListAsync();
        Assert.NotEmpty(ids);
        foreach (var id in ids)
        {
            await AddRecordsAsync(studentId, id, true, true, true);
        }
    }

    private async Task AddRecordsAsync(int studentId, int problemId, params bool[] answers)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var start = DateTime.UtcNow.AddDays(-1);
        for (var i = 0; i < answers.Length; i++)
        {
            db.LearningRecords.Add(new LearningRecord { StudentId = studentId, ProblemId = problemId, IsCorrect = answers[i], SolvedAt = start.AddSeconds(i) });
        }
        await db.SaveChangesAsync();
    }
}
