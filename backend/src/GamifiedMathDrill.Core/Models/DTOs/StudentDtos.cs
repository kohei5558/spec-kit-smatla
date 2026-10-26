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
    /// <summary>2つ目の欄がある問題か（あまりのあるわり算のあまり、時こくの「分」など）</summary>
    public bool HasRemainder { get; set; }
    /// <summary>答えの形（Number・QuotientRemainder・ClockTime など、013）。画面は欄の数と単位をこれで決める</summary>
    public string AnswerFormat { get; set; } = "Number";
    /// <summary>時計の絵に出す時刻（"3:40" など。無ければ空、013）</summary>
    public List<string> ClockTimes { get; set; } = new();
    /// <summary>答えの欄の単位（AnswerFormat が Units のとき。["km", "m"] など、015）</summary>
    public List<string> AnswerUnits { get; set; } = new();
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
    /// <summary>2つ目の答えの正解（あまり、時こくの「分」など。答えが1つの問題は null）</summary>
    public int? CorrectRemainder { get; set; }
    /// <summary>答えの形（画面が「こたえは 4時5分」のように表示するため、013）</summary>
    public string AnswerFormat { get; set; } = "Number";
    /// <summary>答えの欄の単位（015）</summary>
    public List<string> AnswerUnits { get; set; } = new();
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
