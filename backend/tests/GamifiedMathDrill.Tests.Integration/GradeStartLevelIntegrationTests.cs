using System.Net.Http.Json;
using System.Text.Json;
using GamifiedMathDrill.Core.DTOs;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 子供アカウントの学年に応じて始めるレベルが設定されることのテスト（008）
/// </summary>
public class GradeStartLevelIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public GradeStartLevelIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task NewChild_StartsAtLevelForGrade_AndGetsMatchingProblems()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "学年3年");
        var grade1 = await CreateChildAsync(client, family.ParentJwt, "学年1年の子", grade: 1);

        Assert.Equal(5, await GetLevelAsync(family.StudentId)); // FamilyTestHelper の子供は3年生
        Assert.Equal(1, await GetLevelAsync(grade1.StudentId));

        // 3年生（レベル5）には難易度5〜6の問題が出る
        for (var i = 0; i < 10; i++)
        {
            var difficulty = await GetNextProblemDifficultyAsync(client, family.ParentJwt, family.StudentId);
            Assert.InRange(difficulty, 5, 6);
        }
    }

    [Fact]
    public async Task RaisingGrade_RaisesLevel_ButLoweringGrade_KeepsLevel()
    {
        var client = _factory.CreateClient();
        var family = await FamilyTestHelper.CreateFamilyAsync(client, "学年変更");
        var child = await CreateChildAsync(client, family.ParentJwt, "学年を変える子", grade: 2);
        Assert.Equal(3, await GetLevelAsync(child.StudentId));

        await UpdateGradeAsync(client, family.ParentJwt, child, grade: 3);
        Assert.Equal(5, await GetLevelAsync(child.StudentId));

        // レベル8まで上がった後に学年を下げても、レベルは下がらない
        await SetLevelAsync(child.StudentId, 8);
        await UpdateGradeAsync(client, family.ParentJwt, child, grade: 2);
        Assert.Equal(8, await GetLevelAsync(child.StudentId));
    }

    private static async Task<ChildAccountDto> CreateChildAsync(HttpClient client, string parentJwt, string name, int grade)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Post, "/api/child-accounts", parentJwt,
            new ChildAccountCreateDto { Name = name, GradeLevel = grade, PresetAvatarId = 1, PIN = "3579" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ChildAccountDto>())!;
    }

    private static async Task UpdateGradeAsync(HttpClient client, string parentJwt, ChildAccountDto child, int grade)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Put, $"/api/child-accounts/{child.Id}", parentJwt,
            new ChildAccountUpdateDto { Name = child.Name, GradeLevel = grade, PresetAvatarId = 1 });
        response.EnsureSuccessStatusCode();
    }

    private static async Task<int> GetNextProblemDifficultyAsync(HttpClient client, string jwt, int studentId)
    {
        var response = await FamilyTestHelper.SendAsync(client, HttpMethod.Get, $"/api/problems/next?studentId={studentId}", jwt);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("data").GetProperty("difficultyLevel").GetInt32();
    }

    private async Task<int> GetLevelAsync(int studentId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return (await db.Students.FindAsync(studentId))!.CurrentLevelId;
    }

    private async Task SetLevelAsync(int studentId, int level)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Students.FindAsync(studentId))!.CurrentLevelId = level;
        await db.SaveChangesAsync();
    }
}
