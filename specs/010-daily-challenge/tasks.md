# Implementation Tasks: デイリーチャレンジの作り直し

**Feature**: 010-daily-challenge
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月10日

## 設計メモ

- `DailyChallenge` に `StudentId`・`AnsweredAt`（`[ConcurrencyCheck]`）・`IsCorrect`・`StudentAnswer` を追加し、`IsActive` を削除。（`StudentId`, `TargetDate`）で一意
- チャレンジはその日に初めて取得したときに作る（`GetOrCreateTodaysChallengeAsync`）。夜中に作る `DailyChallengeJob` は削除
- 日付は `TimeProvider` の現在時刻 +9 時間（日本は夏時間がないため固定）
- 回答は先にチャレンジを回答済みとして保存し、同時実行で負けた回答（`DbUpdateConcurrencyException`）は 409。ポイントはそのあとで加算
- API: `GET /api/dailychallenges/today?studentId=`（保護者・子供）、`POST /api/dailychallenges/{id}/answer`（子供のみ）
- 画面: ホームのカード（未回答なら「挑戦する！」、回答済みなら結果）と専用画面 `/daily-challenge`

## Phase 1: API とデータ（US1・US2）

- [x] T001 [US1] 結合テスト: 子供ごと・レベルに合わせた問題、正解でボーナス1回・2回目は 409、不正解、保護者は答えられない、他家庭は 404 - backend/tests/GamifiedMathDrill.Tests.Integration/DailyChallengeTests.cs
- [x] T002 [US1][US2] 単体テスト: 最大難易度・あまりのあるわり算を除く・日本時間の日付・問題がないとき - backend/tests/GamifiedMathDrill.Tests.Unit/DailyChallengeProblemSelectionTests.cs
- [x] T003 [US1] モデル・リポジトリ・サービス・コントローラーを作り直し、DailyChallengeJob を削除 - backend/src/
- [x] T004 [US1] マイグレーション PerStudentDailyChallenge を SQLite・PostgreSQL に追加（既存のチャレンジは削除）

## Phase 2: 画面（US1）

- [x] T005 [US1] ホームのチャレンジカードを回答済みの表示に対応し、チャレンジ専用画面を追加 - frontend/GamifiedMathDrill.Client/Pages/DailyChallengePage.razor, Components/Home/DailyChallengeCard.razor

## Phase 3: 仕上げ

- [x] T006 全テスト、既存 DB（SQLite・PostgreSQL）からのマイグレーション、画面での確認（正解でボーナス、2回目は挑戦できない、1年生は難易度2、同時送信でもボーナスは1回）
- [x] T007 README・CLAUDE.md を更新
