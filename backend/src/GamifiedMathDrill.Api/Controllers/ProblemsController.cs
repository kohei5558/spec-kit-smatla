using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models.DTOs;
using GamifiedMathDrill.Core.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Api.Controllers;

/// <summary>
/// 算数問題の取得と回答送信を管理するコントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "Parent,Child")]
public class ProblemsController : ControllerBase
{
    private readonly IProblemService _problemService;
    private readonly ILogger<ProblemsController> _logger;

    public ProblemsController(IProblemService problemService, ILogger<ProblemsController> logger)
    {
        _problemService = problemService;
        _logger = logger;
    }

    /// <summary>
    /// 次の問題を取得します
    /// </summary>
    /// <param name="studentId">学習者ID（必須）</param>
    /// <param name="excludeRecentIds">除外する問題IDのリスト（オプション）</param>
    /// <param name="category">計算カテゴリ（オプション: Addition, Subtraction, Multiplication, Division）</param>
    /// <returns>問題データ</returns>
    /// <response code="200">問題が正常に取得されました</response>
    /// <response code="400">リクエストパラメータが不正です</response>
    /// <response code="404">指定条件に一致する問題が見つかりませんでした</response>
    /// <remarks>
    /// 学習者の履歴に基づいて適切な難易度の問題を自動選択します。
    /// 
    /// カテゴリパラメータの使用例:
    /// - GET /api/problems/next?studentId=1 - すべてのカテゴリからランダム選択
    /// - GET /api/problems/next?studentId=1&amp;category=Addition - 足し算のみ
    /// - GET /api/problems/next?studentId=1&amp;category=Multiplication - 掛け算のみ
    /// 
    /// パフォーマンス:
    /// - カテゴリ指定時: 100ms未満
    /// - カテゴリ未指定時: 50ms未満
    /// </remarks>
    [HttpGet("next")]
    [ProducesResponseType(typeof(ApiResponse<ProblemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProblemDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ProblemDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProblemDto>>> GetNext(
        [FromQuery][Required] int studentId,
        [FromQuery] List<int>? excludeRecentIds = null,
        [FromQuery] Core.Models.CalculationType? category = null)
    {
        if (studentId <= 0)
        {
            return BadRequest(ApiResponse<ProblemDto>.ErrorResponse("Valid student ID is required."));
        }

        // 自動ModelState検証は無効化されているため、不正なカテゴリ（バインド失敗・未定義の数値）をここで弾く
        if (!ModelState.IsValid || (category.HasValue && !Enum.IsDefined(category.Value)))
        {
            return BadRequest(ApiResponse<ProblemDto>.ErrorResponse("Invalid category."));
        }

        try
        {
            var problem = await _problemService.GetNextProblemAsync(studentId, excludeRecentIds, category);
            if (problem == null)
            {
                var message = category.HasValue
                    ? $"選択したカテゴリ（{GetCategoryDisplayName(category.Value)}）の問題が見つかりませんでした。別のカテゴリを選択してください。"
                    : "利用可能な問題が見つかりませんでした。";
                _logger.LogWarning("No problems available for studentId={StudentId}, category={Category}", studentId, category);
                return NotFound(ApiResponse<ProblemDto>.ErrorResponse(message));
            }

            var problemDto = new ProblemDto
            {
                Id = problem.Id,
                Question = problem.Question,
                DifficultyLevel = problem.DifficultyLevel,
                CalculationTypeText = problem.CalculationType.ToString()
            };

            return Ok(ApiResponse<ProblemDto>.SuccessResponse(problemDto));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Student not found: {Message}", ex.Message);
            return NotFound(ApiResponse<ProblemDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting next problem for studentId={StudentId}, category={Category}", studentId, category);
            return StatusCode(500, ApiResponse<ProblemDto>.ErrorResponse("問題の取得中にエラーが発生しました。もう一度お試しください。"));
        }
    }

    private string GetCategoryDisplayName(Core.Models.CalculationType category)
    {
        return category switch
        {
            Core.Models.CalculationType.Addition => "足し算",
            Core.Models.CalculationType.Subtraction => "引き算",
            Core.Models.CalculationType.Multiplication => "掛け算",
            Core.Models.CalculationType.Division => "割り算",
            _ => category.ToString()
        };
    }

    /// <summary>
    /// 問題の回答を送信して採点します
    /// </summary>
    /// <param name="id">問題ID</param>
    /// <param name="studentId">学習者ID</param>
    /// <param name="answerDto">回答データ</param>
    /// <returns>採点結果（正誤、獲得ポイント、レベルアップ情報）</returns>
    /// <response code="200">回答が正常に処理されました</response>
    /// <response code="400">リクエストパラメータが不正です</response>
    /// <response code="404">指定された問題または学習者が見つかりませんでした</response>
    /// <remarks>
    /// 回答を送信すると以下の処理が実行されます:
    /// 1. 自動採点
    /// 2. ポイント付与（正解時のみ）
    /// 3. 学習記録の保存
    /// 4. レベルアップ判定
    /// 
    /// 使用例:
    /// ```
    /// POST /api/problems/123/answer?studentId=1
    /// {
    ///   "answer": 42
    /// }
    /// ```
    /// </remarks>
    [HttpPost("{id}/answer")]
    [ProducesResponseType(typeof(ApiResponse<AnswerResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AnswerResultDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AnswerResultDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AnswerResultDto>>> SubmitAnswer(
        int id,
        [FromQuery][Required] int studentId,
        [FromBody] SubmitAnswerDto answerDto)
    {
        _logger.LogInformation("SubmitAnswer called: ProblemId={ProblemId}, StudentId={StudentId}, Answer={Answer}",
            id, studentId, answerDto.Answer);

        if (studentId <= 0)
        {
            return BadRequest(ApiResponse<AnswerResultDto>.ErrorResponse("Valid student ID is required."));
        }

        if (id <= 0)
        {
            return BadRequest(ApiResponse<AnswerResultDto>.ErrorResponse("Valid problem ID is required."));
        }

        try
        {
            var (isCorrect, pointsEarned, leveledUp, newLevel) =
                await _problemService.SubmitAnswerAsync(studentId, id, answerDto.Answer);

            _logger.LogInformation("Answer result: IsCorrect={IsCorrect}, PointsEarned={PointsEarned}",
                isCorrect, pointsEarned);

            var result = new AnswerResultDto
            {
                IsCorrect = isCorrect,
                CorrectAnswer = 0, // Will be set below
                PointsEarned = pointsEarned,
                LeveledUp = leveledUp,
                NewLevel = newLevel != null ? new LevelDto
                {
                    Id = newLevel.Id,
                    LevelNumber = newLevel.LevelNumber,
                    RequiredCorrectAnswers = newLevel.RequiredCorrectAnswers,
                    MinDifficulty = newLevel.MinDifficulty,
                    MaxDifficulty = newLevel.MaxDifficulty
                } : null
            };

            // Get correct answer from problem repository
            var problemRepo = HttpContext.RequestServices.GetRequiredService<IProblemRepository>();
            var problem = await problemRepo.GetByIdAsync(id);
            if (problem != null)
            {
                result.CorrectAnswer = problem.CorrectAnswer;
            }

            return Ok(ApiResponse<AnswerResultDto>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<AnswerResultDto>.ErrorResponse(ex.Message));
        }
    }
}
