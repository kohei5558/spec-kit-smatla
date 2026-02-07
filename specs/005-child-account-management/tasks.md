# Implementation Tasks: 子供アカウント管理画面

**Feature**: 005-child-account-management  
**Branch**: `005-child-account-management`  
**Created**: 2026年2月7日  
**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

---

## Task Format & Execution Guide

**Checklist Format** (MANDATORY):

```
- [ ] [TaskID] [P?] [Story?] Description with file path
```

**Legend**:

- `[P]` = Parallelizable（他タスクと並行実行可能）
- `[US1]` = User Story 1、`[US2]` = User Story 2、など
- 故意にマーカーなし = 他のタスクに依存、順次実行が必要

**Execution Priority**:

1. **Setup Phase** (Phase 1) - プロジェクト基盤
2. **Foundational Phase** (Phase 2) - すべてのストーリーの前提条件
3. **User Story 1 (P1)** (Phase 3) - MVP: 子供アカウント作成とログイン
4. **User Story 2 & 3 (P2)** (Phase 4-5) - 編集と学習状況確認
5. **User Story 4 & 5 (P3)** (Phase 6-7) - 停止と削除
6. **Polish Phase** (Phase 8) - 横断的改善

---

## Dependency Graph（ユーザーストーリー完了順序）

```
Phase 1: Setup
    ↓
Phase 2: Foundational
    ↓
Phase 3: [US1] 子供アカウント作成・ログイン (P1) ← MVP
    ↓
    ├─→ Phase 4: [US2] アカウント編集 (P2)
    └─→ Phase 5: [US3] 学習状況確認 (P2)
            ↓
            ├─→ Phase 6: [US4] 一時停止 (P3)
            └─→ Phase 7: [US5] アカウント削除 (P3)
                    ↓
                Phase 8: Polish & Cross-Cutting
```

**Independent Testing**:

- Phase 3完了後: 子供アカウント作成→カードでログインのMVPフローがテスト可能
- Phase 4完了後: アカウント編集（PIN変更含む）が独立してテスト可能
- Phase 5完了後: 学習統計表示が独立してテスト可能

---

## Phase 1: Setup（プロジェクト基盤）

**Goal**: データベーススキーマ、プリセットアバター、基本インフラをセットアップ

**Test**: マイグレーション適用後、PresetAvatarテーブルに10〜20レコードが存在することを確認

### Tasks

- [x] T001 [P] データベースマイグレーション作成 - backend/src/GamifiedMathDrill.Infrastructure/Migrations/20260207_AddChildAccountManagementFields.cs
- [x] T002 [P] ApplicationUserにGradeLevelとIsActiveフィールド追加 - backend/src/GamifiedMathDrill.Infrastructure/Identity/ApplicationUser.cs
- [x] T003 [P] PresetAvatarエンティティ作成 - backend/src/GamifiedMathDrill.Core/Models/PresetAvatar.cs
- [x] T004 [P] ApplicationDbContextにPresetAvatars DbSet追加 - backend/src/GamifiedMathDrill.Infrastructure/Data/ApplicationDbContext.cs
- [x] T005 AvatarSeeder作成（10〜20種類のプリセットデータ） - backend/src/GamifiedMathDrill.Infrastructure/Data/Seed/AvatarSeeder.cs
- [x] T006 マイグレーション適用とシードデータ確認 - `dotnet ef database update`
- [x] T007 [P] プリセットアバター画像ファイル配置 - frontend/wwwroot/avatars/\*.png（10〜20ファイル）

---

## Phase 2: Foundational（前提条件）

**Goal**: すべてのユーザーストーリーで使用する共通のDTO、サービス、リポジトリを実装

**Test**: PresetAvatarリポジトリで全アバター一覧取得、ChildAccountServiceの依存注入が正常動作することを確認

### Tasks

