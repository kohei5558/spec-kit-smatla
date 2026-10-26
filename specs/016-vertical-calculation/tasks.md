# Implementation Tasks: ひっ算の見た目

**Feature**: 016-vertical-calculation
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月26日

## 設計メモ

- 画面だけの変更（API・判定・問題の作り方は変えない）。`ProblemDisplay` が問題文（「345 + 278 = ?」）を読んで、ひっ算の形にするか決める
  - たし算・ひき算・かけ算: どちらかの数が2けた以上
  - わり算・あまりのあるわり算: 商が2けた以上（九九の逆は横書きのまま）
- 答えの欄は横書きと同じもの（`FirstInput` / `SecondInput`）を、線の下（わり算は厂の上・あまりは下）に置く
- 見た目は `ProblemDisplay.razor.css`（等幅の数字で一の位をそろえる、厂は枠線で描く）

## Tasks

- [x] T001 [US1] `ProblemDisplay` にひっ算の形（たし算・ひき算・かけ算のたて書き、わり算の厂）を追加 - frontend/GamifiedMathDrill.Client/Components/ProblemDisplay.razor(.css)
- [x] T002 [US1] 画面の確認（Playwright）: 5種類それぞれ、けたの多い問題がひっ算の形で出て、線の下の欄（Enter でも）に答えると正解になる。スマホの幅で横にはみ出さない
- [x] T003 README を更新

> 画面だけの変更のため、自動テストは追加していない（画面のテストの仕組みは Issue #30）。
