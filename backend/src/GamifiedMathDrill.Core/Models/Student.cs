using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 児童
/// </summary>
public class Student
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 名前（ニックネーム可）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 現在のレベルID
    /// </summary>
    public int CurrentLevelId { get; set; } = 1;

    /// <summary>
    /// 獲得ポイント総数
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TotalPoints { get; set; } = 0;

    /// <summary>
    /// 連続学習日数
    /// </summary>
    [Range(0, int.MaxValue)]
    public int ConsecutiveDays { get; set; } = 0;

    /// <summary>
    /// 解いた問題総数
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TotalProblems { get; set; } = 0;

    /// <summary>
    /// 正解数
    /// </summary>
    [Range(0, int.MaxValue)]
    public int CorrectAnswers { get; set; } = 0;

    /// <summary>
    /// アカウント作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最終ログイン日時
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// 親アカウントのユーザーID (ASP.NET Core Identity)
    /// </summary>
    public string? ParentUserId { get; set; }

    /// <summary>
    /// アバターURL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 完了した問題数（計算プロパティ代替）
    /// </summary>
    public int? TotalProblemsCompleted { get; set; }

    /// <summary>
    /// 正答率（計算プロパティ代替）
    /// </summary>
    public decimal? AccuracyRate { get; set; }

    // Navigation properties
    public Level CurrentLevel { get; set; } = null!;
    public ICollection<LearningRecord> LearningRecords { get; set; } = new List<LearningRecord>();
    public ICollection<AcquiredReward> AcquiredRewards { get; set; } = new List<AcquiredReward>();
}
