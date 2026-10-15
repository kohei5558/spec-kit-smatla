using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

/// <summary>
/// デイリーチャレンジ（010）。学習者ごと・日ごとに1問、1回だけ答えられる
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Parent,Child")]
public class DailyChallengesController : ControllerBase
{
    private readonly IDailyChallengeService _dailyChallengeService;
    private readonly IProblemRepository _problemRepository;
    private readonly IStudentAccessService _studentAccess;
    private readonly ILogger<DailyChallengesController> _logger;

    public DailyChallengesController(
        IDailyChallengeService dailyChallengeService,
        IProblemRepository problemRepository,
        IStudentAccessService studentAccess,
        ILogger<DailyChallengesController> logger)
    {
        _studentAccess = studentAccess;
        _dailyChallengeService = dailyChallengeService;
        _problemRepository = problemRepository;
        _logger = logger;
    }

    /// <summary>
    /// 学習者の今日のチャレンジ（まだなければ作る）
    /// </summary>
    [HttpGet("today")]
    [ProducesResponseType(typeof(DailyChallengeResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyChallengeResponseDto>> GetTodaysChallenge([FromQuery] int studentId)
    {
        if (studentId <= 0)
        {
            return BadRequest(new { message = "無効な生徒IDです。" });
        }

        // 自分（子供）または自分の子供（保護者）の学習者のみ。他家庭の学習者は存在しない扱い
        if (!await this.CanAccessStudentAsync(_studentAccess, studentId))
        {
            return NotFound(new { message = "生徒が見つかりません。" });
        }

        var challenge = await _dailyChallengeService.GetOrCreateTodaysChallengeAsync(studentId);
        var problem = challenge == null ? null : await _problemRepository.GetByIdAsync(challenge.ProblemId);
        if (challenge == null || problem == null)
        {
            _logger.LogInformation("No daily challenge available for student {StudentId}", studentId);
            return NotFound(new { message = "今日のデイリーチャレンジはまだ用意されていません。" });
        }

        var answered = challenge.AnsweredAt != null;
        return Ok(new DailyChallengeResponseDto
        {
            Id = challenge.Id,
            Question = problem.Question,
            Difficulty = problem.DifficultyLevel,
            TargetDate = challenge.TargetDate,
            BonusPoints = challenge.BonusPoints,
            IsAnswered = answered,
            IsCorrect = challenge.IsCorrect,
            StudentAnswer = challenge.StudentAnswer,
            // 答える前に正解を返さない
            CorrectAnswer = answered ? problem.CorrectAnswer : null
        });
    }

    /// <summary>
    /// チャレンジに答える。子供本人だけ・1日1回（2回目以降は 409）
    /// </summary>
    [HttpPost("{id}/answer")]
    [Authorize(Roles = "Child")]
    [ProducesResponseType(typeof(DailyChallengeAnswerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DailyChallengeAnswerResponseDto>> SubmitAnswer(
        int id,
        [FromBody] DailyChallengeAnswerRequestDto? request)
    {
        if (request == null || request.StudentId <= 0)
        {
            return BadRequest(new { message = "無効な生徒IDです。" });
        }

        if (!await this.CanAccessStudentAsync(_studentAccess, request.StudentId))
        {
            return NotFound(new { message = "生徒が見つかりません。" });
        }

        var result = await _dailyChallengeService.SubmitChallengeAnswerAsync(request.StudentId, id, request.Answer);
        return result.Status switch
        {
            DailyChallengeAnswerStatus.NotFound => NotFound(new { message = "チャレンジが見つかりません。" }),
            DailyChallengeAnswerStatus.AlreadyAnswered => Conflict(new { message = "今日のチャレンジはもう答えたよ。また明日ちょうせんしてね。" }),
            _ => Ok(new DailyChallengeAnswerResponseDto
            {
                IsCorrect = result.IsCorrect,
                BonusPoints = result.BonusPoints,
                CorrectAnswer = result.CorrectAnswer
            })
        };
    }
}
