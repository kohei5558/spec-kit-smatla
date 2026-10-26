namespace GamifiedMathDrill.Core.DTOs;

/// <summary>
/// 子供の学習統計サマリー
/// </summary>
public class ChildLearningStatsDto
{
    /// <summary>
    /// 総問題数
    /// </summary>
    public int TotalProblems { get; set; }

    /// <summary>
    /// 正答数
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// 正答率（パーセント）
    /// </summary>
    public decimal AccuracyRate { get; set; }

    /// <summary>
    /// 総ポイント
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// 連続学習日数
    /// </summary>
    public int ConsecutiveDays { get; set; }

    /// <summary>
    /// 最終学習日
    /// </summary>
    public DateTime? LastStudyDate { get; set; }

    /// <summary>
    /// 過去7日間のアクティビティ
    /// </summary>
    public List<DailyActivity> RecentActivity { get; set; } = new();

    /// <summary>
    /// 計算の種類ごとのレベル・回答数・正答率（すべての種類、014）
    /// </summary>
    public List<CalculationTypeStats> Levels { get; set; } = new();
}

/// <summary>
/// 計算の種類ごとのレベルと成績（014）
/// </summary>
public class CalculationTypeStats
{
    /// <summary>
    /// 計算の種類（Addition など）
    /// </summary>
    public string CalculationType { get; set; } = string.Empty;

    /// <summary>
    /// この種類のレベル
    /// </summary>
    public int LevelNumber { get; set; }

    /// <summary>
    /// この種類の回答数
    /// </summary>
    public int TotalProblems { get; set; }

    /// <summary>
    /// この種類の正答率（パーセント、回答が無ければ 0）
    /// </summary>
    public decimal AccuracyRate { get; set; }
}

/// <summary>
/// 1日のアクティビティ
/// </summary>
public class DailyActivity
{
    /// <summary>
    /// 日付
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 解いた問題数
    /// </summary>
    public int ProblemsCount { get; set; }
}
