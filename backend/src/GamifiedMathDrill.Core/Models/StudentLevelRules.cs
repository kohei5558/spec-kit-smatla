namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 種類ごとのレベル（014）の始め方
/// </summary>
public static class StudentLevelRules
{
    /// <summary>
    /// 学年で始めるレベルを決める種類（008）。それ以外（新しい単元）はレベル1から始める
    /// </summary>
    public static readonly IReadOnlyList<CalculationType> GradeLeveledTypes = new[]
    {
        CalculationType.Addition,
        CalculationType.Subtraction,
        CalculationType.Multiplication,
        CalculationType.Division,
        CalculationType.DivisionWithRemainder
    };

    public static int StartLevelFor(CalculationType type, int grade) =>
        GradeLeveledTypes.Contains(type) ? GradeStartLevel.ForGrade(grade) : 1;

    /// <summary>
    /// 子供を作るときの、学年でレベルを決める種類の行
    /// </summary>
    public static List<StudentLevel> CreateForGrade(int grade) =>
        GradeLeveledTypes.Select(t => new StudentLevel { CalculationType = t, LevelId = GradeStartLevel.ForGrade(grade) }).ToList();

    /// <summary>
    /// すべての計算の種類のレベルと連続正解数。行が無い種類はレベル1・連続正解数0（画面に出すため）
    /// </summary>
    public static List<(CalculationType Type, Level Level, int CorrectAnswers)> AllTypes(Student student, IEnumerable<Level> levels)
    {
        var byId = levels.ToDictionary(l => l.Id);
        return Enum.GetValues<CalculationType>()
            .Select(type =>
            {
                var row = student.Levels.FirstOrDefault(l => l.CalculationType == type);
                return (type, byId[row?.LevelId ?? 1], row?.CorrectAnswers ?? 0);
            })
            .ToList();
    }

    /// <summary>
    /// その種類の行。無ければレベル1で作って学習者に足す（新しい単元、または学年が分からないまま作られた学習者）
    /// </summary>
    public static StudentLevel GetOrAdd(Student student, CalculationType type)
    {
        var level = student.Levels.FirstOrDefault(l => l.CalculationType == type);
        if (level == null)
        {
            level = new StudentLevel { StudentId = student.Id, CalculationType = type, LevelId = 1 };
            student.Levels.Add(level);
        }
        return level;
    }
}
