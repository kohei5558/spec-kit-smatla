# 開発ワークフロー

このプロジェクトの開発ワークフローに関するルールを記載します。

## ブランチ戦略

### 必須ルール

1. **mainブランチで直接作業しない**
   - 全ての変更は必ずfeatureブランチで行う
   - mainブランチへの直接コミットは禁止

2. **ブランチ命名規則**
   - `feature/` - 新機能追加
   - `fix/` - バグ修正
   - `refactor/` - リファクタリング
   - `docs/` - ドキュメント更新
   - 例: `fix/parent-child-login-redirect`

### ワークフロー手順

```bash
# 1. 作業用ブランチを作成
git checkout -b fix/your-feature-name

# 2. 変更を実装
# ... コーディング ...

# 3. 変更をコミット
git add <files>
git commit -m "fix: 変更内容の説明"

# 4. リモートにプッシュ
git push -u origin fix/your-feature-name

# 5. PRを作成（必須）
gh pr create --title "fix: 変更内容" --body-file pr_body.md --base main
# または
# GitHub UIでPRを作成

# 6. レビュー・修正
# レビュー指摘があれば同じブランチで修正してpush

# 7. マージ
# PRが承認されたらGitHub UIでマージ

# 8. mainブランチに戻る
git checkout main
git pull origin main
```

## PR（Pull Request）ルール

### 必須事項

1. **pushした後は必ずPRを作成する**
   - ブランチをpushしたら即座にPRを作成
   - PRなしでのマージは禁止

2. **PRテンプレート**
   ```markdown
   ## 概要
   変更の概要を記載

   ## 変更内容
   - 変更点1
   - 変更点2

   ## 修正理由
   なぜこの変更が必要か

   ## テスト
   - [ ] テスト項目1
   - [ ] テスト項目2

   ## 関連Issue/PR
   関連するIssueやPRがあれば記載
   ```

3. **コミットメッセージ規則**
   - `feat:` - 新機能
   - `fix:` - バグ修正
   - `refactor:` - リファクタリング
   - `docs:` - ドキュメント
   - `test:` - テスト追加・修正
   - `chore:` - ビルドプロセスや補助ツールの変更

## コードレビュー

- PRは必ずレビューを受ける
- レビュー指摘は同じブランチで修正してpush
- 承認後にマージ

## デプロイ

- mainブランチへのマージ後、自動的にデプロイされる（CI/CD設定がある場合）

## 注意事項

- **忘れやすいポイント**: pushした後のPR作成
- このワークフローを必ず守ること
- わからないことがあればチームに相談
