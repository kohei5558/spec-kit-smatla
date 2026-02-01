using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 景品交換申請
/// </summary>
public class ExchangeRequest
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 生徒ID
    /// </summary>
    [Required]
    public int StudentId { get; set; }

    /// <summary>
    /// 景品ID
    /// </summary>
    [Required]
    public int RewardId { get; set; }

    /// <summary>
    /// ステータス
    /// </summary>
    [Required]
    public ExchangeStatus Status { get; set; } = ExchangeStatus.Pending;

    /// <summary>
    /// 申請日時
    /// </summary>
    [Required]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 承認日時
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// 却下日時
    /// </summary>
    public DateTime? RejectedAt { get; set; }

    /// <summary>
    /// キャンセル日時
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// 承認者（保護者のユーザーID）
    /// </summary>
    [MaxLength(450)]
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// 却下理由
    /// </summary>
    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    /// <summary>
    /// 保護者メモ
    /// </summary>
    [MaxLength(1000)]
    public string? ParentNote { get; set; }

    /// <summary>
    /// 必要ポイント（申請時の景品ポイント）
    /// </summary>
    [Required]
    public int RequiredPoints { get; set; }

    // Navigation properties
    public Student? Student { get; set; }
    public Reward? Reward { get; set; }
}
