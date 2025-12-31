namespace GamifiedMathDrill.Client.Models;

public class LearningRecordDto
{
    public int Id { get; set; }
    public int ProblemId { get; set; }
    public string Question { get; set; } = string.Empty;
    public int CorrectAnswer { get; set; }
    public int StudentAnswer { get; set; }
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
