# Research: Gamified Math Drill App

**Date**: 2025-12-30  
**Phase**: 0 - Outline & Research  
**Purpose**: Technical Context の [NEEDS CLARIFICATION] を解決し、実装に必要な技術選定とベストプラクティスを調査

## Research Tasks

以下の不明点を調査し、各項目について決定、根拠、代替案を文書化します。

### 1. フロントエンドフレームワーク選定

**Question**: フロントエンド開発に使用するフレームワークは何か？（React/Blazor/Angular?）

**Decision**: **Blazor WebAssembly**を採用

**Rationale**:

- ASP.NET Core との統合が最適（同じ C# エコシステム）
- .NET 8.0 対応で、最新の機能とパフォーマンス改善を利用可能
- 小学生向け UI に適したコンポーネントベース設計
- WASM により高速な計算処理（問題生成、正誤判定）が可能
- バックエンドとフロントエンドで C# コードを共有可能（型安全性の向上）
- Razor Component を使用した宣言的 UI

**Alternatives Considered**:

- **React**: より成熟したエコシステムと豊富なコンポーネントライブラリがあるが、JavaScript/TypeScript の学習コストと、バックエンドとの言語ギャップが懸念
- **Angular**: エンタープライズ向けの堅牢な構造だが、小規模教育アプリには過剰で学習曲線が急
- **Blazor Server**: サーバー側レンダリングで初期読み込みが速いが、SignalR 接続が必要で、オフライン（接続切断）時の UX が悪化する可能性

**Implementation Notes**:

- Blazor WebAssembly Standalone App テンプレートを使用
- MudBlazor または Radzen Blazor Components を UI ライブラリとして検討（児童向けの視覚的で親しみやすいコンポーネント）

---

### 2. データベース選定

**Question**: 開発・本番環境で使用するデータベースは何か？（SQL Server/PostgreSQL/SQLite?）

**Decision**:

- **開発環境**: SQLite（ローカル開発用）
- **本番環境**: PostgreSQL（クラウドホスティング用）

**Rationale**:

- **SQLite**:
  - セットアップ不要で開発環境の構築が容易
  - Entity Framework Core で完全サポート
  - ローカルテストに最適
- **PostgreSQL**:
  - オープンソースで無料、クラウドホスティング（Azure, AWS, Heroku）で広くサポート
  - スケーラビリティが高く、将来的なユーザー増加に対応可能
  - JSON カラム型で柔軟なデータ構造（学習記録の拡張など）に対応
  - EF Core で完全サポート、マイグレーションが容易

**Alternatives Considered**:

- **SQL Server**: Microsoft の公式 DB で ASP.NET Core との統合が最適だが、ライセンスコストとホスティングコストが高い。教育アプリには過剰
- **MySQL**: 広く使われているが、PostgreSQL と比較して JSON 処理や拡張性で劣る
- **Cosmos DB**: Azure のマネージド NoSQL だが、リレーショナルデータ（Student, Problem, LearningRecord）には不適切でコストも高い

**Implementation Notes**:

- EF Core のプロバイダーを切り替え可能に設計
- appsettings.json で環境別に接続文字列を管理
- 初期データ（問題セット、景品）のシードを Migration に含める

---

### 3. E2E テストフレームワーク選定

**Question**: End-to-End テストフレームワークは何を使用するか？（Playwright/Selenium?）

**Decision**: **Playwright**を採用

**Rationale**:

- モダンで高速、信頼性の高い E2E テストフレームワーク
- Chromium, Firefox, WebKit（Safari）をサポート
- .NET バインディング（Playwright for .NET）があり、C# でテスト記述可能
- 自動待機機能でフレーキーテストを削減
- ヘッドレスモード対応で CI/CD に統合しやすい
- 詳細なトレースとスクリーンショット機能

**Alternatives Considered**:

- **Selenium**: 長い歴史と豊富なコミュニティだが、Playwright と比較して遅く、安定性に欠ける
- **Cypress**: JavaScript ベースで人気があるが、.NET プロジェクトとの統合が困難
- **bUnit**: Blazor コンポーネント専用のテストライブラリだが、E2E ではなくコンポーネント単体テストに特化

**Implementation Notes**:

- `Microsoft.Playwright` NuGet パッケージを使用
- User Story 単位で E2E テストスイートを作成（US1: 問題解答、US2: 結果確認、US3: 景品交換、US4: 継続学習）
- CI pipeline（GitHub Actions）で自動実行

---

### 4. データ暗号化方式

**Question**: データ暗号化をどのように実装するか？（HTTPS, DB 暗号化, at-rest encryption?）

**Decision**: 多層防御アプローチを採用

**Rationale**:
児童のプライバシーと安全を守るため、複数のレイヤーで暗号化を実施します。

**Implementation**:

1. **通信の暗号化（In-Transit）**:

   - **HTTPS/TLS 1.3** を必須化
   - ASP.NET Core で HSTS（HTTP Strict Transport Security）を有効化
   - すべての API エンドポイントに対して HTTPS リダイレクトを強制

2. **保存データの暗号化（At-Rest）**:

   - **データベース暗号化**: PostgreSQL の Transparent Data Encryption（TDE）または pg_crypto 拡張を使用
   - **個人識別可能な情報（PII）の暗号化**: 児童名などの PII フィールドに対して AES-256 を使用した列レベル暗号化
   - **暗号化キー管理**: Azure Key Vault または環境変数で暗号化キーを安全に管理

3. **アプリケーションレベルの保護**:
   - **Data Protection API**: ASP.NET Core Data Protection を使用してセッショントークンや一時データを暗号化
   - **ハッシュ化**: パスワード（保護者アカウントがある場合）は bcrypt または Argon2 でハッシュ化

