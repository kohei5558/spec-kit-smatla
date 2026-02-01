using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 交換申請DTO
/// </summary>
public class ExchangeRequestDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int RewardId { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public string? RewardImageUrl { get; set; }
    public int RequiredPoints { get; set; }
    public ExchangeStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApproverName { get; set; }
    public string? RejectionReason { get; set; }
    public string? ParentNote { get; set; }
}
