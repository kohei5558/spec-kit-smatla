# インターネットに公開する手順（家族で使う・無料）

この手順で、家のタブレットやスマホから `https://（名前）.pages.dev` を開いて使えるようにします。所要時間は30分〜1時間ほどです。
各サービスの画面の文言・配置は変わることがあります。見つからないときは、近い名前のボタンを探してください。

## 全体の形

| 役割 | サービス（無料プラン） | アドレスの例 |
|---|---|---|
| 画面 | Cloudflare Pages | `https://kazoku-keisan.pages.dev` |
| API | Render（Web Service） | `https://gamified-math-drill-api.onrender.com` |
| DB | Neon（PostgreSQL） | （画面からは見えない） |

| サービス | ここで動くもの | 役割 |
|---|---|---|
| Cloudflare Pages | Blazor WebAssembly のファイル | 画面のファイルを置いて配るだけ。計算はスマホ・タブレットのブラウザの中で動く |
| Render | ASP.NET Core の API（`backend/Dockerfile`） | ログイン・答え合わせ・ポイント・景品などの処理。DB に読み書きできるのは API だけ |
| Neon | PostgreSQL | 保護者・子供・学習記録・ポイント・景品・問題など、すべてのデータ |

```
スマホ・タブレット（ブラウザ）
   │ ①画面のファイルをもらう
   ├──────────────► Cloudflare Pages
   │ ②ログインや答え合わせを頼む
   └──────────────► Render（API）──③読み書き──► Neon（DB）
```

- DB の接続文字列は Render にだけ入れる（ブラウザは DB に直接さわらない）
- Render の `Cors__AllowedOrigins__0` は「この画面から来た頼みだけ受けつける」ための設定
- GitHub の `API_BASE_ADDRESS` は、画面に API の場所を教えるための設定

**DB に Neon を選んだ理由**（2026年11月に見直した）

- 無料プランで、使われないと5分で眠るが、次のアクセスで自動で起きる。家族で使うと1週間使わない日もある
- Supabase の無料プランは、1週間ほど動きが少ないとプロジェクトが一時停止し、管理画面から手で戻すまで使えない
- Supabase に付いてくるログイン機能などは、このアプリでは使わない（ログインは API が ASP.NET Core Identity で持っている）
- どちらも中身は PostgreSQL なので、乗りかえるときは接続文字列を変えるだけでよい

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
   - プロジェクトを作ったあとに「Set up Neon with your coding agent」（AI に設定させる指示文）が出ることがありますが、使いません。Node.js のアプリ向けで、パスワード入りの `.env` ファイルを作るためです。閉じて「Connect」に進みます
   - Branch は `production`（または `main`）、接続プール（Pooled connection）はオフ（Direct）で大丈夫です
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
2. 左のメニューの「Compute」（または「Workers & Pages」）→「Create App」を開き、選択肢の下にある「**Need to use the legacy Pages workflow? Continue to Pages**」を押して、「アセットをアップロード（Direct Upload / Upload assets）」を選ぶ
   - 「Create App」の選択肢（Upload your static files など）は Workers 用です。このアプリの自動公開は Pages にアップロードする作りなので、Pages で作ります
3. プロジェクト名に **0 で決めた画面の名前** を入れて作成する
   - 最初のアップロードを求められたら、中身は何でもかまいません（適当なファイル1つでOK。次の手順で自動的に置き換わります）
4. 作られたアドレス（`https://（名前）.pages.dev`）をメモする。名前が使われていると末尾に文字が付くことがあります
5. 右上のアカウントのメニューなどから **アカウント ID** を確認してメモする（「Workers & Pages」の画面の右側にも表示されます）
6. 「My Profile」→「API Tokens」→「Create Token」で、**Cloudflare Pages の Edit（編集）権限だけ** を持つトークンを作り、メモする（一度しか表示されません）
   - Policies のひな形の一覧で「**Start from scratch**（Custom）」を選ぶ（「Edit Cloudflare Workers」は余分な権限が多いので使わない）
   - 名前は分かるものにする（例: `github-actions-pages-deploy`）
   - 権限は「Developer Platform」の **Cloudflare Pages の Edit** だけにチェック
   - 対象のアカウントは「All accounts」ではなく **自分のアカウント** にしぼる
   - IP の制限（IP Filtering）と有効期限（TTL）は付けない（GitHub Actions は毎回ちがう IP から動き、期限が切れると画面の更新が止まるため）

## 3. API を動かす（Render）

1. https://render.com でアカウントを作る（GitHub アカウントでログインし、このリポジトリへのアクセスを許可します）
2. 「New」→「Blueprint」（または「Blueprints」→「New Blueprint Instance」）→ このリポジトリを選ぶ（リポジトリの `render.yaml` が読み込まれます）
   - GitHub の許可で、Render が読めるリポジトリを選べるときは、このリポジトリだけにします
   - 「How did you hear about Render?」などのアンケートは、何を選んでも設定には影響しません
3. 入力を求められる値を入れる

   | 項目 | 入れる値 |
   |---|---|
   | `ConnectionStrings__DefaultConnection` | 1 でメモした接続文字列（`Host=...;SSL Mode=Require`） |
   | `Cors__AllowedOrigins__0` | 2 でメモした画面のアドレス（例: `https://kazoku-keisan.pages.dev`。最後の `/` は付けない） |
   | `Registration__InviteCode` | 0 で決めた招待コード |

   ログイン用の秘密鍵（`Jwt__SecretKey`）は自動で作られます。
4. 作成（Apply / Deploy）すると、ビルドと起動が始まります（初回は5〜10分ほど。「Building」→「Deploying」→「Live」の順に変わります）
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
| Render の Logs に `libgssapi_krb5.so.2` が無いと出る | 問題ありません（Kerberos という別のログイン方式の部品。Neon はパスワードでつなぐので使いません） |
| Render の Logs に `__EFMigrationsHistory` のエラーが1回だけ出る | 初回の起動では問題ありません（表がまだ無いことを確かめているだけで、そのあと作られます） |
| Render の Logs に `DataProtection-Keys` が保存されないと警告が出る | 今は問題ありません。API が起き直すと、それより前のパスワード再設定のリンクが使えなくなるだけです（今はメールを送っていません） |
| Cloudflare の画面がこの手順とちがう | 画面の作りはよく変わります。検索窓で「Pages」と探すか、`https://dash.cloudflare.com/?to=/:account/workers-and-pages` を開く |

## 一般公開（第2段階）の前にやること

- プライバシーポリシー・利用規約を用意し、画面から見られるようにする
- 保護者の退会（データの削除）ができるようにする
- DB のバックアップの方法を決める
- パスワード再設定のメールを送れるようにする（今は送らない設定）
- Render の `Registration__InviteCode` を消すと、誰でも登録できるようになる
