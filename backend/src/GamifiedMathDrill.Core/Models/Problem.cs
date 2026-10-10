using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 問題
/// </summary>
public class Problem
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 問題文（例: "23 + 45 = ?"）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// 正解の数値
    /// </summary>
    [Required]
    public int CorrectAnswer { get; set; }

    /// <summary>
    /// 正解のあまり（あまりのあるわり算のみ。それ以外は null）
    /// </summary>
    public int? CorrectRemainder { get; set; }

    /// <summary>
    /// 計算種類
    /// </summary>
    [Required]
    public CalculationType CalculationType { get; set; }

    /// <summary>
    /// 難易度（1-10）
    /// </summary>
    [Required]
    [Range(1, 10)]
    public int DifficultyLevel { get; set; }

    // Navigation properties
    public ICollection<LearningRecord> LearningRecords { get; set; } = new List<LearningRecord>();
    public ICollection<DailyChallenge> DailyChallenges { get; set; } = new List<DailyChallenge>();
}
