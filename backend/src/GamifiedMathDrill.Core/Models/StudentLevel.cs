namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 学習者の計算の種類ごとのレベル（014）。学習者と種類の組み合わせで1行
/// </summary>
public class StudentLevel
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public CalculationType CalculationType { get; set; }

    public int LevelId { get; set; }

    /// <summary>
    /// この種類のレベルアップ用の連続正解数（間違えたら0）
    /// </summary>
    public int CorrectAnswers { get; set; }

    public Student? Student { get; set; }

    public Level? Level { get; set; }
}
