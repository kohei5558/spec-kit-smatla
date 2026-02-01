using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 交換申請サービスのインターフェース
/// </summary>
public interface IExchangeRequestService
{
    Task<ExchangeRequest> CreateRequestAsync(int studentId, int rewardId);
    Task<IEnumerable<ExchangeRequest>> GetRequestsByStudentAsync(int studentId);
    Task<ExchangeRequest?> GetRequestByIdAsync(int id);
    Task CancelRequestAsync(int requestId, int studentId);
    Task<ExchangeRequest> ApproveRequestAsync(int requestId, string approverId, string? parentNote);
    Task<ExchangeRequest> RejectRequestAsync(int requestId, string reason, string? parentNote);
}
