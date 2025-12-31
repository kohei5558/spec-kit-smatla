namespace GamifiedMathDrill.Client.Models;

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

public class CreateStudentDto
{
    public string Name { get; set; } = string.Empty;
}
