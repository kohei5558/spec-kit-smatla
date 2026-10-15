namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 学年に応じて始めるレベル（specs/008-grade3-difficulty/spec.md）。
/// レベルの ID と LevelNumber は 1〜10 で一致している（LevelSeeder）
/// </summary>
public static class GradeStartLevel
{
    /// <summary>
    /// 学年（1〜6）から始めるレベル。1年→1、2年→3、3年→5、4年以上→7
    /// </summary>
    public static int ForGrade(int grade) => grade switch
    {
        <= 1 => 1,
        2 => 3,
        3 => 5,
        _ => 7
    };
}
