# Feature Specification: 漢字の6回目（小3の次の30字）

**Feature Branch**: `feature/028-kanji-batch6`
**Created**: 2026年10月30日
**Status**: Implemented
**Input**: 027 の続き（小3の のこり50字を 6回目30字・7回目20字に分ける）

## 決めたこと

- 中身は [kanji-batch6.md](./kanji-batch6.md)（保護者確認済み）。1〜5回目と同じ形・同じ方針。死・酒も入れる（保護者と相談済み）
- 小3までの字で よい ことばが 作れない字（界・君）は、ことばの問題を 作らない（1回目の 寒・湖と同じ）。30字で 88問
- 配当表と 読みは、文部科学省の表で確かめた（[grade-check.md](../020-kanji/grade-check.md) と同じやり方）
- むずかしさは 025 の決まりのまま（入れた順に 20字ずつ）。6回目の30字は むずかしさ8の残り10字と、むずかしさ9の20字に入る
- 前の回の問題と学習記録は変えない。6回目は問題セットの末尾に足す

## Tasks

- [x] T001 テスト: 6回分の一覧と問題が同じ中身、回をまたいで重ならない、むずかしさが 20字ずつ - backend/tests/GamifiedMathDrill.Tests.Unit/KanjiProblemGeneratorTests.cs
- [x] T002 `KanjiBatch6`（一覧からスクリプトで作る）、`KanjiBatches` に足す
- [x] T003 5回目までの問題セットが変わらないこと、5回目までが入った DB で起動して6回目の88問だけが足されること
