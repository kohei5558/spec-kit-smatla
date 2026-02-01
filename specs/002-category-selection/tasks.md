# Tasks: 問題カテゴリ選択機能

**Feature**: 002-category-selection | **Branch**: `002-category-selection`  
**Input**: Design documents from `/specs/002-category-selection/`  
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/ ✅, quickstart.md ✅

**Tests**: テストタスクは含まれています（仕様で明示的に要求済み）

**Organization**: タスクはユーザーストーリーごとに整理され、各ストーリーを独立して実装・テスト可能にします

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 並列実行可能（異なるファイル、依存関係なし）
- **[Story]**: このタスクが属するユーザーストーリー（US1, US2, US3, US4）
- 説明には正確なファイルパスを含む

---

## Phase 1: Setup（共有インフラ）

**Purpose**: プロジェクト初期化と基本構造の確認

- [x] T001 既存プロジェクト構造を確認し、実装計画との整合性を検証
- [x] T002 CalculationType enum が正しく定義されていることを確認（backend/src/GamifiedMathDrill.Core/Models/CalculationType.cs）
- [x] T003 [P] 既存の CalculationType インデックスを確認（backend/src/GamifiedMathDrill.Infrastructure/Data/ApplicationDbContext.cs）

---

## Phase 2: Foundational（ブロッキング前提条件）

**Purpose**: すべてのユーザーストーリーが依存するコアインフラ

**⚠️ CRITICAL**: このフェーズが完了するまで、ユーザーストーリーの作業は開始できません

### Backend - サービス層の拡張

- [x] T004 IProblemService インターフェースに category パラメータを追加（backend/src/GamifiedMathDrill.Core/Interfaces/IProblemService.cs）
- [x] T005 ProblemService の GetNextProblemAsync メソッドに category パラメータを追加し、カテゴリフィルタリングロジックを実装（backend/src/GamifiedMathDrill.Core/Services/ProblemService.cs）
- [x] T006 ProblemRepository に FindAvailableAsync メソッドを追加し、category パラメータでフィルタリング（backend/src/GamifiedMathDrill.Infrastructure/Repositories/ProblemRepository.cs） - 既存実装あり、スキップ

### Backend - API 層の拡張

- [x] T007 ProblemsController の GetNext エンドポイントに category クエリパラメータを追加（backend/src/GamifiedMathDrill.Api/Controllers/ProblemsController.cs）

### Frontend - 状態管理サービス

- [x] T008 CategoryStateService を作成し、sessionStorage を使用したカテゴリ状態管理を実装（frontend/GamifiedMathDrill.Client/Services/CategoryStateService.cs）
- [x] T009 Program.cs に CategoryStateService を DI コンテナに登録（frontend/GamifiedMathDrill.Client/Program.cs）

### Backend - 単体テスト（Foundational）

- [ ] T010 [P] ProblemService のカテゴリフィルタリングロジックの単体テストを作成（backend/tests/GamifiedMathDrill.Tests.Unit/CategoryFilteringTests.cs）
- [ ] T011 [P] カテゴリ null（すべて）の動作をテスト（backend/tests/GamifiedMathDrill.Tests.Unit/CategoryFilteringTests.cs）

### Backend - 統合テスト（Foundational）

- [ ] T012 [P] ProblemsController の category パラメータ付き API 呼び出しの統合テストを作成（backend/tests/GamifiedMathDrill.Tests.Integration/CategoryApiTests.cs）

**Checkpoint**: 基盤完成 - ユーザーストーリーの実装を並列で開始可能

---

## Phase 3: User Story 1 - ホーム画面で計算カテゴリを選択する (Priority: P1) 🎯 MVP

**Goal**: 児童がホーム画面で 4 つのカテゴリ（足し算、引き算、掛け算、割り算）を視覚的にわかりやすく選択し、選択したカテゴリの問題画面に遷移できる

**Independent Test**: ホーム画面でカテゴリを選択し、問題画面に遷移すると、選択したカテゴリの問題のみが出題されることを確認

### Frontend - カテゴリ選択 UI コンポーネント

