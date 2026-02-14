using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Infrastructure.Data;
using GamifiedMathDrill.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GamifiedMathDrill.Api.Controllers;

/// <summary>
/// 交換申請API
/// </summary>
[ApiController]
[Route("api/exchange-requests")]
public class ExchangeRequestsController : ControllerBase
{
    private readonly IExchangeRequestService _exchangeRequestService;
    private readonly IExchangeRequestRepository _exchangeRequestRepository;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ExchangeRequestsController> _logger;

    public ExchangeRequestsController(
        IExchangeRequestService exchangeRequestService,
        IExchangeRequestRepository exchangeRequestRepository,
        ApplicationDbContext db,
        ILogger<ExchangeRequestsController> logger)
    {
        _exchangeRequestService = exchangeRequestService;
        _exchangeRequestRepository = exchangeRequestRepository;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 交換申請を作成（子供用）
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Child")]
    public async Task<ActionResult<ExchangeRequestDto>> CreateRequest([FromBody] CreateExchangeRequestRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "ユーザー情報が取得できません。" });
            }

            // UserIdからStudentIdを取得（DisplayNameで関連付け）
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { message = "ユーザーが見つかりません。" });
            }

            var student = await _db.Students.FirstOrDefaultAsync(s => s.Name == user.DisplayName);
            if (student == null)
            {
                _logger.LogWarning($"Student not found for user {user.DisplayName} (UserId: {userId})");
                return NotFound(new { message = $"生徒情報が見つかりません。ユーザー名: {user.DisplayName}" });
            }

            var studentId = student.Id;
            _logger.LogInformation($"Found student {studentId} for user {user.DisplayName}");

            _logger.LogInformation("Creating exchange request for student {StudentId}, reward {RewardId}", studentId, request.RewardId);

            var exchangeRequest = await _exchangeRequestService.CreateRequestAsync(studentId, request.RewardId);

            _logger.LogInformation("ExchangeRequest created: {RequestId} for student {StudentId}", exchangeRequest.Id, exchangeRequest.StudentId);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var dto = new ExchangeRequestDto
            {
                Id = exchangeRequest.Id,
                StudentId = exchangeRequest.StudentId,
                StudentName = exchangeRequest.Student?.Name ?? "",
                RewardId = exchangeRequest.RewardId,
                RewardName = exchangeRequest.Reward?.Name ?? "",
                RewardImageUrl = string.IsNullOrEmpty(exchangeRequest.Reward?.ImageUrl) ? null : $"{baseUrl}{exchangeRequest.Reward!.ImageUrl}",
                RequiredPoints = exchangeRequest.RequiredPoints,
                Status = exchangeRequest.Status,
                RequestedAt = exchangeRequest.RequestedAt
            };

            return CreatedAtAction(nameof(GetRequest), new { id = dto.Id }, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exchange request");
            return StatusCode(500, new { message = "交換申請の作成に失敗しました。" });
        }
    }

    /// <summary>
    /// 自分の交換申請一覧を取得（子供用）
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Child")]
    public async Task<ActionResult<IEnumerable<ExchangeRequestDto>>> GetMyRequests()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "ユーザー情報が取得できません。" });
            }

            // UserIdからStudentIdを取得
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { message = "ユーザーが見つかりません。" });
            }

            var student = await _db.Students.FirstOrDefaultAsync(s => s.Name == user.DisplayName);
            if (student == null)
            {
                return NotFound(new { message = "生徒情報が見つかりません。" });
            }

            var studentId = student.Id;

            var requests = await _exchangeRequestService.GetRequestsByStudentAsync(studentId);

            var baseUrlMy = $"{Request.Scheme}://{Request.Host}";
            var dtos = requests.Select(er => new ExchangeRequestDto
            {
                Id = er.Id,
                StudentId = er.StudentId,
                StudentName = er.Student?.Name ?? "",
                RewardId = er.RewardId,
                RewardName = er.Reward?.Name ?? "",
                RewardImageUrl = string.IsNullOrEmpty(er.Reward?.ImageUrl) ? null : $"{baseUrlMy}{er.Reward!.ImageUrl}",
                RequiredPoints = er.RequiredPoints,
                Status = er.Status,
                RequestedAt = er.RequestedAt,
                ApprovedAt = er.ApprovedAt,
                RejectedAt = er.RejectedAt,
                CancelledAt = er.CancelledAt,
                RejectionReason = er.RejectionReason,
                ParentNote = er.ParentNote
            });

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exchange requests");
            return StatusCode(500, new { message = "交換申請の取得に失敗しました。" });
        }
    }

    /// <summary>
    /// 交換申請の詳細を取得
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Parent,Child")]
    public async Task<ActionResult<ExchangeRequestDto>> GetRequest(int id)
    {
        try
        {
            var request = await _exchangeRequestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound(new { message = "交換申請が見つかりません。" });
            }

            var dto = new ExchangeRequestDto
            {
                Id = request.Id,
                StudentId = request.StudentId,
                StudentName = request.Student?.Name ?? "",
                RewardId = request.RewardId,
                RewardName = request.Reward?.Name ?? "",
                RewardImageUrl = string.IsNullOrEmpty(request.Reward?.ImageUrl) ? null : $"{Request.Scheme}://{Request.Host}{request.Reward!.ImageUrl}",
                RequiredPoints = request.RequiredPoints,
                Status = request.Status,
                RequestedAt = request.RequestedAt,
                ApprovedAt = request.ApprovedAt,
                RejectedAt = request.RejectedAt,
                CancelledAt = request.CancelledAt,
                ApprovedBy = request.ApprovedBy,
                RejectionReason = request.RejectionReason,
                ParentNote = request.ParentNote
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exchange request {RequestId}", id);
            return StatusCode(500, new { message = "交換申請の取得に失敗しました。" });
        }
    }

    /// <summary>
    /// 交換申請をキャンセル（子供用）
    /// </summary>
    [HttpPut("{id}/cancel")]
    [Authorize(Roles = "Child")]
    public async Task<ActionResult> CancelRequest(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "ユーザー情報が取得できません。" });
            }

            // UserIdからStudentIdを取得
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { message = "ユーザーが見つかりません。" });
            }

            var student = await _db.Students.FirstOrDefaultAsync(s => s.Name == user.DisplayName);
            if (student == null)
            {
                return NotFound(new { message = "生徒情報が見つかりません。" });
            }

            var studentId = student.Id;

            await _exchangeRequestService.CancelRequestAsync(id, studentId);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling exchange request {RequestId}", id);
            return StatusCode(500, new { message = "交換申請のキャンセルに失敗しました。" });
        }
    }

    /// <summary>
    /// 交換申請を承認（保護者用）
    /// </summary>
    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<ExchangeRequestDto>> ApproveRequest(int id, [FromBody] ApproveRequestRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "ユーザー情報が取得できません。" });
            }

            var exchangeRequest = await _exchangeRequestService.ApproveRequestAsync(id, userId, request.ParentNote);

            var dto = new ExchangeRequestDto
            {
                Id = exchangeRequest.Id,
                StudentId = exchangeRequest.StudentId,
                StudentName = exchangeRequest.Student?.Name ?? "",
                RewardId = exchangeRequest.RewardId,
                RewardName = exchangeRequest.Reward?.Name ?? "",
                RewardImageUrl = string.IsNullOrEmpty(exchangeRequest.Reward?.ImageUrl) ? null : $"{Request.Scheme}://{Request.Host}{exchangeRequest.Reward!.ImageUrl}",
                RequiredPoints = exchangeRequest.RequiredPoints,
                Status = exchangeRequest.Status,
                RequestedAt = exchangeRequest.RequestedAt,
                ApprovedAt = exchangeRequest.ApprovedAt,
                ApprovedBy = exchangeRequest.ApprovedBy,
                ParentNote = exchangeRequest.ParentNote
            };

            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving exchange request {RequestId}", id);
            return StatusCode(500, new { message = "交換申請の承認に失敗しました。" });
        }
    }

    /// <summary>
    /// 交換申請を却下（保護者用）
    /// </summary>
    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<ExchangeRequestDto>> RejectRequest(int id, [FromBody] RejectRequestRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var exchangeRequest = await _exchangeRequestService.RejectRequestAsync(id, request.Reason, request.ParentNote);

            var dto = new ExchangeRequestDto
            {
                Id = exchangeRequest.Id,
                StudentId = exchangeRequest.StudentId,
                StudentName = exchangeRequest.Student?.Name ?? "",
                RewardId = exchangeRequest.RewardId,
                RewardName = exchangeRequest.Reward?.Name ?? "",
                RewardImageUrl = string.IsNullOrEmpty(exchangeRequest.Reward?.ImageUrl) ? null : $"{Request.Scheme}://{Request.Host}{exchangeRequest.Reward!.ImageUrl}",
                RequiredPoints = exchangeRequest.RequiredPoints,
                Status = exchangeRequest.Status,
                RequestedAt = exchangeRequest.RequestedAt,
                RejectedAt = exchangeRequest.RejectedAt,
                RejectionReason = exchangeRequest.RejectionReason,
                ParentNote = exchangeRequest.ParentNote
            };

            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting exchange request {RequestId}", id);
            return StatusCode(500, new { message = "交換申請の却下に失敗しました。" });
        }
    }

    /// <summary>
    /// 全交換申請を取得（保護者用）
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Parent")]
    public async Task<ActionResult<IEnumerable<ExchangeRequestDto>>> GetAllRequests([FromQuery] string? status = null)
    {
        try
        {
            // TODO: 保護者の子供のみフィルタリング
            var requests = await _exchangeRequestRepository.GetAllAsync();

            var baseUrlAll = $"{Request.Scheme}://{Request.Host}";
            var dtos = requests.Select(er => new ExchangeRequestDto
            {
                Id = er.Id,
                StudentId = er.StudentId,
                StudentName = er.Student?.Name ?? "",
                RewardId = er.RewardId,
                RewardName = er.Reward?.Name ?? "",
                RewardImageUrl = string.IsNullOrEmpty(er.Reward?.ImageUrl) ? null : $"{baseUrlAll}{er.Reward!.ImageUrl}",
                RequiredPoints = er.RequiredPoints,
                Status = er.Status,
                RequestedAt = er.RequestedAt,
                ApprovedAt = er.ApprovedAt,
                RejectedAt = er.RejectedAt,
                CancelledAt = er.CancelledAt,
                ApprovedBy = er.ApprovedBy,
                RejectionReason = er.RejectionReason,
                ParentNote = er.ParentNote
            });

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all exchange requests");
            return StatusCode(500, new { message = "交換申請の取得に失敗しました。" });
        }
    }
}
