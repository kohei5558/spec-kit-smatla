namespace GamifiedMathDrill.Client.Models;

/// <summary>
/// 計算の種類（API の Addition など）の画面での名前
/// </summary>
public static class CalculationTypeNames
{
    public static string Get(string? calculationType) => calculationType switch
    {
        "Addition" => "足し算",
        "Subtraction" => "引き算",
        "Multiplication" => "掛け算",
        "Division" => "割り算",
        "DivisionWithRemainder" => "あまりのあるわり算",
        _ => "計算"
    };
}
