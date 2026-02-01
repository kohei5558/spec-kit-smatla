using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Models.DTOs;
using GamifiedMathDrill.Core.Models.Responses;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

// 統合テスト用のDTO（実際のAPIレスポンスから取得）
public class TestProblemDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public string CalculationTypeText { get; set; } = string.Empty;
}

public class TestStatisticsDto
{
    public int TotalProblems { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public double AccuracyRate { get; set; }
    public int TotalPoints { get; set; }
    public Dictionary<string, int> ProblemsByType { get; set; } = new();
    public Dictionary<string, double> AccuracyByType { get; set; } = new();
}

public class TestLearningRecordsResponseDto
{
    public List<TestLearningRecordDto> Records { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class TestLearningRecordDto
{
    public int Id { get; set; }
    public int ProblemId { get; set; }
    public string Question { get; set; } = string.Empty;
    public int CorrectAnswer { get; set; }
    public int StudentAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public int TimeTakenSeconds { get; set; }
    public CalculationType CalculationType { get; set; }
    public string CalculationTypeText { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public DateTime SolvedAt { get; set; }
}

public class CategorySelectionTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public CategorySelectionTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetNextProblem_WithoutCategory_ShouldReturnRandomProblem()
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);
        
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Test_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        
        var statusMsg = $"Student creation failed with status: {createResponse.StatusCode}, content: {await createResponse.Content.ReadAsStringAsync()}";
        createResponse.StatusCode.Should().BeOneOf(new[] { HttpStatusCode.OK, HttpStatusCode.Created }, statusMsg);
            
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        studentResponse.Should().NotBeNull("StudentResponse should not be null");
        studentResponse!.Success.Should().BeTrue($"Student creation failed: {studentResponse.Message}");
        studentResponse.Data.Should().NotBeNull("Student Data should not be null");
        
        var student = studentResponse.Data!;

        // Act
        var response = await _client.GetAsync($"/api/problems/next?studentId={student.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var problemResponse = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<TestProblemDto>>(
            content, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        problemResponse.Should().NotBeNull($"Response content: {content}");
        problemResponse!.Success.Should().BeTrue($"API returned error: {problemResponse.Message}");
        
        var problem = problemResponse.Data;
        problem.Should().NotBeNull("Data property should not be null");
        problem!.CalculationTypeText.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("Addition")]
    [InlineData("Subtraction")]
    [InlineData("Multiplication")]
    [InlineData("Division")]
    public async Task GetNextProblem_WithSpecificCategory_ShouldReturnOnlyThatCategory(string category)
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);
        
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Test_{category[..3]}_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        // Act - Get multiple problems to ensure consistency
        for (int i = 0; i < 5; i++)
        {
            var response = await _client.GetAsync(
                $"/api/problems/next?studentId={student.Id}&calculationType={category}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var problemResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestProblemDto>>();
            var problem = problemResponse!.Data;
            problem.Should().NotBeNull();
            problem!.CalculationTypeText.Should().Be(category, 
                $"すべての問題は指定されたカテゴリ（{category}）であるべき");
        }
    }

    [Fact]
    public async Task GetStatistics_WithCategoryFilter_ShouldOnlyCountThatCategory()
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);
        
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Test_Stats_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        // Create learning records for multiple categories
        await CreateLearningRecords(student.Id, "Addition", 5, true);
        await CreateLearningRecords(student.Id, "Subtraction", 3, false);