**Alternatives Considered**:

- HTTPS のみ: 通信は保護されるが、DB が侵害された場合にデータが露出する
- DB 全体暗号化のみ: DB サーバー内でのアクセスは平文になる
- 暗号化なし: 憲法違反であり、児童データ保護の観点から絶対に不可

**Implementation Notes**:

- 暗号化実装は `GamifiedMathDrill.Infrastructure` プロジェクトに配置
- `IEncryptionService` インターフェースを定義し、テスト可能に設計
- 暗号化キーのローテーション戦略を文書化

---

### 5. 児童の年齢確認・保護者同意の取得方法

**Question**: 児童の年齢確認と保護者同意をどのように実装するか？

**Decision**: **シンプルな保護者認証フロー**を採用

**Rationale**:

- 1 デバイス = 1 児童のシングルユーザーモデルのため、複雑な認証は不要
- 初回セットアップ時に保護者が設定することを前提とする
- COPPA（米国）や GDPR（EU）のような児童保護法規制を考慮

**Implementation**:

1. **初回セットアップフロー**:

   - アプリ初回起動時に「保護者向けセットアップ」画面を表示
   - 保護者が児童の情報（名前、学年）と保護者のメールアドレスを入力
   - 保護者の同意チェックボックス（プライバシーポリシーと利用規約への同意）
   - 簡易的な保護者認証（例: 「13 × 7 = ?」のような大人向け算数問題、またはメール確認リンク）

2. **データ収集の最小化**:

   - 児童の名前（ニックネーム可）と学年のみ収集
   - メールアドレスは保護者のみ（任意、結果通知用）
   - 生年月日や住所などの PII は収集しない

3. **保護者向け管理機能**:
   - 設定画面に「保護者確認」ボタン（上記の認証を再度要求）
   - 保護者のみがアクセスできる機能: データエクスポート、アカウント削除、学習結果の詳細表示

**Alternatives Considered**:

- 認証なし: 憲法違反であり、児童データ保護の観点から不適切
- OAuth/SSO（Google, Microsoft）: 複雑すぎ、1 デバイス = 1 児童モデルに不適合
- SMS 認証: コストが高く、すべての保護者が携帯電話を持っているとは限らない

**Implementation Notes**:

- 保護者認証ミドルウェアを ASP.NET Core に実装
- 児童向け画面と保護者向け画面を明確に分離（UI/UX）
- プライバシーポリシーと利用規約を平易な日本語で作成

---

### 6. 同時接続ユーザー数の想定

**Question**: 同時接続ユーザー数をどのように想定するか？

**Decision**: **初期スケール: 100 同時ユーザー、拡張可能設計: 1,000 同時ユーザー**

**Rationale**:

- MVP（最小実用製品）としては小規模から開始
- 教育機関（学校、塾）での採用を想定すると、1 クラス 30 人 × 複数クラスで 100 同時ユーザーが現実的
- 将来的な成長（複数学校、一般家庭での普及）を考慮し、1,000 同時ユーザーまでスケール可能な設計

**Performance Targets**:

- 100 同時ユーザー: 平均応答時間 <200ms、95 パーセンタイル <500ms
- 1,000 同時ユーザー: 平均応答時間 <300ms、95 パーセンタイル <1s
- データベース接続プール: 最小 10、最大 50 接続

**Scaling Strategy**:

- **Horizontal Scaling**: Azure App Service または AWS Elastic Beanstalk でオートスケーリング
- **Database Optimization**: インデックス最適化、読み取りレプリカ（必要に応じて）
- **Caching**: Redis や In-Memory Cache で頻繁にアクセスされる問題データをキャッシュ
- **CDN**: 静的リソース（画像、CSS、JS）を CDN（Cloudflare, Azure CDN）で配信

**Implementation Notes**:

- 負荷テストツール（k6, Apache JMeter）で性能検証
- Application Insights または Prometheus でメトリクス監視
- スケーリング閾値をドキュメント化（CPU 70%、メモリ 80%）

---

## Summary of Decisions

| Item                         | Decision                             | Key Reason                                           |
| ---------------------------- | ------------------------------------ | ---------------------------------------------------- |
| フロントエンドフレームワーク | Blazor WebAssembly                   | C# エコシステム統合、コード共有、WASM パフォーマンス |
| データベース（開発）         | SQLite                               | セットアップ不要、ローカル開発に最適                 |
| データベース（本番）         | PostgreSQL                           | スケーラビリティ、JSON 対応、低コスト                |
| E2E テストフレームワーク     | Playwright                           | 高速、信頼性、.NET バインディング                    |
| データ暗号化                 | HTTPS + DB 暗号化 + 列レベル AES-256 | 多層防御、児童データ保護                             |
| 年齢確認・保護者同意         | 初回セットアップ時の保護者認証フロー | シンプル、COPPA/GDPR 準拠                            |
| 同時接続ユーザー数           | 初期 100、拡張 1,000                 | 現実的な MVP スコープ、成長余地                      |

---

## Next Steps (Phase 1)

すべての [NEEDS CLARIFICATION] が解決されました。Phase 1（設計）に進み、以下を作成します：

1. **data-model.md**: エンティティの詳細設計（Problem, Student, Reward, LearningRecord, Level, DailyChallenge, AcquiredReward）
2. **contracts/**: REST API エンドポイント定義（OpenAPI/Swagger）
3. **quickstart.md**: 開発環境セットアップ手順
4. **Constitution Check（再評価）**: Phase 1 設計が憲法に準拠しているか確認
