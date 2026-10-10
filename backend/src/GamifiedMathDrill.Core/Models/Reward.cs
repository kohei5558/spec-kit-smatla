using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 景品（実物景品対応）
/// </summary>
public class Reward
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 景品名（例: "うまい棒", "ポケモンカード"）
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

    /// <summary>
    /// 在庫数（nullの場合は無制限）
    /// </summary>
    public int? Stock { get; set; }

    /// <summary>
    /// 実物景品かどうか
    /// </summary>
    public bool IsPhysical { get; set; } = true;

    /// <summary>
    /// この景品を持つ家庭の保護者ユーザーID（null の景品はどの家庭にも表示しない）
    /// </summary>
    [MaxLength(450)]
    public string? ParentId { get; set; }

    /// <summary>
    /// 作成者（保護者のユーザーID）
    /// </summary>
    [MaxLength(450)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// アクティブ状態
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 楽観的ロック用
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public ICollection<AcquiredReward> AcquiredRewards { get; set; } = new List<AcquiredReward>();
}
