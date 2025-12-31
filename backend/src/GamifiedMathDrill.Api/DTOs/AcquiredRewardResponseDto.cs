namespace GamifiedMathDrill.Api.DTOs;

public class AcquiredRewardResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int RewardId { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public string RewardDescription { get; set; } = string.Empty;
    public string RewardCategory { get; set; } = string.Empty;
    public string RewardImageUrl { get; set; } = string.Empty;
    public int PointsSpent { get; set; }
    public DateTime AcquiredAt { get; set; }
}
