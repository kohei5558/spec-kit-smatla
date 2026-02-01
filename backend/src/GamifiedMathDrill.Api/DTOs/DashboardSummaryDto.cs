using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 保護者ダッシュボードサマリーDTO
/// </summary>
public class DashboardSummaryDto
{
    public int PendingRequestsCount { get; set; }
    public List<ChildDto> Children { get; set; } = new();
    public List<ExchangeRequestDto> RecentRequests { get; set; } = new();
    public int TotalRewardsCreated { get; set; }
    public int ActiveRewardsCount { get; set; }
}
