using Microsoft.AspNetCore.Identity;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Infrastructure.Identity;

/// <summary>
/// アプリケーションユーザー（保護者・子供）
/// ASP.NET Core Identityを拡張
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// ユーザーのロール（保護者/子供）
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// 子供用PIN（4桁）
    /// </summary>
    public string? PIN { get; set; }

    /// <summary>
    /// 親アカウントのID（子供の場合のみ）
    /// </summary>
    public string? ParentId { get; set; }

    /// <summary>
    /// 表示名
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// アバター画像URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 学年（1-6年生、子供の場合のみ）
    /// </summary>
    public int? GradeLevel { get; set; }

    /// <summary>
    /// 学生ID（Studentテーブルへの参照、子供の場合のみ）
    /// </summary>
    public int? StudentId { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// アクティブ状態
    /// </summary>
    public bool IsActive { get; set; } = true;
}
