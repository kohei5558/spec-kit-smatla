# Implementation Tasks: 問題を起動時に最新のセットに合わせる

**Feature**: 011-problem-sync
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月10日

## 設計メモ

- `Problem.IsActive`（bool）を追加。マイグレーションでは既存の問題を true にする（`defaultValue: true`）
- `Infrastructure/Data/Seed/ProblemSynchronizer.SyncAsync(context, desired)` が起動時に「計算の種類・難易度・問題文」で照合し、追加・使わない状態にする・戻す・正解を直す。同じ問題が複数あれば ID の小さい方を使う。変化がなければ保存しない
- `DatabaseInitializer` はレベルを空のときだけ投入し、問題は毎回 `ProblemSynchronizer` で合わせる。結果はログに出す
- `ProblemRepository.GetRandomProblemAsync` / `GetProblemsByDifficultyAsync` と `DailyChallengeService` は `IsActive` の問題だけを選ぶ

## Phase 1: 実装（US1・US2）

- [x] T001 [US1][US2] テスト: 空の DB・2回目は変化なし、古い問題は使わない状態で学習記録は残る、セットに戻ったら同じ ID で戻る、難易度が変わったら別の問題、正解の修正と重なりの解消、出題は使う問題だけ - backend/tests/GamifiedMathDrill.Tests.Integration/ProblemSynchronizerTests.cs
- [x] T002 [US2] デイリーチャレンジが使わない問題を選ばないテスト - backend/tests/GamifiedMathDrill.Tests.Unit/DailyChallengeProblemSelectionTests.cs
- [x] T003 [US1][US2] Problem.IsActive、ProblemSynchronizer、DatabaseInitializer、出題・チャレンジの絞り込み - backend/src/
- [x] T004 [US1] マイグレーション AddProblemIsActive を SQLite・PostgreSQL に追加（既存の問題は true）

## Phase 2: 仕上げ

- [x] T005 全テスト。008 より前の問題（700問）と学習記録が入った PostgreSQL の DB、最新の SQLite の DB で起動して確認（追加・使わない状態の数、学習記録・ポイントが変わらないこと、2回目は変化なし、API が使う問題だけを出すこと）
- [x] T006 README・CLAUDE.md を更新
