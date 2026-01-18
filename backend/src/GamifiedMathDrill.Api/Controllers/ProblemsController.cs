using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models.DTOs;
using GamifiedMathDrill.Core.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProblemsController : ControllerBase
{
    private readonly IProblemService _problemService;
    private readonly ILogger<ProblemsController> _logger;

    public ProblemsController(IProblemService problemService, ILogger<ProblemsController> logger)
    {
        _problemService = problemService;
        _logger = logger;
    }

    [HttpGet("next")]
    public async Task<ActionResult<ApiResponse<ProblemDto>>> GetNext(
        [FromQuery] int studentId,
        [FromQuery] List<int>? excludeRecentIds = null,
        [FromQuery] Core.Models.CalculationType? category = null)
    {
        if (studentId <= 0)
        {
            return BadRequest(ApiResponse<ProblemDto>.ErrorResponse("Valid student ID is required."));
        }

        try
        {
            var problem = await _problemService.GetNextProblemAsync(studentId, excludeRecentIds, category);
            if (problem == null)
            {
                var message = category.HasValue 
                    ? $"No available problems found for category {category.Value}."
                    : "No available problems found.";
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
            return NotFound(ApiResponse<ProblemDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("{id}/answer")]
    public async Task<ActionResult<ApiResponse<AnswerResultDto>>> SubmitAnswer(
        int id,
        [FromQuery] int studentId,
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
