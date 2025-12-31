using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

[ApiController]
[Route("api/learning-records")]
public class LearningRecordsController : ControllerBase
{
    private readonly ILearningRecordService _learningRecordService;
    private readonly ILogger<LearningRecordsController> _logger;

    public LearningRecordsController(
        ILearningRecordService learningRecordService,
        ILogger<LearningRecordsController> logger)
    {
        _learningRecordService = learningRecordService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<LearningRecordsResponseDto>>> GetRecords(
        [FromQuery] int studentId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] CalculationType? calculationType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (studentId <= 0)
            {
                return BadRequest(new ErrorResponse(
                    "InvalidStudentId",
                    "Student ID must be greater than 0"
                ));
            }

            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 100) pageSize = 20;

            var (records, totalCount) = await _learningRecordService.GetRecordsAsync(
                studentId, startDate, endDate, calculationType, page, pageSize);

            var recordDtos = records.Select(r => new LearningRecordDto
            {
                Id = r.Id,
                ProblemId = r.ProblemId,
                Question = r.Problem.Question,
                CorrectAnswer = r.Problem.CorrectAnswer,
                StudentAnswer = r.StudentAnswer,
                IsCorrect = r.IsCorrect,
                PointsEarned = r.PointsEarned,
                TimeTakenSeconds = r.TimeTakenSeconds,
                CalculationType = r.Problem.CalculationType,
                CalculationTypeText = r.Problem.CalculationType.ToString(),
                DifficultyLevel = r.Problem.DifficultyLevel,
                SolvedAt = r.SolvedAt
            }).ToList();

            var response = new LearningRecordsResponseDto
            {
                Records = recordDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(new SuccessResponse<LearningRecordsResponseDto>(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving learning records for student {StudentId}", studentId);
            return StatusCode(500, new ErrorResponse(
                "InternalServerError",
                "Failed to retrieve learning records"
            ));
        }
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<StatisticsDto>>> GetStatistics(
        [FromQuery] int studentId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            if (studentId <= 0)
            {
                return BadRequest(new ErrorResponse(
                    "InvalidStudentId",
                    "Student ID must be greater than 0"
                ));
            }

            var statistics = await _learningRecordService.GetStatisticsAsync(
                studentId, startDate, endDate);

            var dto = new StatisticsDto
            {
                TotalProblems = statistics.TotalProblems,
                CorrectAnswers = statistics.CorrectAnswers,
                IncorrectAnswers = statistics.IncorrectAnswers,
                AccuracyRate = statistics.AccuracyRate,
                TotalPoints = statistics.TotalPoints,
                ProblemsByType = statistics.ProblemsByType
                    .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
                AccuracyByType = statistics.AccuracyByType
                    .ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value),
                DailyProblems = statistics.DailyProblems,
                DailyAccuracy = statistics.DailyAccuracy
            };

            return Ok(new SuccessResponse<StatisticsDto>(dto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics for student {StudentId}", studentId);
            return StatusCode(500, new ErrorResponse(
                "InternalServerError",
                "Failed to retrieve statistics"
            ));
        }
    }
}

public class LearningRecordsResponseDto
{
    public List<LearningRecordDto> Records { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class LearningRecordDto
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

public class StatisticsDto
{
    public int TotalProblems { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public double AccuracyRate { get; set; }
    public int TotalPoints { get; set; }
    public Dictionary<string, int> ProblemsByType { get; set; } = new();
    public Dictionary<string, double> AccuracyByType { get; set; } = new();
    public Dictionary<string, int> DailyProblems { get; set; } = new();
    public Dictionary<string, double> DailyAccuracy { get; set; } = new();
}
