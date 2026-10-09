using System.Security.Claims;
using GamifiedMathDrill.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GamifiedMathDrill.Api.Extensions;

/// <summary>
/// コントローラーからログイン中のユーザーで学習者へのアクセス可否を判定するヘルパー
/// </summary>
public static class StudentAccessExtensions
{
    /// <summary>
    /// ログイン中のユーザーが学習者にアクセスできるか（子供は自分、保護者は自分の子供のみ）。
    /// false のときは、他家庭の学習者の存在を推測させないよう 404 を返すこと
    /// </summary>
    public static async Task<bool> CanAccessStudentAsync(this ControllerBase controller, IStudentAccessService access, int studentId)
    {
        var userId = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = controller.User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
        {
            return false;
        }

        return await access.CanAccessAsync(userId, role, studentId);
    }
}
