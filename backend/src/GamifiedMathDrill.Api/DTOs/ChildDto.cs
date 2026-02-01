using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 子供情報DTO
/// </summary>
public class ChildDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int TotalPoints { get; set; }
    public int PendingRequestsCount { get; set; }
    public int TotalProblemsCompleted { get; set; }
    public decimal AccuracyRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
