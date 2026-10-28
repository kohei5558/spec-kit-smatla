namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// 計算の種類（API の Addition など）の画面での名前
/// </summary>
public static class CalculationTypeNames
{
    /// <summary>
    /// 国語の種類をまぜて出す「こくご ミックス」の選択（API には subject=Japanese で送る、020）。null は さんすうの「すべて」
    /// </summary>
    public const string JapaneseMix = "Japanese";

    /// <summary>
    /// 国語（漢字）の種類か、こくご ミックスか
    /// </summary>
    public static bool IsJapanese(string? calculationType) =>
        calculationType is "KanjiReading" or "KanjiWriting" or "KanjiWords" or JapaneseMix;

    public static string Get(string? calculationType) => calculationType switch
    {
        "Addition" => "足し算",
        "Subtraction" => "引き算",
        "Multiplication" => "掛け算",
        "Division" => "割り算",
        "DivisionWithRemainder" => "あまりのあるわり算",
        "Time" => "時こくと時間",
        "Measurement" => "長さ・重さ・かさ",
        "WordProblem" => "文章問題",
        "KanjiReading" => "漢字の読み",
        "KanjiWriting" => "漢字の書き",
        "KanjiWords" => "漢字のことば",
        CalculationTypeNames.JapaneseMix => "こくご ミックス",
        _ => "計算"
    };
}
