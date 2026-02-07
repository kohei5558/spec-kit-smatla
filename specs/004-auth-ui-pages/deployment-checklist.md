# デプロイメントチェックリスト - 004-auth-ui-pages

このドキュメントは、認証機能（ログイン、パスワードリセット、新規登録）を本番環境にデプロイする際の手順とチェックポイントを記載しています。

---

## 1. 事前準備

### 1.1 データベース移行

- [ ] **マイグレーション実行**

  ```bash
  cd backend/src/GamifiedMathDrill.Api
  dotnet ef database update
  ```

- [ ] **新しいテーブルの確認**
  - `PasswordResetTokens` テーブルが作成されていること
  - 必要なカラム: `Id`, `UserId`, `TokenHash`, `CreatedAt`, `ExpiresAt`, `IsUsed`, `IpAddress`

- [ ] **インデックスの確認**
  - `PasswordResetTokens.UserId` にインデックスが作成されていること
  - `PasswordResetTokens.TokenHash` にインデックスが作成されていること

### 1.2 環境変数の設定

本番環境の `appsettings.Production.json` または環境変数で以下を設定：

#### JWT設定 (必須)

- [ ] **Jwt:SecretKey**
  - 最低32文字以上のランダムな文字列
  - 本番環境用に新しいキーを生成すること
  - 例: `openssl rand -base64 64`
- [ ] **Jwt:Issuer**
  - 発行者名（例: "GamifiedMathDrill.Api"）
- [ ] **Jwt:Audience**
  - 対象者名（例: "GamifiedMathDrill.Client"）
- [ ] **Jwt:SessionExpiryMinutes**
  - セッション（RememberMe=false）の有効期限（デフォルト: 60分）
- [ ] **Jwt:RememberMeExpiryMinutes**
  - 永続的ログイン（RememberMe=true）の有効期限（デフォルト: 43200分 = 30日）

#### メール設定 (必須)

- [ ] **Email:EnableSending**
  - `true` に設定（本番環境ではメール送信を有効化）
- [ ] **Email:SmtpHost**
  - SMTPサーバーのホスト名（例: "smtp.gmail.com", "smtp.sendgrid.net"）
- [ ] **Email:SmtpPort**
  - SMTPポート（通常は 587 または 465）
- [ ] **Email:Username**
  - SMTP認証用のユーザー名（メールアドレス）
- [ ] **Email:Password**
  - SMTP認証用のパスワード（環境変数で管理推奨）
- [ ] **Email:FromAddress**
  - 送信元メールアドレス（例: "noreply@yourdomain.com"）
- [ ] **Email:FromName**
  - 送信者名（例: "算数ゲーム運営チーム"）

#### アプリケーションURL (必須)

- [ ] **AppUrl**
  - フロントエンドのURL（パスワードリセットリンクに使用）
  - 例: "https://yourdomain.com"

#### 暗号化設定 (必須)

- [ ] **Encryption:Key**
  - 256ビット（32バイト）のBase64エンコードされたキー
  - 例: `openssl rand -base64 32`
- [ ] **Encryption:IV**
  - 128ビット（16バイト）のBase64エンコードされたIV
  - 例: `openssl rand -base64 16`

#### データベース接続文字列 (必須)

- [ ] **ConnectionStrings:DefaultConnection**
  - 本番データベースの接続文字列
  - SQLite例: "Data Source=/var/data/gamified_math_drill.db"
  - SQL Server例: "Server=localhost;Database=GamifiedMathDrill;User Id=sa;Password=YourPassword;"

---

## 2. セキュリティチェック

### 2.1 パスワードセキュリティ

- [ ] **ハッシュアルゴリズム**: ASP.NET Identity のデフォルト（PBKDF2）が使用されていること
- [ ] **パスワード強度**: 最低8文字、大文字・小文字・数字・記号を含むこと（RegisterRequest.csで検証済み）
- [ ] **パスワードリセットトークン**: SHA256でハッシュ化されて保存されていること

### 2.2 HTTPS設定

- [ ] **SSL証明書**: 有効なSSL証明書が設定されていること
- [ ] **HSTS**: Strict-Transport-Security ヘッダーが有効（Program.cs で設定済み）
- [ ] **リダイレクト**: HTTPからHTTPSへの自動リダイレクトが有効

### 2.3 レート制限

- [ ] **RateLimitMiddleware**: 有効化されていること（Program.cs で確認）
- [ ] **設定値**:
  - 最大試行回数: 5回（15分間）
  - ブロック期間: 15分
- [ ] **対象エンドポイント**: `/api/auth/login`, `/api/auth/register`, `/api/auth/forgot-password`

### 2.4 トークン管理

- [ ] **パスワードリセットトークン有効期限**: 15分（AuthService.csで確認）
- [ ] **JWT有効期限**: セッション60分、RememberMe 30日（appsettings で確認）
- [ ] **使用済みトークンの無効化**: `IsUsed` フラグで管理されていること

---

## 3. 機能テスト

### 3.1 ログイン機能

- [ ] **正常なログイン**: 正しいメールアドレスとパスワードでログインできること
- [ ] **失敗ログイン**: 間違ったパスワードで401 Unauthorizedが返ること
- [ ] **RememberMe機能**: チェックボックスがトークン有効期限に反映されること
- [ ] **レート制限**: 5回失敗後に429 Too Many Requestsが返ること

