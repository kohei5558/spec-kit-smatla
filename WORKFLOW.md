# 開発ワークフロー

このプロジェクトの開発ワークフローに関するルールを記載します。

## ブランチ戦略

### ブランチ構成

- **main**: 本番環境用ブランチ（保護ブランチ、直接コミット禁止）
- **develop**: 開発用ブランチ（全ての機能追加・修正の起点）
- **feature/xxx, fix/xxx**: 作業用ブランチ（developから派生）

### 必須ルール

1. **mainブランチとdevelopブランチで直接作業しない**
   - 全ての変更は必ずfeature/fixブランチで行う
   - main・developブランチへの直接コミットは禁止
   - **作業開始前に必ずdevelopブランチから作業ブランチを作成する**

2. **ブランチ作成は最優先事項**
   - **必ずdevelopブランチから新しいブランチを切る**
   - コーディングを始める前に必ずブランチを作成
   - 手順:
     ```bash
     # developブランチに移動
     git checkout develop
     git pull origin develop
     # 作業ブランチを作成
     git checkout -b feature/xxx または fix/xxx
     ```
   - ブランチ作成を忘れた場合は、すぐに以下の手順で対処：
     ```bash
     # 変更をパッチに保存
     git diff > /tmp/changes.patch
     # 変更を元に戻す
     git restore .
     # developブランチに移動
     git checkout develop
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

# 1. developブランチに移動して最新化（必須）
git checkout develop
git pull origin develop

# 2. 作業用ブランチを作成（必須）
git checkout -b fix/your-feature-name

# 3. 変更を実装
# ... コーディング ...

# 4. 変更をコミット
git add <files>
git commit -m "fix: 変更内容の説明"

# 5. リモートにプッシュ
git push -u origin fix/your-feature-name

# 6. PRを作成（必須）
# base: develop (重要！mainではなくdevelopへのPR)
gh pr create --title "fix: 変更内容" --body-file pr_body.md --base develop
# または
# GitHub UIでPRを作成（baseをdevelopに設定）

# 7. レビュー・修正
# レビュー指摘があれば同じブランチで修正してpush

# 8. マージ
# PRが承認されたらGitHub UIでマージ（developへ）

# 9. developブランチに戻る
git checkout develop
git pull origin develop

# 10. mainへのリリース（定期的に実施）
# developからmainへのPRを作成してマージ
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
- developブランチは開発用環境として使用

## 注意事項

- **最重要**: 作業開始前に必ずdevelopブランチから作業ブランチを作成する
- **main・developブランチでの作業は絶対禁止**
- **PRのbase指定**: 必ずdevelopブランチへのPRを作成（mainではない）
- **忘れやすいポイント**: pushした後のPR作成
- このワークフローを必ず守ること
- わからないことがあればチームに相談

## チェックリスト

作業前に必ず確認：

- [ ] developブランチから最新を取得したか（`git pull origin develop`）
- [ ] developブランチから新しいブランチを作成したか
- [ ] コミットメッセージは規則に従っているか
- [ ] pushした後にPRを作成したか
- [ ] PRのbaseがdevelopになっているか確認
