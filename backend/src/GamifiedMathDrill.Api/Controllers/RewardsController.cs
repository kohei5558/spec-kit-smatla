using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    [Authorize(Roles = "Parent,Child")]
    public async Task<ActionResult<IEnumerable<RewardDto>>> GetRewards(
        [FromQuery] string? category = null,
        [FromQuery] int? maxPoints = null)
    {
        try
        {
            var rewards = await _rewardService.GetRewardsAsync(category, maxPoints);

            // 子供ロールの場合は有効な景品のみ表示
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (userRole == "Child")
            {
                rewards = rewards.Where(r => r.IsActive);
            }

            var responseDtos = rewards.Select(r => new RewardDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description ?? string.Empty,
                RequiredPoints = r.RequiredPoints,
                Category = r.Category,
                ImageUrl = r.ImageUrl,
                Stock = r.Stock,
                IsPhysical = r.IsPhysical,
                CreatedBy = r.CreatedBy,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                IsActive = r.IsActive,
                RowVersion = r.RowVersion
            });

            return Ok(responseDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rewards");
            return StatusCode(500, new { message = "景品の取得に失敗しました。" });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Parent,Child")]
    public async Task<ActionResult<RewardDto>> GetReward(int id)
    {
        try
        {
            var reward = await _rewardService.GetRewardByIdAsync(id);
            if (reward == null)
            {
                return NotFound(new { message = "景品が見つかりません。" });
            }

            var dto = new RewardDto
            {
                Id = reward.Id,
                Name = reward.Name,
                Description = reward.Description ?? string.Empty,
                RequiredPoints = reward.RequiredPoints,
                Category = reward.Category,
                ImageUrl = reward.ImageUrl,
                Stock = reward.Stock,
                IsPhysical = reward.IsPhysical,
                CreatedBy = reward.CreatedBy,
                CreatedAt = reward.CreatedAt,
                UpdatedAt = reward.UpdatedAt,
                IsActive = reward.IsActive,
                RowVersion = reward.RowVersion
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reward {RewardId}", id);
            return StatusCode(500, new { message = "景品の取得に失敗しました。" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<RewardDto>> CreateReward([FromForm] CreateRewardRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "ユーザー情報が取得できません。" });
            }

            var reward = new Reward
            {
                Name = request.Name,
                Description = request.Description,
                RequiredPoints = request.RequiredPoints,
                Category = request.Category,
                IsPhysical = request.IsPhysical,
                Stock = request.Stock
            };

            Reward createdReward;
            if (request.Image != null)
            {
                using var stream = request.Image.OpenReadStream();
                createdReward = await _rewardService.CreateRewardAsync(reward, stream, request.Image.FileName, request.Image.ContentType, userId);
            }
            else
            {
                createdReward = await _rewardService.CreateRewardAsync(reward, null, null, null, userId);
            }

            var dto = new RewardDto
            {
                Id = createdReward.Id,
                Name = createdReward.Name,
                Description = createdReward.Description ?? string.Empty,
                RequiredPoints = createdReward.RequiredPoints,
                Category = createdReward.Category,
                ImageUrl = createdReward.ImageUrl,
                Stock = createdReward.Stock,
                IsPhysical = createdReward.IsPhysical,
                CreatedBy = createdReward.CreatedBy,
                CreatedAt = createdReward.CreatedAt,
                UpdatedAt = createdReward.UpdatedAt,
                IsActive = createdReward.IsActive,
                RowVersion = createdReward.RowVersion
            };

            return CreatedAtAction(nameof(GetReward), new { id = dto.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating reward");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reward");
            return StatusCode(500, new { message = "景品の作成に失敗しました。" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<RewardDto>> UpdateReward(int id, [FromForm] UpdateRewardRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "ユーザー情報が取得できません。" });
            }

            var reward = new Reward
            {
                Name = request.Name,
                Description = request.Description,
                RequiredPoints = request.RequiredPoints,
                Category = request.Category,
                IsPhysical = request.IsPhysical,
                Stock = request.Stock,
                IsActive = request.IsActive,
                RowVersion = request.RowVersion
            };

            Reward updatedReward;
            if (request.Image != null)
            {
                using var stream = request.Image.OpenReadStream();
                updatedReward = await _rewardService.UpdateRewardAsync(id, reward, stream, request.Image.FileName, request.Image.ContentType, userId);
            }
            else
            {
                updatedReward = await _rewardService.UpdateRewardAsync(id, reward, null, null, null, userId);
            }

            var dto = new RewardDto
            {
                Id = updatedReward.Id,
                Name = updatedReward.Name,
                Description = updatedReward.Description ?? string.Empty,
                RequiredPoints = updatedReward.RequiredPoints,
                Category = updatedReward.Category,
                ImageUrl = updatedReward.ImageUrl,
                Stock = updatedReward.Stock,
                IsPhysical = updatedReward.IsPhysical,
                CreatedBy = updatedReward.CreatedBy,
                CreatedAt = updatedReward.CreatedAt,
                UpdatedAt = updatedReward.UpdatedAt,
                IsActive = updatedReward.IsActive,
                RowVersion = updatedReward.RowVersion
            };

            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating reward {RewardId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reward {RewardId}", id);
            return StatusCode(500, new { message = "景品の更新に失敗しました。" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult> DeleteReward(int id)
    {
        try
        {
            await _rewardService.DeleteRewardAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reward {RewardId}", id);
            return StatusCode(500, new { message = "景品の削除に失敗しました。" });
        }
    }

    [HttpPost("{id}/exchange")]
    [Authorize(Roles = "Child")]
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
    [Authorize(Roles = "Parent,Child")]
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
