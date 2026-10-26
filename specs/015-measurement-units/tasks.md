# Implementation Tasks: 長さ・重さ・かさ（単位）

**Feature**: 015-measurement-units
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月26日

## 設計メモ

- `CalculationType.Measurement = 6` を末尾に追加（014 により新しい種類はレベル1から。デイリーチャレンジは四則だけなので対象外のまま）
- 答えの形 `AnswerFormat.Units = 7`（欄の単位を問題ごとに持つ形）を追加し、欄の単位を `Problem.AnswerUnits`（"mm"、2つなら "km m"）に持つ。2つ目の答えは 013 と同じく `CorrectRemainder` / `StudentRemainder`
- マイグレーション `AddMeasurementProblems`（SQLite・PostgreSQL）: `AnswerUnits` を追加。`ProblemSynchronizer` は `AnswerUnits` も合わせる
- `MeasurementProblemGenerator`: 種類リストの末尾（時こくのあと）で作る。単位の組は 10倍・100倍（cm-mm、m-cm、L-dL、dL-mL）と 1000倍（km-m、kg-g、t-kg、L-mL）。1000倍の組の小さい単位は10きざみ（1250m、3050g など）
  - 問題文: 「3cmは 何mm？」「4cm5mmは 何mm？」「45mmは 何cm何mm？」「1km300m + 400mは 何km何m？」。0 の部分は書かない（「2km」）
- API: 問題・回答結果・学習記録に `answerUnits` を足す
- 画面: `AnswerText` に単位の形を足し（「1km250m」「3050g」、小さい単位が 0 なら「2km」）、`ProblemDisplay` の欄の単位、種類の名前・アイコン・ボタン・学習結果のタブ

## Phase 1: 問題を作る（US1）

- [x] T001 [US1] テスト: 全問題が難易度表の条件を満たす（単位の組と倍率、答えの範囲、くり上がり・くり下がり、正解、答えの形と単位、問題文が重ならない、各難易度30問）。既存の問題セットは変わらない - backend/tests/GamifiedMathDrill.Tests.Unit/MeasurementProblemGeneratorTests.cs
- [x] T002 [US1] `CalculationType.Measurement`、`AnswerFormat.Units`、`Problem.AnswerUnits`、`MeasurementProblemGenerator`、`ProblemSynchronizer`
- [x] T003 [US1] マイグレーション `AddMeasurementProblems`（SQLite・PostgreSQL）

## Phase 2: 出題と回答（US1）

- [x] T004 [US1] テスト: レベル1で出題／問題 API が答えの形と単位を返す（正解は返さない）／2つの欄は両方合うときだけ正解／回答結果・学習記録が単位を返す／デイリーチャレンジに出ない - backend/tests/GamifiedMathDrill.Tests.Integration/MeasurementProblemTests.cs
- [x] T005 [US1] 問題・回答結果・学習記録の API に `answerUnits`

## Phase 3: 画面（US1・US2）

- [x] T006 [US1] `AnswerText`（単位の形）、`ProblemDisplay`（欄の単位）、回答結果・学習記録の表示 - frontend/
- [x] T007 [US2] 種類の名前・アイコン・ホームのボタン・学習結果のタブと集計に「長さ・重さ・かさ」

## Phase 4: 仕上げ

- [x] T008 全テスト。今の main で作った SQLite・PostgreSQL の DB で起動し、問題が追加され既存の問題・学習記録が変わらないこと
- [x] T009 画面の確認（Playwright）
- [x] T010 README・CLAUDE.md を更新
