# Feature Specification: 漢字の4回目（小3の次の30字）

**Feature Branch**: `feature/026-kanji-batch4`
**Created**: 2026年10月30日
**Status**: Implemented
**Input**: 024 の続き

## 決めたこと

- 中身は [kanji-batch4.md](./kanji-batch4.md)（保護者確認済み）。1〜3回目と同じ形・同じ方針。30字すべてに ことばの問題がある
- 配当表と 読みは、文部科学省の表で確かめた（[grade-check.md](../020-kanji/grade-check.md) と同じやり方）
- むずかしさは 025 の決まりのまま（入れた順に 20字ずつ）。4回目の30字は むずかしさ5の残り10字と、むずかしさ6の20字に入る
- 前の回の問題と学習記録は変えない。4回目は問題セットの末尾に足す

## Tasks

- [x] T001 テスト: 4回分の一覧と問題が同じ中身、回をまたいで重ならない、むずかしさが 20字ずつ - backend/tests/GamifiedMathDrill.Tests.Unit/KanjiProblemGeneratorTests.cs
- [x] T002 `KanjiBatch4`（一覧からスクリプトで作る）、`KanjiBatches` に足す
- [x] T003 3回目までの問題セットが変わらないこと、3回目までが入った DB で起動して4回目の90問だけが足されること