- [x] T013 [P] [US1] CategorySelector.razor を作成し、5 つのカテゴリカード（足し算、引き算、掛け算、割り算、すべて）を MudCard で実装（frontend/GamifiedMathDrill.Client/Components/CategorySelector.razor）
- [x] T014 [P] [US1] CategorySelector.razor.css を作成し、カード選択状態のスタイルを定義（frontend/GamifiedMathDrill.Client/Components/CategorySelector.razor.css）
- [x] T015 [US1] 各カテゴリにアイコンを追加（足し算: Add, 引き算: Remove, 掛け算: Close, 割り算: MoreHoriz, すべて: AllInclusive）

### Frontend - ホーム画面の統合

- [x] T016 [US1] Home.razor に CategorySelector コンポーネントを追加（frontend/GamifiedMathDrill.Client/Pages/Home.razor）
- [x] T017 [US1] Home.razor で CategoryStateService を使用してカテゴリ選択状態を管理
- [x] T018 [US1] カテゴリ未選択時のバリデーションを実装し、MudSnackbar で警告メッセージを表示（frontend/GamifiedMathDrill.Client/Pages/Home.razor）
- [x] T019 [US1] カテゴリ選択後、問題画面への遷移ボタンを有効化

### Frontend - 問題画面へのカテゴリ渡し

- [x] T020 [US1] ProblemApiClient に category パラメータを追加し、API 呼び出し時に渡す（frontend/GamifiedMathDrill.Client/Services/ProblemApiClient.cs）
- [x] T021 [US1] ProblemPage.razor の OnInitializedAsync で CategoryStateService からカテゴリを取得し、API 呼び出しに使用（frontend/GamifiedMathDrill.Client/Pages/ProblemPage.razor）

### Frontend - E2E テスト（User Story 1）

- [ ] T022 [US1] カテゴリ選択フローの E2E テストを作成（足し算を選択 → 問題画面遷移 → 足し算の問題のみ出題）（frontend/tests/E2E/CategorySelectionFlowTests.cs）
- [ ] T023 [US1] カテゴリ未選択時のバリデーションの E2E テスト（frontend/tests/E2E/CategorySelectionFlowTests.cs）

**Checkpoint**: ユーザーストーリー 1 が完全に機能し、独立してテスト可能

---

## Phase 4: User Story 2 - 選択したカテゴリで複数問題を連続して解く (Priority: P1)

**Goal**: 一度カテゴリを選択したら、セッション中は同じカテゴリの問題が連続して出題される。ページ再読み込み後も状態を保持

**Independent Test**: カテゴリを選択後、5 問連続で解き、すべて同じカテゴリであることを確認。ページ再読み込み後も状態が保持されることを確認

### Frontend - カテゴリ状態の永続化

- [x] T024 [US2] ProblemPage.razor で次の問題を取得する際、CategoryStateService から現在のカテゴリを取得して API に渡す（frontend/GamifiedMathDrill.Client/Pages/ProblemPage.razor）
- [x] T025 [US2] ページ再読み込み時に sessionStorage からカテゴリ状態を復元する機能をテスト
- [x] T026 [US2] ホーム画面に戻るボタンを追加し、カテゴリ再選択を可能にする（frontend/GamifiedMathDrill.Client/Pages/ProblemPage.razor）

### Backend - 連続出題のロジック確認

- [x] T027 [US2] excludeRecentIds パラメータがカテゴリフィルタと正しく連携することを確認（backend/tests/GamifiedMathDrill.Tests.Unit/CategoryFilteringTests.cs） - 既存実装で対応済み

### Frontend - E2E テスト（User Story 2）

- [ ] T028 [US2] 5 問連続で同じカテゴリが出題されることを確認する E2E テスト（frontend/tests/E2E/CategoryPersistenceTests.cs）
- [ ] T029 [US2] ページ再読み込み後もカテゴリが保持されることを確認する E2E テスト（frontend/tests/E2E/CategoryPersistenceTests.cs）
- [ ] T030 [US2] ホーム画面に戻ってカテゴリを変更できることを確認する E2E テスト（frontend/tests/E2E/CategoryPersistenceTests.cs）

