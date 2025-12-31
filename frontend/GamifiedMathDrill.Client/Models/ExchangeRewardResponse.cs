namespace GamifiedMathDrill.Client.Models;

public class ExchangeRewardResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int AcquiredRewardId { get; set; }
    public int PointsSpent { get; set; }
    public DateTime AcquiredAt { get; set; }
}
