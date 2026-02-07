# Implementation Plan: 子供アカウント管理画面

**Branch**: `005-child-account-management` | **Date**: 2026年2月7日 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/005-child-account-management/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

保護者が管理画面から子供アカウントを作成・編集・削除・一時停止でき、各子供の学習状況を確認できる機能を提供する。子供は視覚的なカード形式のUIから自分のアバターを選択し、4桁PINでログインする。既存のASP.NET Core Identity（ApplicationUser）とStudentモデルを活用し、保護者と子供のアカウント管理を一元化する。

## Technical Context

**Language/Version**: C# 12 / .NET 8  
**Primary Dependencies**: ASP.NET Core Identity, Entity Framework Core, PostgreSQL  
**Storage**: PostgreSQL（既存のApplicationDbContext使用）  
**Testing**: xUnit, Integration Tests（既存のTestWebApplicationFactory使用）  
**Target Platform**: Web（Blazor WebAssembly + ASP.NET Core API）  
**Project Type**: Web application (frontend + backend)  
**Performance Goals**: 子供アカウント一覧表示1秒以内、アカウント作成3分以内完了  
**Constraints**: PINロックアウト5分間、子供アカウント上限10件/保護者、オンライン必須  
**Scale/Scope**: 保護者1人あたり最大10子供アカウント、10〜20種類のプリセットアバター

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

### I. User-Centric Design ✅ PASS

- **子供向けUI**: アバター付きカード形式ログイン画面は8-9歳児に視覚的でわかりやすい
- **保護者向けUI**: 管理画面で子供の学習状況を一目で確認可能
- **直感的操作**: カードタップ→PIN入力の2ステップで完結
- **アクセシビリティ**: プリセットアバターにより画像選択の手間を削減

### II. Data Privacy & Child Safety ✅ PASS

- **最小限のデータ収集**: 名前、学年、PIN、アバターIDのみ（メール不要）
- **PIN暗号化**: ApplicationUserのPINフィールドはハッシュ化して保存
- **不正アクセス防止**: 3回失敗で5分間ロックアウト、自動解除
- **保護者管理**: 子供アカウントの全操作は保護者が管理

### III. Specification-First Development ✅ PASS

- **仕様完成**: spec.mdに5つのユーザーストーリー、15の機能要件、7つの成功基準を定義
- **明確化完了**: 5つの重要な質問に回答済み（PIN重複、ロックアウト、UI詳細、アバター提供、PIN忘れ対応）
- **テスト可能**: 各ユーザーストーリーに独立したテストシナリオあり
- **実装詳細なし**: 仕様はユーザー価値に焦点、技術選択は本planで決定

### IV. Test-Driven Quality ✅ PASS

- **テストケース定義**: 27の受け入れシナリオ、8つのエッジケース
- **既存テストインフラ活用**: TestWebApplicationFactory、AuthenticationHelper使用
- **パフォーマンス基準**: 一覧表示1秒以内、作成操作3分以内（SC-001, SC-002）
- **自動テスト**: xUnit統合テストで全シナリオをカバー

### V. Continuous Learning & Adaptation ✅ PASS

- **学習状況可視化**: 保護者が総問題数、正答率、連続学習日数を確認可能
- **適応的サポート**: PIN忘れ時に保護者が即座にリセット可能
- **フィードバックループ**: 学習統計サマリーにより子供の進捗を把握

### Technology Stack Compliance ✅ PASS

- **Backend Framework**: ASP.NET Core（既存プロジェクト準拠）
- **Programming Language**: C#（既存コードベース準拠）
- **Architecture**: オンライン必須（仕様に明記）
- **Database**: PostgreSQL + EF Core（既存ApplicationDbContext活用）

**GATE STATUS**: ✅ **ALL CHECKS PASSED** - Proceed to Phase 0

## Project Structure

### Documentation (this feature)

```text
specs/005-child-account-management/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── child-account-api.yaml  # OpenAPI specification
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── GamifiedMathDrill.Api/
│   │   ├── Controllers/
│   │   │   └── ChildAccountController.cs          # 新規: 子供アカウントCRUD API
│   │   └── DTOs/
│   │       ├── ChildAccountCreateDto.cs           # 新規: 作成リクエスト
│   │       ├── ChildAccountUpdateDto.cs           # 新規: 更新リクエスト
│   │       ├── ChildAccountDto.cs                 # 既存: 拡張（学年、ステータス追加）
│   │       ├── ChildLoginRequest.cs               # 既存: カード形式UI用に拡張
│   │       └── ChildLearningStatsDto.cs           # 新規: 学習統計
│   ├── GamifiedMathDrill.Core/
│   │   ├── Interfaces/
│   │   │   └── IChildAccountService.cs            # 新規: サービスインターフェース
│   │   ├── Models/
│   │   │   └── PresetAvatar.cs                    # 新規: プリセットアバターマスタ
│   │   └── Services/
│   │       └── ChildAccountService.cs             # 新規: ビジネスロジック
│   └── GamifiedMathDrill.Infrastructure/
│       ├── Identity/
│       │   └── ApplicationUser.cs                 # 既存: GradeLevel, IsActive追加
│       ├── Repositories/
│       │   └── ChildAccountRepository.cs          # 新規: データアクセス
│       └── Data/
│           ├── ApplicationDbContext.cs            # 既存: PresetAvatars追加
│           └── Seed/
│               └── AvatarSeeder.cs                # 新規: アバターマスタ初期データ
└── tests/
    └── GamifiedMathDrill.Tests.Integration/
        ├── ChildAccountTests.cs                   # 新規: CRUD統合テスト
        ├── ChildLoginTests.cs                     # 既存: カード形式UI拡張
        └── ChildAccountSecurityTests.cs           # 新規: PINロックアウトテスト

frontend/
└── GamifiedMathDrill.Client/
    ├── Pages/
    │   └── Parent/
    │       ├── ChildAccountManagement.razor       # 新規: 子供アカウント一覧
    │       ├── CreateChildAccount.razor           # 新規: 作成フォーム
    │       ├── EditChildAccount.razor             # 新規: 編集フォーム
    │       └── ChildDetail.razor                  # 新規: 学習状況詳細
    ├── Components/
    │   ├── ChildAccountCard.razor                 # 新規: アバター付きカード
    │   ├── AvatarSelector.razor                   # 新規: アバター選択UI
    │   └── PINInput.razor                         # 新規: 4桁PIN入力
    ├── Services/
    │   └── ChildAccountService.cs                 # 新規: API呼び出しラッパー
    └── Models/
        └── ChildAccountViewModel.cs               # 新規: フロントエンドモデル
```

