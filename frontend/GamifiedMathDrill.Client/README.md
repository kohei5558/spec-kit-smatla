# Gamified Math Drill - Frontend

Blazor WebAssemblyで実装されたゲーミフィケーション要素を持つ算数ドリルアプリケーションのフロントエンド。

## 主な機能

### 1. 認証とアカウント管理

- **保護者ログイン**: メールアドレスとパスワードによる認証
- **子供ログイン**: 表示名とPINによる簡単ログイン
- **子供アカウント管理**: 保護者による子供アカウントの作成・編集・削除

### 2. 問題演習

- **カテゴリ選択**: 足し算、引き算、掛け算、割り算から選択
- **リアルタイム採点**: 即座にフィードバックとポイント付与
- **学習履歴**: 過去の問題と正解率の確認

### 3. ポイントとリワード

- **景品一覧**: 利用可能な景品の表示
- **交換申請**: 子供がポイントを使って景品交換を申請
- **申請管理**: 保護者による交換申請の承認/却下

### 4. ダッシュボード

- **学習統計**: カテゴリ別の正答率とポイント
- **子供の進捗管理**: 複数の子供アカウントの学習状況を一覧表示

## 技術スタック

- **Blazor WebAssembly (.NET 8.0)**: SPA フレームワーク
- **MudBlazor 7.15.0**: マテリアルデザイン コンポーネントライブラリ
- **Blazored.LocalStorage**: ブラウザローカルストレージ管理
- **Blazored.SessionStorage**: セッションストレージ管理

## プロジェクト構造

```
frontend/GamifiedMathDrill.Client/
├── Pages/                      # ページコンポーネント
│   ├── Login.razor            # 保護者ログイン
│   ├── ChildLogin.razor       # 子供ログイン
│   ├── Register.razor         # 新規登録
│   ├── ProblemPage.razor      # 問題演習
│   ├── ResultsPage.razor      # 結果表示
│   ├── RewardsPage.razor      # 景品一覧
│   ├── RewardManagement.razor # 景品管理（保護者）
│   ├── ParentDashboard.razor  # ダッシュボード（保護者）
│   └── ChildAccountManagement.razor # 子供アカウント管理
├── Components/                 # 再利用可能なコンポーネント
├── Services/                   # APIクライアントとサービス
│   ├── AuthService.cs         # 認証サービス
│   ├── TokenService.cs        # JWTトークン管理
│   ├── ProblemApiClient.cs    # 問題APIクライアント
│   ├── RewardApiClient.cs     # 景品APIクライアント
│   └── CategoryStateService.cs # カテゴリ状態管理
├── Models/                     # データモデル
├── Layout/                     # レイアウトコンポーネント
├── wwwroot/                    # 静的ファイル
│   ├── css/                   # スタイルシート
│   ├── avatars/               # アバター画像
│   └── index.html             # エントリーポイント
├── App.razor                   # ルートコンポーネント
├── Program.cs                  # アプリケーションエントリーポイント
└── _Imports.razor             # グローバルusing
```

## セットアップ

### 前提条件

- .NET 8.0 SDK以上
- バックエンドAPIが起動していること（デフォルト: `http://localhost:5242`）

### インストール

1. リポジトリをクローン

```bash
git clone <repository-url>
cd frontend/GamifiedMathDrill.Client
```

2. 依存パッケージの復元

```bash
dotnet restore
```

3. アプリケーションの起動

```bash
dotnet run
```

アプリケーションは `https://localhost:7083` で起動します。

### 設定のカスタマイズ

