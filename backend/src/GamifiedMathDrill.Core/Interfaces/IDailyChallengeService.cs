using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IDailyChallengeService
{
    Task<DailyChallenge?> GetTodaysChallengeAsync();
    Task<(bool IsCorrect, int BonusPoints, bool LeveledUp, Level? NewLevel)> SubmitChallengeAnswerAsync(
        int studentId, 
        int challengeId, 
        int answer);
    Task<DailyChallenge> CreateDailyChallengeAsync(DateTime targetDate);
}
