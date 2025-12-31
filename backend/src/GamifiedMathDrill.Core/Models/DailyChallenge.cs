using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// デイリーチャレンジ
/// </summary>
public class DailyChallenge
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 問題ID
    /// </summary>
    [Required]
    public int ProblemId { get; set; }

    /// <summary>
    /// 対象日（YYYY-MM-DD）
    /// </summary>
    [Required]
    public DateOnly TargetDate { get; set; }

    /// <summary>
    /// ボーナスポイント
    /// </summary>
    [Range(0, int.MaxValue)]
    public int BonusPoints { get; set; } = 20;

    /// <summary>
    /// 有効フラグ
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Problem Problem { get; set; } = null!;
}
