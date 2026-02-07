using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IProblemService
{
    Task<Problem?> GetNextProblemAsync(int studentId, List<int>? excludeRecentIds = null, CalculationType? category = null);
    Task<(bool IsCorrect, int PointsEarned, bool LeveledUp, Level? NewLevel)> SubmitAnswerAsync(
        int studentId,
        int problemId,
        int answer);
}
