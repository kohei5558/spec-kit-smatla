# Feature Specification: 漢字の5回目（小3の次の30字）

**Feature Branch**: `feature/027-kanji-batch5`
**Created**: 2026年10月30日
**Status**: Draft（一覧の確認待ち）
**Input**: 026 の続き

## 決めたこと

- 中身は [kanji-batch5.md](./kanji-batch5.md)（保護者の確認待ち）。1〜4回目と同じ形・同じ方針。30字すべてに ことばの問題がある
- 配当表と 読みは、文部科学省の表で確かめた（[grade-check.md](../020-kanji/grade-check.md) と同じやり方）
- むずかしさは 025 の決まりのまま（入れた順に 20字ずつ）。5回目の30字は むずかしさ7の20字と、むずかしさ8の10字に入る
- 前の回の問題と学習記録は変えない。5回目は問題セットの末尾に足す

## Tasks

- [ ] T001 テスト: 5回分の一覧と問題が同じ中身、回をまたいで重ならない、むずかしさが 20字ずつ - backend/tests/GamifiedMathDrill.Tests.Unit/KanjiProblemGeneratorTests.cs
- [ ] T002 `KanjiBatch5`（一覧からスクリプトで作る）、`KanjiBatches` に足す
- [ ] T003 4回目までの問題セットが変わらないこと、4回目までが入った DB で起動して5回目の90問だけが足されること
