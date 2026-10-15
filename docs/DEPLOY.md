# インターネットに公開する手順（家族で使う・無料）

この手順で、家のタブレットやスマホから `https://（名前）.pages.dev` を開いて使えるようにします。所要時間は30分〜1時間ほどです。
各サービスの画面の文言・配置は変わることがあります。見つからないときは、近い名前のボタンを探してください。

## 全体の形

| 役割 | サービス（無料プラン） | アドレスの例 |
|---|---|---|
| 画面 | Cloudflare Pages | `https://kazoku-keisan.pages.dev` |
| API | Render（Web Service） | `https://gamified-math-drill-api.onrender.com` |
| DB | Neon（PostgreSQL） | （画面からは見えない） |

**無料プランで知っておくこと**

- API は15分使われないと止まります。次に開いたとき、起きるまで最大1分ほど「じゅんびちゅう…」と表示されます（故障ではありません）
- DB の容量は 1GB です（家族で使うには十分です）。無料の DB はバックアップの保証がありません
- 招待コードを知らない人は保護者登録できません（家族だけで使うため）

## 0. 準備

- GitHub のこのリポジトリで、作業ブランチを **main に取り込んでおきます**（自動公開は main を使います）
- 次の値を決めてメモしておきます
  - **画面の名前**（英小文字・数字・ハイフン。例: `kazoku-keisan`）。画面のアドレスが `https://（名前）.pages.dev` になります
  - **招待コード**（家族だけに伝える合言葉。例: `sakura-2026-xyz`。推測されにくいものにする）

## 1. DB を作る（Neon）

1. https://neon.com でアカウントを作る（GitHub アカウントでログインできます）
2. 「New Project」でプロジェクトを作る
   - Region: **AWS Asia Pacific (Singapore)** など日本に近い地域
3. ダッシュボードの「Connect」（接続情報）で、接続文字列（`postgresql://ユーザー:パスワード@ホスト/データベース?sslmode=require` の形）を表示する
4. 次の形に書き換えてメモする（API はこの形で読みます）

   ```
   Host=ホスト;Database=データベース;Username=ユーザー;Password=パスワード;SSL Mode=Require
   ```

   例: `postgresql://neondb_owner:AbC123@ep-cool-name-123456.ap-southeast-1.aws.neon.tech/neondb?sslmode=require` なら

   ```
   Host=ep-cool-name-123456.ap-southeast-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=AbC123;SSL Mode=Require
   ```

   **この文字列はパスワードを含みます。人に見せたり、リポジトリに書いたりしないでください。**

## 2. 画面の置き場所を作る（Cloudflare Pages）

1. https://dash.cloudflare.com でアカウントを作る
2. 「Workers & Pages」→「作成（Create）」→「Pages」→「アセットをアップロード（Direct Upload / Upload assets）」を選ぶ
3. プロジェクト名に **0 で決めた画面の名前** を入れて作成する
   - 最初のアップロードを求められたら、中身は何でもかまいません（適当なファイル1つでOK。次の手順で自動的に置き換わります）
4. 作られたアドレス（`https://（名前）.pages.dev`）をメモする。名前が使われていると末尾に文字が付くことがあります
5. 右上のアカウントのメニューなどから **アカウント ID** を確認してメモする（「Workers & Pages」の画面の右側にも表示されます）
6. 「My Profile」→「API Tokens」→「Create Token」で、**Cloudflare Pages の Edit（編集）権限** を持つトークンを作り、メモする（一度しか表示されません）

## 3. API を動かす（Render）

1. https://render.com でアカウントを作る（GitHub アカウントでログインし、このリポジトリへのアクセスを許可します）
2. 「New」→「Blueprint」→ このリポジトリを選ぶ（リポジトリの `render.yaml` が読み込まれます）
3. 入力を求められる値を入れる

   | 項目 | 入れる値 |
   |---|---|
   | `ConnectionStrings__DefaultConnection` | 1 でメモした接続文字列（`Host=...;SSL Mode=Require`） |
   | `Cors__AllowedOrigins__0` | 2 でメモした画面のアドレス（例: `https://kazoku-keisan.pages.dev`。最後の `/` は付けない） |
   | `Registration__InviteCode` | 0 で決めた招待コード |

   ログイン用の秘密鍵（`Jwt__SecretKey`）は自動で作られます。
4. 作成（Apply / Deploy）すると、ビルドと起動が始まります（初回は5〜10分ほど）
5. 作られたサービスのアドレス（`https://（サービス名）.onrender.com`）をメモする
6. ブラウザで `https://（サービス名）.onrender.com/healthz` を開き、`"ok"` と出れば API は動いています

## 4. 画面を公開する（GitHub Actions）

1. GitHub のリポジトリの「Settings」→「Secrets and variables」→「Actions」を開く
2. 「Secrets」タブに次の2つを追加する

   | 名前 | 値 |
   |---|---|
   | `CLOUDFLARE_API_TOKEN` | 2-6 の API トークン |
   | `CLOUDFLARE_ACCOUNT_ID` | 2-5 のアカウント ID |

3. 「Variables」タブに次の2つを追加する

   | 名前 | 値 |
   |---|---|
   | `CLOUDFLARE_PAGES_PROJECT` | 画面の名前（例: `kazoku-keisan`） |
   | `API_BASE_ADDRESS` | 3-5 の API のアドレス（例: `https://gamified-math-drill-api.onrender.com`。最後の `/` は付けない） |

4. 「Actions」タブ →「Deploy frontend」→「Run workflow」で実行する（数分かかります）
5. 緑のチェックが付いたら、画面のアドレス（`https://（名前）.pages.dev`）を開く

## 5. 使い始める

1. 画面のアドレスを開く（初回は「じゅんびちゅう…」のあと、ログイン画面が出ます）
2. 「新規登録」で保護者アカウントを作る（**招待コード** を入れる）
3. 「子供アカウント管理」で子供を登録し、**子供が使うタブレットで**「この端末を子供用に登録する」を押す
4. そのタブレットで子供ログイン画面を開き、子供が PIN でログインする

家のタブレットでは、画面のアドレスをホーム画面に追加しておくと便利です。

## 更新のしかた

- main に push すると、自動で新しい版になります
  - API: Render が自動で作り直します（DB の構造の変更・問題の入れ替えも起動時に反映されます）
  - 画面: GitHub Actions の「Deploy frontend」が自動で実行されます（`frontend/` を変更したときだけ）

## 困ったとき

| 症状 | 確認すること |
|---|---|
| 「つながりませんでした」と出る | `https://（API）.onrender.com/healthz` が開けるか。Render の「Logs」にエラーが出ていないか |
| ログイン・登録でエラーになる | Render の `Cors__AllowedOrigins__0` が画面のアドレスと完全に同じか（`https://` から、最後の `/` なし） |
| 「招待コードが正しくありません」 | Render の `Registration__InviteCode` と同じか（大文字・小文字も区別します） |
| API が起動しない | Render の「Logs」を確認。接続文字列の形（`Host=...;SSL Mode=Require`）が正しいか |

## 一般公開（第2段階）の前にやること

- プライバシーポリシー・利用規約を用意し、画面から見られるようにする
- 保護者の退会（データの削除）ができるようにする
- DB のバックアップの方法を決める
- パスワード再設定のメールを送れるようにする（今は送らない設定）
- Render の `Registration__InviteCode` を消すと、誰でも登録できるようになる
