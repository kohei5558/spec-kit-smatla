using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 保護者ダッシュボードと子供の学習統計が、子供アカウントに紐付いた学習者の記録を表示することのテスト
/// </summary>
public class ParentDashboardTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ParentDashboardTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Dashboard_ShowsOwnChildrenWithLearningProgress()
    {
        var client = _factory.CreateClient();
        var a = await FamilyTestHelper.CreateFamilyAsync(client, "ダッシュA");
        var b = await FamilyTestHelper.CreateFamilyAsync(client, "ダッシュB");
        await SetProgressAsync(a.StudentId, totalProblems: 10, correctAnswers: 7, points: 50);

        var children = await GetAsync<List<ChildDto>>(client, a.ParentJwt, "/api/parent/children");
        var child = Assert.Single(children);
        Assert.Equal(a.StudentId, child.Id);
        Assert.Equal(10, child.TotalProblemsCompleted);
        Assert.Equal(70m, child.AccuracyRate);
        Assert.Equal(50, child.TotalPoints);

        var statistics = await GetAsync<ParentStatisticsDto>(client, a.ParentJwt, "/api/parent/statistics");
        Assert.Equal(1, statistics.TotalChildren);
        Assert.Equal(10, statistics.TotalProblemsCompleted);
        Assert.Equal(50, statistics.TotalPointsEarned);

        // 交換申請は自分の子供の分が数えられる
        var rewardId = await FamilyTestHelper.CreateRewardAsync(client, a.ParentJwt, "ダッシュボード確認用", requiredPoints: 10);
        var request = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/exchange-requests", a.ChildJwt,
            new CreateExchangeRequestRequest { RewardId = rewardId });
        request.EnsureSuccessStatusCode();
        var dashboard = await GetAsync<DashboardSummaryDto>(client, a.ParentJwt, "/api/parent/dashboard");
        Assert.Single(dashboard.Children);
        Assert.Equal(1, dashboard.PendingRequestsCount);
        Assert.Single(dashboard.RecentRequests);

        // 他の家庭のダッシュボードには出ない
        var childrenOfB = await GetAsync<List<ChildDto>>(client, b.ParentJwt, "/api/parent/children");
        Assert.DoesNotContain(a.StudentId, childrenOfB.Select(c => c.Id));
    }

    [Fact]
    public async Task Dashboard_AndChildDetail_UseLinkedStudent_WhenOldDuplicateStudentExists()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "重複学習者");
        var originalStudentId = family.StudentId;

        // 以前の不具合で、初回ログイン時に別の学習者が作られ子供アカウントに紐付いた状態を再現する
        int linkedStudentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var linked = new Student { Name = "重複学習者の子", TotalProblems = 20, CorrectAnswers = 15, TotalPoints = 80 };
            db.Students.Add(linked);
            await db.SaveChangesAsync();
            var user = await db.Users.FindAsync(family.ChildId);
            user!.StudentId = linked.Id;
            await db.SaveChangesAsync();
            linkedStudentId = linked.Id;
        }

        var children = await GetAsync<List<ChildDto>>(client, family.ParentJwt, "/api/parent/children");
        var child = Assert.Single(children);
        Assert.Equal(linkedStudentId, child.Id);
        Assert.Equal(20, child.TotalProblemsCompleted);

        var detail = await GetAsync<ChildAccountDto>(client, family.ParentJwt, $"/api/child-accounts/{family.ChildId}/detail");
        Assert.Equal(20, detail.LearningStats!.TotalProblems);
        Assert.Equal(80, detail.LearningStats.TotalPoints);
        Assert.NotEqual(originalStudentId, linkedStudentId);
    }

    private async Task SetProgressAsync(int studentId, int totalProblems, int correctAnswers, int points)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var student = await db.Students.FindAsync(studentId);
        student!.TotalProblems = totalProblems;
        student.CorrectAnswers = correctAnswers;
        student.TotalPoints = points;
        await db.SaveChangesAsync();
    }

    private static async Task<T> GetAsync<T>(HttpClient client, string jwt, string url)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, url, jwt);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }
}
