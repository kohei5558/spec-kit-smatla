using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface ILearningRecordService
{
    Task<(List<LearningRecord> Records, int TotalCount)> GetRecordsAsync(
        int studentId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CalculationType? calculationType = null,
        int page = 1,
        int pageSize = 20);

    Task<LearningStatistics> GetStatisticsAsync(
        int studentId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CalculationType? calculationType = null);
}

public class LearningStatistics
{
    public int TotalProblems { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public double AccuracyRate { get; set; }
    public int TotalPoints { get; set; }
    public Dictionary<CalculationType, int> ProblemsByType { get; set; } = new();
    public Dictionary<CalculationType, double> AccuracyByType { get; set; } = new();
    public Dictionary<string, int> DailyProblems { get; set; } = new();
    public Dictionary<string, double> DailyAccuracy { get; set; } = new();
}
