namespace GamifiedMathDrill.Client.Models;

public class StatisticsDto
{
    public int TotalProblems { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public double AccuracyRate { get; set; }
    public int TotalPoints { get; set; }
    public Dictionary<string, int> ProblemsByType { get; set; } = new();
    public Dictionary<string, double> AccuracyByType { get; set; } = new();
    public Dictionary<string, int> DailyProblems { get; set; } = new();
    public Dictionary<string, double> DailyAccuracy { get; set; } = new();
}
