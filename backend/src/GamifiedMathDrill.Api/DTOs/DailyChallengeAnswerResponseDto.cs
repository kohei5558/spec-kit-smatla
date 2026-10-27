namespace GamifiedMathDrill.Api.DTOs;

public class DailyChallengeAnswerResponseDto
{
    public bool IsCorrect { get; set; }
    public int BonusPoints { get; set; }
    public int CorrectAnswer { get; set; }
    /// <summary>まちがえたときの考え方（正解のときは空、019）</summary>
    public List<string> Explanation { get; set; } = new();
    /// <summary>よくある まちがいの一言（当てはまらなければ null、019）</summary>
    public string? Hint { get; set; }
}
