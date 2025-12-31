namespace GamifiedMathDrill.Api.DTOs;

public class ExchangeRewardResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int AcquiredRewardId { get; set; }
    public int PointsSpent { get; set; }
    public DateTime AcquiredAt { get; set; }
}
