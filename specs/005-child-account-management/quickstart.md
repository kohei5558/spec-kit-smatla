# Quickstart Guide: 子供アカウント管理画面

**Feature**: 005-child-account-management  
**For**: 開発者向けクイックスタート  
**Date**: 2026年2月7日

このガイドでは、子供アカウント管理機能の開発環境セットアップから基本的な動作確認までを30分以内で完了できるよう案内します。

---

## Prerequisites

開始前に以下が準備されていることを確認してください：

- [ ] .NET 8 SDK インストール済み
- [ ] PostgreSQL 14+ 起動中（または Docker Compose使用）
- [ ] Visual Studio 2022 または VS Code + C# Dev Kit
- [ ] Git（ブランチ `005-child-account-management` にチェックアウト済み）
- [ ] 既存のバックエンドが正常に起動すること確認済み

---

## Step 1: ブランチ確認とプロジェクト起動 (5分)

### 1.1 ブランチ確認

```bash
# 現在のブランチ確認
git branch --show-current
# 出力: 005-child-account-management

# 最新の変更を取得
git pull origin 005-child-account-management
```

### 1.2 データベースマイグレーション

```bash
cd backend/src/GamifiedMathDrill.Api

# マイグレーション適用
dotnet ef database update --project ../GamifiedMathDrill.Infrastructure

# 期待される出力:
# Applying migration '20260207_AddChildAccountManagementFields'
# Done.
```

### 1.3 バックエンド起動

```bash
# API起動
dotnet run --project GamifiedMathDrill.Api

# 起動確認（別ターミナルで）
curl http://localhost:5000/api/health
# 出力: {"status":"Healthy"}
```

### 1.4 フロントエンド起動

```bash
cd ../../frontend/GamifiedMathDrill.Client

# フロントエンド起動
dotnet run

# ブラウザで開く: http://localhost:5001
```

---

## Step 2: シードデータ確認 (3分)

### 2.1 プリセットアバター確認

ブラウザまたはcurlで以下にアクセス：

```bash
curl http://localhost:5000/api/preset-avatars
```

**期待される出力**:

```json
[
  {"id": 1, "name": "猫", "imageUrl": "/avatars/cat-01.png", "displayOrder": 1},
  {"id": 2, "name": "犬", "imageUrl": "/avatars/dog-01.png", "displayOrder": 2},
  ...
]
```

### 2.2 テスト用保護者アカウント

マイグレーション時に作成されるテスト用保護者アカウント：

- **Email**: `parent.test@example.com`
- **Password**: `Test1234!`

---

## Step 3: 基本フロー動作確認 (15分)

### 3.1 保護者ログイン

1. ブラウザで `http://localhost:5001` を開く
2. 「保護者ログイン」をクリック
3. テストアカウントでログイン：
   - Email: `parent.test@example.com`
   - Password: `Test1234!`
4. 保護者ダッシュボードが表示されることを確認

### 3.2 子供アカウント作成

1. ダッシュボードで「子供アカウント管理」メニューをクリック
2. 「新規追加」ボタンをクリック
3. 作成フォームに入力：
   - 名前: `太郎`
   - 学年: `3年生` を選択
   - アバター: 猫のアイコンを選択
   - PIN: `1234` を入力
4. 「作成」ボタンをクリック
5. 一覧に「太郎」が表示されることを確認

**API経由での確認**:

```bash
# 認証トークン取得（保護者ログイン）
TOKEN=$(curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"parent.test@example.com","password":"Test1234!"}' \
  | jq -r '.token')

# 子供アカウント一覧取得
curl http://localhost:5000/api/child-accounts \
  -H "Authorization: Bearer $TOKEN"
```

### 3.3 子供ログイン

1. 右上のユーザーメニューから「ログアウト」
2. 「子供ログイン」をクリック
3. 太郎のカード（猫のアバター）をクリック
4. PIN `1234` を入力
5. 子供用ホーム画面が表示されることを確認

**API経由での確認**:

```bash
# 子供アカウントIDを取得（前ステップのレスポンスから）
CHILD_ID="<上記で取得したID>"

# 子供ログイン
curl -X POST http://localhost:5000/api/auth/child/login \
  -H "Content-Type: application/json" \
  -d "{\"childAccountId\":\"$CHILD_ID\",\"pin\":\"1234\"}"
```

### 3.4 子供アカウント編集

1. 保護者で再ログイン
2. 子供アカウント一覧で「太郎」の「編集」ボタンをクリック
3. 学年を `4年生` に変更
4. アバターを犬に変更
5. 「保存」ボタンをクリック
6. 変更が反映されていることを確認

### 3.5 学習状況確認

1. 子供アカウント一覧で「太郎」の「詳細」ボタンをクリック
2. 学習統計が表示されることを確認：
   - 総問題数: 0
   - 正答率: 0%
   - 獲得ポイント: 0
   - 連続学習日数: 0
   - 「まだ学習記録がありません」メッセージ

**API経由での確認**:

```bash
curl http://localhost:5000/api/child-accounts/$CHILD_ID \
  -H "Authorization: Bearer $TOKEN"
```

