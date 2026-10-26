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
/// 時こくと時間（013）の出題と回答
/// </summary>
public class TimeProblemTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public TimeProblemTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TimeProblems_StartAtLevel1_AndReturnAnswerFormatAndClock_WithoutAnswer()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "時こくの出題"); // 小3でも時こくはレベル1

        for (var i = 0; i < 10; i++)
        {
            var data = await NextAsync(client, family);
            Assert.Equal("Time", data.GetProperty("calculationTypeText").GetString());
            Assert.InRange(data.GetProperty("difficultyLevel").GetInt32(), 1, 2);
            Assert.False(data.TryGetProperty("correctAnswer", out _));

            var problem = await GetProblemAsync(data.GetProperty("id").GetInt32());
            Assert.Equal(problem.AnswerFormat.ToString(), data.GetProperty("answerFormat").GetString());
            Assert.Equal(problem.CorrectRemainder != null, data.GetProperty("hasRemainder").GetBoolean());
            var clocks = data.GetProperty("clockTimes").EnumerateArray().Select(c => c.GetString()!).ToList();
            Assert.Equal(problem.ClockTimes?.Split(' ').ToList() ?? new List<string>(), clocks);
        }
    }

    /// <summary>
    /// 難易度1は3問しかないため、画面が「最近出た問題」をすべて除くと出せる問題が無くなっていた。
    /// そのときは、最近出た問題でも出す（「問題の読み込みに失敗しました」にしない）
    /// </summary>
    [Fact]
    public async Task WhenAllProblemsAreRecent_StillReturnsAProblem()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "最近の問題ばかり");
        List<int> recent;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // レベル1（難易度1〜2）の時こくの問題をすべて「最近出た」にする
            recent = await db.Problems.Where(p => p.CalculationType == CalculationType.Time && p.DifficultyLevel <= 2).Select(p => p.Id).ToListAsync();
        }

        var query = string.Join("", recent.Select(id => $"&excludeRecentIds={id}"));
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}&category=Time{query}", family.ChildJwt);

        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        var data = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
        Assert.Contains(data.GetProperty("id").GetInt32(), recent);
    }

    [Fact]
    public async Task ClockTimeProblem_IsCorrectOnlyWhenBothHourAndMinuteMatch()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "時と分の判定");
        var problem = await FindProblemAsync(AnswerFormat.ClockTime);

        var wrongMinute = await AnswerAsync(client, family, problem.Id, problem.CorrectAnswer, problem.CorrectRemainder!.Value + 1);
        Assert.False(wrongMinute.GetProperty("isCorrect").GetBoolean());
        Assert.Equal("ClockTime", wrongMinute.GetProperty("answerFormat").GetString());
        Assert.Equal(problem.CorrectAnswer, wrongMinute.GetProperty("correctAnswer").GetInt32());
        Assert.Equal(problem.CorrectRemainder, wrongMinute.GetProperty("correctRemainder").GetInt32());

        var noMinute = await AnswerAsync(client, family, problem.Id, problem.CorrectAnswer, null);
        Assert.False(noMinute.GetProperty("isCorrect").GetBoolean());

        var correct = await AnswerAsync(client, family, problem.Id, problem.CorrectAnswer, problem.CorrectRemainder);
        Assert.True(correct.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(10, correct.GetProperty("pointsEarned").GetInt32());
    }

    [Fact]
    public async Task SingleAnswerTimeProblem_IgnoresSecondAnswer_AndRecordShowsFormat()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "分だけの判定");
        var problem = await FindProblemAsync(AnswerFormat.Minutes);

        var result = await AnswerAsync(client, family, problem.Id, problem.CorrectAnswer, 99);
        Assert.True(result.GetProperty("isCorrect").GetBoolean());

        var records = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/learning-records?studentId={family.StudentId}&calculationType=Time", family.ChildJwt);
        records.EnsureSuccessStatusCode();
        var items = (await records.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("records");
        var record = items.EnumerateArray().First(r => r.GetProperty("problemId").GetInt32() == problem.Id);
        Assert.Equal("Minutes", record.GetProperty("answerFormat").GetString());
        Assert.Equal(JsonValueKind.Null, record.GetProperty("studentRemainder").ValueKind); // 答えが1つの問題は2つ目を残さない
    }

    [Fact]
    public async Task DailyChallenge_NeverUsesTimeProblems()
    {
        var client = _factory.CreateClient();
        for (var i = 0; i < 5; i++)
        {
            var family = await FamilyTestHelper.CreateFamilyAsync(client, $"チャレンジに時こくは出ない{i}");
            var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/dailychallenges/today?studentId={family.StudentId}", family.ChildJwt);
            response.EnsureSuccessStatusCode();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var challenge = await db.DailyChallenges.Include(c => c.Problem).AsNoTracking().FirstAsync(c => c.StudentId == family.StudentId);
            Assert.NotEqual(CalculationType.Time, challenge.Problem!.CalculationType);
        }
    }

    private static async Task<JsonElement> NextAsync(HttpClient client, FamilyTestHelper.Family family)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}&category=Time", family.ChildJwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    private static async Task<JsonElement> AnswerAsync(HttpClient client, FamilyTestHelper.Family family, int problemId, int answer, int? remainder)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problemId}/answer?studentId={family.StudentId}", family.ChildJwt,
            new { answer, remainder });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    private async Task<Problem> FindProblemAsync(AnswerFormat format)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Problems.AsNoTracking().FirstAsync(p => p.CalculationType == CalculationType.Time && p.AnswerFormat == format);
    }

    private async Task<Problem> GetProblemAsync(int id)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Problems.AsNoTracking().FirstAsync(p => p.Id == id);
    }
}