- [x] T008 [P] ChildAccountCreateDto作成 - backend/src/GamifiedMathDrill.Api/DTOs/ChildAccountCreateDto.cs
- [x] T009 [P] ChildAccountUpdateDto作成 - backend/src/GamifiedMathDrill.Api/DTOs/ChildAccountUpdateDto.cs
- [x] T010 [P] ChildAccountDto作成 - backend/src/GamifiedMathDrill.Api/DTOs/ChildAccountDto.cs
- [x] T011 [P] ChildLearningStatsDto作成 - backend/src/GamifiedMathDrill.Api/DTOs/ChildLearningStatsDto.cs
- [x] T012 [P] PresetAvatarDto作成 - backend/src/GamifiedMathDrill.Api/DTOs/PresetAvatarDto.cs
- [x] T013 [P] IChildAccountServiceインターフェース作成 - backend/src/GamifiedMathDrill.Core/Interfaces/IChildAccountService.cs
- [x] T014 ChildAccountRepositoryの実装（基本CRUD） - backend/src/GamifiedMathDrill.Infrastructure/Repositories/PresetAvatarRepository.cs
- [x] T015 ChildAccountService実装（ビジネスロジック骨組み） - backend/src/GamifiedMathDrill.Core/Services/ChildAccountService.cs
- [x] T016 Program.csにDI登録追加 - backend/src/GamifiedMathDrill.Api/Program.cs

---

## Phase 3: User Story 1 - 子供アカウント作成・ログイン (P1) 🎯 MVP

**Goal**: 保護者が子供アカウントを作成し、子供がカード形式UIでPINログインできる

**Independent Test**:

- 保護者で子供アカウント「太郎」を作成
- ログアウト後、子供ログイン画面で太郎のカードが表示される
- PIN「1234」でログイン成功し、子供ホーム画面が表示される

### Backend Tasks

- [ ] T017 [US1] ChildAccountController.ListAsync実装（GET /api/child-accounts） - backend/src/GamifiedMathDrill.Api/Controllers/ChildAccountController.cs
- [ ] T018 [US1] ChildAccountService.ListAsync実装（保護者IDで子供一覧取得） - backend/src/GamifiedMathDrill.Core/Services/ChildAccountService.cs
- [ ] T019 [US1] ChildAccountController.CreateAsync実装（POST /api/child-accounts） - backend/src/GamifiedMathDrill.Api/Controllers/ChildAccountController.cs
- [ ] T020 [US1] ChildAccountService.CreateAsync実装（ApplicationUser+Student作成、PINハッシュ化、重複名チェック、10件上限チェック） - backend/src/GamifiedMathDrill.Core/Services/ChildAccountService.cs
- [ ] T021 [US1] PresetAvatarController.ListAsync実装（GET /api/preset-avatars） - backend/src/GamifiedMathDrill.Api/Controllers/PresetAvatarController.cs
- [ ] T022 [US1] ChildAuthController.LoginAsync実装（POST /api/auth/child/login、PINハッシュ検証、3回ロックアウト） - backend/src/GamifiedMathDrill.Api/Controllers/ChildAuthController.cs
- [ ] T023 [US1] PINロックアウト機能実装（IMemoryCache使用、5分自動解除） - backend/src/GamifiedMathDrill.Core/Services/ChildAccountService.cs

### Frontend Tasks

- [ ] T024 [P] [US1] ChildAccountViewModel作成 - frontend/GamifiedMathDrill.Client/Models/ChildAccountViewModel.cs
- [ ] T025 [P] [US1] ChildAccountServiceクライアント実装（API呼び出しラッパー） - frontend/GamifiedMathDrill.Client/Services/ChildAccountService.cs
- [ ] T026 [US1] ChildAccountManagement.razor実装（子供一覧表示、新規追加ボタン） - frontend/GamifiedMathDrill.Client/Pages/Parent/ChildAccountManagement.razor
- [ ] T027 [US1] CreateChildAccount.razor実装（名前、学年、アバター選択、PIN入力フォーム） - frontend/GamifiedMathDrill.Client/Pages/Parent/CreateChildAccount.razor
- [ ] T028 [P] [US1] AvatarSelector.razorコンポーネント実装（プリセット一覧、グリッド表示） - frontend/GamifiedMathDrill.Client/Components/AvatarSelector.razor
- [ ] T029 [P] [US1] PINInput.razorコンポーネント実装（4桁数字入力、マスク表示） - frontend/GamifiedMathDrill.Client/Components/PINInput.razor
- [ ] T030 [US1] 子供ログイン画面のカード形式UI実装（ChildAccountCard.razorコンポーネント、CSS Grid） - frontend/GamifiedMathDrill.Client/Components/ChildAccountCard.razor
- [ ] T031 [US1] 子供ログイン画面実装（カード一覧、PIN入力、ログイン処理） - frontend/GamifiedMathDrill.Client/Pages/Auth/ChildLogin.razor

### Tests

