# Implementation Tasks: 文章問題

**Feature**: 017-word-problems
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月26日

## 設計メモ

- `CalculationType.WordProblem = 7` を末尾に追加（レベル1から、デイリーチャレンジは対象外）
- 答えの形は、答えが1つなら 015 の `AnswerFormat.Units`（`AnswerUnits` = "本" など）。あまりのあるわり算は `AnswerFormat.UnitsRemainder = 8`（`AnswerUnits` = "こ こ"、"人 まい"。画面は「[ ]こ あまり [ ]こ」）
- DB の形は変わらない（enum の値が増えるだけ）ので、マイグレーションは無し
- `WordProblemGenerator`: templates.md のひな形20個を、難易度ごとの数の条件つきで作る（長さ・重さ・かさのあと）
- 画面: `AnswerText` に `UnitsRemainder`、`ProblemDisplay` で「あまり」をはさむ・文章は小さめの文字、種類の名前・アイコン・ボタン・学習結果のタブ

## Tasks

- [ ] T001 [US1] テスト: 全問題の文章をひな形として読み直し、正解・欄の単位・難易度ごとのひな形と数の条件・問題数・重なり・長さ（100文字以内）を確認。既存の問題セットは変わらない - backend/tests/GamifiedMathDrill.Tests.Unit/WordProblemGeneratorTests.cs
- [ ] T002 [US1] `CalculationType.WordProblem`、`AnswerFormat.UnitsRemainder`、`WordProblemGenerator`
- [ ] T003 [US1] テスト: レベル1で出題・欄の単位／あまりありは両方合うときだけ正解／デイリーチャレンジに出ない - backend/tests/GamifiedMathDrill.Tests.Integration/WordProblemTests.cs
- [ ] T004 [US1][US2] 画面（欄と「あまり」、答えの表示、種類の名前・アイコン・ボタン・タブ）
- [ ] T005 移行の確認（今の main の DB で起動して問題が追加される）、画面の確認（Playwright）、README・CLAUDE.md
