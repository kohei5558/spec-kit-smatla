using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// デイリーチャレンジ。学習者ごと・日（日本時間）ごとに1つで、1回だけ答えられる（010）
/// </summary>
public class DailyChallenge
{
    /// <summary>
    /// 正解したときのボーナスポイント（通常の問題は10ポイント）
    /// </summary>
    public const int DefaultBonusPoints = 30;

    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 学習者ID
    /// </summary>
    [Required]
    public int StudentId { get; set; }

    /// <summary>
    /// 問題ID
    /// </summary>
    [Required]
    public int ProblemId { get; set; }

    /// <summary>
    /// 対象日（日本時間）
    /// </summary>
    [Required]
    public DateOnly TargetDate { get; set; }

    /// <summary>
    /// ボーナスポイント
    /// </summary>
    [Range(0, int.MaxValue)]
    public int BonusPoints { get; set; } = DefaultBonusPoints;

    /// <summary>
    /// 答えた日時（未回答は null）。同時に2回答えてもボーナスが1回分になるよう、同時実行の確認に使う
    /// </summary>
    [ConcurrencyCheck]
    public DateTime? AnsweredAt { get; set; }

    /// <summary>
    /// 正解だったか（未回答は null）
    /// </summary>
    public bool? IsCorrect { get; set; }

    /// <summary>
    /// 子供の答え（未回答は null）
    /// </summary>
    public int? StudentAnswer { get; set; }

    // Navigation properties
    public Student Student { get; set; } = null!;
    public Problem Problem { get; set; } = null!;
}
