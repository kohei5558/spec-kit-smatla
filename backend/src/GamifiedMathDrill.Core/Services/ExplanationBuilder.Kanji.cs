using System.Text.RegularExpressions;
using GamifiedMathDrill.Core.Models;

namespace GamifiedMathDrill.Core.Services;

/// <summary>
/// 漢字の解説（022、型は specs/022-kanji-explanations/explanations.md）。
/// 中身（字・読み・ことば）は確認済みの KanjiBatches から使い、新しく書く文は型だけ
/// </summary>
public static partial class ExplanationBuilder
{
    private static readonly Regex Target = new(@"\[([^\]]+)\]");
    private static readonly Regex LeadingHiragana = new(@"^[ぁ-ゖ]+");

    private static Explanation? Kanji(Problem problem, string? text)
    {
        var question = problem.Question;
        var correct = problem.CorrectText;
        var target = Target.Match(question);
        if (correct == null || !target.Success)
        {
            return null;
        }

        switch (problem.CalculationType)
        {
            case CalculationType.KanjiReading:
                {
                    var item = KanjiBatches.Items.FirstOrDefault(i => i.ReadingQuestion == question);
                    if (item == null)
                    {
                        return null;
                    }
                    var steps = new List<string> { $"「{target.Groups[1].Value}」は「{correct}」と 読むよ" };
                    AddWord(steps, item);

                    // 送りがな（[ ] のあとの ひらがな）まで つけた
                    var okurigana = LeadingHiragana.Match(question[(target.Index + target.Length)..]).Value;
                    var extra = text != null && text.Length > correct.Length && text.StartsWith(correct) ? text[correct.Length..] : "";
                    var hint = extra.Length > 0 && okurigana.StartsWith(extra) ? $"ひらがなの ところ（{extra}）は 入れないよ" : null;
                    return new(steps, hint);
                }
            case CalculationType.KanjiWriting:
                {
                    var item = KanjiBatches.Items.FirstOrDefault(i => i.WritingQuestion == question);
                    if (item == null)
                    {
                        return null;
                    }
                    var steps = new List<string> { $"「{target.Groups[1].Value}」は「{correct}」と 書くよ" };
                    AddWord(steps, item);
                    return new(steps, null);
                }
            case CalculationType.KanjiWords:
                {
                    var item = KanjiBatches.Items.FirstOrDefault(i => i.WordReading == target.Groups[1].Value);
                    if (item == null)
                    {
                        return null;
                    }
                    var steps = new[] { $"「{item.WordReading}」は「{correct}」と 書くよ", $"「{item.Kanji}」を つかう ことばだよ" };
                    return new(steps, WordHint(correct, item.Kanji, text));
                }
            default:
                return null;
        }
    }

    // 「ことば: 駅員（えきいん）」。ことばの問題が無い字（1回目の寒・湖）は出さない
    private static void AddWord(List<string> steps, KanjiItem item)
    {
        if (item.WordReading != null && item.WordChoices != null)
        {
            steps.Add($"ことば: {item.WordChoices[0]}（{item.WordReading}）");
        }
    }

    /// <summary>
    /// ことばで、ちがう字が1つだけのとき。問題の字がちがう →「1つめの 字が ちがうよ」、ほかの字がちがう →「駅は あってるよ。2つめの…」
    /// </summary>
    private static string? WordHint(string correct, string kanji, string? text)
    {
        if (text == null || text.Length != correct.Length)
        {
            return null;
        }
        var different = Enumerable.Range(0, correct.Length).Where(i => text[i] != correct[i]).ToList();
        if (different.Count != 1)
        {
            return null;
        }
        var position = different[0];
        return correct[position].ToString() == kanji
            ? $"{position + 1}つめの 字が ちがうよ。「{kanji}」だよ"
            : $"「{kanji}」は あってるよ。{position + 1}つめの 字を よく 見てね";
    }
}
