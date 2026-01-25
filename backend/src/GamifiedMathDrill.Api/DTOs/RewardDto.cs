using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Api.DTOs;

/// <summary>
/// 景品情報のDTO
/// </summary>
public class RewardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredPoints { get; set; }
    public RewardCategory Category { get; set; }
    public string? ImageUrl { get; set; }
    public int? Stock { get; set; }
    public bool IsPhysical { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public byte[]? RowVersion { get; set; }
}
