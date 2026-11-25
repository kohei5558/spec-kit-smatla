using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 問題リポジトリインターフェース
/// </summary>
public interface IProblemRepository : IRepository<Problem>
{
    Task<Problem?> GetRandomProblemAsync(int difficultyLevel, CalculationType? type = null, List<int>? excludeIds = null);
    Task<IEnumerable<Problem>> GetProblemsByDifficultyAsync(int minDifficulty, int maxDifficulty);
    /// <summary>その種類の、出題する（IsActive の）問題のうち、ID が ids にあるもの（復習、030）</summary>
    Task<List<Problem>> GetActiveByIdsAsync(IReadOnlyCollection<int> ids, IReadOnlyCollection<CalculationType> types);
    /// <summary>その種類に 出題する問題が あるか（030）</summary>
    Task<bool> AnyActiveAsync(IReadOnlyCollection<CalculationType> types);
}
