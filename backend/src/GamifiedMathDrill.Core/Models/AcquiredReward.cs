using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 獲得景品
/// </summary>
public class AcquiredReward
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 児童ID
    /// </summary>
    [Required]
    public int StudentId { get; set; }

    /// <summary>
    /// 景品ID
    /// </summary>
    [Required]
    public int RewardId { get; set; }

    /// <summary>
    /// 消費ポイント
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int PointsSpent { get; set; }

    /// <summary>
    /// 獲得日時
    /// </summary>
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Student Student { get; set; } = null!;
    public Reward Reward { get; set; } = null!;
}
