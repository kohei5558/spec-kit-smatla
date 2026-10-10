# Implementation Tasks: 問題の難易度を小3まで広げ、学年に応じたレベルから始める

**Feature**: 008-grade3-difficulty
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月10日

## 設計メモ

- 問題の生成を `Core/Services/ProblemGenerator.cs`（DB に依存しない静的クラス、乱数の種を引数で受け取る）に切り出し、`ProblemSeeder` はそれを呼ぶだけにする
- 学年→始めるレベルの対応は `Core/Models/GradeStartLevel.cs` にまとめる
- レベルの ID と LevelNumber は 1〜10 で一致している（LevelSeeder）ため、始めるレベルは CurrentLevelId に設定する

## Phase 1: User Story 1 - 小3の内容の問題 (P1)

- [ ] T001 [US1] 難易度表の各条件（桁数・くり上がり/くり下がり・割り切れる・答えが0以上・重複なし・各30問）を検証する単体テスト - backend/tests/GamifiedMathDrill.Tests.Unit/ProblemGeneratorTests.cs
- [ ] T002 [US1] ProblemGenerator を実装 - backend/src/GamifiedMathDrill.Core/Services/ProblemGenerator.cs
- [ ] T003 [US1] ProblemSeeder を ProblemGenerator に置き換え - backend/src/GamifiedMathDrill.Infrastructure/Data/Seed/ProblemSeeder.cs

## Phase 2: User Story 2 - 学年に応じたレベル (P1)

- [ ] T004 [US2] 学年→始めるレベルの単体テスト - backend/tests/GamifiedMathDrill.Tests.Unit/GradeStartLevelTests.cs
- [ ] T005 [US2] 子供アカウントの作成・学年変更でレベルが設定されることの結合テスト - backend/tests/GamifiedMathDrill.Tests.Integration/GradeStartLevelIntegrationTests.cs
- [ ] T006 [US2] GradeStartLevel を実装 - backend/src/GamifiedMathDrill.Core/Models/GradeStartLevel.cs
- [ ] T007 [US2] ChildAccountService の作成・更新でレベルを設定（デモの子供も含む） - backend/src/GamifiedMathDrill.Infrastructure/Services/ChildAccountService.cs, backend/src/GamifiedMathDrill.Infrastructure/Data/Seed/UserSeeder.cs

## Phase 3: 仕上げ

- [ ] T008 全テスト実行、開発環境で学年3年の子供の出題を手動確認
- [ ] T009 README（使ってみる）と CLAUDE.md を更新
