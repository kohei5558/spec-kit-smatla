# Implementation Tasks: 景品の家庭ごとの管理

**Feature**: 007-family-rewards
**Spec**: [spec.md](./spec.md)
**Created**: 2026年10月10日

## Format: `[ID] [P?] [Story] Description - file path`

## 設計メモ

- `Reward.ParentId`（string?, インデックスあり）を追加。既存の行は null のまま（どの家庭にも表示しない）。保護者の削除機能はないため外部キーは張らない
- ログイン中のユーザーの家庭（保護者ID）は、保護者なら自分の ID、子供なら子供ユーザーの `ParentId`。`IStudentAccessService.GetFamilyParentIdAsync` で解決する
- 他の家庭の景品は存在しない扱い（404）。交換・交換申請はコントローラーで景品の家庭を確認してからサービスを呼ぶ
- 初期景品は `RewardSeeder.GetRewards()` をテンプレートとして扱い、ID を振り直して家庭ごとにコピーする（`IRewardService.CopyStarterRewardsAsync`）
- フロントエンドは API が家庭で絞り込むため変更不要

---

## Phase 1: Foundational

- [x] T001 Reward に ParentId を追加 - backend/src/GamifiedMathDrill.Core/Models/Reward.cs
- [x] T002 ApplicationDbContext に ParentId のインデックスを追加 - backend/src/GamifiedMathDrill.Infrastructure/Data/ApplicationDbContext.cs
- [x] T003 EF マイグレーション AddRewardParentId 追加 - backend/src/GamifiedMathDrill.Infrastructure/Migrations/
- [x] T004 IStudentAccessService に GetFamilyParentIdAsync を追加・実装 - backend/src/GamifiedMathDrill.Core/Interfaces/IStudentAccessService.cs, backend/src/GamifiedMathDrill.Infrastructure/Services/StudentAccessService.cs

## Phase 2: User Story 1・2 - 家庭ごとの景品の閲覧・管理・交換 (P1) 🎯 MVP

### Tests（先に書く）

- [x] T005 [P] [US1] 保護者が自分の家庭の景品だけ一覧・詳細・更新・削除できるテスト（他家庭は404） - backend/tests/GamifiedMathDrill.Tests.Integration/FamilyRewardsTests.cs
- [x] T006 [P] [US2] 子供が自分の家庭の景品だけ一覧・交換・交換申請できるテスト（他家庭は404、ポイント・在庫が変わらない） - backend/tests/GamifiedMathDrill.Tests.Integration/FamilyRewardsTests.cs
- [x] T007 [P] [US2] 同じ初期景品でも交換で他家庭の在庫が減らないテスト - backend/tests/GamifiedMathDrill.Tests.Integration/FamilyRewardsTests.cs

### Implementation

- [x] T008 [US1] RewardService.GetRewardsAsync を家庭で絞り込み、CreateRewardAsync で ParentId を設定 - backend/src/GamifiedMathDrill.Core/Services/RewardService.cs, backend/src/GamifiedMathDrill.Core/Interfaces/IRewardService.cs
- [x] T009 [US1] RewardsController の一覧・詳細・登録・更新・削除を家庭で制限 - backend/src/GamifiedMathDrill.Api/Controllers/RewardsController.cs
- [x] T010 [US2] RewardsController の交換、ExchangeRequestsController の交換申請で景品の家庭を確認 - backend/src/GamifiedMathDrill.Api/Controllers/RewardsController.cs, backend/src/GamifiedMathDrill.Api/Controllers/ExchangeRequestsController.cs
- [x] T011 [US1] 保護者ダッシュボードの景品集計を家庭で絞り込み - backend/src/GamifiedMathDrill.Core/Services/ParentDashboardService.cs

## Phase 3: User Story 3 - 初期景品のコピー (P2)

### Tests（先に書く）

- [x] T012 [P] [US3] 新規登録した保護者に初期景品20種類があり、家庭ごとに独立して編集できるテスト - backend/tests/GamifiedMathDrill.Tests.Integration/FamilyRewardsTests.cs

### Implementation

- [x] T013 [US3] IRewardService.CopyStarterRewardsAsync を実装 - backend/src/GamifiedMathDrill.Core/Services/RewardService.cs
- [x] T014 [US3] 保護者の新規登録時に初期景品をコピー（失敗しても登録は成功させる） - backend/src/GamifiedMathDrill.Api/Controllers/AuthController.cs
- [x] T015 [US3] DB 初期化時の全家庭共通の景品作成をやめ、デモ保護者の作成時にコピー - backend/src/GamifiedMathDrill.Infrastructure/Data/DatabaseInitializer.cs, backend/src/GamifiedMathDrill.Infrastructure/Data/Seed/UserSeeder.cs

## Phase 4: 仕上げ

- [x] T016 既存テスト（景品・交換申請・ダッシュボード）を家庭単位のデータに合わせて更新し、全テスト実行
- [x] T017 開発環境で手動確認（2家庭で景品の分離・初期景品・交換）
- [x] T018 CLAUDE.md を更新 - CLAUDE.md

## 依存関係

- Phase 1 → Phase 2 → Phase 3 → Phase 4
