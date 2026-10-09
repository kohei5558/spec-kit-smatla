using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

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

    [HttpGet("today")]
    public async Task<ActionResult<DailyChallengeResponseDto>> GetTodaysChallenge()
    {
        try
        {
            var challenge = await _dailyChallengeService.GetTodaysChallengeAsync();

            if (challenge == null)
            {
                _logger.LogInformation("No daily challenge available for today");
                return NotFound(new { message = "今日のデイリーチャレンジはまだ用意されていません。" });
            }

            var problem = await _problemRepository.GetByIdAsync(challenge.ProblemId);
            if (problem == null)
            {
                _logger.LogError("Problem not found for challenge ID {ChallengeId}", challenge.Id);
                return NotFound(new { message = "問題が見つかりませんでした。" });
            }

            var response = new DailyChallengeResponseDto
            {
                Id = challenge.Id,
                ProblemId = challenge.ProblemId,
                Question = problem.Question,
                Difficulty = problem.DifficultyLevel,
                TargetDate = challenge.TargetDate.ToDateTime(TimeOnly.MinValue),
                BonusPoints = challenge.BonusPoints,
                IsActive = challenge.IsActive
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving today's daily challenge");
            return StatusCode(500, new { message = "デイリーチャレンジの取得に失敗しました。" });
        }
    }

    [HttpPost("{id}/answer")]
    public async Task<ActionResult<DailyChallengeAnswerResponseDto>> SubmitAnswer(
        int id,
        [FromBody] DailyChallengeAnswerRequestDto request)
    {
        try
        {
            if (request.StudentId <= 0)
            {
                return BadRequest(new { message = "無効な生徒IDです。" });
            }

            // 自分（子供）または自分の子供（保護者）の学習者のみ。他家庭の学習者は存在しない扱い
            if (!await this.CanAccessStudentAsync(_studentAccess, request.StudentId))
            {
                return NotFound(new { message = "生徒が見つかりません。" });
            }

            var result = await _dailyChallengeService.SubmitChallengeAnswerAsync(
                request.StudentId,
                id,
                request.Answer);

            var response = new DailyChallengeAnswerResponseDto
            {
                IsCorrect = result.IsCorrect,
                BonusPoints = result.BonusPoints,
                LeveledUp = result.LeveledUp,
                NewLevel = result.NewLevel?.LevelNumber
            };

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found when submitting challenge answer");
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when submitting challenge answer");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting daily challenge answer");
            return StatusCode(500, new { message = "回答の送信に失敗しました。" });
        }
    }
}
