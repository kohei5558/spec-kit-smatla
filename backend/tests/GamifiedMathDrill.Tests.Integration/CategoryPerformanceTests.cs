using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GamifiedMathDrill.Core.Models.Responses;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

// 統合テスト用の軽量DTO
public class TestStudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Grade { get; set; }
}

public class CategoryPerformanceTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public CategoryPerformanceTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("Addition")]
    [InlineData("Subtraction")]
    [InlineData("Multiplication")]
    [InlineData("Division")]
    public async Task GetNextProblem_WithCategory_ShouldReturnInLessThan100Ms(string category)
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);

        // Create student first
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Perf_{category[..3]}_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        // Warm-up call to exclude cold start time
        await _client.GetAsync($"/api/problems/next?studentId={student.Id}&category={category}");

        // Act - Measure performance
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync($"/api/problems/next?studentId={student.Id}&category={category}");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100,
            $"カテゴリ指定の問題取得は100ms以内に完了すべき (実測: {stopwatch.ElapsedMilliseconds}ms)");

        var problemResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestProblemDto>>();
        var problem = problemResponse!.Data;
        problem.Should().NotBeNull();
        problem!.CalculationTypeText.Should().Be(category);
    }

    [Fact]
    public async Task GetStatistics_WithCategoryFilter_ShouldReturnInLessThan200Ms()
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);

        // Create student and learning records
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Perf_Stats_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        // Create some learning records for different categories
        for (int i = 0; i < 10; i++)
        {
            var problemApiResponse = await _client.GetAsync(
                $"/api/problems/next?studentId={student.Id}&category=Addition");
            var problemResponse = await problemApiResponse.Content.ReadFromJsonAsync<ApiResponse<TestProblemDto>>();
            var problem = problemResponse!.Data;

            // Parse the question to calculate the correct answer
            var correctAnswer = ParseQuestionForAnswer(problem!.Question);

            await _client.PostAsJsonAsync(
                $"/api/problems/{problem.Id}/answer?studentId={student.Id}",
                new { Answer = correctAnswer });
        }

        // Warm-up call
        await _client.GetAsync(
            $"/api/learning-records/statistics?studentId={student.Id}&category=Addition");

        // Act - Measure performance
        var stopwatch = Stopwatch.StartNew();
        var response = await _client.GetAsync(
            $"/api/learning-records/statistics?studentId={student.Id}&category=Addition");
        stopwatch.Stop();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200,
            $"カテゴリ別統計取得は200ms以内に完了すべき (実測: {stopwatch.ElapsedMilliseconds}ms)");

        var statsResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestStatisticsDto>>();
        var stats = statsResponse!.Data;
        stats.Should().NotBeNull();
        stats!.ProblemsByType.Should().ContainKey("Addition");
    }

    [Fact]
    public async Task CategorySelectionToDisplayFlow_ShouldCompleteInLessThan2Seconds()
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);

        // Create student
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Perf_Flow_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        // Warm-up
        await _client.GetAsync($"/api/problems/next?studentId={student.Id}&category=Addition");

        // Act - Measure end-to-end flow
        var stopwatch = Stopwatch.StartNew();

        // 1. Get problem with category
        var problemApiResponse = await _client.GetAsync(
            $"/api/problems/next?studentId={student.Id}&category=Multiplication");
        var problemResponse = await problemApiResponse.Content.ReadFromJsonAsync<ApiResponse<TestProblemDto>>();
        var problem = problemResponse!.Data;

        stopwatch.Stop();

        // Assert
        problemApiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        problem.Should().NotBeNull();
        problem!.CalculationTypeText.Should().Be("Multiplication");

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000,
            $"カテゴリ選択から問題表示までは2秒以内に完了すべき (実測: {stopwatch.ElapsedMilliseconds}ms)");
    }

    [Fact]
    public async Task ConcurrentCategoryRequests_ShouldHandleLoadEfficiently()
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);

        // Create student
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Perf_Conc_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        var categories = new[] { "Addition", "Subtraction", "Multiplication", "Division" };

        // Act - Make concurrent requests
        var stopwatch = Stopwatch.StartNew();

        var tasks = categories.Select(category =>
            _client.GetAsync($"/api/problems/next?studentId={student.Id}&category={category}")
        ).ToArray();

        var responses = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.OK));

        // Average time per request should be reasonable
        var avgTimePerRequest = stopwatch.ElapsedMilliseconds / (double)categories.Length;
        avgTimePerRequest.Should().BeLessThan(150,
            $"並行リクエストの平均応答時間は150ms以内であるべき (実測: {avgTimePerRequest:F1}ms)");
    }

    private int ParseQuestionForAnswer(string question)
    {
        // Remove "= ?" from the end
        var parts = question.Replace(" = ?", "").Trim().Split(' ');
        if (parts.Length != 3) return 0;

        if (!int.TryParse(parts[0], out var num1)) return 0;
        if (!int.TryParse(parts[2], out var num2)) return 0;

        var op = parts[1];
        return op switch
        {
            "+" => num1 + num2,
            "-" => num1 - num2,
            "×" or "*" => num1 * num2,
            "÷" or "/" => num2 != 0 ? num1 / num2 : 0,
            _ => 0
        };
    }
}