- [ ] T032 [US1] ChildAccountControllerの統合テスト（作成、一覧取得） - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountTests.cs
- [ ] T033 [US1] 子供ログインの統合テスト（カード選択、PIN認証） - backend/tests/GamifiedMathDrill.Tests.Integration/ChildLoginTests.cs
- [ ] T034 [US1] PINロックアウトの統合テスト（3回失敗、5分解除） - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountSecurityTests.cs
- [ ] T035 [US1] バリデーションテスト（重複名、PIN形式、10件上限） - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountTests.cs

---

## Phase 4: User Story 2 - アカウント編集 (P2)

**Goal**: 保護者が子供アカウント情報（名前、学年、アバター、PIN）を編集できる

**Independent Test**:

- 既存の子供アカウント「太郎」を選択
- 学年を3年生→4年生、アバターを猫→犬に変更
- 保存後、一覧で変更が反映されていることを確認
- PIN変更後、新PINでログイン成功を確認

### Backend Tasks

- [ ] T036 [US2] ChildAccountController.GetAsync実装（GET /api/child-accounts/{id}） - backend/src/GamifiedMathDrill.Api/Controllers/ChildAccountController.cs
- [ ] T037 [US2] ChildAccountService.GetAsync実装（ID検索、保護者権限チェック） - backend/src/GamifiedMathDrill.Core/Services/ChildAccountService.cs
- [ ] T038 [US2] ChildAccountController.UpdateAsync実装（PUT /api/child-accounts/{id}） - backend/src/GamifiedMathDrill.Api/Controllers/ChildAccountController.cs
- [ ] T039 [US2] ChildAccountService.UpdateAsync実装（ApplicationUser+Student同期更新、PINハッシュ化、重複名チェック） - backend/src/GamifiedMathDrill.Core/Services/ChildAccountService.cs

### Frontend Tasks

- [ ] T040 [US2] EditChildAccount.razor実装（編集フォーム、既存値プリセット、保存処理） - frontend/GamifiedMathDrill.Client/Pages/Parent/EditChildAccount.razor
- [ ] T041 [US2] ChildAccountManagementに「編集」ボタン追加、編集画面への遷移 - frontend/GamifiedMathDrill.Client/Pages/Parent/ChildAccountManagement.razor

### Tests

- [ ] T042 [US2] アカウント更新の統合テスト（名前、学年、アバター変更） - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountTests.cs
- [ ] T043 [US2] PIN変更の統合テスト（新PINでログイン成功） - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountTests.cs

---

## Phase 5: User Story 3 - 学習状況確認 (P2)

**Goal**: 保護者が子供の学習統計（総問題数、正答率、ポイント、連続日数、過去7日間アクティビティ）を確認できる

**Independent Test**:

- 子供アカウント詳細画面を開く
- 学習統計サマリーが表示される
- 学習記録がない場合「まだ学習記録がありません」メッセージが表示される

### Backend Tasks

- [ ] T044 [P] [US3] ChildAccountController.GetDetailAsync実装（GET /api/child-accounts/{id}/detail、学習統計含む） - backend/src/GamifiedMathDrill.Api/Controllers/ChildAccountController.cs
- [ ] T045 [US3] ChildAccountService.GetLearningStatsAsync実装（Studentモデルから統計集計、過去7日間アクティビティ） - backend/src/GamifiedMathDrill.Core/Services/ChildAccountService.cs

### Frontend Tasks

- [ ] T046 [US3] ChildDetail.razor実装（基本情報 + 学習統計サマリー表示） - frontend/GamifiedMathDrill.Client/Pages/Parent/ChildDetail.razor
- [ ] T047 [US3] ChildAccountManagementに「詳細」ボタン追加、詳細画面への遷移 - frontend/GamifiedMathDrill.Client/Pages/Parent/ChildAccountManagement.razor
- [ ] T048 [P] [US3] LearningStatsChart.razorコンポーネント実装（過去7日間の棒グラフ） - frontend/GamifiedMathDrill.Client/Components/LearningStatsChart.razor

### Tests

- [ ] T049 [US3] 学習統計取得の統合テスト（データあり/なし両方） - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountTests.cs

---

## Phase 6: User Story 4 - 一時停止 (P3)

**Goal**: 保護者が子供アカウントを一時停止/再開でき、停止中はログイン不可

**Independent Test**:

- 子供アカウントを停止
- 子供でログイン試行→「アカウントは現在使用できません」メッセージ
- 再開後、ログイン成功を確認

