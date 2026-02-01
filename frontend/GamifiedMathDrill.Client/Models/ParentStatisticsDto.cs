namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// 保護者統計情報DTO
/// </summary>
public class ParentStatisticsDto
{
    public int TotalChildren { get; set; }
    public int TotalProblemsCompleted { get; set; }
    public int TotalPointsEarned { get; set; }
    public int TotalExchangesApproved { get; set; }
    public MonthlyStatsDto? MonthlyStats { get; set; }
    public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
    public List<RewardCategoryBreakdownDto> RewardCategoryBreakdown { get; set; } = new();
}

/// <summary>
/// 月次統計DTO
/// </summary>
public class MonthlyStatsDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public int ProblemsCompleted { get; set; }
    public int PointsEarned { get; set; }
    public int ExchangesApproved { get; set; }
}

/// <summary>
/// カテゴリー別内訳DTO
/// </summary>
public class CategoryBreakdownDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProblemsCompleted { get; set; }
    public int PointsEarned { get; set; }
}

/// <summary>
/// 景品カテゴリー別内訳DTO
/// </summary>
public class RewardCategoryBreakdownDto
{
    public string Category { get; set; } = string.Empty;
    public int TotalExchanges { get; set; }
    public int TotalPointsUsed { get; set; }
}
