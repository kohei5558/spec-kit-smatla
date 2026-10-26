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

        var today = stats.GetProperty("recentActivity").EnumerateArray().Last();
        Assert.Equal(2, today.GetProperty("problemsCount").GetInt32());
        Assert.True(today.TryGetProperty("date", out _));
    }

    /// <summary>
    /// グラフが「7日間」に見えるよう、解いていない日も 0 問として、古い日から今日まで7日分を返す
    /// </summary>
    [Fact]
    public async Task Detail_ReturnsSevenDays_IncludingDaysWithoutStudy()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "学習記録の7日分");
        // 今日2問・おととい1問・8日前1問（7日より前は数えない）
        await AddRecordsAsync(family.StudentId, new[] { 0, 0, 2, 8 });

        var detail = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/child-accounts/{family.ChildId}/detail", family.ParentJwt);
        detail.EnsureSuccessStatusCode();
        var days = (await detail.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("learningStats").GetProperty("recentActivity").EnumerateArray().ToList();

        Assert.Equal(new[] { 0, 0, 0, 0, 1, 0, 2 }, days.Select(d => d.GetProperty("problemsCount").GetInt32()));
        var japanToday = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(9));
        Assert.Equal(
            Enumerable.Range(0, 7).Select(i => japanToday.AddDays(i - 6)),
            days.Select(d => DateOnly.FromDateTime(d.GetProperty("date").GetDateTime())));
    }

    /// <summary>
    /// 日本時間で n 日前の正午（UTC）
    /// </summary>
    private static DateTime JapanNoonUtc(int daysAgo)
    {
        var japanToday = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(9));
        return japanToday.AddDays(-daysAgo).ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc).AddHours(-9);
    }

    private async Task AddRecordsAsync(int studentId, IEnumerable<int> daysAgo)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var problemId = await db.Problems.Select(p => p.Id).FirstAsync();
        foreach (var d in daysAgo)
        {
            db.LearningRecords.Add(new LearningRecord { StudentId = studentId, ProblemId = problemId, StudentAnswer = 0, SolvedAt = JapanNoonUtc(d) });
        }
        await db.SaveChangesAsync();
    }
}