**Structure Decision**: Web application (Blazor WebAssembly + ASP.NET Core API)を選択。既存のバックエンド構造（Api/Core/Infrastructure 3層アーキテクチャ）を維持し、新機能を追加。既存のApplicationUserとStudentモデルを拡張して子供アカウント管理を統合。

---

## Phase 0: Research & Decisions (Generated by /speckit.plan)

See [research.md](./research.md) for detailed research findings.

---

## Phase 1: Data Model & Contracts (Generated by /speckit.plan)

- **Data Model**: [data-model.md](./data-model.md) - エンティティ、リレーション、バリデーションルール
- **API Contracts**: [contracts/child-account-api.yaml](./contracts/child-account-api.yaml) - OpenAPI 3.0仕様
- **Quickstart Guide**: [quickstart.md](./quickstart.md) - 開発環境セットアップと動作確認

---

## Constitution Check (Re-evaluation after Phase 1)

### I. User-Centric Design ✅ PASS

**Design Decisions**:

- カード形式UI（ChildAccountCard.razor）: CSS Gridでレスポンシブ、視覚的識別容易
- プリセットアバター10〜20種類: 子供が選ぶ楽しみ + 不適切画像リスク回避
- PINリセット簡略化: 保護者が編集画面で直接変更、複雑なフロー不要

**Validation**: ✅ すべてのUI設計が8-9歳児と保護者の使いやすさを優先

### II. Data Privacy & Child Safety ✅ PASS

**Security Implementation**:

- PIN: PasswordHasher<ApplicationUser>でハッシュ化（PBKDF2）
- ロックアウト: IMemoryCacheで3回失敗→5分間ロック
- データ最小化: メールアドレス不要（保護者管理下のサブアカウント）
- アバター: プリセットのみ（カスタムアップロード禁止で不適切画像防止）

**Validation**: ✅ すべてのセキュリティ要件が実装計画に反映

### III. Specification-First Development ✅ PASS

**Documentation Complete**:

- spec.md: 5ユーザーストーリー、15機能要件、7成功基準
- plan.md: 技術選択、プロジェクト構造、フェーズ0-1完了
- data-model.md: ER図、エンティティ仕様、バリデーションルール
- contracts/: OpenAPI 3.0仕様（11エンドポイント）
- quickstart.md: 30分セットアップガイド

**Validation**: ✅ 実装前に全ドキュメント完成、レビュー可能

### IV. Test-Driven Quality ✅ PASS

**Test Strategy**:

- 統合テスト: ChildAccountTests.cs, ChildLoginTests.cs, ChildAccountSecurityTests.cs
- テストインフラ: 既存TestWebApplicationFactory活用
- テストカバレッジ: 27受け入れシナリオ + 8エッジケース
- パフォーマンス: quickstart.mdで1秒/3分基準を検証可能

**Validation**: ✅ すべてのユーザーストーリーにテストケース定義済み

### V. Continuous Learning & Adaptation ✅ PASS

**Adaptive Features**:

- 学習統計集計: Studentモデル + LearningRecords集計クエリ
- 保護者可視化: ChildLearningStatsDto（正答率、連続日数、過去7日間）
- PIN忘れ対応: 即座リセット可能（保護者が編集画面で変更）

**Validation**: ✅ フィードバックループと進捗追跡が設計済み

### Technology Stack Compliance ✅ PASS

**Implementation Alignment**:

- Backend: ASP.NET Core + EF Core（既存プロジェクト構造維持）
- Frontend: Blazor WebAssembly（既存技術スタック）
- Database: PostgreSQL（既存ApplicationDbContext拡張）
- Architecture: 3層（Api/Core/Infrastructure）維持

**Validation**: ✅ すべての技術選択が憲法に準拠

---

**FINAL GATE STATUS**: ✅ **ALL CHECKS PASSED** - Ready for Phase 2 (Task Breakdown)

**Next Command**: `/speckit.tasks` - タスク分割と実装チェックリスト生成
