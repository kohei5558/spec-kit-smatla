namespace GamifiedMathDrill.Client.Models;

public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int ConsecutiveDays { get; set; }
    public int TotalProblems { get; set; }
    public DateTime? LastLoginAt { get; set; }
    /// <summary>種類ごとのレベルのうち、いちばん高いもの（014）</summary>
    public int HighestLevel { get; set; }
    /// <summary>計算の種類ごとのレベル（すべての種類、014）</summary>
    public List<StudentLevelDto> Levels { get; set; } = new();

    /// <summary>その種類（Addition など）のレベル。無ければ null</summary>
    public StudentLevelDto? LevelFor(string? calculationType) =>
        Levels.FirstOrDefault(l => l.CalculationType == calculationType);
}

public class StudentLevelDto
{
    public string CalculationType { get; set; } = string.Empty;
    public int LevelNumber { get; set; }
    /// <summary>この種類のレベルアップ用の連続正解数</summary>
    public int CorrectAnswers { get; set; }
    public int RequiredCorrectAnswers { get; set; }

    /// <summary>いちばん上のレベル（これ以上は上がらない）</summary>
    public bool IsMaxLevel => LevelNumber >= 10;

    /// <summary>レベルアップまであと何問か</summary>
    public int RemainingToLevelUp => Math.Max(0, RequiredCorrectAnswers - CorrectAnswers);
}

public class CreateStudentDto
{
    public string Name { get; set; } = string.Empty;
}
