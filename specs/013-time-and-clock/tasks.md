# Implementation Tasks: 時こくと時間

**Feature**: 013-time-and-clock
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月26日

## 設計メモ

### 答えの形（`AnswerFormat`）
- `Core/Models/AnswerFormat.cs`（enum）: `Number`（今までの計算、単位なし）、`QuotientRemainder`（あまりのあるわり算）、`ClockTime`（[ ]時[ ]分）、`HoursMinutes`（[ ]時間[ ]分）、`Minutes`（[ ]分）、`MinutesSeconds`（[ ]分[ ]秒）、`Seconds`（[ ]秒）
- 2つの欄がある形は、2つ目の答えを今の「あまり」と同じ列で持つ（`Problem.CorrectRemainder`、`LearningRecord.StudentRemainder`、回答 API の `remainder`）。列名は変えない（既存のデータと API をそのまま使うため）
- 判定: `CorrectRemainder` を持つ問題は、1つ目と2つ目の両方が合うときだけ正解（今のあまりのあるわり算の判定を、種類ではなく「2つ目の答えがあるか」で行う）

### 問題
- `CalculationType.Time = 5` を末尾に追加（014 により、新しい種類はレベル1から）
- `Problem.AnswerFormat`（既定 `Number`）、`Problem.ClockTimes`（時計に出す時刻。`"3:40"` や `"9:50 10:30"`、無ければ null）を追加
- マイグレーション `AddTimeProblems`（SQLite・PostgreSQL）: 2列を追加し、既存のあまりのあるわり算の問題を `QuotientRemainder` にする
- `ProblemGenerator`: 種類リストの末尾に `Time` を足し、難易度表（spec.md）どおりに各難易度30問（組み合わせが30未満の難易度1は、ありうる問題すべて）。同じ難易度の中で問題文は重ならない
- `ProblemSynchronizer`: `AnswerFormat`・`ClockTimes` も正解と同じく、違えば直す

### API
- 問題（`ProblemDto`）: `AnswerFormat`（文字列）と `ClockTimes`（時刻のリスト）を足す。`HasRemainder` は2つ目の欄があるとき true（今の画面との互換）
- 回答結果: 今の `CorrectAnswer`・`CorrectRemainder` に加え `AnswerFormat` を返す（画面が「こたえは 4時5分」と表示するため）
- 学習記録: `AnswerFormat` を返す（一覧で「4時5分」と表示するため）
- デイリーチャレンジは四則だけなので、変更なし（014）

### 画面
- `Components/AnalogClock.razor`（新規）: SVG のアナログ時計（文字盤・数字・長針・短針。短針は分に応じて進める）
- `ProblemDisplay`: 時刻があれば時計を並べて表示。答えの欄と単位を `AnswerFormat` で切りかえる（例: [ ]時[ ]分）。文章の問題は文字を小さくする
- 答えの表示（回答結果・学習記録の一覧）: `AnswerFormat` に合わせて「4時5分」「1時間40分」「80分」「1分30秒」「90秒」「3 あまり 2」にする（`Models/AnswerText.cs`）
- 種類の名前（`CalculationTypeNames`）・アイコン・ホームの種類のボタン・学習結果のタブに「時こくと時間」を足す

## Phase 1: 問題を作る（US1）

- [ ] T001 [US1] テスト: 時こくの全問題が難易度表の条件を満たす（時刻 1時〜11時台・12時をまたがない・きざみ・時をまたぐかどうか・答えが正しい・答えの形・時計の時刻・問題文が重ならない・各難易度の数） - backend/tests/GamifiedMathDrill.Tests.Unit/TimeProblemGeneratorTests.cs
- [ ] T002 [US1] `AnswerFormat`、`CalculationType.Time`、`Problem.AnswerFormat`・`ClockTimes`、`ProblemGenerator` の時こく、`ProblemSynchronizer` - backend/src/
- [ ] T003 [US1] マイグレーション `AddTimeProblems`（SQLite・PostgreSQL、既存のあまりのあるわり算は `QuotientRemainder`）

## Phase 2: 出題と回答（US1）

- [ ] T004 [US1] テスト: 時こくを選ぶと時こくの問題がレベル1の難易度で出る／問題 API が答えの形と時刻を返す（正解は返さない）／「時・分」の問題は両方合うときだけ正解／答えが1つの問題は2つ目を無視する／回答結果と学習記録が答えの形を返す／デイリーチャレンジに出ない - backend/tests/GamifiedMathDrill.Tests.Integration/TimeProblemTests.cs
- [ ] T005 [US1] 判定を「2つ目の答えがあるか」にする、`ProblemDto`・回答結果・学習記録の API に答えの形と時刻を足す

## Phase 3: 画面（US1・US2）

- [ ] T006 [US1] `AnalogClock`、`ProblemDisplay`（時計・欄と単位）、答えの表示（`AnswerText`） - frontend/
- [ ] T007 [US2] 種類の名前・アイコン・ホームのボタン・学習結果のタブ・学習記録の一覧に「時こくと時間」

## Phase 4: 仕上げ

- [ ] T008 全テスト。今の main で作った SQLite・PostgreSQL の DB に新しい版で起動し、時こくの問題が追加され、既存の問題・学習記録が変わらないこと
- [ ] T009 画面の確認（Playwright）: 時こくを選んで、時計つきの問題・「時・分」の欄・正解／不正解の表示・学習結果
- [ ] T010 README・CLAUDE.md を更新
