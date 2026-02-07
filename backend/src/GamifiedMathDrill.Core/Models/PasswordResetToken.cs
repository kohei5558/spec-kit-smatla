using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// パスワードリセットトークンエンティティ
/// </summary>
public class PasswordResetToken
{
    /// <summary>
    /// トークンID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ユーザーID（ApplicationUser.Id）
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ハッシュ化されたトークン値（SHA256ハッシュ）
    /// </summary>
    [Required]
    [StringLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// トークン作成日時（UTC）
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// トークン有効期限（UTC）
    /// </summary>
    [Required]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 使用済みフラグ
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// 使用日時（UTC）
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// リクエスト元IPアドレス（IPv6対応）
    /// </summary>
    [StringLength(45)]
    public string? IpAddress { get; set; }
}
