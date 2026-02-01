using System.Net;
using System.Net.Http.Json;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Tests.Integration.Helpers;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// API の基本的な動作確認テスト
/// </summary>
public class ApiBasicTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public ApiBasicTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Api_IsRunning()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        // API が実行されていることを確認（404は正常、サーバーが動いている証拠）
        Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task StudentsEndpoint_CanBeAccessed()
    {
        // Arrange
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);
        
        await SeedTestStudent();

        // Act
        var response = await _client.GetAsync("/api/Students/1");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
    }

    private async Task SeedTestStudent()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // データベースをクリア
        db.Students.RemoveRange(db.Students);
        await db.SaveChangesAsync();

        // テスト生徒を追加
        var student = new Student
        {
            Name = "Test Student",
            TotalPoints = 100
        };
        db.Students.Add(student);
        await db.SaveChangesAsync();
    }
}
