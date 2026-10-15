using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IProblemService
{
    /// <summary>今日（日本時間）答えた問題の数</summary>
    Task<int> CountTodaysAnswersAsync(int studentId);
    Task<Problem?> GetNextProblemAsync(int studentId, List<int>? excludeRecentIds = null, CalculationType? category = null);
    Task<(bool IsCorrect, int PointsEarned, bool LeveledUp, Level? NewLevel)> SubmitAnswerAsync(
        int studentId,
        int problemId,
        int answer,
        int? remainder = null);
}
