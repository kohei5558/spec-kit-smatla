using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 保護者ダッシュボードサービスインターフェース
/// </summary>
public interface IParentDashboardService
{
    /// <summary>
    /// ダッシュボードサマリーを取得
    /// </summary>
    Task<DashboardSummary> GetDashboardSummaryAsync(string parentUserId);

    /// <summary>
    /// 未承認の交換申請数を取得
    /// </summary>
    Task<int> GetPendingRequestsCountAsync(string parentUserId);

    /// <summary>
    /// 保護者の子供一覧を取得
    /// </summary>
    Task<List<Student>> GetChildrenAsync(string parentUserId);

    /// <summary>
    /// 保護者の統計情報を取得
    /// </summary>
    Task<ParentStatistics> GetStatisticsAsync(string parentUserId);
}

/// <summary>
/// ダッシュボードサマリー
/// </summary>
public class DashboardSummary
{
    public int PendingRequestsCount { get; set; }
    public List<Student> Children { get; set; } = new();
    public List<ExchangeRequest> RecentRequests { get; set; } = new();
    public int TotalRewardsCreated { get; set; }
    public int ActiveRewardsCount { get; set; }
}

/// <summary>
/// 保護者統計情報
/// </summary>
public class ParentStatistics
{
    public int TotalChildren { get; set; }
    public int TotalProblemsCompleted { get; set; }
    public int TotalPointsEarned { get; set; }
    public int TotalExchangesApproved { get; set; }
    public MonthlyStats? MonthlyStats { get; set; }
    public List<CategoryBreakdown> CategoryBreakdown { get; set; } = new();
    public List<RewardCategoryBreakdown> RewardCategoryBreakdown { get; set; } = new();
}

/// <summary>
/// 月次統計
/// </summary>
public class MonthlyStats
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int ProblemsCompleted { get; set; }
    public int PointsEarned { get; set; }
    public int ExchangesApproved { get; set; }
}

/// <summary>
/// カテゴリー別内訳
/// </summary>
public class CategoryBreakdown
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProblemsCompleted { get; set; }
    public int PointsEarned { get; set; }
}

/// <summary>
/// 景品カテゴリー別内訳
/// </summary>
public class RewardCategoryBreakdown
{
    public string Category { get; set; } = string.Empty;
    public int TotalExchanges { get; set; }
    public int TotalPointsUsed { get; set; }
}
