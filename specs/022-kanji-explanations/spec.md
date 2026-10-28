# Feature Specification: 漢字の解説

**Feature Branch**: `feature/022-kanji-explanations`
**Created**: 2026年10月28日
**Status**: Implemented
**Input**: 020 で残した「漢字で まちがえたときの解説」

## 決めたこと

- くわしさは「正しい答え ＋ ことばの例」。文の型は [explanations.md](./explanations.md)（保護者確認済み）
- 中身（字・読み・ことば）は確認済みの kanji-batch1.md（`KanjiBatch1`）から使い、新しく書くのは型だけ
- よくある まちがいの一言は、確実に見分けられるものだけ（読みの送りがな、ことばで ちがう字が1つ）。書きは一言なし
- 解説は算数と同じく、回答の結果の `explanation`・`hint` に入る（不正解のときだけ）

## Tasks

- [x] T001 テスト: 全漢字問題で解説が作られる、型どおりの文、一言はその まちがい方のときだけ - backend/tests/GamifiedMathDrill.Tests.Unit/ExplanationBuilderTests.cs
- [x] T002 `ExplanationBuilder.Kanji`、`Build` に文字の答えを渡す
- [x] T003 結合テスト・E2E（まちがえたら「かんがえかた」が出る）
