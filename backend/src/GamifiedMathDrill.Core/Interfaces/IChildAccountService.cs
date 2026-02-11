using GamifiedMathDrill.Core.DTOs;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 子供アカウント管理サービス
/// </summary>
public interface IChildAccountService
{
    /// <summary>
    /// 保護者の子供アカウント一覧を取得
    /// </summary>
    Task<List<ChildAccountDto>> ListAllActiveAsync();
    Task<List<ChildAccountDto>> ListAsync(string parentId);

    /// <summary>
    /// 子供アカウント詳細を取得
    /// </summary>
    Task<ChildAccountDto?> GetAsync(string childId, string parentId);

    /// <summary>
    /// 子供アカウント詳細（学習統計含む）を取得
    /// </summary>
    Task<ChildAccountDto?> GetDetailAsync(string childId, string parentId);

    /// <summary>
    /// 子供アカウントを作成
    /// </summary>
    Task<ChildAccountDto> CreateAsync(string parentId, ChildAccountCreateDto dto);

    /// <summary>
    /// 子供アカウントを更新
    /// </summary>
    Task<ChildAccountDto> UpdateAsync(string childId, string parentId, ChildAccountUpdateDto dto);

    /// <summary>
    /// 子供アカウントを一時停止
    /// </summary>
    Task SuspendAsync(string childId, string parentId);

    /// <summary>
    /// 子供アカウントを再開
    /// </summary>
    Task ActivateAsync(string childId, string parentId);

    /// <summary>
    /// 子供アカウントを削除
    /// </summary>
    Task DeleteAsync(string childId, string parentId);

    /// <summary>
    /// 子供の学習統計を取得
    /// </summary>
    Task<ChildLearningStatsDto> GetLearningStatsAsync(string childId);

    /// <summary>
    /// 子供アカウントのPINを検証
    /// </summary>
    Task<bool> VerifyPinAsync(string childId, string pin);

    /// <summary>
    /// PINロックアウト状態を確認
    /// </summary>
    Task<bool> IsLockedOutAsync(string childId);

    /// <summary>
    /// PIN失敗回数を記録
    /// </summary>
    Task RecordFailedPinAttemptAsync(string childId);

    /// <summary>
    /// PIN失敗カウンターをクリア（ログイン成功時）
    /// </summary>
    Task ClearFailedPinAttemptsAsync(string childId);

    /// <summary>
    /// 同じPINを使用している兄弟がいるか確認
    /// </summary>
    Task<bool> HasDuplicatePinAsync(string parentId, string pin, string? excludeChildId = null);

    /// <summary>
    /// 子供アカウントにStudentIdを設定
    /// </summary>
    Task UpdateStudentIdAsync(string childId, int studentId);
}