        // Act - Get statistics with category filter
        var response = await _client.GetAsync(
            $"/api/learning-records/statistics?studentId={student.Id}&calculationType=Addition");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var statsResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestStatisticsDto>>();
        var stats = statsResponse!.Data;
        stats.Should().NotBeNull();
        stats!.TotalProblems.Should().Be(5, "足し算の問題のみカウントされるべき");
        stats.ProblemsByType.Should().ContainKey("Addition");
        stats.ProblemsByType["Addition"].Should().Be(5);
    }

    [Fact]
    public async Task GetStatistics_WithoutCategoryFilter_ShouldCountAllCategories()
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);
        
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Test_All_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        // Create learning records for multiple categories
        await CreateLearningRecords(student.Id, "Addition", 5, true);
        await CreateLearningRecords(student.Id, "Subtraction", 3, true);
        await CreateLearningRecords(student.Id, "Multiplication", 4, true);

        // Act - Get all statistics
        var response = await _client.GetAsync(
            $"/api/learning-records/statistics?studentId={student.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var statsResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestStatisticsDto>>();
        var stats = statsResponse!.Data;
        stats.Should().NotBeNull();
        stats!.TotalProblems.Should().Be(12, "すべてのカテゴリの問題がカウントされるべき");
        stats.ProblemsByType.Should().HaveCount(3);
        stats.ProblemsByType["Addition"].Should().Be(5);
        stats.ProblemsByType["Subtraction"].Should().Be(3);
        stats.ProblemsByType["Multiplication"].Should().Be(4);
    }

    [Fact]
    public async Task GetRecords_WithCategoryFilter_ShouldOnlyReturnThatCategory()
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);
        
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Test_Rec_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        // Create learning records
        await CreateLearningRecords(student.Id, "Addition", 3, true);
        await CreateLearningRecords(student.Id, "Multiplication", 2, true);

        // Act
        var response = await _client.GetAsync(
            $"/api/learning-records?studentId={student.Id}&calculationType=Addition");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var resultResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestLearningRecordsResponseDto>>();
        var result = resultResponse!.Data;
        result.Should().NotBeNull();
        result!.Records.Should().HaveCount(3);
        result.Records.Should().AllSatisfy(r => 
            r.CalculationType.Should().Be(CalculationType.Addition));
    }

    [Fact]
    public async Task CategoryStatistics_ShouldShowAccuracyByCategory()
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);
        
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Test_Acc_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        // Create records with different accuracy rates
        // Addition: 4/5 = 80%
        await CreateLearningRecords(student.Id, "Addition", 4, true);
        await CreateLearningRecords(student.Id, "Addition", 1, false);

        // Multiplication: 2/4 = 50%
        await CreateLearningRecords(student.Id, "Multiplication", 2, true);
        await CreateLearningRecords(student.Id, "Multiplication", 2, false);

        // Act
        var response = await _client.GetAsync(
            $"/api/learning-records/statistics?studentId={student.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var statsResponse = await response.Content.ReadFromJsonAsync<ApiResponse<TestStatisticsDto>>();
        var stats = statsResponse!.Data;
        stats.Should().NotBeNull();
        stats!.AccuracyByType["Addition"].Should().BeApproximately(80.0, 0.1);
        stats.AccuracyByType["Multiplication"].Should().BeApproximately(50.0, 0.1);
    }

    [Fact]
    public async Task GetNextProblem_WithInvalidCategory_ShouldReturnBadRequest()
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);
        
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Test_Inv_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        // Act
        var response = await _client.GetAsync(
            $"/api/problems/next?studentId={student.Id}&calculationType=InvalidCategory");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CategoryFlow_SelectToSolveMultipleProblems_ShouldMaintainCategory()
    {
        // Arrange - Authenticate as parent
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);
        var token = await AuthenticationHelper.LoginAsParentAsync(_client, _factory.Services);
        AuthenticationHelper.AddAuthorizationHeader(_client, token);
        
        var createResponse = await _client.PostAsJsonAsync("/api/students", new
        {
            Name = $"Test_Flow_{Guid.NewGuid().ToString()[..8]}",
            Grade = 3
        });
        var studentResponse = await createResponse.Content.ReadFromJsonAsync<ApiResponse<TestStudentDto>>();
        var student = studentResponse!.Data!;

        const string selectedCategory = "Multiplication";

        // Act & Assert - Solve 10 problems in the same category
        for (int i = 0; i < 10; i++)
        {
            // Get problem
            var problemApiResponse = await _client.GetAsync(
                $"/api/problems/next?studentId={student.Id}&calculationType={selectedCategory}");
            problemApiResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var problemResponse = await problemApiResponse.Content.ReadFromJsonAsync<ApiResponse<TestProblemDto>>();
            var problem = problemResponse!.Data;
            problem.Should().NotBeNull();
            problem!.CalculationTypeText.Should().Be(selectedCategory);

            // Submit answer (固定値を使用)
            var answerResponse = await _client.PostAsJsonAsync(
                "/api/problems/answer",
                new
                {
                    StudentId = student.Id,
                    ProblemId = problem.Id,
                    Answer = 1
                });
            answerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Verify statistics
        var statsApiResponse = await _client.GetAsync(
            $"/api/learning-records/statistics?studentId={student.Id}&calculationType={selectedCategory}");
        var statsResponse = await statsApiResponse.Content.ReadFromJsonAsync<ApiResponse<TestStatisticsDto>>();
        var stats = statsResponse!.Data;
        stats.Should().NotBeNull();
        stats!.TotalProblems.Should().Be(10);
        stats.ProblemsByType[selectedCategory].Should().Be(10);
    }

    private async Task CreateLearningRecords(int studentId, string category, int count, bool correct)
    {
        for (int i = 0; i < count; i++)
        {
            var problemApiResponse = await _client.GetAsync(
                $"/api/problems/next?studentId={studentId}&calculationType={category}");
            var problemResponse = await problemApiResponse.Content.ReadFromJsonAsync<ApiResponse<TestProblemDto>>();
            var problem = problemResponse?.Data;

            if (problem == null) continue;

            // ProblemDtoには問題文のみが含まれているため、正答を計算できない
            // テストのため、正解時は1、不正解時は999を送信
            var answer = correct ? 1 : 999;

            await _client.PostAsJsonAsync("/api/problems/answer", new
            {
                StudentId = studentId,
                ProblemId = problem.Id,
                Answer = answer
            });
        }
    }
}

public class CreateStudentRequest
{
    public string Name { get; set; } = "";
    public int Grade { get; set; }
}

public class SubmitAnswerRequest
{
    public int StudentId { get; set; }
    public int ProblemId { get; set; }
    public int Answer { get; set; }
}