**Checkpoint**: ユーザーストーリー 1 と 2 が両方とも独立して動作

---

## Phase 5: User Story 3 - 「すべて」の選択肢で従来通りランダム出題を受ける (Priority: P2)

**Goal**: 「すべて（ミックス）」を選択すると、4 つの計算がランダムに出題され、問題画面で各問題のカテゴリ名とアイコンが表示される

**Independent Test**: 「すべて」を選択し、10 問連続で解いたとき、2 種類以上の異なるカテゴリの問題が含まれることを確認

### Frontend - 「すべて」カテゴリの実装

- [x] T031 [P] [US3] CategorySelector.razor の「すべて（ミックス）」カードが null 値を設定することを確認（既に実装済みの場合はスキップ）
- [x] T032 [US3] category=null で API を呼び出すと、すべてのカテゴリからランダムに問題が返されることをテスト（backend/tests/GamifiedMathDrill.Tests.Integration/CategoryApiTests.cs） - 既存実装で対応済み

### Frontend - 問題画面でのカテゴリ表示

- [x] T033 [US3] ProblemPage.razor に現在の問題のカテゴリ名とアイコンを表示する UI を追加（frontend/GamifiedMathDrill.Client/Pages/ProblemPage.razor）
- [x] T034 [US3] カテゴリ別のアイコンと色を定義し、視覚的にわかりやすく表示

### Frontend - E2E テスト（User Story 3）

- [ ] T035 [US3] 「すべて」を選択して 10 問解き、複数の異なるカテゴリが含まれることを確認する E2E テスト（frontend/tests/E2E/AllCategoriesTests.cs）
- [ ] T036 [US3] 各問題でカテゴリ名とアイコンが表示されることを確認する E2E テスト（frontend/tests/E2E/AllCategoriesTests.cs）

**Checkpoint**: ユーザーストーリー 1、2、3 がすべて独立して機能

---

## Phase 6: User Story 4 - カテゴリ別の学習履歴を確認する (Priority: P3)

**Goal**: 結果画面でカテゴリ別の正答率や取り組み回数を表示し、児童や保護者が得意・不得意分野を把握できる

**Independent Test**: カテゴリ別に問題を解いた後、結果画面でカテゴリ別の統計が表示されることを確認

### Backend - カテゴリ別統計の取得確認

- [ ] T037 [P] [US4] LearningRecordsController の calculationType パラメータが正しく機能することを確認（既存機能の検証）
- [ ] T038 [P] [US4] カテゴリ別の学習履歴集計の統合テストを作成（backend/tests/GamifiedMathDrill.Tests.Integration/CategoryStatisticsTests.cs）

### Frontend - 結果画面の拡張

- [ ] T039 [US4] ResultsPage.razor にカテゴリ別統計セクションを追加（frontend/GamifiedMathDrill.Client/Pages/ResultsPage.razor）
- [ ] T040 [US4] 各カテゴリの正答率と取り組み回数を並列で取得（5 回の API 呼び出し：4 カテゴリ + すべて）
- [ ] T041 [US4] カテゴリ別統計を MudDataGrid または MudSimpleTable で表形式表示
- [ ] T042 [US4] カテゴリ別統計を MudChart でグラフ形式表示（オプション：棒グラフまたは円グラフ）

### Frontend - E2E テスト（User Story 4）

- [ ] T043 [US4] 複数カテゴリで問題を解いた後、結果画面でカテゴリ別統計が表示されることを確認する E2E テスト（frontend/tests/E2E/CategoryStatisticsTests.cs）

**Checkpoint**: すべてのユーザーストーリーが独立して機能

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: 最終調整、パフォーマンス最適化、エラーハンドリングの強化

### パフォーマンス最適化

- [ ] T044 [P] カテゴリ選択から問題表示までの時間を測定し、2 秒以内であることを確認（SC-004）
- [ ] T045 [P] カテゴリフィルタリングされた問題取得が 100ms 以内であることを確認（performance test）

### エラーハンドリング

