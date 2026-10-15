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
/// あまりのあるわり算（009）の問題取得・回答のテスト
/// </summary>
public class DivisionWithRemainderTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public DivisionWithRemainderTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RemainderProblem_IsCorrectOnlyWhenQuotientAndRemainderMatch()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "あまり");

        var problem = await GetNextProblemAsync(client, family.ChildJwt, family.StudentId, "DivisionWithRemainder");
        Assert.True(problem.GetProperty("hasRemainder").GetBoolean());
        Assert.Contains("÷", problem.GetProperty("question").GetString());
        var problemId = problem.GetProperty("id").GetInt32();
        var (quotient, remainder) = await GetCorrectAsync(problemId);

        // あまりだけ違う → 不正解。正解の商とあまりが返る
        var wrongRemainder = await AnswerAsync(client, family.ChildJwt, family.StudentId, problemId, quotient, remainder + 1);
        Assert.False(wrongRemainder.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(quotient, wrongRemainder.GetProperty("correctAnswer").GetInt32());
        Assert.Equal(remainder, wrongRemainder.GetProperty("correctRemainder").GetInt32());

        // あまりを答えない → 不正解
        var noRemainder = await AnswerAsync(client, family.ChildJwt, family.StudentId, problemId, quotient, null);
        Assert.False(noRemainder.GetProperty("isCorrect").GetBoolean());

        // 商とあまりの両方が合う → 正解
        var correct = await AnswerAsync(client, family.ChildJwt, family.StudentId, problemId, quotient, remainder);
        Assert.True(correct.GetProperty("isCorrect").GetBoolean());
        Assert.True(correct.GetProperty("pointsEarned").GetInt32() > 0);

        // 学習記録に答えたあまりが残る
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var records = await db.LearningRecords.Where(r => r.StudentId == family.StudentId && r.ProblemId == problemId)
            .OrderBy(r => r.Id).ToListAsync();
        Assert.Equal(new int?[] { remainder + 1, null, remainder }, records.Select(r => r.StudentRemainder));

        // 学習記録の一覧でも、正しいあまりと答えたあまりが見られる
        var listResponse = await FamilyTestHelper.SendAsync(client, HttpMethod.Get,
            $"/api/learning-records?studentId={family.StudentId}&calculationType=DivisionWithRemainder", family.ChildJwt);
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("records")
            .EnumerateArray().First(r => r.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(remainder, listed.GetProperty("correctRemainder").GetInt32());
        Assert.Equal(remainder, listed.GetProperty("studentRemainder").GetInt32());
    }

    [Fact]
    public async Task OtherProblems_HaveNoRemainder_AndIgnoreSentRemainder()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "あまりなし");

        var problem = await GetNextProblemAsync(client, family.ChildJwt, family.StudentId, "Division");
        Assert.False(problem.GetProperty("hasRemainder").GetBoolean());
        var problemId = problem.GetProperty("id").GetInt32();
        var (quotient, _) = await GetCorrectAsync(problemId);

        var result = await AnswerAsync(client, family.ChildJwt, family.StudentId, problemId, quotient, 3);
        Assert.True(result.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("correctRemainder").ValueKind);
    }

    private static async Task<JsonElement> GetNextProblemAsync(HttpClient client, string jwt, int studentId, string category)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={studentId}&category={category}", jwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    private static async Task<JsonElement> AnswerAsync(HttpClient client, string jwt, int studentId, int problemId, int answer, int? remainder)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problemId}/answer?studentId={studentId}", jwt,
            new { answer, remainder });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    private async Task<(int Quotient, int Remainder)> GetCorrectAsync(int problemId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var problem = await db.Problems.FindAsync(problemId);
        return (problem!.CorrectAnswer, problem.CorrectRemainder ?? 0);
    }
}
