using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RewardsController : ControllerBase
{
    private readonly IRewardService _rewardService;
    private readonly ILogger<RewardsController> _logger;

    public RewardsController(IRewardService rewardService, ILogger<RewardsController> logger)
    {
        _rewardService = rewardService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RewardResponseDto>>> GetRewards(
        [FromQuery] string? category = null,
        [FromQuery] int? maxPoints = null)
    {
        try
        {
            var rewards = await _rewardService.GetRewardsAsync(category, maxPoints);

            var responseDtos = rewards.Select(r => new RewardResponseDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                RequiredPoints = r.RequiredPoints,
                Category = r.Category.ToString(),
                ImageUrl = r.ImageUrl
            });

            return Ok(responseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rewards");
            return StatusCode(500, new { message = "景品の取得に失敗しました。" });
        }
    }

    [HttpPost("{id}/exchange")]
    public async Task<ActionResult<ExchangeRewardResponseDto>> ExchangeReward(
        int id,
        [FromBody] ExchangeRewardRequestDto request)
    {
        try
        {
            if (request.StudentId <= 0)
            {
                return BadRequest(new { message = "無効な生徒IDです。" });
            }

            var (success, message, acquiredReward) = await _rewardService.ExchangeRewardAsync(request.StudentId, id);

            if (!success)
            {
                // Check if it's an insufficient points error
                if (message.Contains("ポイントが足りません"))
                {
                    return BadRequest(new { message });
                }

                return NotFound(new { message });
            }

            var response = new ExchangeRewardResponseDto
            {
                Success = true,
                Message = message,
                AcquiredRewardId = acquiredReward!.Id,
                PointsSpent = acquiredReward.PointsSpent,
                AcquiredAt = acquiredReward.AcquiredAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging reward {RewardId} for student {StudentId}", id, request.StudentId);
            return StatusCode(500, new { message = "景品の交換に失敗しました。" });
        }
    }

    [HttpGet("acquired")]
    public async Task<ActionResult<IEnumerable<AcquiredRewardResponseDto>>> GetAcquiredRewards(
        [FromQuery] int studentId)
    {
        try
        {
            if (studentId <= 0)
            {
                return BadRequest(new { message = "無効な生徒IDです。" });
            }

            var acquiredRewards = await _rewardService.GetAcquiredRewardsAsync(studentId);

            var responseDtos = acquiredRewards.Select(ar => new AcquiredRewardResponseDto
            {
                Id = ar.Id,
                StudentId = ar.StudentId,
                RewardId = ar.RewardId,
                RewardName = ar.Reward?.Name ?? "",
                RewardDescription = ar.Reward?.Description ?? "",
                RewardCategory = ar.Reward?.Category.ToString() ?? "",
                RewardImageUrl = ar.Reward?.ImageUrl ?? "",
                PointsSpent = ar.PointsSpent,
                AcquiredAt = ar.AcquiredAt
            });

            return Ok(responseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving acquired rewards for student {StudentId}", studentId);
            return StatusCode(500, new { message = "獲得景品の取得に失敗しました。" });
        }
    }
}
