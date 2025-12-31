using System.ComponentModel.DataAnnotations;

namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// レベル
/// </summary>
public class Level
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// レベル番号（1-10）
    /// </summary>
    [Range(1, 10)]
    public int LevelNumber { get; set; }

    /// <summary>
    /// レベルアップに必要な連続正解数
    /// </summary>
    [Range(1, 100)]
    public int RequiredCorrectAnswers { get; set; }

    /// <summary>
    /// 最小難易度
    /// </summary>
    [Range(1, 10)]
    public int MinDifficulty { get; set; }

    /// <summary>
    /// 最大難易度
    /// </summary>
    [Range(1, 10)]
    public int MaxDifficulty { get; set; }

    // Navigation properties
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
