using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 漢字の問題（020）。中身は保護者が確認した KanjiBatches だけを使う。
/// すべての回を入れた順に通して、20字ずつ むずかしさ 1〜10 につめる（025。小3の200字で ちょうど 20字 × 10。
/// 新しい回は空いている上のむずかしさに入るので、前に入れた字は動かず、レベルが上がると新しい字が出る）
/// </summary>
public static class KanjiProblemGenerator
{
    private const int KanjiPerDifficulty = 20;

    public static List<Problem> Generate(Random random)
    {
        var problems = new List<Problem>();
        var items = KanjiBatches.Items.ToList();
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
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
