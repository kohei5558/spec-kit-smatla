using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 子供の詳細の学習統計の JSON の名前は specs/005 の contracts/child-account-api.yaml に合わせる。
/// 画面（ChildLearningStatsViewModel）も同じ名前で受け取る。以前は画面だけ last7DaysActivity / problemsSolved で、
/// 「過去7日間の学習記録」のグラフが表示されなかった
/// </summary>
public class ChildLearningStatsContractTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ChildLearningStatsContractTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Detail_ReturnsRecentActivity_WithContractNames()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "学習統計の名前");
        for (var i = 0; i < 2; i++)
        {
            var next = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}", family.ChildJwt);
            var problemId = (await next.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("id").GetInt32();
            (await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problemId}/answer?studentId={family.StudentId}", family.ChildJwt, new { answer = -1 })).EnsureSuccessStatusCode();
        }

        var detail = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/child-accounts/{family.ChildId}/detail", family.ParentJwt);
        detail.EnsureSuccessStatusCode();
        var stats = (await detail.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("learningStats");

        var today = Assert.Single(stats.GetProperty("recentActivity").EnumerateArray());
        Assert.Equal(2, today.GetProperty("problemsCount").GetInt32());
        Assert.True(today.TryGetProperty("date", out _));
    }
}
