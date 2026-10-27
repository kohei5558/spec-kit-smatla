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
/// まちがえたときの解説（019）が回答の結果に入る
/// </summary>
public class ExplanationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ExplanationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task WrongAnswer_ReturnsExplanationAndHint_CorrectAnswerDoesNot()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "解説の問題");
        var problem = await FindProblemAsync(p => p.CalculationType == CalculationType.DivisionWithRemainder && p.DifficultyLevel <= 5);

        // あまりが わる数より大きい → 一言も出る
        var divisor = int.Parse(problem.Question.Split(' ')[2]);
        var wrong = await AnswerAsync(client, family, problem.Id, problem.CorrectAnswer - 1, problem.CorrectRemainder + divisor);
        Assert.False(wrong.GetProperty("isCorrect").GetBoolean());
        var steps = wrong.GetProperty("explanation").EnumerateArray().Select(s => s.GetString()!).ToList();
        Assert.NotEmpty(steps);
        Assert.EndsWith($"こたえは {problem.CorrectAnswer} あまり {problem.CorrectRemainder}", steps[^1]);
        Assert.Equal($"あまりは わる数の {divisor} より 小さく なるよ。もう 1つ ふやせるかも？", wrong.GetProperty("hint").GetString());

        var correct = await AnswerAsync(client, family, problem.Id, problem.CorrectAnswer, problem.CorrectRemainder);
        Assert.True(correct.GetProperty("isCorrect").GetBoolean());
        Assert.Empty(correct.GetProperty("explanation").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, correct.GetProperty("hint").ValueKind);
    }

    [Fact]
    public async Task WordProblem_WrongAnswer_ReturnsExplanation()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "解説の文章問題");
        var problem = await FindProblemAsync(p => p.CalculationType == CalculationType.WordProblem && p.Question.StartsWith("赤い 花が"));

        var wrong = await AnswerAsync(client, family, problem.Id, problem.CorrectAnswer + 1, null);
        var steps = wrong.GetProperty("explanation").EnumerateArray().Select(s => s.GetString()!).ToList();
        Assert.Equal("「あわせて」は たし算", steps[0]);
        Assert.EndsWith($"= {problem.CorrectAnswer}", steps[^1]);
    }

    [Fact]
    public async Task DailyChallenge_WrongAnswer_ReturnsExplanation_AlsoWhenReopened()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "解説のチャレンジ");
        var today = await GetJsonAsync(client, family, $"/api/dailychallenges/today?studentId={family.StudentId}");
        Assert.Empty(today.GetProperty("explanation").EnumerateArray()); // 答える前は出ない

        Problem problem;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var challenge = await db.DailyChallenges.Include(c => c.Problem).AsNoTracking().FirstAsync(c => c.StudentId == family.StudentId);
            problem = challenge.Problem!;
        }

        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/dailychallenges/{today.GetProperty("id").GetInt32()}/answer", family.ChildJwt,
            new { studentId = family.StudentId, answer = problem.CorrectAnswer + 1 });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var steps = result.GetProperty("explanation").EnumerateArray().Select(s => s.GetString()!).ToList();
        Assert.EndsWith($"= {problem.CorrectAnswer}", steps[^1]);

        // 開き直しても同じ解説
        var reopened = await GetJsonAsync(client, family, $"/api/dailychallenges/today?studentId={family.StudentId}");
        Assert.Equal(steps, reopened.GetProperty("explanation").EnumerateArray().Select(s => s.GetString()!).ToList());
    }

    private async Task<Problem> FindProblemAsync(System.Linq.Expressions.Expression<Func<Problem, bool>> predicate)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Problems.AsNoTracking().FirstAsync(predicate);
    }

    private static async Task<JsonElement> AnswerAsync(HttpClient client, FamilyTestHelper.Family family, int problemId, int answer, int? remainder)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problemId}/answer?studentId={family.StudentId}", family.ChildJwt,
            new { answer, remainder });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    private static async Task<JsonElement> GetJsonAsync(HttpClient client, FamilyTestHelper.Family family, string url)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, url, family.ChildJwt);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }
}
