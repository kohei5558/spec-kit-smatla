using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Core.Models.Responses;
using GamifiedMathDrill.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Api.Controllers;

/// <summary>
/// 学習記録と統計データを管理するコントローラー
/// </summary>
[ApiController]
[Route("api/learning-records")]
[Produces("application/json")]
[Authorize(Roles = "Parent,Child")]
public class LearningRecordsController : ControllerBase
{
    private readonly ILearningRecordService _learningRecordService;
    private readonly IStudentAccessService _studentAccess;
    private readonly ILogger<LearningRecordsController> _logger;

    public LearningRecordsController(
        ILearningRecordService learningRecordService,
        IStudentAccessService studentAccess,
        ILogger<LearningRecordsController> logger)
    {
        _learningRecordService = learningRecordService;
        _studentAccess = studentAccess;
        _logger = logger;
    }

    /// <summary>
    /// 学習記録の一覧を取得します
    /// </summary>
    /// <param name="studentId">学習者ID（必須）</param>
    /// <param name="startDate">開始日（オプション）</param>
    /// <param name="endDate">終了日（オプション）</param>
    /// <param name="calculationType">計算カテゴリでフィルタ（オプション: Addition, Subtraction, Multiplication, Division）</param>
    /// <param name="page">ページ番号（デフォルト: 1）</param>
    /// <param name="pageSize">ページサイズ（デフォルト: 20、最大: 100）</param>
    /// <returns>学習記録のページングされたリスト</returns>
    /// <response code="200">学習記録が正常に取得されました</response>
    /// <response code="400">リクエストパラメータが不正です</response>
    /// <remarks>
    /// カテゴリフィルタの使用例:
    /// - GET /api/learning-records?studentId=1 - すべてのカテゴリの記録
    /// - GET /api/learning-records?studentId=1&amp;calculationType=Addition - 足し算のみ
    /// - GET /api/learning-records?studentId=1&amp;startDate=2024-01-01&amp;endDate=2024-01-31 - 期間指定
    /// - GET /api/learning-records?studentId=1&amp;calculationType=Multiplication&amp;page=2&amp;pageSize=10 - カテゴリ+ページング
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<LearningRecordsResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<LearningRecordsResponseDto>>> GetRecords(
        [FromQuery][Required] int studentId,
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

            // 自分（子供）または自分の子供（保護者）の記録のみ。他家庭の学習者は存在しない扱い
            if (!await this.CanAccessStudentAsync(_studentAccess, studentId))
            {
                return NotFound(new ErrorResponse("StudentNotFound", "Student not found"));
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

    /// <summary>
    /// 学習統計データを取得します
    /// </summary>
    /// <param name="studentId">学習者ID（必須）</param>
    /// <param name="startDate">開始日（オプション）</param>
    /// <param name="endDate">終了日（オプション）</param>
    /// <param name="calculationType">特定カテゴリの統計のみ取得（オプション）</param>
    /// <returns>統計データ（問題数、正答率、カテゴリ別内訳）</returns>
    /// <response code="200">統計データが正常に取得されました</response>
    /// <response code="400">リクエストパラメータが不正です</response>
    /// <remarks>
    /// レスポンスには以下の情報が含まれます:
    /// - totalProblems: 解いた問題の総数
    /// - correctAnswers: 正解数
    /// - averageAccuracy: 全体の正答率
    /// - totalPoints: 獲得ポイント合計
    /// - problemsByType: カテゴリ別の問題数
    /// - accuracyByType: カテゴリ別の正答率
    /// 
    /// 使用例:
    /// - GET /api/learning-records/statistics?studentId=1 - 全期間の統計
    /// - GET /api/learning-records/statistics?studentId=1&amp;calculationType=Addition - 足し算のみの統計
    /// - GET /api/learning-records/statistics?studentId=1&amp;startDate=2024-01-01 - 期間指定
    /// 
    /// パフォーマンス:
    /// - カテゴリ指定時: 200ms未満
    /// - 全体統計: 300ms未満
    /// </remarks>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<StatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StatisticsDto>>> GetStatistics(
        [FromQuery][Required] int studentId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] CalculationType? calculationType = null)
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

            // 自分（子供）または自分の子供（保護者）の記録のみ。他家庭の学習者は存在しない扱い
            if (!await this.CanAccessStudentAsync(_studentAccess, studentId))
            {
                return NotFound(new ErrorResponse("StudentNotFound", "Student not found"));
            }

            var statistics = await _learningRecordService.GetStatisticsAsync(
                studentId, startDate, endDate, calculationType);

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
