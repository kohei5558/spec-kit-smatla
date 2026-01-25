namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// ユーザーのロールを表す列挙型
/// </summary>
public enum UserRole
{
    /// <summary>
    /// 保護者アカウント
    /// </summary>
    Parent = 0,
    
    /// <summary>
    /// 子供アカウント
    /// </summary>
    Child = 1
}
