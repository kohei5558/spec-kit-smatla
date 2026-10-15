using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 学習記録
/// </summary>
public class LearningRecord
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 児童ID
    /// </summary>
    [Required]
    public int StudentId { get; set; }

    /// <summary>
    /// 問題ID
    /// </summary>
    [Required]
    public int ProblemId { get; set; }

    /// <summary>
    /// 正誤（true=正解, false=不正解）
    /// </summary>
    [Required]
    public bool IsCorrect { get; set; }

    /// <summary>
    /// 児童の回答
    /// </summary>
    [Required]
    public int StudentAnswer { get; set; }

    /// <summary>
    /// 児童が答えたあまり（あまりのあるわり算のみ）
    /// </summary>
    public int? StudentRemainder { get; set; }

    /// <summary>
    /// 獲得ポイント
    /// </summary>
    [Range(0, int.MaxValue)]
    public int PointsEarned { get; set; }

    /// <summary>
    /// 所要時間（秒）
    /// </summary>
    [Range(0, int.MaxValue)]
    public int TimeTakenSeconds { get; set; }

    /// <summary>
    /// 解答日時
    /// </summary>
    public DateTime SolvedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Student Student { get; set; } = null!;
    public Problem Problem { get; set; } = null!;
}
