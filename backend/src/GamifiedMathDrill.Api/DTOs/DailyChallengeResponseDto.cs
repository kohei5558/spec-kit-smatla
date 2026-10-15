namespace GamifiedMathDrill.Api.DTOs;

public class DailyChallengeResponseDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public DateOnly TargetDate { get; set; }
    public int BonusPoints { get; set; }
    /// <summary>今日すでに答えたか（1日1回）</summary>
    public bool IsAnswered { get; set; }
    /// <summary>以下は答えたあとだけ値が入る</summary>
    public bool? IsCorrect { get; set; }
    public int? StudentAnswer { get; set; }
    public int? CorrectAnswer { get; set; }
}