### 3.2 パスワードリセット機能

- [ ] **メール送信**: forgot-password APIでメールが送信されること
- [ ] **リセットリンク**: メール内のリンクが正しいこと（`{AppUrl}/reset-password?token={token}`）
- [ ] **トークン検証**: 有効なトークンでパスワードリセット画面が表示されること
- [ ] **パスワード変更**: 新しいパスワードでログインできること
- [ ] **トークン無効化**: 使用済みトークンは再利用できないこと

### 3.3 新規登録機能

- [ ] **アカウント作成**: 新しいメールアドレスでアカウントが作成されること
- [ ] **自動ログイン**: 登録後に自動的にログインできること
- [ ] **重複メール**: 既存のメールアドレスでは登録できないこと
- [ ] **パスワード検証**: 弱いパスワードが拒否されること

---

## 4. パフォーマンステスト

### 4.1 レスポンスタイム

- [ ] **ログインAPI**: 平均 < 500ms
- [ ] **登録API**: 平均 < 1s
- [ ] **パスワードリセットAPI**: 平均 < 2s（メール送信含む）

### 4.2 負荷テスト

- [ ] **同時ログイン**: 100ユーザー同時ログインで問題ないこと
- [ ] **レート制限**: 大量の失敗試行でサーバーがダウンしないこと

---

## 5. 監視設定

### 5.1 ログ設定

- [ ] **ログレベル**: 本番環境で "Warning" 以上（appsettings.Production.json）
- [ ] **ログ保存先**: `/logs` ディレクトリに日付別ファイルが作成されること
- [ ] **センシティブ情報**: パスワードやトークンがログに記録されていないこと

### 5.2 アラート設定

- [ ] **失敗ログイン**: 連続失敗が閾値を超えたらアラート
- [ ] **メール送信失敗**: SMTP エラーが発生したらアラート
- [ ] **データベース接続エラー**: DB接続失敗でアラート

---

## 6. ロールバックプラン

### 6.1 データベースロールバック

- [ ] **マイグレーション履歴**: 現在のマイグレーション名を記録
- [ ] **ロールバックコマンド**:
  ```bash
  dotnet ef database update <前のマイグレーション名>
  ```

### 6.2 アプリケーションロールバック

- [ ] **前バージョンのバイナリ**: 保存されていること
- [ ] **設定ファイルのバックアップ**: appsettings.jsonのバックアップがあること

---

## 7. デプロイ後の確認

### 7.1 即時確認（デプロイ後5分以内）

- [ ] **アプリケーション起動**: サーバーが正常に起動していること
- [ ] **ヘルスチェック**: `/health` エンドポイントが200を返すこと
- [ ] **データベース接続**: DBクエリが実行できること
- [ ] **ログ出力**: ログファイルに起動ログが記録されていること

### 7.2 機能確認（デプロイ後30分以内）

- [ ] **ログイン**: 既存ユーザーでログインできること
- [ ] **新規登録**: 新しいアカウントを作成できること
- [ ] **パスワードリセット**: メールが届くこと

### 7.3 モニタリング（デプロイ後24時間）

- [ ] **エラーログ**: 予期しないエラーが発生していないこと
- [ ] **パフォーマンス**: レスポンスタイムが許容範囲内であること
- [ ] **メール送信**: パスワードリセットメールが正常に送信されていること

---

## 8. トラブルシューティング

### 8.1 よくある問題と対処法

#### ログインできない

- [ ] JWT SecretKey が正しく設定されているか確認
- [ ] データベース接続が正常か確認
- [ ] ユーザーが存在するか確認（`AspNetUsers` テーブル）

#### パスワードリセットメールが届かない

- [ ] `Email:EnableSending` が `true` になっているか確認
- [ ] SMTP設定（ホスト、ポート、認証情報）が正しいか確認
- [ ] ログファイルでメール送信エラーを確認
- [ ] 迷惑メールフォルダを確認

#### レート制限が機能しない

- [ ] `Program.cs` で `RateLimitMiddleware` が登録されているか確認
- [ ] テスト環境（`Testing`）ではミドルウェアが無効化されている

#### 429エラーが表示される

- [ ] 15分待つか、サーバーを再起動（開発環境のみ）
- [ ] `RateLimitMiddleware.CleanupOldAttempts()` を手動で呼び出す（開発環境のみ）

---

## 9. 完了確認

すべてのチェック項目が完了したら、以下にサインしてください：

- **デプロイ実施者**: ********\_\_\_********
- **デプロイ日時**: ********\_\_\_********
- **承認者**: ********\_\_\_********
- **本番確認者**: ********\_\_\_********

---

## 10. 関連ドキュメント

- [specs/004-auth-ui-pages/spec.md](./spec.md) - 機能仕様
- [specs/004-auth-ui-pages/plan.md](./plan.md) - 実装計画
- [specs/004-auth-ui-pages/quickstart.md](./quickstart.md) - クイックスタートガイド
- [backend/src/GamifiedMathDrill.Api/appsettings.Development.json](../../backend/src/GamifiedMathDrill.Api/appsettings.Development.json) - 設定例

---

**最終更新**: 2026-02-07  
**バージョン**: 1.0
