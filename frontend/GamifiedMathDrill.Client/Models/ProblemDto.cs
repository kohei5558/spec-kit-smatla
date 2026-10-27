namespace GamifiedMathDrill.Client.Models;

public class ProblemDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public string CalculationTypeText { get; set; } = string.Empty;
    /// <summary>2つ目の欄がある問題か（あまりのあるわり算のあまり、時こくの「分」など）</summary>
    public bool HasRemainder { get; set; }
    /// <summary>答えの形（Number・QuotientRemainder・ClockTime など、013）</summary>
    public string AnswerFormat { get; set; } = "Number";
    /// <summary>時計の絵に出す時刻（"3:40" など、013）</summary>
    public List<string> ClockTimes { get; set; } = new();
    /// <summary>答えの欄の単位（AnswerFormat が Units のとき。["km", "m"] など、015）</summary>
    public List<string> AnswerUnits { get; set; } = new();
    /// <summary>今日すでに答えた数（「今日の◯問め」用）</summary>
    public int TodayAnsweredCount { get; set; }
}

public class SubmitAnswerDto
{
    public int Answer { get; set; }
    public int? Remainder { get; set; }
}

public class AnswerResultDto
{
    public bool IsCorrect { get; set; }
    public int CorrectAnswer { get; set; }
    public int? CorrectRemainder { get; set; }
    /// <summary>答えの形（013）</summary>
    public string AnswerFormat { get; set; } = "Number";
    /// <summary>答えの欄の単位（015）</summary>
    public List<string> AnswerUnits { get; set; } = new();
    public int PointsEarned { get; set; }
    public bool LeveledUp { get; set; }
    public LevelDto? NewLevel { get; set; }
    /// <summary>レベルが上がった計算の種類（Addition など、014）</summary>
    public string? LeveledUpCalculationType { get; set; }
    /// <summary>まちがえたときの考え方（019）</summary>
    public List<string> Explanation { get; set; } = new();
    /// <summary>よくある まちがいの一言（019）</summary>
    public string? Hint { get; set; }
}

public class LevelDto
{
    public int Id { get; set; }
    public int LevelNumber { get; set; }
    public int RequiredCorrectAnswers { get; set; }
    public int MinDifficulty { get; set; }
    public int MaxDifficulty { get; set; }
}