---

## Step 4: エッジケース確認 (7分)

### 4.1 PIN間違い（ロックアウト）

1. 子供ログイン画面で太郎のカードを選択
2. 間違ったPIN `9999` を入力 → 「PINが正しくありません」
3. 再度間違ったPINを2回入力（合計3回）
4. 「PINの入力に3回失敗しました。5分後に再試行してください」メッセージ確認
5. 5分待機またはメモリキャッシュをクリア（開発環境では再起動）
6. 正しいPIN `1234` でログイン成功を確認

### 4.2 同名アカウント作成エラー

1. 保護者で「新規追加」をクリック
2. 名前に `太郎`（既存）を入力
3. その他の情報を入力して「作成」をクリック
4. 「同じ名前の子供アカウントが既に存在します」エラーメッセージ確認

### 4.3 アカウント一時停止

1. 子供アカウント一覧で「太郎」の「停止」ボタンをクリック
2. 確認ダイアログで「停止する」を選択
3. ステータスが「停止中」に変更されることを確認
4. ログアウト後、子供ログインで太郎を選択しPIN入力
5. 「このアカウントは現在使用できません。保護者に相談してください」メッセージ確認
6. 保護者で「再開」ボタンをクリックし、再度ログイン可能になることを確認

---

## Step 5: テスト実行 (5分)

### 5.1 統合テスト実行

```bash
cd backend/tests/GamifiedMathDrill.Tests.Integration

# 子供アカウント関連テストのみ実行
dotnet test --filter "FullyQualifiedName~ChildAccount"

# 期待される出力:
# Passed:  12
# Failed:  0
# Skipped: 0
```

### 5.2 テストカバレッジ確認

```bash
# 主要テストファイル
ls -l *ChildAccount*.cs

# 出力:
# ChildAccountTests.cs          - CRUD操作
# ChildLoginTests.cs            - カード形式ログイン
# ChildAccountSecurityTests.cs  - PINロックアウト
```

---

## Troubleshooting

### Issue 1: マイグレーションエラー

**症状**: `dotnet ef database update` でエラー

**解決策**:

```bash
# データベース削除して再作成
dotnet ef database drop --force
dotnet ef database update
```

### Issue 2: アバター画像が表示されない

**症状**: カード一覧でアバター画像が404

**解決策**:

```bash
# wwwroot/avatars/ にファイルが存在するか確認
ls frontend/wwwroot/avatars/

# ファイルがない場合、サンプル画像を配置
# （開発環境用のプレースホルダー画像をコピー）
```

### Issue 3: PINロックアウトが解除されない

**症状**: 5分待っても再ログインできない

**解決策**:

```bash
# メモリキャッシュはアプリ再起動でクリア
# APIを再起動
cd backend/src/GamifiedMathDrill.Api
dotnet run
```

### Issue 4: 子供ログインでトークンエラー

**症状**: ログイン後すぐに401エラー

**解決策**:

- `appsettings.Development.json` でJWT設定を確認
- トークン有効期限が短すぎないか確認（開発環境では24時間推奨）

---

## Next Steps

クイックスタートが完了したら、以下を確認してください：

1. **仕様書**: [spec.md](./spec.md) - 全ユーザーストーリーと受け入れ基準
2. **データモデル**: [data-model.md](./data-model.md) - エンティティ詳細とリレーション
3. **API仕様**: [contracts/child-account-api.yaml](./contracts/child-account-api.yaml) - 全エンドポイント定義
4. **実装計画**: [plan.md](./plan.md) - アーキテクチャと技術選択
5. **タスク一覧**: tasks.md - `/speckit.tasks` コマンドで生成予定

---

## Development Environment

```bash
# 環境情報確認
dotnet --version    # 8.0.x
psql --version      # PostgreSQL 14.x

# プロジェクト起動確認
cd backend/src/GamifiedMathDrill.Api && dotnet run &  # API: http://localhost:5000
cd frontend/GamifiedMathDrill.Client && dotnet run &  # UI: http://localhost:5001

# ログ確認
tail -f backend/src/GamifiedMathDrill.Api/logs/app.log
```

---

## Key Files Reference

```
backend/
├── src/GamifiedMathDrill.Api/
│   └── Controllers/ChildAccountController.cs       # 新規追加
├── src/GamifiedMathDrill.Core/
│   ├── Services/ChildAccountService.cs             # 新規追加
│   └── Models/PresetAvatar.cs                      # 新規追加
└── src/GamifiedMathDrill.Infrastructure/
    ├── Identity/ApplicationUser.cs                 # GradeLevel, IsActive追加
    └── Data/Seed/AvatarSeeder.cs                   # 新規追加

frontend/
├── Pages/Parent/
│   ├── ChildAccountManagement.razor                # 新規追加
│   ├── CreateChildAccount.razor                    # 新規追加
│   └── EditChildAccount.razor                      # 新規追加
└── Components/
    ├── ChildAccountCard.razor                      # 新規追加
    └── AvatarSelector.razor                        # 新規追加
```

---

**Status**: ✅ Quickstart guide completed. Ready for development.