### Backend Tasks

- [ ] T050 [P] [US4] ChildAccountController.SuspendAsync実装（POST /api/child-accounts/{id}/suspend） - backend/src/GamifiedMathDrill.Api/Controllers/ChildAccountController.cs
- [ ] T051 [P] [US4] ChildAccountController.ActivateAsync実装（POST /api/child-accounts/{id}/activate） - backend/src/GamifiedMathDrill.Api/Controllers/ChildAccountController.cs
- [ ] T052 [US4] ChildAccountService.Suspend/ActivateAsync実装（IsActiveフラグ切り替え） - backend/src/GamifiedMathDrill.Core/Services/ChildAccountService.cs
- [ ] T053 [US4] ChildAuthController.LoginAsyncにIsActiveチェック追加 - backend/src/GamifiedMathDrill.Api/Controllers/ChildAuthController.cs

### Frontend Tasks

- [ ] T054 [US4] ChildAccountManagementに「停止」「再開」ボタン追加（ステータスに応じて表示切替） - frontend/GamifiedMathDrill.Client/Pages/Parent/ChildAccountManagement.razor
- [ ] T055 [US4] 停止確認ダイアログ実装 - frontend/GamifiedMathDrill.Client/Pages/Parent/ChildAccountManagement.razor

### Tests

- [ ] T056 [US4] アカウント停止/再開の統合テスト - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountTests.cs
- [ ] T057 [US4] 停止中ログイン拒否の統合テスト - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountSecurityTests.cs

---

## Phase 7: User Story 5 - アカウント削除 (P3)

**Goal**: 保護者が子供アカウントを完全削除でき、学習データも連鎖削除される

**Independent Test**:

- 子供アカウントを削除（確認ダイアログで警告表示）
- 削除後、一覧から消えることを確認
- データベースからApplicationUser、Student、LearningRecordsが削除されることを確認

### Backend Tasks

- [ ] T058 [US5] ChildAccountController.DeleteAsync実装（DELETE /api/child-accounts/{id}） - backend/src/GamifiedMathDrill.Api/Controllers/ChildAccountController.cs
- [ ] T059 [US5] ChildAccountService.DeleteAsync実装（ApplicationUser削除、Student連鎖削除、LearningRecords連鎖削除） - backend/src/GamifiedMathDrill.Core/Services/ChildAccountService.cs

### Frontend Tasks

- [ ] T060 [US5] ChildAccountManagementに「削除」ボタン追加 - frontend/GamifiedMathDrill.Client/Pages/Parent/ChildAccountManagement.razor
- [ ] T061 [US5] 削除確認ダイアログ実装（警告メッセージ「学習データも全て削除されます」） - frontend/GamifiedMathDrill.Client/Pages/Parent/ChildAccountManagement.razor

### Tests

- [ ] T062 [US5] アカウント削除の統合テスト（連鎖削除確認） - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountTests.cs
- [ ] T063 [US5] 削除後ログイン不可の統合テスト - backend/tests/GamifiedMathDrill.Tests.Integration/ChildAccountSecurityTests.cs

---

## Phase 8: Polish & Cross-Cutting Concerns（最終調整）

**Goal**: エラーハンドリング、パフォーマンス最適化、UIポリッシュ

**Test**: すべてのエッジケースが正しく処理され、エラーメッセージがユーザーフレンドリーであることを確認

### Tasks

- [ ] T064 [P] 同一PIN警告メッセージ実装（CreateChildAccountとEditChildAccountフォーム） - frontend/GamifiedMathDrill.Client/Pages/Parent/CreateChildAccount.razor, EditChildAccount.razor
- [ ] T065 [P] デフォルトアバター自動設定ロジック追加（アバター未選択時） - backend/src/GamifiedMathDrill.Core/Services/ChildAccountService.cs
- [ ] T066 [P] 一覧表示パフォーマンス最適化（ParentId+Roleインデックス確認） - backend/src/GamifiedMathDrill.Infrastructure/Data/ApplicationDbContext.cs
- [ ] T067 [P] エラーハンドリング統一（BusinessRuleViolationException、ValidationException） - backend/src/GamifiedMathDrill.Api/Middleware/ErrorHandlingMiddleware.cs
- [ ] T068 [P] ChildAccountCard.razorのCSS調整（レスポンシブ、子供に優しいデザイン） - frontend/GamifiedMathDrill.Client/Components/ChildAccountCard.razor.css
- [ ] T069 [P] アクセシビリティ改善（aria-label、キーボードナビゲーション） - frontend/GamifiedMathDrill.Client/Components/\*.razor
- [ ] T070 統合テスト全実行と回帰テスト - backend/tests/GamifiedMathDrill.Tests.Integration/
- [ ] T071 quickstart.mdの動作確認（30分セットアップガイド） - specs/005-child-account-management/quickstart.md

