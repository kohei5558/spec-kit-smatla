using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 学習記録リポジトリインターフェース
/// </summary>
public interface ILearningRecordRepository : IRepository<LearningRecord>
{
    Task<IEnumerable<LearningRecord>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<LearningRecord>> GetByStudentIdAndDateRangeAsync(int studentId, DateTime startDate, DateTime endDate);
}
