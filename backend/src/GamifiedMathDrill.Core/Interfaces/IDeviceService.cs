using GamifiedMathDrill.Core.DTOs;

namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 子供用端末の登録管理サービス
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// 1人の保護者が登録できる端末数の上限
    /// </summary>
    const int MaxDevicesPerParent = 10;

    /// <summary>
    /// 端末を登録し、端末トークンを発行する。上限超過時は InvalidOperationException
    /// </summary>
    Task<RegisterDeviceResponse> RegisterAsync(string parentId, string name);

    /// <summary>
    /// 保護者が登録した端末一覧を取得
    /// </summary>
    Task<List<RegisteredDeviceDto>> ListAsync(string parentId);

    /// <summary>
    /// 端末の登録を解除する。保護者の端末でなければ false
    /// </summary>
    Task<bool> RevokeAsync(string parentId, int deviceId);

    /// <summary>
    /// 端末トークンから登録した保護者IDを取得する（無効なら null）。最終利用日時も更新する
    /// </summary>
    Task<string?> ResolveParentIdAsync(string? token);
}
