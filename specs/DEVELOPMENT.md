# 開発ガイドライン

このドキュメントは、spec-kit-smatlaプロジェクトにおける開発ルールと推奨プラクティスを定義します。

## Gitコミットルール

### コミット単位

**必須**: コミットはタスク単位で行うこと

- tasks.mdに記載された各タスク（T001, T002, ...）ごとにコミットを作成する
- 複数の関連タスクをまとめてコミットする場合は、コミットメッセージに全タスクIDを記載する
- 実装中もこまめにコミットし、進捗を記録する

**理由**:
- 変更履歴の追跡が容易になる
- レビューが簡単になる
- 問題が発生した際のロールバックが容易
- タスクの完了状況が明確になる

### コミットメッセージフォーマット

```
<type>(<scope>): <task-ids> <subject>

<body>

<footer>
```

#### Type（必須）
- `feat`: 新機能の追加
- `fix`: バグ修正
- `docs`: ドキュメントのみの変更
- `style`: コードの意味に影響しない変更（フォーマット、セミコロン等）
- `refactor`: バグ修正や機能追加ではないコードの変更
- `perf`: パフォーマンス改善
- `test`: テストの追加や修正
- `chore`: ビルドプロセスやツールの変更

#### Scope（推奨）
- 影響を受ける機能やUser Story（例：US1, US2, US3, US4, US5）
- または影響を受けるモジュール名

#### Task IDs（推奨）
- タスクIDを記載（例：T001, T002-T003）

#### Subject（必須）
- 変更内容の簡潔な説明（50文字以内）
- 現在形で記述（"Add" not "Added"）

#### Body（任意）
- 変更の詳細説明
- 実装したファイルのリスト
- 技術的な決定事項

#### Footer（任意）
- Breaking changes
- 関連Issue番号

### コミット例

```bash
# 単一タスクのコミット
git commit -m "feat(US2): T066 Add ExchangeRequest model

- ExchangeStatus enum (Pending/Approved/Rejected/Cancelled)
- ExchangeRequest model with Student/Reward relations
- Navigation properties configured"

# 複数タスクのコミット
git commit -m "feat(US3): T085-T087 Add approval/rejection UI

- RejectReasonDialog component
- ExchangeRequestList page with filtering
- ExchangeRequestDetail page
- Parent role authorization"

# ドキュメント更新
git commit -m "docs: Update task completion status

- Mark Phase 5 (US2) tasks as complete: T066-T075
- Total: 103/111 tasks (93%) complete"
```

## ブランチ戦略

### ブランチ命名規則

- Feature branches: `{feature-number}-{feature-name}`
  - 例: `003-parent-admin-rewards`
- Bug fix branches: `fix/{issue-number}-{description}`
  - 例: `fix/123-login-error`
- Hotfix branches: `hotfix/{version}-{description}`
  - 例: `hotfix/1.0.1-security-patch`

### ワークフロー

1. `main`ブランチから feature branch を作成
2. タスク単位でコミット
3. feature 完成後、Pull Request を作成
4. レビュー・承認後、`main`にマージ
5. feature branch を削除

## タスク管理

### tasks.mdの更新

実装完了後、必ずtasks.mdを更新する：

```markdown
# 完了前
- [ ] T001 Create branch `003-parent-admin-rewards` from main

# 完了後
- [X] T001 Create branch `003-parent-admin-rewards` from main
```

### フェーズ完了の確認

各Phaseのすべてのタスクが完了したら：
1. tasks.mdのチェックボックスをすべて更新
2. 動作確認を実施
3. テストを実行（該当する場合）
4. 次のPhaseに進む前にコミット＆プッシュ

## コーディング規約

### .NET (C#)

- **命名規則**:
  - クラス・メソッド: PascalCase
  - 変数・パラメータ: camelCase
  - プライベートフィールド: _camelCase
  - 定数: UPPER_SNAKE_CASE

- **コメント**:
  - public APIには必ず XMLドキュメントコメント（/// <summary>）を記述
  - 複雑なロジックには実装コメントを追加

- **ファイル構成**:
  - 1ファイル1クラスを原則とする
  - 名前空間はプロジェクト構造に従う

### Blazor (Razor)

- **コンポーネント命名**: PascalCase
- **パラメータ**: [Parameter]属性を明示
- **イベントハンドラー**: On{EventName}形式（例：OnClick, OnSubmit）

### データベースマイグレーション

- **命名規則**: 
  - 説明的な名前を使用（例：`AddExchangeRequests`, `AddParentDashboardFields`）
  - 日付プレフィックスは自動生成に任せる

- **作成手順**:
  ```bash
  cd backend/src/GamifiedMathDrill.Infrastructure
  dotnet ef migrations add {MigrationName} --startup-project ../GamifiedMathDrill.Api
  ```

- **適用手順**:
  ```bash
  dotnet ef database update --startup-project ../GamifiedMathDrill.Api
  ```

- **確認**:
  - マイグレーション適用後、必ずデータベースの状態を確認
  - 既存データに影響がないことを確認

## Pull Request ガイドライン

### PR作成前のチェックリスト

- [ ] すべてのタスクが完了している
- [ ] tasks.mdが更新されている
- [ ] コードがビルドできる
- [ ] テストが通る（該当する場合）
- [ ] コミットメッセージが規約に従っている
- [ ] 不要なコメントアウトやデバッグコードを削除している

### PRタイトル

```
[Feature-{number}] {Feature Name} - Phase {phase-number}
```

例：
```
[Feature-003] Parent Admin Rewards - Phase 5-7 Implementation
```

### PR説明

以下の情報を含める：

```markdown
## 概要
このPRで実装した機能の概要

## 実装内容
- Phase 5: 交換申請機能（US2）
- Phase 6: 承認/却下機能（US3）
- Phase 7: ダッシュボード（US5）

## 完了タスク
T066-T098 (33 tasks)

## 動作確認
- [ ] 保護者ログイン → 景品管理
- [ ] 子供ログイン → 交換申請
- [ ] 保護者 → 承認/却下
- [ ] ダッシュボード表示

## スクリーンショット
（該当する場合）

## 注意事項
特記事項があれば記載
```

## トラブルシューティング

### よくある問題と解決方法

#### ビルドエラー: "型または名前空間の名前が見つかりません"

1. プロジェクト参照を確認
2. `dotnet restore`を実行
3. IDEを再起動

#### マイグレーションエラー

1. 既存のマイグレーションを確認: `dotnet ef migrations list`
2. データベースをリセット: `dotnet ef database drop`（開発環境のみ）
3. マイグレーションを再適用: `dotnet ef database update`

#### フロントエンドでAPIが呼べない

1. バックエンドが起動しているか確認
2. CORS設定を確認（Program.cs）
3. HttpClientのbaseAddressを確認（Program.cs）
4. JWT tokenが正しく設定されているか確認

## 参考リソース

- [.NET 8.0 Documentation](https://learn.microsoft.com/ja-jp/dotnet/)
- [Blazor Documentation](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/)
- [Entity Framework Core](https://learn.microsoft.com/ja-jp/ef/core/)
- [MudBlazor Components](https://mudblazor.com/)

## 更新履歴

- 2026-02-01: 初版作成（開発ガイドライン、Gitルール）
