# Implementation Plan: 問題カテゴリ選択機能

**Branch**: `002-category-selection` | **Date**: 2026-01-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-category-selection/spec.md`

## Summary

ホーム画面で児童が計算のカテゴリ（足し算、引き算、掛け算、割り算、すべて）を選択し、選択したカテゴリの問題のみを連続して解けるようにする機能を実装します。カテゴリ選択状態はセッション中保持され、ページ再読み込み後も維持されます。バックエンド API（ASP.NET Core）にカテゴリフィルタリング機能を追加し、フロントエンド（Blazor WebAssembly + MudBlazor）にカテゴリ選択 UI とカテゴリ表示機能を実装します。

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: ASP.NET Core 8.0, Entity Framework Core, Blazor WebAssembly, MudBlazor (UI components)  
**Storage**: SQLite (development), PostgreSQL (production) - 既存の Problem テーブルに CalculationType インデックスあり  
**Testing**: xUnit, Moq, Playwright for .NET (E2E tests)  
**Target Platform**: Web application (ブラウザベース - Chrome, Safari, Edge)  
**Project Type**: Web (backend + frontend)  
**Performance Goals**:

- カテゴリ選択から問題表示まで 2 秒以内 (SC-004)
- カテゴリフィルタリングされた問題取得 100ms 以内
- UI レスポンス時間 50ms 以内（カテゴリ選択時のフィードバック）

**Constraints**:

- 既存の GetNextProblemAsync API を拡張し、後方互換性を維持
- カテゴリ未選択時は問題画面への遷移を防止
- ページ再読み込み時にカテゴリ選択状態を保持（ブラウザストレージ使用）
- 小学 3 年生にも理解できる視覚的なカテゴリ選択 UI

**Scale/Scope**:

- 既存の 4 つの CalculationType enum 値を使用（Addition, Subtraction, Multiplication, Division）
- 新規追加: 「すべて（ミックス）」選択肢（NULL または特別な値で表現）
- 影響を受けるコンポーネント: 3 画面（ホーム、問題、結果）、2 API エンドポイント、1 サービスメソッド

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### Principle I: User-Centric Design ✅

- ✅ 小学 3 年生向けの視覚的でわかりやすいカテゴリ選択 UI（FR-001）
- ✅ 直感的な操作（1 回のクリック/タップで選択、SC-001）
- ✅ カテゴリ名とアイコンの両方を表示（FR-008、明確化済み）
- ✅ カテゴリ未選択時は選択を促すメッセージ（FR-011、明確化済み）

**Status**: PASS - 仕様は児童の使いやすさを最優先

---

### Principle II: Data Privacy & Child Safety ✅

- ✅ カテゴリ選択はセッション管理のみ（個人情報収集なし）
- ✅ 既存のセキュリティ実装を継承（HTTPS、DB 暗号化）
- ✅ 第三者共有なし（クライアント側のローカルストレージのみ使用）

**Status**: PASS - プライバシーへの影響なし

---

### Principle III: Specification-First Development ✅

- ✅ 詳細な仕様書が作成済み（spec.md、明確化セッション完了）
- ✅ 実装の詳細を含まない（技術非依存な表現）
- ✅ テスト可能な要件（11 の FR、6 の SC）
- ✅ Edge case と受入基準が定義済み（5 つのエッジケースを明確化）
- ✅ すべての曖昧性が解決済み（Clarifications セクションに記録）

**Status**: PASS - 仕様書は憲法の基準を満たす

---

### Principle IV: Test-Driven Quality ✅

- ✅ 成功基準が測定可能（SC-001〜SC-006）
- ✅ User Story 単位でテスト可能（4 つの独立したストーリー）
- ✅ パフォーマンス基準が明確（2 秒以内、SC-004）
- ✅ 各 User Story に詳細な Acceptance Scenarios あり（計 13 シナリオ）

**Status**: PASS - テスト戦略の基盤が確立

---

### Principle V: Continuous Learning & Adaptation ✅

- ✅ カテゴリ別学習履歴の記録（FR-009、US4）
- ✅ カテゴリ別統計表示（FR-010、SC-006）
- ✅ 児童の得意・不得意分野の可視化（US4 の Acceptance Scenario 2）

**Status**: PASS - 継続学習の分析機能を強化

---

### Technology Stack Compliance ✅

- ✅ ASP.NET Core 使用（憲法の必須制約）
- ✅ C# 12 / .NET 8.0
- ✅ Blazor WebAssembly（既存スタック）
- ✅ Entity Framework Core（既存の ORM を継続使用）

**Status**: PASS - 技術スタックの一貫性を維持

---

**Overall Gate Status**: ✅ **PASS** - Phase 0 研究に進む準備完了

**Post-Phase 1 Re-check**: ✅ **PASS**

- データモデル: 既存構造を活用、新規テーブル不要
- API 契約: 後方互換性を維持
- 実装計画: Constitution の全原則に準拠
- 複雑性: 新たな複雑性の導入なし

## Project Structure

### Documentation (this feature)

```text
specs/002-category-selection/
├── spec.md              # 機能仕様書（完成）
├── plan.md              # このファイル（作成中）
├── research.md          # Phase 0 output（次に作成）
├── data-model.md        # Phase 1 output（Phase 0後に作成）
├── quickstart.md        # Phase 1 output（Phase 0後に作成）
├── contracts/           # Phase 1 output（API契約定義）
└── tasks.md             # Phase 2 output（/speckit.tasks コマンド）
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── GamifiedMathDrill.Api/
│   │   ├── Controllers/
│   │   │   └── ProblemsController.cs          # 修正: カテゴリパラメータ追加
│   │   └── DTOs/
│   │       └── ProblemDto.cs                   # 確認: カテゴリ情報含む
│   ├── GamifiedMathDrill.Core/
│   │   ├── Models/
│   │   │   ├── CalculationType.cs             # 既存: 4つのenum値
│   │   │   └── Problem.cs                      # 既存: CalculationTypeプロパティあり
│   │   ├── Interfaces/
│   │   │   └── IProblemService.cs              # 修正: カテゴリパラメータ追加
│   │   └── Services/
│   │       └── ProblemService.cs               # 修正: フィルタリングロジック実装
│   └── GamifiedMathDrill.Infrastructure/
│       ├── Data/
│       │   └── ApplicationDbContext.cs         # 確認: CalculationTypeインデックスあり
│       └── Repositories/
│           └── ProblemRepository.cs            # 修正: カテゴリ検索機能追加
└── tests/
    ├── GamifiedMathDrill.Tests.Unit/
    │   └── CategorySelectionTests.cs           # 新規: 単体テスト
    └── GamifiedMathDrill.Tests.Integration/
        └── CategoryFilteringTests.cs           # 新規: 統合テスト

