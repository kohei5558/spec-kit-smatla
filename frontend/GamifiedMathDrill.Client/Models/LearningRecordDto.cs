namespace GamifiedMathDrill.Client.Models;

public class LearningRecordDto
{
    public int Id { get; set; }
    public int ProblemId { get; set; }
    public string Question { get; set; } = string.Empty;
    public int CorrectAnswer { get; set; }
    public int StudentAnswer { get; set; }
    /// <summary>2つ目の答え（あまり、時こくの「分」など）。答えが1つの問題は null</summary>
    public int? CorrectRemainder { get; set; }
    public int? StudentRemainder { get; set; }
    /// <summary>答えの形（013）</summary>
    public string AnswerFormat { get; set; } = "Number";
    /// <summary>答えの欄の単位（015）</summary>
    public List<string> AnswerUnits { get; set; } = new();
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public int TimeTakenSeconds { get; set; }
    public string CalculationTypeText { get; set; } = string.Empty;
    public int DifficultyLevel { get; set; }
    public DateTime SolvedAt { get; set; }
}

public class LearningRecordsResponseDto
{
    public List<LearningRecordDto> Records { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
