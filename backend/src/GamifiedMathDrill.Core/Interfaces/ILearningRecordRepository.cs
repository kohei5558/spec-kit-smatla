using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 学習記録リポジトリインターフェース
/// </summary>
public interface ILearningRecordRepository : IRepository<LearningRecord>
{
    Task<IEnumerable<LearningRecord>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<LearningRecord>> GetByStudentIdAndDateRangeAsync(int studentId, DateTime startDate, DateTime endDate);
    /// <summary>マスター・復習の判定に使う、その子の すべての答え（030）</summary>
    Task<List<AnswerHistory>> GetAnswerHistoryAsync(int studentId);
}