---

## Task Summary

| Phase                 | Task Count | Parallelizable | Story Label |
| --------------------- | ---------- | -------------- | ----------- |
| Phase 1: Setup        | 7          | 6              | -           |
| Phase 2: Foundational | 9          | 7              | -           |
| Phase 3: US1 (P1) MVP | 19         | 4              | [US1]       |
| Phase 4: US2 (P2)     | 8          | 0              | [US2]       |
| Phase 5: US3 (P2)     | 6          | 2              | [US3]       |
| Phase 6: US4 (P3)     | 8          | 3              | [US4]       |
| Phase 7: US5 (P3)     | 6          | 0              | [US5]       |
| Phase 8: Polish       | 8          | 7              | -           |
| **Total**             | **71**     | **29** (41%)   | 5 Stories   |

---

## Parallel Execution Examples

### Phase 1 (Setup) - 6タスクを並行実行可能

```bash
# 同時に実行可能
T001: マイグレーション作成
T002: ApplicationUser拡張
T003: PresetAvatarエンティティ作成
T004: DbContext追加
T007: アバター画像ファイル配置

# その後、順次実行
T005: AvatarSeeder（T003, T004に依存）
T006: マイグレーション適用（T001, T005に依存）
```

### Phase 3 (US1) - バックエンドとフロントエンドを並行開発

```bash
# バックエンドチーム
T017-T023: API実装（7タスク、一部順次）

# フロントエンドチーム（並行実行）
T024: ViewModelT025: ServiceクライアントT028: AvatarSelectorコンポーネント
T029: PINInputコンポーネント

# バックエンド完了後、フロントエンド統合
T026, T027, T030, T031: Razorページ実装

# 両方完了後、テスト
T032-T035: 統合テスト
```

---

## Implementation Strategy

### MVP First（Phase 3完了でデプロイ可能）

Phase 3（User Story 1）完了時点で、以下が動作する最小限のシステムが完成：

- 保護者が子供アカウントを作成
- 子供がカード形式UIでPINログイン
- PINロックアウト機能
- 基本的なバリデーション

**Phase 3完了後の判断ポイント**:

- MVPとして本番デプロイ可能か評価
- ユーザーフィードバック収集
- Phase 4-7の優先順位再評価

### Incremental Delivery（フェーズごとにリリース）

- **Phase 3 (US1)**: 初回リリース（MVP）
- **Phase 4 (US2)**: 編集機能追加リリース
- **Phase 5 (US3)**: 学習状況確認追加リリース
- **Phase 6-7 (US4-5)**: 停止・削除機能追加リリース
- **Phase 8**: 最終ポリッシュリリース

各フェーズ完了後、独立してテスト・デプロイ可能。

---

## Validation Checklist

実装完了後、以下を確認：

### Functional Requirements Coverage

- [ ] FR-001〜FR-015: すべての機能要件が実装済み
- [ ] 27の受け入れシナリオがすべてパス
- [ ] 8つのエッジケースが正しく処理される

### Success Criteria Achievement

- [ ] SC-001: アカウント作成3分以内
- [ ] SC-002: 一覧表示1秒以内
- [ ] SC-003: 編集反映1秒以内
- [ ] SC-004: 学習統計の理解しやすさ（ユーザーテスト）
- [ ] SC-005: 削除後データ残存なし
- [ ] SC-006: 停止中ログインエラーメッセージ明確
- [ ] SC-007: PINロックアウト5分自動解除

### Constitution Compliance

- [ ] User-Centric Design: 子供向けカードUI、保護者向けダッシュボード
- [ ] Data Privacy: PINハッシュ化、データ最小化
- [ ] Test-Driven: 統合テスト全パス
- [ ] Specification-First: 実装がspec.mdに準拠

---

**Status**: ✅ Tasks breakdown completed. Ready for implementation.

**Next Steps**: Phase 1（Setup）から開始し、順次実装を進める。Phase 3完了時点でMVPレビューを実施。
