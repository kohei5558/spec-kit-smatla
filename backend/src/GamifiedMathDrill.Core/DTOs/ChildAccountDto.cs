namespace GamifiedMathDrill.Core.DTOs;

/// <summary>
/// 子供アカウント情報（レスポンス）
/// </summary>
public class ChildAccountDto
{
    /// <summary>
    /// アカウントID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 子供の名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 学年（1〜6年生）
    /// </summary>
    public int GradeLevel { get; set; }

    /// <summary>
    /// アバター画像URL
    /// </summary>
    public string AvatarUrl { get; set; } = string.Empty;

    /// <summary>
    /// アクティブ状態（true: 有効, false: 停止中）
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 学習統計サマリー（詳細表示時のみ含まれる）
    /// </summary>
    public ChildLearningStatsDto? LearningStats { get; set; }
}
