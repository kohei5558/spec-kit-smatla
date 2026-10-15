using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 子供用として登録された端末
/// </summary>
public class RegisteredDevice
{
    /// <summary>
    /// 端末ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 登録した保護者のユーザーID（ApplicationUser.Id）
    /// </summary>
    [Required]
    public string ParentId { get; set; } = string.Empty;

    /// <summary>
    /// 端末名（例: リビングのタブレット）
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 端末トークンのSHA256ハッシュ（16進64文字）。トークン本体は保存しない
    /// </summary>
    [Required]
    [StringLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// 登録日時（UTC）
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最終利用日時（UTC）
    /// </summary>
    public DateTime LastUsedAt { get; set; }
}
