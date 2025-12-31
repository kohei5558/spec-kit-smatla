# Implementation Plan: Gamified Math Drill App

**Branch**: `001-gamified-math-drill` | **Date**: 2025-12-30 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-gamified-math-drill/spec.md`

## Summary

小学 3 年生向けの計算ドリルアプリを構築します。児童が計算問題を解いてポイントを獲得し、そのポイントで景品と交換できるゲーミフィケーション機能を提供します。学習継続を促進するために、連続学習日数の記録、日替わりチャレンジ問題、段階的なレベル制を実装します。ASP.NET Core と C#をバックエンドとし、児童にとって直感的なフロントエンド UI を提供します。オンライン必須アーキテクチャで、1 デバイス=1 児童のシングルユーザーモデルを採用します。

## Technical Context

**Language/Version**: C# 12 / .NET 8.0  
**Primary Dependencies**: ASP.NET Core 8.0, Entity Framework Core, Blazor WebAssembly, MudBlazor (UI components)  
**Storage**: SQLite (development), PostgreSQL (production)  
**Testing**: xUnit, Moq, Playwright for .NET (E2E tests)  
**Target Platform**: Web application (ブラウザベース - Chrome, Safari, Edge)  
**Project Type**: Web (backend + frontend)  
**Performance Goals**:

- アプリ起動から問題表示まで 3 秒以内 (SC-006)
- 1 問あたりの回答処理 200ms 以内
- 景品交換トランザクション 500ms 以内
- 学習結果画面の表示 1 秒以内  
  **Constraints**:
- オンライン必須（インターネット接続が常に必要）
- シングルユーザー専用（マルチユーザー切り替え不要）
- 児童データのプライバシー保護（HTTPS + DB 暗号化 + 列レベル AES-256）
- 初期スケール: 100 同時ユーザー、拡張可能: 1,000 同時ユーザー
  **Scale/Scope**:
- 想定ユーザー数: 初期 100 同時接続、拡張時 1,000 同時接続
- 問題データベースサイズ: 小学 3 年生レベルの計算問題（推定 500-1000 問）
- 画面数: 約 10 画面（ホーム、問題、結果、景品一覧、景品交換、履歴など）
- 問題データベースサイズ: 小学 3 年生レベルの計算問題（推定 500-1000 問）
- 画面数: 約 10 画面（ホーム、問題、結果、景品一覧、景品交換、履歴など）

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### Principle I: User-Centric Design ✅

- ✅ 対象年齢（8-9 歳）に適した UI/UX 設計を仕様で明確化
- ✅ 直感的な操作（問題を解く → ポイント獲得 → 景品交換）
- ✅ 保護者向けの学習結果確認機能（US2）
- ✅ User Stories は児童と保護者の両方の視点を含む

**Status**: PASS - 仕様は児童と保護者のニーズに焦点を当てている

---

### Principle II: Data Privacy & Child Safety ⚠️ NEEDS VERIFICATION

- ✅ 個人情報の最小化（仕様に明示的な PII 収集なし）
- ⚠️ [NEEDS CLARIFICATION: データ暗号化方式 - HTTPS, DB 暗号化, at-rest encryption?]
- ⚠️ [NEEDS CLARIFICATION: 児童の年齢確認・保護者同意の取得方法]
- ✅ 第三者共有なし（オンライン必須だが外部サービス連携の記載なし）
- ✅ 広告なし（景品は内部リソース）

**Status**: CONDITIONAL PASS - Phase 0 でセキュリティ実装の詳細を明確化する必要あり

---

### Principle III: Specification-First Development ✅

- ✅ 詳細な仕様書が作成済み（spec.md）
- ✅ 実装の詳細を含まない（技術スタックは憲法で定義）
- ✅ テスト可能な要件（19 の FR、10 の SC）
- ✅ Edge case と受入基準が定義済み
- ✅ [NEEDS CLARIFICATION]マーカーは残っているが、実装前に解決予定

**Status**: PASS - 仕様書は憲法の基準を満たす

---

### Principle IV: Test-Driven Quality ⚠️ PENDING

- ✅ 成功基準が測定可能（SC-001〜SC-010）
- ✅ User Story 単位でテスト可能（Independent Test 定義済み）
- ⚠️ テストケースの詳細は Phase 1 で定義必要
- ⚠️ 自動テスト戦略は Phase 0 の調査対象
- ✅ パフォーマンス基準が明確（起動 3 秒以内など）

**Status**: CONDITIONAL PASS - テスト戦略を Phase 0 で調査、Phase 1 で具体化

---

### Principle V: Continuous Learning & Adaptation ✅

- ✅ 学習データ分析機能（FR-015: 間違いパターン・苦手分析）
- ✅ 難易度調整機能（FR-017: レベル制）
- ✅ 測定可能な指標（SC-002: 正答率 10%向上、SC-003: 週 5 日継続率）
- ✅ フィードバック機能（FR-012: 励ましメッセージ）

**Status**: PASS - 継続学習と適応の仕組みが組み込まれている

---

### Technology Stack Compliance ✅

- ✅ ASP.NET Core 使用（憲法の必須制約）
- ✅ C#使用（憲法の必須制約）
- ✅ オンライン必須アーキテクチャ（FR-018）
- ✅ シングルユーザー専用（FR-019）

**Status**: PASS - 技術スタックは憲法に完全準拠

---

### Overall Gate Status: ⚠️ CONDITIONAL PASS

**Blockers**: なし  
**Warnings**:

- データプライバシー実装の詳細を Phase 0 で明確化
- テスト戦略を Phase 0 で調査
- Technical Context の[NEEDS CLARIFICATION]を Phase 0 で解決

**Action**: Phase 0（調査）に進むことを承認。上記の警告事項は調査フェーズで解決する。

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
backend/
├── src/
│   ├── GamifiedMathDrill.Api/         # ASP.NET Core Web API
│   │   ├── Controllers/               # API endpoints
│   │   ├── Middleware/                # Authentication, error handling
│   │   └── Program.cs                 # Application entry point
│   ├── GamifiedMathDrill.Core/        # Business logic & domain models
│   │   ├── Models/                    # Domain entities (Problem, Student, etc.)
│   │   ├── Services/                  # Business logic services
│   │   └── Interfaces/                # Service contracts
│   └── GamifiedMathDrill.Infrastructure/  # Data access & external services
│       ├── Data/                      # EF Core DbContext, repositories
│       ├── Repositories/              # Data access implementations
│       └── Migrations/                # Database migrations
└── tests/
    ├── GamifiedMathDrill.Tests.Unit/      # Unit tests (xUnit)
    ├── GamifiedMathDrill.Tests.Integration/  # Integration tests
    └── GamifiedMathDrill.Tests.E2E/       # End-to-end tests

frontend/
├── src/
│   ├── components/                    # Reusable UI components
│   │   ├── Problem/                   # Problem display & answer input
│   │   ├── Results/                   # Learning results & charts
│   │   └── Rewards/                   # Reward list & exchange
│   ├── pages/                         # Page components (routing)
│   │   ├── HomePage.jsx               # Landing & start
│   │   ├── ProblemPage.jsx            # Problem solving interface
│   │   ├── ResultsPage.jsx            # Learning history
│   │   └── RewardsPage.jsx            # Reward shop
│   ├── services/                      # API client services
│   └── App.jsx                        # Main application
└── tests/
    └── e2e/                           # Frontend E2E tests

shared/
└── contracts/                         # API contracts (OpenAPI/Swagger specs)
```