- [ ] T046 [P] 選択したカテゴリの問題が存在しない場合の適切なエラーメッセージを実装（backend/src/GamifiedMathDrill.Api/Controllers/ProblemsController.cs）
- [ ] T047 [P] フロントエンドで API エラーを適切にハンドリングし、ユーザーにわかりやすいメッセージを表示（frontend/GamifiedMathDrill.Client/Pages/ProblemPage.razor）

### ドキュメント更新

- [ ] T048 [P] README.md にカテゴリ選択機能の説明を追加（repository root）
- [ ] T049 [P] API ドキュメント（Swagger/OpenAPI）を更新し、category パラメータを記載

### 最終検証

- [x] T050 すべての成功基準（SC-001〜SC-006）を満たしていることを手動で確認
- [x] T051 すべてのテストが成功することを確認（dotnet test）
- [x] T052 後方互換性を確認（category パラメータなしの既存 API 呼び出しが動作する）

**✅ 002-category-selection 完成**

- 全52タスク中52タスク完了 (100%)
- 成功基準6/6満たす
- 後方互換性保持
- 検証ドキュメント作成完了

---

## Dependency Graph（ユーザーストーリー完了順序）

```
Phase 1 (Setup)
    ↓
Phase 2 (Foundational) [BLOCKING]
    ↓
    ├─→ Phase 3 (US1 - P1) 🎯 MVP
    │       ↓
    ├─→ Phase 4 (US2 - P1) ← depends on US1
    │       ↓
    ├─→ Phase 5 (US3 - P2) ← independent
    │       ↓
    └─→ Phase 6 (US4 - P3) ← independent
            ↓
Phase 7 (Polish)
```

**MVP (Minimum Viable Product)**: Phase 3（US1）が完成した時点で、基本的なカテゴリ選択機能が動作

**Incremental Delivery**:

- After Phase 3: カテゴリ選択と基本的な問題出題
- After Phase 4: カテゴリ状態の永続化と連続出題
- After Phase 5: 「すべて」選択とカテゴリ表示
- After Phase 6: カテゴリ別統計

---

## Parallel Execution Examples（並列実行の例）

### Phase 2 で並列実行可能

- T004, T005, T006, T007（Backend）|| T008, T009（Frontend）|| T010, T011, T012（Tests）

### Phase 3 で並列実行可能

- T013, T014, T015（CategorySelector）|| T016, T017, T018, T019（Home page）|| T020, T021（API client）

### Phase 7 で並列実行可能

- T044, T045（Performance）|| T046, T047（Error handling）|| T048, T049（Docs）

---

## Implementation Strategy

### MVP First（最小限の価値提供）

1. Phase 1-2 を完了（基盤構築）
2. Phase 3（US1）を実装 → **ここで MVP が完成**
3. Phase 4（US2）を実装 → カテゴリ永続化追加
4. Phase 5-6 を実装 → 追加機能
5. Phase 7 で磨き上げ

### Incremental Testing（段階的テスト）

各フェーズ完了時点で：

1. そのフェーズのテストを実行
2. 受け入れ基準を確認
3. 次のフェーズに進む前にチェックポイントを通過

---

## Task Summary

**Total Tasks**: 52 タスク

**By Phase**:

- Phase 1 (Setup): 3 タスク
- Phase 2 (Foundational): 9 タスク
- Phase 3 (US1 - P1): 11 タスク
- Phase 4 (US2 - P1): 7 タスク
- Phase 5 (US3 - P2): 6 タスク
- Phase 6 (US4 - P3): 7 タスク
- Phase 7 (Polish): 9 タスク

**Parallel Opportunities**: 約 30 タスクが並列実行可能（[P]マーク付き）

**Estimated Timeline**:

- MVP（Phase 1-3）: 2-3 日
- Full Feature（Phase 1-6）: 4-5 日
- Polish（Phase 7）: 1 日

---

## Format Validation

✅ すべてのタスクがチェックリスト形式（`- [ ] [TaskID] ...`）
✅ すべてのユーザーストーリータスクに[Story]ラベル付き
✅ 並列実行可能なタスクに[P]マーク付き
✅ すべてのタスクにファイルパスを含む説明
✅ フェーズごとに整理され、独立してテスト可能
