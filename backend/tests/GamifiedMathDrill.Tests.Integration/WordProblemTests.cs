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
/// 文章問題（017）の出題と回答
/// </summary>
public class WordProblemTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public WordProblemTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Problems_StartAtLevel1_AndReturnUnits_WithoutAnswer()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "文章問題の出題"); // 小3でもレベル1

        for (var i = 0; i < 10; i++)
        {
            var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}&category=WordProblem", family.ChildJwt);
            response.EnsureSuccessStatusCode();
            var data = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
            Assert.Equal("WordProblem", data.GetProperty("calculationTypeText").GetString());
            Assert.InRange(data.GetProperty("difficultyLevel").GetInt32(), 1, 2);
            Assert.Equal("Units", data.GetProperty("answerFormat").GetString());
            Assert.False(data.GetProperty("hasRemainder").GetBoolean());
            Assert.False(data.TryGetProperty("correctAnswer", out _));

            var problem = await GetProblemAsync(data.GetProperty("id").GetInt32());
            Assert.Equal(new[] { problem.AnswerUnits }, data.GetProperty("answerUnits").EnumerateArray().Select(u => u.GetString()));
        }
    }

    [Fact]
    public async Task RemainderProblem_IsCorrectOnlyWhenBothMatch_AndRecordShowsUnits()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "文章問題のあまり");
        Problem problem;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            problem = await db.Problems.AsNoTracking().FirstAsync(p => p.CalculationType == CalculationType.WordProblem && p.CorrectRemainder != null);
        }
        Assert.Equal(AnswerFormat.UnitsRemainder, problem.AnswerFormat);

        var wrong = await AnswerAsync(client, family, problem.Id, problem.CorrectAnswer, problem.CorrectRemainder!.Value + 1);
        Assert.False(wrong.GetProperty("isCorrect").GetBoolean());
        Assert.Equal("UnitsRemainder", wrong.GetProperty("answerFormat").GetString());
        Assert.Equal(problem.AnswerUnits!.Split(' '), wrong.GetProperty("answerUnits").EnumerateArray().Select(u => u.GetString()!));

        var correct = await AnswerAsync(client, family, problem.Id, problem.CorrectAnswer, problem.CorrectRemainder);
        Assert.True(correct.GetProperty("isCorrect").GetBoolean());

        var records = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/learning-records?studentId={family.StudentId}&calculationType=WordProblem", family.ChildJwt);
        records.EnsureSuccessStatusCode();
        var record = (await records.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("records").EnumerateArray().First();
        Assert.Equal("UnitsRemainder", record.GetProperty("answerFormat").GetString());
        Assert.Equal(problem.AnswerUnits!.Split(' '), record.GetProperty("answerUnits").EnumerateArray().Select(u => u.GetString()!));
    }

    [Fact]
    public async Task DailyChallenge_NeverUsesWordProblems()
    {
        var client = _factory.CreateClient();
        for (var i = 0; i < 5; i++)
        {
            var family = await FamilyTestHelper.CreateFamilyAsync(client, $"チャレンジに文章問題は出ない{i}");
            (await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/dailychallenges/today?studentId={family.StudentId}", family.ChildJwt)).EnsureSuccessStatusCode();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var challenge = await db.DailyChallenges.Include(c => c.Problem).AsNoTracking().FirstAsync(c => c.StudentId == family.StudentId);
            Assert.NotEqual(CalculationType.WordProblem, challenge.Problem!.CalculationType);
        }
    }

    private static async Task<JsonElement> AnswerAsync(HttpClient client, FamilyTestHelper.Family family, int problemId, int answer, int? remainder)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problemId}/answer?studentId={family.StudentId}", family.ChildJwt,
            new { answer, remainder });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    private async Task<Problem> GetProblemAsync(int id)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Problems.AsNoTracking().FirstAsync(p => p.Id == id);
    }
}
