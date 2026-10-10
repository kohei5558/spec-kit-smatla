using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 問題画面の「今日の◯問め」のもとになる、今日（日本時間）答えた数のテスト
/// </summary>
public class TodayAnsweredCountTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public TodayAnsweredCountTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task NextProblem_ReturnsTodaysAnsweredCount()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "今日の数");

        var first = await NextAsync(client, family);
        Assert.Equal(0, first.GetProperty("todayAnsweredCount").GetInt32());

        // 正解・不正解にかかわらず、答えるたびに増える
        await AnswerAsync(client, family, first.GetProperty("id").GetInt32(), -1);
        var second = await NextAsync(client, family);
        await AnswerAsync(client, family, second.GetProperty("id").GetInt32(), -1);

        Assert.Equal(1, second.GetProperty("todayAnsweredCount").GetInt32());
        Assert.Equal(2, (await NextAsync(client, family)).GetProperty("todayAnsweredCount").GetInt32());
    }

    private static async Task<JsonElement> NextAsync(HttpClient client, FamilyTestHelper.Family family)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}", family.ChildJwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
    }

    private static async Task AnswerAsync(HttpClient client, FamilyTestHelper.Family family, int problemId, int answer)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problemId}/answer?studentId={family.StudentId}", family.ChildJwt, new { answer });
        response.EnsureSuccessStatusCode();
    }
}
