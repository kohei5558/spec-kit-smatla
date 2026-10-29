# Feature Specification: 漢字の3回目（小3の次の30字）

**Feature Branch**: `feature/024-kanji-batch3`
**Created**: 2026年10月29日
**Status**: Implemented
**Input**: 023 の続き

## 決めたこと

- 中身は [kanji-batch3.md](./kanji-batch3.md)（保護者確認済み）。1回目・2回目と同じ形・同じ方針。30字すべてに ことばの問題がある（「取」は送りがな入りの「書き取り」）
- むずかしさは 3字ずつ 1〜10。どのむずかしさにも 1〜3回目の3字ずつ（9字）が並ぶ
- 前の回の問題と学習記録は変えない。3回目は問題セットの末尾に足す

## Tasks

- [x] T001 テスト: 3回分の一覧と問題が同じ中身、回をまたいで重ならない - backend/tests/GamifiedMathDrill.Tests.Unit/KanjiProblemGeneratorTests.cs
- [x] T002 `KanjiBatch3`（一覧からスクリプトで作る）、`KanjiBatches` に足す
- [x] T003 2回目までの問題セット（2,468問）が変わらないこと、2回目までが入った DB で起動して3回目の90問だけが足されること
