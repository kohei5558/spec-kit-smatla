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
/// 計算の種類ごとのレベル（014）
/// </summary>
public class UnitLevelTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public UnitLevelTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task LevelUp_OnlyChangesThatType()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "種類ごとのレベルアップ");
        // レベル5はあと1問（15問連続）でレベルアップ
        await StudentLevelTestHelper.SetAsync(_factory.Services, family.StudentId, 5, 14, CalculationType.Addition, CalculationType.Division);

        var result = await AnswerAsync(client, family, CalculationType.Addition, correct: true);

        Assert.True(result.GetProperty("leveledUp").GetBoolean());
        Assert.Equal("Addition", result.GetProperty("leveledUpCalculationType").GetString());
        Assert.Equal((6, 0), await GetAsync(family.StudentId, CalculationType.Addition));
        Assert.Equal((5, 14), await GetAsync(family.StudentId, CalculationType.Division));
    }

    [Fact]
    public async Task WrongAnswer_ResetsOnlyThatTypesStreak()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "種類ごとの連続正解");
        await StudentLevelTestHelper.SetAsync(_factory.Services, family.StudentId, 5, 10, CalculationType.Addition);
        await StudentLevelTestHelper.SetAsync(_factory.Services, family.StudentId, 5, 5, CalculationType.Division);

        var result = await AnswerAsync(client, family, CalculationType.Division, correct: false);

        Assert.False(result.GetProperty("leveledUp").GetBoolean());
        Assert.Equal((5, 0), await GetAsync(family.StudentId, CalculationType.Division));
        Assert.Equal((5, 10), await GetAsync(family.StudentId, CalculationType.Addition));
    }

    [Fact]
    public async Task Problems_UseEachTypesLevel_AlsoInMix()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "種類ごとの難易度");
        // たし算だけレベル9（難易度9〜10）、ほかはレベル1（難易度1〜2）
        await StudentLevelTestHelper.SetAsync(_factory.Services, family.StudentId, 1, 0, StudentLevelRules.GradeLeveledTypes.ToArray());
        await StudentLevelTestHelper.SetAsync(_factory.Services, family.StudentId, 9, 0, CalculationType.Addition);

        for (var i = 0; i < 5; i++)
        {
            Assert.InRange((await NextAsync(client, family, CalculationType.Addition)).Difficulty, 9, 10);
            Assert.InRange((await NextAsync(client, family, CalculationType.Division)).Difficulty, 1, 2);
        }

        // 「すべて」でも、出た問題の種類のレベルの難易度
        var types = new HashSet<CalculationType>();
        for (var i = 0; i < 30; i++)
        {
            var (type, difficulty) = await NextAsync(client, family, null);
            types.Add(type);
            if (type == CalculationType.Addition)
            {
                Assert.InRange(difficulty, 9, 10);
            }
            else
            {
                Assert.InRange(difficulty, 1, 2);
            }
        }
        Assert.True(types.Count >= 3, "「すべて」でいろいろな種類が出る");
    }

    [Fact]
    public async Task TypeWithoutRow_StartsAtLevel1_AndRowIsCreatedOnAnswer()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "行がない種類");
        // 新しい単元を足した直後と同じ状態（小3でも、行が無い種類はレベル1）
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.StudentLevels.RemoveRange(db.StudentLevels.Where(l => l.StudentId == family.StudentId && l.CalculationType == CalculationType.DivisionWithRemainder));
            await db.SaveChangesAsync();
        }

        for (var i = 0; i < 5; i++)
        {
            Assert.InRange((await NextAsync(client, family, CalculationType.DivisionWithRemainder)).Difficulty, 1, 2);
        }

        await AnswerAsync(client, family, CalculationType.DivisionWithRemainder, correct: true);
        Assert.Equal((1, 1), await GetAsync(family.StudentId, CalculationType.DivisionWithRemainder));
    }

    [Fact]
    public async Task StudentApi_ReturnsAllTypesLevels_AndHighestLevel()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "学習者APIのレベル");
        await StudentLevelTestHelper.SetAsync(_factory.Services, family.StudentId, 7, 3, CalculationType.Multiplication);

        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/students/{family.StudentId}", family.ChildJwt);
        response.EnsureSuccessStatusCode();
        var data = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");

        Assert.Equal(7, data.GetProperty("highestLevel").GetInt32());
        var levels = data.GetProperty("levels").EnumerateArray().ToDictionary(l => l.GetProperty("calculationType").GetString()!);
        Assert.Equal(Enum.GetNames<CalculationType>().OrderBy(n => n), levels.Keys.OrderBy(n => n));
        Assert.Equal(7, levels["Multiplication"].GetProperty("levelNumber").GetInt32());
        Assert.Equal(3, levels["Multiplication"].GetProperty("correctAnswers").GetInt32());
        Assert.Equal(20, levels["Multiplication"].GetProperty("requiredCorrectAnswers").GetInt32());
        Assert.Equal(5, levels["Addition"].GetProperty("levelNumber").GetInt32()); // 小3
    }

    private Task<(int, int)?> GetAsync(int studentId, CalculationType type) =>
        StudentLevelTestHelper.GetAsync(_factory.Services, studentId, type);

    private async Task<(CalculationType Type, int Difficulty)> NextAsync(HttpClient client, FamilyTestHelper.Family family, CalculationType? type)
    {
        var url = $"/api/problems/next?studentId={family.StudentId}" + (type.HasValue ? $"&category={type}" : "");
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, url, family.ChildJwt);
        response.EnsureSuccessStatusCode();
        var data = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
        var problem = await GetProblemAsync(data.GetProperty("id").GetInt32());
        return (problem.CalculationType, data.GetProperty("difficultyLevel").GetInt32());
    }

    private async Task<JsonElement> AnswerAsync(HttpClient client, FamilyTestHelper.Family family, CalculationType type, bool correct)
    {
        var next = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}&category={type}", family.ChildJwt);
        var problemId = (await next.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("id").GetInt32();
        var problem = await GetProblemAsync(problemId);
        var answer = correct ? problem.CorrectAnswer : problem.CorrectAnswer + 1;

        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problemId}/answer?studentId={family.StudentId}", family.ChildJwt,
            new { answer, remainder = problem.CorrectRemainder });
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
