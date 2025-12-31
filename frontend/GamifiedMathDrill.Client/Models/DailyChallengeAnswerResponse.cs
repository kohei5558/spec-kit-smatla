namespace GamifiedMathDrill.Client.Models;

public class DailyChallengeAnswerResponse
{
    public bool IsCorrect { get; set; }
    public int BonusPoints { get; set; }
    public bool LeveledUp { get; set; }
    public int? NewLevel { get; set; }
}
