# Implementation Tasks: 国語（漢字）

**Feature**: 020-kanji　**Plan**: [plan.md](./plan.md)

## Phase 1: 問題を作る

- [x] T001 テスト: 漢字の問題が kanji-batch1.md どおり（字・例文・答え・選択肢の中身、正解が選択肢に1つ、むずかしさの分け方）。算数の問題セットは変わらない - backend/tests/GamifiedMathDrill.Tests.Unit/KanjiProblemGeneratorTests.cs
- [x] T002 `CalculationType`・`Subject`・`AnswerFormat`・`Problem`・`LearningRecord` のモデル、`KanjiBatch1`、`KanjiProblemGenerator`、`ProblemSynchronizer`
- [x] T003 マイグレーション `AddKanjiProblems`（SQLite・PostgreSQL）

## Phase 2: 出題と判定

- [x] T004 テスト: 教科ごとの出題（国語ミックスに算数が出ない・算数ミックスに漢字が出ない）、文字の答えの判定、API が正解を返さない、学習記録の文字の答え - backend/tests/GamifiedMathDrill.Tests.Integration/KanjiProblemTests.cs
- [x] T005 API（`subject`・`text`・`choices`・`correctText`・`studentText`）

## Phase 3: 画面

- [x] T006 ホームの教科の切りかえ、問題画面（強調・50音ボタン・選択肢）、回答の結果・学習記録・学習結果・保護者の画面
- [x] T007 E2E（読みを50音ボタンで答える・書きを選ぶ）

## Phase 4: 仕上げ

- [x] T008 今の main の DB（SQLite・PostgreSQL）で起動して、漢字の問題が追加され 算数の問題・学習記録が変わらないこと
- [x] T009 README・CLAUDE.md
