using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

public interface IDailyChallengeRepository : IRepository<DailyChallenge>
{
    Task<DailyChallenge?> GetByDateAsync(DateTime date);
}
