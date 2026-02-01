using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GamifiedMathDrill.Api.Controllers;

[ApiController]
[Route("api/parent")]
[Authorize(Roles = "Parent")]
public class ParentDashboardController : ControllerBase
{
    private readonly IParentDashboardService _dashboardService;
    private readonly ILogger<ParentDashboardController> _logger;

    public ParentDashboardController(
        IParentDashboardService dashboardService,
        ILogger<ParentDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// ダッシュボードサマリーを取得
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboard()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "ユーザーIDが見つかりません。" });
            }

            var summary = await _dashboardService.GetDashboardSummaryAsync(userId);

            var dto = new DashboardSummaryDto
            {
                PendingRequestsCount = summary.PendingRequestsCount,
                Children = summary.Children.Select(c => new ChildDto
                {
                    Id = c.Id,
                    DisplayName = c.Name,
                    AvatarUrl = c.AvatarUrl,
                    TotalPoints = c.TotalPoints,
                    PendingRequestsCount = 0, // TODO: 子供ごとの未承認数
                    TotalProblemsCompleted = c.TotalProblemsCompleted ?? 0,
                    AccuracyRate = c.AccuracyRate ?? 0,
                    CreatedAt = c.CreatedAt,
                    IsActive = true
                }).ToList(),
                RecentRequests = summary.RecentRequests.Select(r => new ExchangeRequestDto
                {
                    Id = r.Id,
                    StudentId = r.StudentId,
                    StudentName = r.Student?.Name ?? "",
                    RewardId = r.RewardId,
                    RewardName = r.Reward?.Name ?? "",
                    RewardImageUrl = r.Reward?.ImageUrl,
                    Status = r.Status,
                    RequiredPoints = r.RequiredPoints,
                    RequestedAt = r.RequestedAt,
                    ApprovedAt = r.ApprovedAt,
                    RejectedAt = r.RejectedAt,
                    CancelledAt = r.CancelledAt,
                    ApprovedBy = r.ApprovedBy,
                    RejectionReason = r.RejectionReason,
                    ParentNote = r.ParentNote
                }).ToList(),
                TotalRewardsCreated = summary.TotalRewardsCreated,
                ActiveRewardsCount = summary.ActiveRewardsCount
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return StatusCode(500, new { message = "ダッシュボード情報の取得に失敗しました。" });
        }
    }

    /// <summary>
    /// 未承認の交換申請数を取得
    /// </summary>
    [HttpGet("pending-requests")]
    public async Task<ActionResult<int>> GetPendingRequestsCount()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "ユーザーIDが見つかりません。" });
            }

            var count = await _dashboardService.GetPendingRequestsCountAsync(userId);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending requests count");
            return StatusCode(500, new { message = "未承認申請数の取得に失敗しました。" });
        }
    }

    /// <summary>
    /// 保護者の子供一覧を取得
    /// </summary>
    [HttpGet("children")]
    public async Task<ActionResult<List<ChildDto>>> GetChildren()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "ユーザーIDが見つかりません。" });
            }

            var children = await _dashboardService.GetChildrenAsync(userId);

            var dtos = children.Select(c => new ChildDto
            {
                Id = c.Id,
                DisplayName = c.Name,
                AvatarUrl = c.AvatarUrl,
                TotalPoints = c.TotalPoints,
                PendingRequestsCount = 0, // TODO: 子供ごとの未承認数
                TotalProblemsCompleted = c.TotalProblemsCompleted ?? 0,
                AccuracyRate = c.AccuracyRate ?? 0,
                CreatedAt = c.CreatedAt,
                IsActive = true
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving children");
            return StatusCode(500, new { message = "子供一覧の取得に失敗しました。" });
        }
    }

    /// <summary>
    /// 保護者の統計情報を取得
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ParentStatisticsDto>> GetStatistics()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "ユーザーIDが見つかりません。" });
            }

            var stats = await _dashboardService.GetStatisticsAsync(userId);

            var dto = new ParentStatisticsDto
            {
                TotalChildren = stats.TotalChildren,
                TotalProblemsCompleted = stats.TotalProblemsCompleted,
                TotalPointsEarned = stats.TotalPointsEarned,
                TotalExchangesApproved = stats.TotalExchangesApproved,
                MonthlyStats = stats.MonthlyStats != null ? new MonthlyStatsDto
                {
                    Month = stats.MonthlyStats.Month,
                    Year = stats.MonthlyStats.Year,
                    ProblemsCompleted = stats.MonthlyStats.ProblemsCompleted,
                    PointsEarned = stats.MonthlyStats.PointsEarned,
                    ExchangesApproved = stats.MonthlyStats.ExchangesApproved
                } : null,
                CategoryBreakdown = stats.CategoryBreakdown.Select(cb => new CategoryBreakdownDto
                {
                    CategoryName = cb.CategoryName,
                    ProblemsCompleted = cb.ProblemsCompleted,
                    PointsEarned = cb.PointsEarned
                }).ToList(),
                RewardCategoryBreakdown = stats.RewardCategoryBreakdown.Select(rcb => new RewardCategoryBreakdownDto
                {
                    Category = rcb.Category,
                    TotalExchanges = rcb.TotalExchanges,
                    TotalPointsUsed = rcb.TotalPointsUsed
                }).ToList()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(500, new { message = "統計情報の取得に失敗しました。" });
        }
    }
}
