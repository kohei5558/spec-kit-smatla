namespace GamifiedMathDrill.Core.Models;

/// <summary>
/// 漢字の中身の一覧（保護者が確認した回ごと、020・023・024・026〜029）。新しい回は末尾に足す（前の回の問題セットを変えないため）
/// </summary>
public static class KanjiBatches
{
    public static readonly IReadOnlyList<KanjiItem[]> All = new[] { KanjiBatch1.Items, KanjiBatch2.Items, KanjiBatch3.Items, KanjiBatch4.Items, KanjiBatch5.Items, KanjiBatch6.Items, KanjiBatch7.Items };

    /// <summary>
    /// すべての回の字（回の順）
    /// </summary>
    public static IEnumerable<KanjiItem> Items => All.SelectMany(batch => batch);
}