バックエンドAPIのベースURLを変更する場合は、[Program.cs](Program.cs#L20)を編集：

```csharp
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://your-api-url.com")
});
```

## デバッグ

### ブラウザでのデバッグ

**Chrome/Edgeでのデバッグ:**

1. アプリケーションを起動

```bash
dotnet run
```

2. ブラウザで開発者ツールを開く（F12キー）
3. Sourcesタブで.NETアセンブリを確認
4. ブレークポイントを設定して実行

**ブラウザのコンソール:**

- `Console`タブでJavaScript/C#のログを確認
- `Network`タブでAPIリクエスト/レスポンスを監視

### VS Codeでのデバッグ

1. **デバッグ構成**

`.vscode/launch.json` を作成または確認：

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Launch and Debug Blazor WebAssembly",
      "type": "blazorwasm",
      "request": "launch",
      "cwd": "${workspaceFolder}/frontend/GamifiedMathDrill.Client",
      "browser": "chrome"
    }
  ]
}
```

2. **デバッグ開始**
   - F5キーを押すか、「実行とデバッグ」パネルから起動
   - VS Code内でブレークポイントを設定
   - コード実行時にブレークポイントで停止

### Visual Studioでのデバッグ

1. `.csproj` ファイルをVisual Studioで開く
2. F5キーでデバッグ開始
3. Visual Studio内でブレークポイントを設定
4. ブラウザが自動的に起動してデバッグセッションが開始

### ブラウザリロードを無効にしてデバッグ

Shift + F5（強制リロード）を使用すると、ブレークポイントが維持されます。

### ログの使用

**Consoleへのログ出力:**

```csharp
@inject ILogger<MyComponent> Logger

Logger.LogInformation("This is a log message");
Logger.LogError("This is an error message");
```

**ブラウザコンソールでの確認:**

開発者ツール > Consoleタブでログメッセージを確認

### ローカルストレージとセッションストレージのデバッグ

**ブラウザ開発者ツールで確認:**

1. 開発者ツールを開く（F12）
2. Applicationタブ（Chrome）またはStorageタブ（Firefox）
3. Local Storage / Session Storageを選択
4. 保存されているキーと値を確認

**コードでの確認:**

```csharp
@inject ILocalStorageService LocalStorage
@inject ISessionStorageService SessionStorage

// 値の取得
var token = await LocalStorage.GetItemAsync<string>("authToken");
var category = await SessionStorage.GetItemAsync<string>("selectedCategory");

// 値のクリア
await LocalStorage.RemoveItemAsync("authToken");
await SessionStorage.ClearAsync();
```

### APIリクエストのデバッグ

**ネットワークトラフィックの監視:**

1. ブラウザ開発者ツール > Networkタブ
2. XHR/Fetchフィルターを選択
3. APIリクエストをクリックして詳細を確認
   - Headers: リクエストヘッダー（Authorization トークンなど）
   - Payload: リクエストボディ
   - Response: レスポンスデータ
   - Preview: フォーマットされたレスポンス

**HttpClientのログ:**

`Program.cs`でHttpClientのログを有効化：

```csharp
builder.Services.AddScoped(sp =>
{
    var client = new HttpClient { BaseAddress = new Uri("http://localhost:5242") };
    client.DefaultRequestHeaders.Add("User-Agent", "GamifiedMathDrill.Client");
    return client;
});
```

### よくあるデバッグシナリオ

**問題: 認証トークンが無効**

```bash
# ブラウザコンソールで確認
localStorage.getItem('authToken')

# LocalStorageをクリア
localStorage.clear()
```

再度ログインしてトークンを取得。

**問題: APIリクエストが失敗する（CORS エラー）**

ブラウザコンソールで以下のようなエラーが表示される場合：

```
Access to fetch at 'http://localhost:5242/api/...' from origin 'https://localhost:7083' has been blocked by CORS policy
```

バックエンドの[Program.cs](../backend/src/GamifiedMathDrill.Api/Program.cs)でCORS設定を確認：

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        builder => builder
            .WithOrigins("https://localhost:7083")
            .AllowAnyMethod()
            .AllowAnyHeader());
});

app.UseCors("AllowBlazorClient");
```

**問題: コンポーネントが再レンダリングされない**

