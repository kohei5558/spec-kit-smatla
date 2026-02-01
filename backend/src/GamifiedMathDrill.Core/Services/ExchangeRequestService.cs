using GamifiedMathDrill.Core.Interfaces;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 交換申請サービス
/// </summary>
public class ExchangeRequestService : IExchangeRequestService
{
    private readonly IExchangeRequestRepository _exchangeRequestRepository;
    private readonly IRewardRepository _rewardRepository;
    private readonly IStudentRepository _studentRepository;

    public ExchangeRequestService(
        IExchangeRequestRepository exchangeRequestRepository,
        IRewardRepository rewardRepository,
        IStudentRepository studentRepository)
    {
        _exchangeRequestRepository = exchangeRequestRepository;
        _rewardRepository = rewardRepository;
        _studentRepository = studentRepository;
    }

    public async Task<ExchangeRequest> CreateRequestAsync(int studentId, int rewardId)
    {
        // 生徒を取得
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student == null)
        {
            throw new KeyNotFoundException("生徒が見つかりません。");
        }

        // 景品を取得
        var reward = await _rewardRepository.GetByIdAsync(rewardId);
        if (reward == null)
        {
            throw new KeyNotFoundException("景品が見つかりません。");
        }

        // 景品がアクティブかチェック
        if (!reward.IsActive)
        {
            throw new InvalidOperationException("この景品は現在利用できません。");
        }

        // ポイントチェック
        if (student.TotalPoints < reward.RequiredPoints)
        {
            throw new InvalidOperationException($"ポイントが不足しています。必要ポイント: {reward.RequiredPoints}、所持ポイント: {student.TotalPoints}");
        }

        // 在庫チェック（実物景品の場合）
        if (reward.IsPhysical && reward.Stock.HasValue)
        {
            if (reward.Stock.Value <= 0)
            {
                throw new InvalidOperationException("在庫がありません。");
            }

            // 在庫を減らす（申請時点で仮確保）
            reward.Stock--;
            reward.UpdatedAt = DateTime.UtcNow;
            await _rewardRepository.UpdateAsync(reward);
        }

        // 交換申請を作成
        var exchangeRequest = new ExchangeRequest
        {
            StudentId = studentId,
            RewardId = rewardId,
            Status = ExchangeStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            RequiredPoints = reward.RequiredPoints
        };

        return await _exchangeRequestRepository.AddAsync(exchangeRequest);
    }

    public async Task<IEnumerable<ExchangeRequest>> GetRequestsByStudentAsync(int studentId)
    {
        return await _exchangeRequestRepository.GetByStudentIdAsync(studentId);
    }

    public async Task<ExchangeRequest?> GetRequestByIdAsync(int id)
    {
        return await _exchangeRequestRepository.GetByIdAsync(id);
    }

    public async Task CancelRequestAsync(int requestId, int studentId)
    {
        var request = await _exchangeRequestRepository.GetByIdAsync(requestId);
        if (request == null)
        {
            throw new KeyNotFoundException("交換申請が見つかりません。");
        }

        // 申請者本人かチェック
        if (request.StudentId != studentId)
        {
            throw new UnauthorizedAccessException("この申請をキャンセルする権限がありません。");
        }

        // Pending状態のみキャンセル可能
        if (request.Status != ExchangeStatus.Pending)
        {
            throw new InvalidOperationException("承認済みまたは却下された申請はキャンセルできません。");
        }

        // 在庫を戻す
        var reward = await _rewardRepository.GetByIdAsync(request.RewardId);
        if (reward != null && reward.IsPhysical && reward.Stock.HasValue)
        {
            reward.Stock++;
            reward.UpdatedAt = DateTime.UtcNow;
            await _rewardRepository.UpdateAsync(reward);
        }

        // ステータスを更新
        request.Status = ExchangeStatus.Cancelled;
        request.CancelledAt = DateTime.UtcNow;
        await _exchangeRequestRepository.UpdateAsync(request);
    }

    public async Task<ExchangeRequest> ApproveRequestAsync(int requestId, string approverId, string? parentNote)
    {
        var request = await _exchangeRequestRepository.GetByIdAsync(requestId);
        if (request == null)
        {
            throw new KeyNotFoundException("交換申請が見つかりません。");
        }

        // Pending状態のみ承認可能
        if (request.Status != ExchangeStatus.Pending)
        {
            throw new InvalidOperationException("この申請は既に処理されています。");
        }

        // 生徒のポイントを再確認
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new KeyNotFoundException("生徒が見つかりません。");
        }

        if (student.TotalPoints < request.RequiredPoints)
        {
            throw new InvalidOperationException("生徒のポイントが不足しています。");
        }

        // ポイントを消費
        student.TotalPoints -= request.RequiredPoints;
        await _studentRepository.UpdateAsync(student);

        // 申請を承認
        request.Status = ExchangeStatus.Approved;
        request.ApprovedAt = DateTime.UtcNow;
        request.ApprovedBy = approverId;
        request.ParentNote = parentNote;

        return await _exchangeRequestRepository.UpdateAsync(request);
    }

    public async Task<ExchangeRequest> RejectRequestAsync(int requestId, string reason, string? parentNote)
    {
        var request = await _exchangeRequestRepository.GetByIdAsync(requestId);
        if (request == null)
        {
            throw new KeyNotFoundException("交換申請が見つかりません。");
        }

        // Pending状態のみ却下可能
        if (request.Status != ExchangeStatus.Pending)
        {
            throw new InvalidOperationException("この申請は既に処理されています。");
        }

        // 在庫を戻す
        var reward = await _rewardRepository.GetByIdAsync(request.RewardId);
        if (reward != null && reward.IsPhysical && reward.Stock.HasValue)
        {
            reward.Stock++;
            reward.UpdatedAt = DateTime.UtcNow;
            await _rewardRepository.UpdateAsync(reward);
        }

        // 申請を却下
        request.Status = ExchangeStatus.Rejected;
        request.RejectedAt = DateTime.UtcNow;
        request.RejectionReason = reason;
        request.ParentNote = parentNote;

        return await _exchangeRequestRepository.UpdateAsync(request);
    }
}