**Structure Decision**: Web アプリケーション構成を採用。バックエンド（ASP.NET Core）とフロントエンド（Blazor WebAssembly）を分離し、RESTful API で通信します。Clean Architecture 原則に従い、バックエンドを 3 層（Api、Core、Infrastructure）に分割してテスタビリティと保守性を確保します。

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

**No violations** - このプロジェクトは憲法のすべての原則に準拠しています。

---

## Constitution Check (Phase 1 - Post-Design Re-evaluation)

Phase 0 の調査と Phase 1 の設計完了後、憲法への準拠を再評価します。

### Principle II: Data Privacy & Child Safety ✅ RESOLVED

**Phase 0 で解決された項目**:

- ✅ データ暗号化方式: HTTPS + DB 暗号化（PostgreSQL TDE） + 列レベル AES-256
- ✅ 児童の年齢確認・保護者同意: 初回セットアップ時の保護者認証フロー（簡易算数問題またはメール確認）
- ✅ 暗号化キー管理: Azure Key Vault または環境変数で安全に管理
- ✅ COPPA/GDPR 準拠: 最小限のデータ収集、保護者同意、データエクスポート/削除機能

**Status**: ✅ PASS - すべてのセキュリティ要件が明確化され、実装計画に含まれる

---

### Principle IV: Test-Driven Quality ✅ RESOLVED

**Phase 0 で解決された項目**:

- ✅ E2E テストフレームワーク: Playwright for .NET
- ✅ テスト戦略: xUnit（Unit）, Integration Tests, Playwright（E2E）
- ✅ テストケースの構造: User Story 単位でテストスイートを作成
- ✅ CI/CD 統合: GitHub Actions で自動実行

**Phase 1 で定義された項目**:

- ✅ API 契約テスト: contracts/api.md でエンドポイントとレスポンス形式を定義
- ✅ データモデル検証: data-model.md でバリデーションルールを定義
- ✅ パフォーマンステスト基準: 負荷テスト（k6, JMeter）で 100/1,000 同時ユーザーを検証

**Status**: ✅ PASS - 包括的なテスト戦略が確立され、実装準備完了

---

### Design Artifacts Alignment Check

**Phase 1 成果物の憲法準拠確認**:

1. **data-model.md** ✅

   - User-Centric: 児童（Student）と保護者のニーズに焦点を当てたエンティティ設計
   - Data Privacy: PII（児童名）の列レベル暗号化を明記
   - Continuous Learning: LearningRecord, Level による進捗追跡と適応

2. **contracts/api.md** ✅

   - Specification-First: 実装前に API 契約を明確に定義
   - Test-Driven: 各エンドポイントのレスポンス形式が明確でテスト可能
   - User-Centric: エラーメッセージが児童向けにわかりやすい日本語

3. **quickstart.md** ✅

   - Documentation Standards: 開発環境セットアップが明確で再現可能
   - Simplicity: ステップバイステップで初心者でも実行可能
   - Tools Alignment: .NET 8.0, Blazor, PostgreSQL の選定が憲法の技術スタックに準拠

4. **research.md** ✅
   - Decision Documentation: すべての技術選定に根拠と代替案を記載
   - Constitution Reference: 各決定が憲法の原則にどう関連するかを明記
   - Traceability: [NEEDS CLARIFICATION]がすべて解決され、トレーサビリティを確保

---

### Overall Gate Status: ✅ PASS

**Result**: 設計フェーズ完了。すべての憲法原則に準拠し、実装フェーズ（Phase 2）に進む準備が整いました。

**Verified**:

- ✅ すべての [NEEDS CLARIFICATION] が解決
- ✅ データプライバシーとセキュリティ実装が明確
- ✅ テスト戦略が確立
- ✅ API 契約とデータモデルが定義
- ✅ 開発環境セットアップ手順が完備

**Next Phase**: `/speckit.tasks` コマンドで Phase 2（タスクリスト作成）に進みます。
