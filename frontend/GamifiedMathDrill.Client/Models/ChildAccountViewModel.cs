namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// 子供アカウント情報（ビューモデル）
/// </summary>
public class ChildAccountViewModel
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
    /// 学習統計（詳細取得時のみ）
    /// </summary>
    public ChildLearningStatsViewModel? LearningStats { get; set; }
}

/// <summary>
/// 子供アカウント作成リクエスト
/// </summary>
public class ChildAccountCreateRequest
{
    /// <summary>
    /// 子供の名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 学年（1〜6年生）
    /// </summary>
    public int GradeLevel { get; set; }

    /// <summary>
    /// プリセットアバターID
    /// </summary>
    public int PresetAvatarId { get; set; }

    /// <summary>
    /// PINコード（4桁の数字）
    /// </summary>
    public string PIN { get; set; } = string.Empty;
}

/// <summary>
/// 子供アカウント更新リクエスト
/// </summary>
public class ChildAccountUpdateRequest
{
    /// <summary>
    /// 子供の名前
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 学年（1〜6年生）
    /// </summary>
    public int GradeLevel { get; set; }

    /// <summary>
    /// プリセットアバターID
    /// </summary>
    public int PresetAvatarId { get; set; }

    /// <summary>
    /// 新しいPINコード（変更時のみ入力）
    /// </summary>
    public string? NewPIN { get; set; }
}

/// <summary>
/// プリセットアバター情報
/// </summary>
public class PresetAvatarViewModel
{
    /// <summary>
    /// アバターID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// アバター名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 画像URL
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// 表示順序
    /// </summary>
    public int DisplayOrder { get; set; }
}

/// <summary>
/// 子供ログインリクエスト
/// </summary>
public class ChildLoginRequestModel
{
    /// <summary>
    /// 子供アカウントID
    /// </summary>
    public string ChildAccountId { get; set; } = string.Empty;

    /// <summary>
    /// PINコード
    /// </summary>
    public string PIN { get; set; } = string.Empty;
}

/// <summary>
/// 子供の学習統計情報
/// </summary>
public class ChildLearningStatsViewModel
{
    /// <summary>
    /// 総問題数
    /// </summary>
    public int TotalProblems { get; set; }

    /// <summary>
    /// 正答率（パーセント）
    /// </summary>
    public decimal AccuracyRate { get; set; }

    /// <summary>
    /// 獲得ポイント
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// 連続学習日数
    /// </summary>
    public int ConsecutiveDays { get; set; }

    /// <summary>
    /// 過去7日間のアクティビティ
    /// </summary>
    public List<DailyActivityDto>? Last7DaysActivity { get; set; }
}

/// <summary>
/// 日次アクティビティデータ
/// </summary>
public class DailyActivityDto
{
    /// <summary>
    /// 日付
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 解いた問題数
    /// </summary>
    public int ProblemsSolved { get; set; }

    /// <summary>
    /// 獲得ポイント
    /// </summary>
    public int PointsEarned { get; set; }
}
