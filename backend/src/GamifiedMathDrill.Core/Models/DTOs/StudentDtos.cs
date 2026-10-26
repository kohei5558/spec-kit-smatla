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
    public int TotalProblems { get; set; }
    public DateTime? LastLoginAt { get; set; }
    /// <summary>種類ごとのレベルのうち、いちばん高いもの（ホームの大きな表示用、014）</summary>
    public int HighestLevel { get; set; }
    /// <summary>計算の種類ごとのレベル（すべての種類。まだ解いていない種類も始めるレベルで入る、014）</summary>
    public List<StudentLevelDto> Levels { get; set; } = new();
}

public class StudentLevelDto
{
    /// <summary>計算の種類（Addition など）</summary>
    public string CalculationType { get; set; } = string.Empty;
    public int LevelNumber { get; set; }
    /// <summary>この種類のレベルアップ用の連続正解数</summary>
    public int CorrectAnswers { get; set; }
    /// <summary>このレベルから上がるのに必要な連続正解数</summary>
    public int RequiredCorrectAnswers { get; set; }
}

public class ProblemDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public string CalculationTypeText { get; set; } = string.Empty;
    /// <summary>あまりも答える問題か（あまりのあるわり算）</summary>
    public bool HasRemainder { get; set; }
    /// <summary>今日（日本時間）すでに答えた数（問題画面の「今日の◯問め」用）</summary>
    public int TodayAnsweredCount { get; set; }
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
    /// <summary>レベルが上がった計算の種類（Addition など。上がっていなければ null、014）</summary>
    public string? LeveledUpCalculationType { get; set; }
}

public class LevelDto
{
    public int Id { get; set; }
    public int LevelNumber { get; set; }
    public int RequiredCorrectAnswers { get; set; }
    public int MinDifficulty { get; set; }
    public int MaxDifficulty { get; set; }
}