`StateHasChanged()` を明示的に呼び出す：

```csharp
private async Task LoadData()
{
    data = await ApiClient.GetDataAsync();
    StateHasChanged(); // コンポーネントの再レンダリングを強制
}
```

**問題: NullReferenceException**

- `@if (data != null)` で null チェックを追加
- `OnInitializedAsync` でデータが正しく読み込まれているか確認
- ブレークポイントを設定して変数の値を確認

**問題: ルーティングが機能しない**

- [App.razor](App.razor)の `Router` コンポーネントを確認
- `@page` ディレクティブが正しく設定されているか確認
- ナビゲーションに `NavigationManager` を使用：

```csharp
@inject NavigationManager Navigation

Navigation.NavigateTo("/login");
```

### ホットリロード

.NET 8では、コード変更時に自動的にブラウザがリロードされます：

```bash
# ホットリロード有効で起動
dotnet watch run
```

コードを編集すると、ブラウザが自動的に更新されます。

### パフォーマンスのデバッグ

**ブラウザのパフォーマンスツール:**

1. 開発者ツール > Performanceタブ
2. 記録開始（●ボタン）
3. アプリケーションを操作
4. 記録停止
5. フレームレート、メモリ使用量、レンダリング時間を分析

**Blazorの診断:**

```csharp
// コンポーネントのレンダリング回数をカウント
private int renderCount = 0;

protected override void OnAfterRender(bool firstRender)
{
    renderCount++;
    Console.WriteLine($"Component rendered {renderCount} times");
}
```

### MudBlazorコンポーネントのデバッグ

MudBlazorコンポーネントのプロパティとイベントを確認：

```razor
<MudButton OnClick="@HandleClick"
           Color="Color.Primary"
           Variant="Variant.Filled"
           @onclick:stopPropagation="true">
    Click Me
</MudButton>

@code {
    private void HandleClick()
    {
        Console.WriteLine("Button clicked!");
    }
}
```

## ビルドと公開

### 開発ビルド

```bash
dotnet build
```

### 本番用ビルド

```bash
dotnet publish -c Release -o ./publish
```

生成されたファイルは `./publish/wwwroot` に出力されます。

### 静的ファイルのホスティング

生成された `wwwroot` フォルダを以下のサービスにデプロイ可能：

- Azure Static Web Apps
- GitHub Pages
- Netlify
- Vercel
- AWS S3 + CloudFront

## テスト

### 単体テスト

bUnitフレームワークを使用してBlazorコンポーネントをテスト：

```bash
# テストプロジェクトの作成（初回のみ）
dotnet new bunit -o ../tests/GamifiedMathDrill.Client.Tests

# テストの実行
dotnet test
```

### E2Eテスト

Playwrightを使用したエンドツーエンドテスト：

```bash
# Playwrightのインストール（初回のみ）
dotnet add package Microsoft.Playwright

# テストの実行
dotnet test
```

## トラブルシューティング

### 問題: アプリケーションが起動しない

```bash
# ポートが使用中か確認
lsof -i :7083

# プロセスを終了
kill -9 <PID>

# 別のポートで起動
dotnet run --urls "https://localhost:7084"
```

### 問題: 依存パッケージのエラー

```bash
# パッケージキャッシュをクリア
dotnet nuget locals all --clear

# 依存関係を再インストール
dotnet restore --force
```

### 問題: ビルドエラー

```bash
# クリーンビルド
dotnet clean
dotnet build
```

## 開発のベストプラクティス

1. **状態管理**: 複雑な状態は専用のServiceクラスに分離
2. **再利用性**: 共通UIはComponentsフォルダに抽出
3. **エラーハンドリング**: try-catchで適切にエラーをキャッチ
4. **ローディング表示**: 非同期処理中はローディングインジケーターを表示
5. **レスポンシブデザイン**: MudBlazorのグリッドシステムを活用

## ライセンス

MIT License
