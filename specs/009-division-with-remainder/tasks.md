# Implementation Tasks: あまりのあるわり算

**Feature**: 009-division-with-remainder
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月10日

## 設計メモ

- 問題文は既存と同じ `17 ÷ 5 = ?` 形式。`Problem.CorrectAnswer` に商、`CorrectRemainder` にあまり
- `ProblemDto.HasRemainder`（bool）で画面が入力欄を2つにする。`SubmitAnswerDto.Remainder`（int?）、`AnswerResultDto.CorrectRemainder`（int?）を追加
- モデル変更のため SQLite・PostgreSQL 両方にマイグレーションを追加

## Phase 1: 問題の生成とデータ（US1）

- [x] T001 [US1] あまりのあるわり算の難易度表を検証する単体テストを追加 - backend/tests/GamifiedMathDrill.Tests.Unit/ProblemGeneratorTests.cs
- [x] T002 [US1] CalculationType に DivisionWithRemainder、Problem に CorrectRemainder、LearningRecord に StudentRemainder を追加 - backend/src/GamifiedMathDrill.Core/Models/
- [x] T003 [US1] ProblemGenerator にあまりのあるわり算を追加 - backend/src/GamifiedMathDrill.Core/Services/ProblemGenerator.cs
- [x] T004 [US1] マイグレーション AddDivisionWithRemainder を SQLite・PostgreSQL に追加 - backend/src/GamifiedMathDrill.Migrations.*/Migrations/

## Phase 2: 回答 API（US1）

- [x] T005 [US1] 結合テスト: 問題取得で HasRemainder が返る、商・あまりの両方が合うときだけ正解、学習記録にあまりが残る、他の種類はあまりを無視 - backend/tests/GamifiedMathDrill.Tests.Integration/DivisionWithRemainderTests.cs
- [x] T006 [US1] DTO（HasRemainder / Remainder / CorrectRemainder）と ProblemService.SubmitAnswerAsync・ProblemsController を対応 - backend/src/GamifiedMathDrill.Core/Models/DTOs/StudentDtos.cs, backend/src/GamifiedMathDrill.Core/Services/ProblemService.cs, backend/src/GamifiedMathDrill.Api/Controllers/ProblemsController.cs
- [x] T007 [US1] デイリーチャレンジの問題選択からあまりのあるわり算を除外（単体テスト付き） - backend/src/GamifiedMathDrill.Core/Services/DailyChallengeService.cs

## Phase 3: 画面（US1・US2）

- [ ] T008 [US1] 種類選択に「あまりのあるわり算」を追加 - frontend/GamifiedMathDrill.Client/Components/CategorySelector.razor
- [ ] T009 [US1] 問題画面に「あまり」の入力欄と正解表示を追加 - frontend/GamifiedMathDrill.Client/Pages/ProblemPage.razor, Models, Services
- [ ] T010 [US2] 学習結果・学習記録一覧・統計の表示名を追加し、あまりを含めて表示 - frontend/GamifiedMathDrill.Client/Pages/ResultsPage.razor, Components/Results/*

## Phase 4: 仕上げ

- [ ] T011 全テスト、開発環境で「あまりのあるわり算」を画面から解いて確認
- [ ] T012 README・CLAUDE.md を更新
