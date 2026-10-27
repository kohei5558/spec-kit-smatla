using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 漢字の問題（020）。中身は保護者が確認した KanjiBatch1 だけを使う。
/// 字を並びの順に 3字ずつ むずかしさ 1〜10 に分ける（レベルが上がると新しい字が出る）
/// </summary>
public static class KanjiProblemGenerator
{
    private const int KanjiPerDifficulty = 3;

    public static List<Problem> Generate(Random random)
    {
        var problems = new List<Problem>();
        for (var i = 0; i < KanjiBatch1.Items.Length; i++)
        {
            var item = KanjiBatch1.Items[i];
            var difficulty = Math.Min(10, i / KanjiPerDifficulty + 1);

            problems.Add(new Problem
            {
                Question = item.ReadingQuestion,
                CorrectText = item.ReadingAnswer,
                AnswerFormat = AnswerFormat.Text,
                CalculationType = CalculationType.KanjiReading,
                DifficultyLevel = difficulty
            });
            problems.Add(Choice(item.WritingQuestion, item.WritingChoices, CalculationType.KanjiWriting, difficulty, random));
            if (item.WordReading != null && item.WordChoices != null)
            {
                problems.Add(Choice($"[{item.WordReading}]", item.WordChoices, CalculationType.KanjiWords, difficulty, random));
            }
        }
        return problems;
    }

    // 選択肢は いちばん左が正解。並びをまぜて保存する（乱数の種は固定なので毎回同じ並び）
    private static Problem Choice(string question, string[] choices, CalculationType type, int difficulty, Random random) => new()
    {
        Question = question,
        CorrectText = choices[0],
        Choices = string.Join(' ', choices.OrderBy(_ => random.Next())),
        AnswerFormat = AnswerFormat.Choice,
        CalculationType = type,
        DifficultyLevel = difficulty
    };
}
