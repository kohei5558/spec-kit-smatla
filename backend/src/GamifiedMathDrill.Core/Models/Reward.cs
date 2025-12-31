using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 景品
/// </summary>
public class Reward
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 景品名（例: "金メダルバッジ"）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 説明
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 必要ポイント
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int RequiredPoints { get; set; }

    /// <summary>
    /// カテゴリー
    /// </summary>
    [Required]
    public RewardCategory Category { get; set; }

    /// <summary>
    /// 画像URL
    /// </summary>
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    // Navigation properties
    public ICollection<AcquiredReward> AcquiredRewards { get; set; } = new List<AcquiredReward>();
}
