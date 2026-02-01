using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 交換申請リポジトリのインターフェース
/// </summary>
public interface IExchangeRequestRepository : IRepository<ExchangeRequest>
{
    Task<IEnumerable<ExchangeRequest>> GetByStudentIdAsync(int studentId);
    Task<IEnumerable<ExchangeRequest>> GetByStatusAsync(ExchangeStatus status);
    Task<IEnumerable<ExchangeRequest>> GetByParentIdAsync(string parentId);
}
