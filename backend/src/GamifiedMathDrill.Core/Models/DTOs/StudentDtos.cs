namespace GamifiedMathDrill.Core.Models.DTOs;

public class CreateStudentDto
{
    public string Name { get; set; } = string.Empty;
}

public class StudentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int ConsecutiveDays { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalProblems { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int CurrentLevelId { get; set; }
    public string? CurrentLevelName { get; set; }
    public int? CurrentLevelMinDifficulty { get; set; }
    public int? CurrentLevelMaxDifficulty { get; set; }
}

public class ProblemDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public string CalculationTypeText { get; set; } = string.Empty;
    /// <summary>あまりも答える問題か（あまりのあるわり算）</summary>
    public bool HasRemainder { get; set; }
}

public class SubmitAnswerDto
{
    public int Answer { get; set; }
    /// <summary>あまり（あまりのあるわり算のときだけ使う）</summary>
    public int? Remainder { get; set; }
}

public class AnswerResultDto
{
    public bool IsCorrect { get; set; }
    public int CorrectAnswer { get; set; }
    /// <summary>正しいあまり（あまりのあるわり算のときだけ）</summary>
    public int? CorrectRemainder { get; set; }
    public int PointsEarned { get; set; }
    public bool LeveledUp { get; set; }
    public LevelDto? NewLevel { get; set; }
}

public class LevelDto
{
    public int Id { get; set; }
    public int LevelNumber { get; set; }
    public int RequiredCorrectAnswers { get; set; }
    public int MinDifficulty { get; set; }
    public int MaxDifficulty { get; set; }
}
