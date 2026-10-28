namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 教科（020）。問題の種類（CalculationType）から決まり、DB には持たない
/// </summary>
public enum Subject
{
    /// <summary>
    /// 算数
    /// </summary>
    Math = 0,

    /// <summary>
    /// 国語
    /// </summary>
    Japanese = 1
}

public static class Subjects
{
    private static readonly CalculationType[] JapaneseTypes =
    {
        CalculationType.KanjiReading, CalculationType.KanjiWriting, CalculationType.KanjiWords
    };

    public static Subject Of(CalculationType type) => JapaneseTypes.Contains(type) ? Subject.Japanese : Subject.Math;

    /// <summary>
    /// 教科の問題の種類（CalculationType の順）
    /// </summary>
    public static CalculationType[] Types(Subject subject) =>
        Enum.GetValues<CalculationType>().Where(t => Of(t) == subject).ToArray();
}