frontend/
├── GamifiedMathDrill.Client/
│   ├── Pages/
│   │   ├── Home.razor                          # 修正: カテゴリ選択UI追加
│   │   ├── ProblemPage.razor                   # 修正: カテゴリ表示追加
│   │   └── ResultsPage.razor                   # 修正: カテゴリ別統計追加
│   ├── Components/
│   │   └── CategorySelector.razor              # 新規: カテゴリ選択コンポーネント
│   ├── Services/
│   │   ├── ProblemApiClient.cs                 # 修正: カテゴリパラメータ追加
│   │   └── CategoryStateService.cs             # 新規: カテゴリ状態管理
│   └── Models/
│       └── CalculationType.cs                  # 確認: バックエンドと同期
└── tests/
    └── E2E/
        └── CategorySelectionFlow.cs            # 新規: E2Eテスト
```

**Structure Decision**: 既存の Web application 構造（backend + frontend）を使用。

- Backend: 3 層アーキテクチャ（Api, Core, Infrastructure）を維持
- Frontend: Blazor WebAssembly + MudBlazor UI components を継続使用
- 新規ファイル: 最小限（CategorySelector.razor, CategoryStateService.cs, テスト）
- 既存ファイル修正: 7 ファイル（後方互換性を維持しながら拡張）

## Complexity Tracking

**Status**: ✅ No violations - Constitution Check passed without exceptions.

この機能は既存のアーキテクチャパターンに従い、新たな複雑性を導入しません。

---

## Phase Completion Summary

### Phase 0: Research ✅ COMPLETE

- [research.md](research.md) 作成完了
- 5 つの技術的な疑問を解決
- 全決定に根拠を記載

### Phase 1: Design & Contracts ✅ COMPLETE

- [data-model.md](data-model.md) 作成完了
- [contracts/get-next-problem-with-category.md](contracts/get-next-problem-with-category.md) 作成完了
- [quickstart.md](quickstart.md) 作成完了
- Agent context 更新完了（.github/agents/copilot-instructions.md）

### Phase 2: Task Breakdown - PENDING

次のコマンドで実行: `/speckit.tasks`

---

## Artifacts Generated

| Document            | Path                                                                                       | Status            |
| ------------------- | ------------------------------------------------------------------------------------------ | ----------------- |
| Implementation Plan | [plan.md](plan.md)                                                                         | ✅ このファイル   |
| Research            | [research.md](research.md)                                                                 | ✅ 完成           |
| Data Model          | [data-model.md](data-model.md)                                                             | ✅ 完成           |
| API Contract        | [contracts/get-next-problem-with-category.md](contracts/get-next-problem-with-category.md) | ✅ 完成           |
| Quickstart Guide    | [quickstart.md](quickstart.md)                                                             | ✅ 完成           |
| Agent Context       | `.github/agents/copilot-instructions.md`                                                   | ✅ 更新完了       |
| Task Breakdown      | [tasks.md](tasks.md)                                                                       | ⏳ Phase 2 で作成 |

---

## Key Implementation Decisions

1. **カテゴリ状態の管理**: sessionStorage（ページ再読み込み耐性あり、次回起動時はクリア）
2. **「すべて」の表現**: `category = null`（後方互換性、自然な表現）
3. **学習履歴集計**: 既存のインデックスを活用（パフォーマンス十分）
4. **UI デザイン**: MudCard + アイコン（小学 3 年生向け視認性）
5. **バリデーション**: フロントエンド主体（即座のフィードバック、UX 優先）

---

## Next Steps

**Ready for Phase 2**: タスク分解と実装開始

```bash
# タスク分解を実行
/speckit.tasks
```

**Branch**: `002-category-selection`  
**Feature Dir**: `/Users/murayama/dev/ai/spec-kit-smatla/specs/002-category-selection`  
**Implementation Plan**: このファイル
