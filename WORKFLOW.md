# 開発ワークフロー

このプロジェクトの開発ワークフローに関するルールを記載します。

## ブランチ戦略

### 必須ルール

1. **mainブランチで直接作業しない**
   - 全ての変更は必ずfeatureブランチで行う
   - mainブランチへの直接コミットは禁止
   - **作業開始前に必ずブランチを作成する**
   - mainブランチにいる場合は絶対にファイルを編集しない

2. **ブランチ作成は最優先事項**
   - コーディングを始める前に必ずブランチを作成
   - `git checkout -b feature/xxx` または `git checkout -b fix/xxx`
   - ブランチ作成を忘れた場合は、すぐに以下の手順で対処：
     ```bash
     # 変更をパッチに保存
     git diff > /tmp/changes.patch
     # 変更を元に戻す
     git restore .
     # ブランチを作成
     git checkout -b fix/your-feature-name
     # 変更を再適用
     git apply /tmp/changes.patch
     ```

3. **ブランチ命名規則**
   - `feature/` - 新機能追加
   - `fix/` - バグ修正
   - `refactor/` - リファクタリング
   - `docs/` - ドキュメント更新
   - 例: `fix/parent-child-login-redirect`

### ワークフロー手順

```bash
# 0. 現在のブランチを確認（重要！）
git branch --show-current
# → main なら絶対に作業を開始しない！

# 1. 作業用ブランチを作成（必須）
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

- **最重要**: 作業開始前に必ずブランチを確認・作成する
- **mainブランチでの作業は絶対禁止**
- **忘れやすいポイント**: pushした後のPR作成
- このワークフローを必ず守ること
- わからないことがあればチームに相談

## チェックリスト

作業前に必ず確認：
- [ ] mainブランチにいないか確認（`git branch --show-current`）
- [ ] 新しいブランチを作成したか
- [ ] コミットメッセージは規則に従っているか
- [ ] pushした後にPRを作成したか
