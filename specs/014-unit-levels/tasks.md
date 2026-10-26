# Implementation Tasks: 単元（計算の種類）ごとのレベル

**Feature**: 014-unit-levels
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月26日

## 設計メモ

### データ
- 新しい表 `StudentLevels`（`StudentLevel`）: `StudentId`、`CalculationType`、`LevelId`（Level への FK）、`CorrectAnswers`（その種類の連続正解数）。`(StudentId, CalculationType)` で一意。学習者を消したら一緒に消える
- `Student.CurrentLevelId`・`Student.CorrectAnswers`・`Student.CurrentLevel` は**削除**する（残すと「どちらが正しいか」で迷うため）
- マイグレーション `AddStudentLevels`（SQLite・PostgreSQL）: 表を作る → 既存の学習者ごとに、今の `CurrentLevelId`・`CorrectAnswers` を既存の5種類（0〜4）に写す SQL → `Students` から2列を消す。データを写す SQL は列を消す前に置く

### 種類ごとのレベルの決まり（Core）
- `Core/Models/StudentLevelRules.cs`
  - `GradeLeveledTypes`: 学年でレベルを決める種類（たし算・ひき算・かけ算・わり算・あまりのあるわり算）
  - `StartLevelFor(type, grade)`: 上の種類は `GradeStartLevel.ForGrade(grade)`、それ以外は 1
- `IStudentLevelRepository`: `GetAllAsync(studentId)`、`GetOrCreateAsync(studentId, type)`（無ければ**レベル1**で作る。学年でレベルを決める種類は、子供を作るときに行を作っておくため、ここで作るのは新しい単元か、古い作り方の学習者だけ）、`UpdateAsync`
- 子供を作るところ（`ChildAccountService.CreateAsync`、`UserSeeder`、`StudentService.CreateAsync`）で、学年でレベルを決める5種類の行を作る。`StudentService.CreateAsync`（子供ログイン時の自動作成）は学年が分からないので、今までどおりレベル1
- 学年を上げたとき（`ChildAccountService.UpdateAsync`）: 学年でレベルを決める種類のうち、学年のレベルより低い行だけ上げ、連続正解数を0にする

### 出題・回答（`ProblemService`）
- `GetNextProblemAsync`: 種類の指定があればその種類、「すべて」なら問題がある種類をランダムな順に試す。難易度の決め方（レベル内の進み具合で範囲の中を上がっていく方式）は今までどおりで、使うレベルと連続正解数をその種類のものにする
- `SubmitAnswerAsync`: その問題の種類の `StudentLevel` だけ連続正解数を増やす・0にする・レベルアップする。戻り値に上がった種類を足す
- `Student.TotalProblems`・`TotalPoints` は今までどおり学習者に1つ

### デイリーチャレンジ（`DailyChallengeService`）
- 対象の4種類（たし算・ひき算・かけ算・わり算）から1つをランダムに選び、その種類のレベルの最大難易度・その種類の問題から作る。その種類に問題がなければ、ほかの対象の種類を試す

### API
- `StudentDto`: `CurrentLevelId`・`CurrentLevelName`・`CurrentLevelMinDifficulty`・`CurrentLevelMaxDifficulty`・`CorrectAnswers` をやめ、`Levels`（種類・レベル・連続正解数・レベルアップに必要な数）と `HighestLevel` を返す。まだ行が無い種類も、始めるレベルで返す（すべての種類を返す）
- `AnswerResultDto`: `LeveledUpCalculationType` を足す
- 保護者の子供の詳細（`ChildLearningStatsDto`）: `Levels`（種類・レベル・回答数・正答率）を足す。既存の項目はそのまま（005 の contracts に追記）

### 画面
- ホーム: 「レベル ○」は `HighestLevel`。「連続正解」の枠は、種類ごとに分かれて1つに決まらないため外し、種類のボタン（`CategorySelector`）に「レベル7・あと3問でレベルアップ」を出す
- 問題画面: レベルと連続正解数は今の問題の種類のもの。レベルアップの演出は「たし算が レベル7 になったよ！」
- 保護者の子供の詳細: 種類ごとのレベル・回答数・正答率の表

## Phase 1: データとレベルの決まり

- [ ] T001 [US1][US2] テスト: 種類ごとに連続正解数・レベルアップが独立する／ほかの種類は変わらない／種類ごとのレベルの難易度で出題される／「すべて」でも問題の種類のレベルで出題・判定される - backend/tests/GamifiedMathDrill.Tests.Integration/UnitLevelTests.cs
- [ ] T002 [US2] テスト: 小3の子供は5種類がレベル5、新しい種類（行が無い種類）はレベル1／学年を上げると低い種類だけ上がる - 同上
- [ ] T003 [US1][US2] `StudentLevel`・`StudentLevelRules`・`IStudentLevelRepository`、`Student` から2列を削除、`ApplicationDbContext` の設定 - backend/src/
- [ ] T004 [US2] マイグレーション `AddStudentLevels`（SQLite・PostgreSQL、既存のレベルを5種類に写す）
- [ ] T005 [US1] `ProblemService`（出題・回答）、`DailyChallengeService` を種類ごとのレベルにする
- [ ] T006 [US2] 子供の作成（`ChildAccountService`・`UserSeeder`・`StudentService`）と学年変更で行を作る・上げる
- [ ] T007 既存テストのうち `CurrentLevelId`・`Student.CorrectAnswers` を使うもの（GradeStartLevel・DailyChallenge・CategorySelection・ParentDashboard など）を種類ごとのレベルに直す

## Phase 2: API と画面

- [ ] T008 [US3] テスト: 学習者の API が全種類のレベル・連続正解数・いちばん高いレベルを返す／回答結果に上がった種類が入る／保護者の子供の詳細に種類ごとのレベルと正答率が入る
- [ ] T009 [US3] `StudentDto`・`AnswerResultDto`・`ChildLearningStatsDto`、005 の contracts に追記
- [ ] T010 [US3] ホーム（いちばん高いレベル、種類のボタンにレベルと「あと○問」）、問題画面（今の種類のレベルと連続正解数、演出に種類の名前）、保護者の子供の詳細（種類ごとの表） - frontend/

## Phase 3: 仕上げ

- [ ] T011 移行の確認: 今の main で作った SQLite・PostgreSQL の DB（学習記録・レベルあり）に新しい版で起動し、5種類に今のレベルが写ること、出題される難易度が変わらないこと
- [ ] T012 画面の確認（Playwright）: 種類ごとにレベルを変えた子供でホーム・問題画面・レベルアップ・保護者の子供の詳細
- [ ] T013 README・CLAUDE.md（`Student.CorrectAnswers` の記述など）を更新
