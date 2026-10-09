namespace GamifiedMathDrill.Core.Interfaces;

/// <summary>
/// 学習者（Student）へのアクセス可否を判定するサービス。
/// 子供は自分自身の学習者だけ、保護者は自分の子供の学習者だけにアクセスできる
/// </summary>
public interface IStudentAccessService
{
    /// <summary>
    /// ユーザーが学習者にアクセスできるか
    /// </summary>
    Task<bool> CanAccessAsync(string userId, string role, int studentId);

    /// <summary>
    /// 子供ユーザー自身の学習者ID（未作成なら null）
    /// </summary>
    Task<int?> GetOwnStudentIdAsync(string childUserId);

    /// <summary>
    /// 保護者の子供たちの学習者ID一覧
    /// </summary>
    Task<List<int>> GetChildrenStudentIdsAsync(string parentId);
}
