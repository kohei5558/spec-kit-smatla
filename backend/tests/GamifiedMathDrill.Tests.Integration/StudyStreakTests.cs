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
/// 「◯日連続」は1問以上解いた日（日本時間）で数え、学習記録から計算する
/// </summary>
public class StudyStreakTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public StudyStreakTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Streak_CountsTodayAsSoonAsOneProblemIsAnswered()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "連続今日");
        Assert.Equal(0, await GetStreakAsync(client, family));

        // 問題画面を開いたとき（ログイン更新）ではなく、答えたことで数える
        var next = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={family.StudentId}", family.ChildJwt);
        var problemId = (await next.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("id").GetInt32();
        (await FamilyTestHelper.SendAsync(client, HttpMethod.Post, $"/api/problems/{problemId}/answer?studentId={family.StudentId}", family.ChildJwt, new { answer = -1 })).EnsureSuccessStatusCode();

        Assert.Equal(1, await GetStreakAsync(client, family));
    }

    [Theory]
    [InlineData(new[] { 0, 1, 2 }, 3)]     // 今日・昨日・おととい
    [InlineData(new[] { 1, 2, 3 }, 3)]     // 今日はまだ。昨日まで続いていれば途切れていない
    [InlineData(new[] { 0, 2, 3 }, 1)]     // 昨日がぬけた
    [InlineData(new[] { 2, 3 }, 0)]        // 昨日も今日も解いていない
    public async Task Streak_IsCountedFromLearningRecords(int[] daysAgo, int expected)
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, $"連続{string.Join("-", daysAgo)}");
        await AddRecordsAsync(family.StudentId, daysAgo);

        Assert.Equal(expected, await GetStreakAsync(client, family));

        // 保護者の画面（子供の詳細）も同じ値
        var detail = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/child-accounts/{family.ChildId}/detail", family.ParentJwt);
        detail.EnsureSuccessStatusCode();
        var stats = (await detail.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("learningStats");
        Assert.Equal(expected, stats.GetProperty("consecutiveDays").GetInt32());
    }

    [Fact]
    public async Task LastStudyDate_IsLastAnsweredTime_NotLoginTime()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "最後に学習");
        await AddRecordsAsync(family.StudentId, new[] { 3 });

        var detail = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/child-accounts/{family.ChildId}/detail", family.ParentJwt);
        var stats = (await detail.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("learningStats");

        Assert.Equal(JapanNoonUtc(3), stats.GetProperty("lastStudyDate").GetDateTime().ToUniversalTime(), TimeSpan.FromSeconds(1));
    }

    private static async Task<int> GetStreakAsync(HttpClient client, FamilyTestHelper.Family family)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/students/{family.StudentId}", family.ChildJwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("consecutiveDays").GetInt32();
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
