namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// ダッシュボードサマリーDTO
/// </summary>
public class DashboardSummaryDto
{
    public int PendingRequestsCount { get; set; }
    public List<ChildDto> Children { get; set; } = new();
    public List<ExchangeRequestDto> RecentRequests { get; set; } = new();
    public int TotalRewardsCreated { get; set; }
    public int ActiveRewardsCount { get; set; }
}
