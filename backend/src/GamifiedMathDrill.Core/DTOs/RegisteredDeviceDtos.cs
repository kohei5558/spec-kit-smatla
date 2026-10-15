using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.DTOs;

/// <summary>
/// 端末登録リクエスト
/// </summary>
public class RegisterDeviceRequest
{
    /// <summary>
    /// 端末名（例: リビングのタブレット）
    /// </summary>
    [Required(ErrorMessage = "端末名を入力してください")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "端末名は1〜30文字で入力してください")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 端末登録レスポンス（端末トークンはこのレスポンスでのみ返す）
/// </summary>
public class RegisterDeviceResponse
{
    /// <summary>
    /// 登録した端末の情報
    /// </summary>
    public RegisteredDeviceDto Device { get; set; } = new();

    /// <summary>
    /// 端末トークン（端末側に保存し、X-Device-Token ヘッダーで送る）
    /// </summary>
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// 登録済み端末（トークンは含まない）
/// </summary>
public class RegisteredDeviceDto
{
    /// <summary>
    /// 端末ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 端末名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 登録日時（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最終利用日時（UTC）
    /// </summary>
    public DateTime LastUsedAt { get; set; }
}
