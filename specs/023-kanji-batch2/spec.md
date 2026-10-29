# Feature Specification: 漢字の2回目（小3の次の30字）

**Feature Branch**: `feature/023-kanji-batch2`
**Created**: 2026年10月29日
**Status**: Implemented
**Input**: 020 の続き。子どもは楽しく使えているので、次の30字を足す

## 決めたこと

- 中身は [kanji-batch2.md](./kanji-batch2.md)（保護者確認済み）。1回目と同じ形・同じ方針
- ことばが思いつかなかった5字（拾・助・待・筆・美）は、ウェブで調べて保護者と決めた（拾い物・助手・期待・筆算・美人）
- むずかしさは 1回目と同じく 3字ずつ 1〜10。どのむずかしさにも 1回目の3字 ＋ 2回目の3字が並ぶ
- 1回目の問題（と学習記録）は変えない。2回目は問題セットの末尾に足す
- 解説（022）は そのまま2回目の字にも出る

## Tasks

- [x] T001 テスト: 2回分の一覧と問題が同じ中身、字・問題文・ことばの読みが回をまたいで重ならない - backend/tests/GamifiedMathDrill.Tests.Unit/KanjiProblemGeneratorTests.cs
- [x] T002 `KanjiBatch2`（一覧からスクリプトで作る）、`KanjiBatches`、`KanjiProblemGenerator` と解説が全部の回を使う
- [x] T003 1回目までの問題セット（2,378問）が変わらないこと、1回目が入った DB で起動して2回目の90問だけが足されること
