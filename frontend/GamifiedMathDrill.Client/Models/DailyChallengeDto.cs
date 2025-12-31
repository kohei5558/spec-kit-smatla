namespace GamifiedMathDrill.Client.Models;

public class DailyChallengeDto
{
    public int Id { get; set; }
    public int ProblemId { get; set; }
    public string Question { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public DateTime TargetDate { get; set; }
    public int BonusPoints { get; set; }
    public bool IsActive { get; set; }
}
