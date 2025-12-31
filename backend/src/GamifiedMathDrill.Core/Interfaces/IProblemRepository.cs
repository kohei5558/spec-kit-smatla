using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 問題リポジトリインターフェース
/// </summary>
public interface IProblemRepository : IRepository<Problem>
{
    Task<Problem?> GetRandomProblemAsync(int difficultyLevel, CalculationType? type = null, List<int>? excludeIds = null);
    Task<IEnumerable<Problem>> GetProblemsByDifficultyAsync(int minDifficulty, int maxDifficulty);
}
