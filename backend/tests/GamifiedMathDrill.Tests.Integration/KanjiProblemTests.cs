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
/// 国語（漢字、020）の出題と判定
/// </summary>
public class KanjiProblemTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public KanjiProblemTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Subject_DecidesWhichTypesAreMixed()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "教科のミックス");
        var japanese = new[] { "KanjiReading", "KanjiWriting", "KanjiWords" };

        for (var i = 0; i < 10; i++)
        {
            var kanji = await NextAsync(client, family, "subject=Japanese");
            Assert.Contains(kanji.GetProperty("calculationTypeText").GetString(), japanese);
        }
        // 教科を指定しない「すべて」は、今までどおり算数だけ
        for (var i = 0; i < 20; i++)
        {
            Assert.DoesNotContain((await NextAsync(client, family, "")).GetProperty("calculationTypeText").GetString(), japanese);
            Assert.DoesNotContain((await NextAsync(client, family, "subject=Math")).GetProperty("calculationTypeText").GetString(), japanese);
        }

        var invalid = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}&subject=Science", family.ChildJwt);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task Reading_IsJudgedByText_AndRecordKeepsTheText()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "漢字の読み");
        var data = await NextAsync(client, family, "category=KanjiReading");
        Assert.Equal("Text", data.GetProperty("answerFormat").GetString());
        Assert.Empty(data.GetProperty("choices").EnumerateArray());
        Assert.False(data.TryGetProperty("correctText", out _)); // 正解は返さない
        Assert.InRange(data.GetProperty("difficultyLevel").GetInt32(), 1, 2); // レベル1から

        var problem = await GetProblemAsync(data.GetProperty("id").GetInt32());
        var wrong = await AnswerAsync(client, family, problem.Id, problem.CorrectText + "あ");
        Assert.False(wrong.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(problem.CorrectText, wrong.GetProperty("correctText").GetString());
        // まちがえたら解説が出る（022）。送りがなまで入れたわけではないので一言は出ない
        var steps = wrong.GetProperty("explanation").EnumerateArray().Select(e => e.GetString()!).ToList();
        Assert.EndsWith($"は「{problem.CorrectText}」と 読むよ", steps[0]);
        Assert.Equal(JsonValueKind.Null, wrong.GetProperty("hint").ValueKind);

        var correct = await AnswerAsync(client, family, problem.Id, $" {problem.CorrectText} ");
        Assert.True(correct.GetProperty("isCorrect").GetBoolean());
        Assert.Empty(correct.GetProperty("explanation").EnumerateArray());
        Assert.Equal(10, correct.GetProperty("pointsEarned").GetInt32());

        var records = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/learning-records?studentId={family.StudentId}&calculationType=KanjiReading", family.ChildJwt);
        records.EnsureSuccessStatusCode();
        var list = (await records.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("records").EnumerateArray().ToList();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, r => r.GetProperty("studentText").GetString() == problem.CorrectText && r.GetProperty("isCorrect").GetBoolean());
        Assert.All(list, r => Assert.Equal(problem.CorrectText, r.GetProperty("correctText").GetString()));
    }

    [Fact]
    public async Task Writing_ReturnsChoices_AndIsJudgedByTheChosenOne()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "漢字の書き");
        var data = await NextAsync(client, family, "category=KanjiWriting");
        Assert.Equal("Choice", data.GetProperty("answerFormat").GetString());
        var choices = data.GetProperty("choices").EnumerateArray().Select(c => c.GetString()!).ToList();
        Assert.Equal(4, choices.Count);

        var problem = await GetProblemAsync(data.GetProperty("id").GetInt32());
        Assert.Contains(problem.CorrectText, choices);
        var wrong = await AnswerAsync(client, family, problem.Id, choices.First(c => c != problem.CorrectText));
        Assert.False(wrong.GetProperty("isCorrect").GetBoolean());
        Assert.True((await AnswerAsync(client, family, problem.Id, problem.CorrectText!)).GetProperty("isCorrect").GetBoolean());
    }

    [Fact]
    public async Task HighLevel_StillGetsKanji_FromTheHighestFilledDifficulty()
    {
        // 漢字がまだ入っていない むずかしさ（6〜10）の レベルでも、字がある いちばん上の むずかしさから出す（025）
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "漢字の高いレベル");
        await StudentLevelTestHelper.SetAsync(_factory.Services, family.StudentId, 10, 0, CalculationType.KanjiReading);

        for (var i = 0; i < 5; i++)
        {
            var data = await NextAsync(client, family, "category=KanjiReading");
            Assert.Equal("KanjiReading", data.GetProperty("calculationTypeText").GetString());
            Assert.Equal(5, data.GetProperty("difficultyLevel").GetInt32());
        }
    }

    [Fact]
    public async Task ArithmeticProblems_StillJudgedByNumbers()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "漢字と算数");
        var data = await NextAsync(client, family, "category=Addition");
        var problem = await GetProblemAsync(data.GetProperty("id").GetInt32());
        var result = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problem.Id}/answer?studentId={family.StudentId}", family.ChildJwt,
            new { answer = problem.CorrectAnswer, text = "えき" });
        result.EnsureSuccessStatusCode();
        var body = (await result.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
        Assert.True(body.GetProperty("isCorrect").GetBoolean());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("correctText").ValueKind);
    }

    private static async Task<JsonElement> NextAsync(HttpClient client, FamilyTestHelper.Family family, string query)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}&{query}", family.ChildJwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    private static async Task<JsonElement> AnswerAsync(HttpClient client, FamilyTestHelper.Family family, int problemId, string text)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problemId}/answer?studentId={family.StudentId}", family.ChildJwt,
            new { answer = 0, text });
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
