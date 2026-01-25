# Research: 保護者管理画面と実物景品交換システム

**Feature**: 003-parent-admin-rewards | **Date**: 2026-01-25  
**Purpose**: Phase 0 - 技術的な不明点を解決し、実装の方向性を確立する

## Research Topics

### 1. 認証方式の選択

**Question**: ASP.NET Core で保護者（パスワード認証）と子供（簡易認証）を実装する最適な方法は？

**Research**:

- **ASP.NET Core Identity**: フル機能の認証・認可フレームワーク。ユーザー管理、パスワードハッシュ、ロール管理が組み込み
- **JWT トークン**: ステートレス認証。Blazor WebAssembly と相性が良い
- **Cookie 認証**: サーバー側セッション管理。SPA では扱いづらい
- **カスタム認証**: 子供用の簡易認証（PIN）には部分的にカスタム実装が必要

**Decision**: **ASP.NET Core Identity + JWT トークン認証**

**Rationale**:

- ASP.NET Core Identity で保護者アカウント（メール、パスワード）を管理
- JWT トークンで Blazor WebAssembly とステートレス認証を実現
- 子供アカウントは Identity の User として作成し、PIN は CustomProperty に格納
- ロールベースアクセス制御（RBAC）を使用して Parent/Child を区別

**Implementation Note**:

```csharp
public class ApplicationUser : IdentityUser
{
    public UserRole Role { get; set; }
    public string? PIN { get; set; }  // 子供用（4桁、ハッシュ化）
    public int? ParentId { get; set; }  // 子供の場合、保護者への参照
    public ApplicationUser? Parent { get; set; }
}

public enum UserRole
{
    Parent,
    Child
}
```

**Alternatives Considered**:

- Cookie認証: Blazor WebAssembly では複雑（CORS、SameSite 問題）
- カスタム認証のみ: パスワードハッシュ、トークン管理を自前実装する必要がある

---

### 2. 画像アップロードの実装方法

**Question**: 保護者が景品画像をアップロードする際、どのストレージを使用するか？

**Research**:

- **ローカルファイルシステム（wwwroot）**: 開発・テストが容易。スケールしにくい
- **Azure Blob Storage**: クラウドストレージ。スケーラブルだが追加コストと設定が必要
- **データベース（BLOB列）**: 小さい画像なら可能だが、パフォーマンス問題の可能性

**Decision**: **ローカルファイルシステム（`wwwroot/uploads/rewards/`）**

**Rationale**:

- MVP では1家庭での利用が前提（大量画像アップロードは想定外）
- 追加のクラウドサービス不要でコスト削減
- 開発・デバッグが容易
- 将来的に Azure Blob Storage に移行可能（IFileStorageService インターフェースで抽象化）

**Implementation Note**:

```csharp
public interface IFileStorageService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName);
    Task DeleteImageAsync(string filePath);
    string GetImageUrl(string filePath);
}

// ローカル実装
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private const string UploadPath = "uploads/rewards";

    public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
    {
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var uploadPath = Path.Combine(_env.WebRootPath, UploadPath);
        Directory.CreateDirectory(uploadPath);

        var filePath = Path.Combine(uploadPath, uniqueFileName);
        using var fileStreamOut = File.Create(filePath);
        await fileStream.CopyToAsync(fileStreamOut);

        return $"/{UploadPath}/{uniqueFileName}";
    }
}
```

**Alternatives Considered**:

- Azure Blob Storage: コスト、設定複雑、MVP には過剰
- データベース: 5MB 画像を DB に保存するとパフォーマンス低下

---

### 3. 在庫管理と同時更新の競合解決

**Question**: 複数の子供が同時に同じ景品を申請した場合、在庫マイナスを防ぐ方法は？

**Research**:

- **楽観的ロック（RowVersion）**: Entity Framework Core の ConcurrencyToken。更新時にバージョンチェック
- **悲観的ロック（トランザクション）**: データベースレベルでロック（`SELECT FOR UPDATE`）
- **アプリケーションレベルロック**: C# の lock や SemaphoreSlim

**Decision**: **楽観的ロック（RowVersion）+ トランザクション**

**Rationale**:

- 楽観的ロックで同時更新を検出
- 在庫デクリメント処理をトランザクション内で実行
- 競合発生時はリトライまたはユーザーにエラー通知
- データベースレベルでの整合性保証

**Implementation Note**:

```csharp
public class Reward
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int RequiredPoints { get; set; }
    public int? Stock { get; set; }  // null = 無制限

    [Timestamp]  // RowVersion for optimistic concurrency
    public byte[] RowVersion { get; set; }
}

// 申請処理
public async Task<(bool Success, string Message)> CreateExchangeRequestAsync(int studentId, int rewardId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        var reward = await _context.Rewards.FindAsync(rewardId);

        // 在庫チェック
        if (reward.Stock.HasValue && reward.Stock <= 0)
            return (false, "在庫切れです");

        // 在庫デクリメント（RowVersion による楽観的ロック）
        if (reward.Stock.HasValue)
            reward.Stock--;

        var request = new AcquiredReward
        {
            StudentId = studentId,
            RewardId = rewardId,
            Status = ExchangeStatus.Pending
        };

        _context.AcquiredRewards.Add(request);
        await _context.SaveChangesAsync();  // DbUpdateConcurrencyException の可能性

        await transaction.CommitAsync();
        return (true, "申請が完了しました");
    }
    catch (DbUpdateConcurrencyException)
    {
        await transaction.RollbackAsync();
        return (false, "在庫が更新されました。再度お試しください");
    }
}
```

**Alternatives Considered**:

- 悲観的ロック: デッドロックリスク、パフォーマンス低下
- アプリケーションロック: 複数サーバー環境で機能しない

---

### 4. 申請・承認フローのステータス管理

**Question**: 交換申請のステータス遷移をどう設計するか？

**Research**:

既存の AcquiredReward モデルを確認：

```csharp
public class AcquiredReward
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int RewardId { get; set; }
    public DateTime AcquiredAt { get; set; }
}
```

**Decision**: **ExchangeStatus enum を追加し、状態遷移を厳密に管理**

**Rationale**:

- Pending（申請中）→ Approved（承認）/ Rejected（却下）/ Cancelled（キャンセル）
- 状態遷移を明示的にし、不正な遷移を防ぐ
- 承認日時、却下理由などのメタデータを追加

**Implementation Note**:

```csharp
public enum ExchangeStatus
{
    Pending = 0,      // 申請中（初期状態）
    Approved = 1,     // 承認済み（保護者が実物を渡した）
    Rejected = 2,     // 却下（保護者が拒否）
    Cancelled = 3     // キャンセル（子供が取り消し）
}

public class AcquiredReward
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int RewardId { get; set; }

    public ExchangeStatus Status { get; set; } = ExchangeStatus.Pending;
    public DateTime RequestedAt { get; set; }     // 申請日時
    public DateTime? ApprovedAt { get; set; }     // 承認日時
    public DateTime? RejectedAt { get; set; }     // 却下日時
    public string? RejectionReason { get; set; }  // 却下理由
    public int? ApprovedBy { get; set; }          // 承認した保護者ID
}

// 状態遷移ロジック
public async Task<(bool Success, string Message)> ApproveRequestAsync(int requestId, int parentId)
{
    var request = await _context.AcquiredRewards
        .Include(r => r.Reward)
        .Include(r => r.Student)
        .FirstOrDefaultAsync(r => r.Id == requestId);

    if (request.Status != ExchangeStatus.Pending)
        return (false, "この申請は既に処理済みです");

    // ポイント再チェック
    if (request.Student.CurrentPoints < request.Reward.RequiredPoints)
        return (false, "ポイントが不足しています");

    using var transaction = await _context.Database.BeginTransactionAsync();

    // ポイント消費
    request.Student.CurrentPoints -= request.Reward.RequiredPoints;

    // ステータス更新
    request.Status = ExchangeStatus.Approved;
    request.ApprovedAt = DateTime.UtcNow;
    request.ApprovedBy = parentId;

    await _context.SaveChangesAsync();
    await transaction.CommitAsync();

    return (true, "承認が完了しました");
}
```

**Alternatives Considered**:

- bool フラグのみ: 却下とキャンセルを区別できない
- 文字列ステータス: 型安全性がない

---

### 5. Blazor WebAssembly での画像アップロード

**Question**: Blazor WebAssembly から画像ファイルを API にアップロードする方法は？

**Research**:

- **IBrowserFile**: Blazor の InputFile コンポーネントで取得
- **MultipartFormDataContent**: HTTP POST で画像を送信
- **StreamContent**: ファイルストリームを API に送信

**Decision**: **IBrowserFile + MultipartFormDataContent**

**Rationale**:

- Blazor の標準機能で実装可能
- ファイルサイズ制限（5MB）をクライアント側でチェック可能
- プレビュー機能も実装しやすい

**Implementation Note**:

フロントエンド：

```csharp
@page "/admin/rewards/create"
@inject HttpClient Http

<InputFile OnChange="HandleFileSelection" accept="image/*" />
<img src="@imagePreview" style="max-width:300px" />

@code {
    private IBrowserFile? selectedFile;
    private string? imagePreview;

    private async Task HandleFileSelection(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;

        if (selectedFile.Size > 5 * 1024 * 1024)
        {
            // エラー: 5MB超過
            return;
        }

        // プレビュー生成
        var format = "image/png";
        var resizedImage = await selectedFile.RequestImageFileAsync(format, 300, 300);
        var buffer = new byte[resizedImage.Size];
        await resizedImage.OpenReadStream().ReadAsync(buffer);
        imagePreview = $"data:{format};base64,{Convert.ToBase64String(buffer)}";
    }

    private async Task CreateReward()
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(name), "name");
        content.Add(new StringContent(requiredPoints.ToString()), "requiredPoints");

        if (selectedFile != null)
        {
            var fileContent = new StreamContent(selectedFile.OpenReadStream(5 * 1024 * 1024));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(selectedFile.ContentType);
            content.Add(fileContent, "image", selectedFile.Name);
        }

        var response = await Http.PostAsync("/api/rewards", content);
    }
}
```

バックエンド：

```csharp
[HttpPost]
public async Task<IActionResult> CreateReward([FromForm] CreateRewardDto dto, IFormFile? image)
{
    if (image != null)
    {
        var imagePath = await _fileStorage.UploadImageAsync(image.OpenReadStream(), image.FileName);
        dto.ImagePath = imagePath;
    }

    var reward = new Reward { ... };
    await _rewardRepository.AddAsync(reward);

    return Ok(reward);
}
```

**Alternatives Considered**:

- Base64 エンコード: 画像サイズが1.3倍に増加
- SignalR でストリーミング: 複雑すぎる

---

### 6. 保護者ダッシュボードの通知バッジ

**Question**: 未承認申請件数をリアルタイムに表示する方法は？

**Research**:

- **ポーリング**: 定期的に API を呼び出して件数取得
- **SignalR**: リアルタイム双方向通信
- **ロングポーリング**: サーバーが変更を検知するまで接続保持

**Decision**: **シンプルなポーリング（30秒間隔）**

**Rationale**:

- 申請頻度は低い（数分〜数時間に1回程度）
- SignalR は過剰（WebSocket 接続のオーバーヘッド）
- 30秒間隔のポーリングで十分な UX

**Implementation Note**:

```csharp
@page "/admin/dashboard"
@inject HttpClient Http
@implements IDisposable

<MudBadge Content="@pendingCount" Color="Color.Error" Visible="@(pendingCount > 0)">
    <MudIconButton Icon="@Icons.Material.Filled.Notifications" />
</MudBadge>

@code {
    private int pendingCount;
    private Timer? timer;

    protected override void OnInitialized()
    {
        timer = new Timer(async _ => await LoadPendingCount(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private async Task LoadPendingCount()
    {
        var response = await Http.GetFromJsonAsync<int>("/api/exchange-requests/pending/count");
        pendingCount = response;
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose() => timer?.Dispose();
}
```

**Alternatives Considered**:

- SignalR: 複雑、WebSocket 対応必須
- 手動更新のみ: UX が低下

---

## Summary of Decisions

| 技術課題         | 選択した解決策                   | 主な理由                         |
| ---------------- | -------------------------------- | -------------------------------- |
| 認証方式         | ASP.NET Core Identity + JWT      | 標準機能、ロールベース制御が容易 |
| 画像ストレージ   | ローカルファイルシステム         | MVP に十分、将来移行可能         |
| 在庫管理         | 楽観的ロック + トランザクション  | 同時更新を検出、整合性保証       |
| ステータス管理   | ExchangeStatus enum              | 型安全、状態遷移明確             |
| 画像アップロード | IBrowserFile + MultipartFormData | Blazor 標準、プレビュー可能      |
| 通知バッジ       | 30秒ポーリング                   | シンプル、申請頻度に適合         |

## Risks and Mitigations

| リスク                                 | 影響度 | 緩和策                                           |
| -------------------------------------- | ------ | ------------------------------------------------ |
| 画像ファイルサイズ超過                 | 中     | クライアント側で5MB制限、サーバー側でも再検証    |
| 同時申請による在庫マイナス             | 高     | 楽観的ロック + トランザクションで防止            |
| 大量画像アップロードでディスク容量不足 | 低     | MVP では想定外、将来は Azure Blob に移行         |
| JWT トークン漏洩                       | 中     | HTTPS 必須、短い有効期限（1時間）、Refresh Token |
| 子供が保護者パスワードを知る           | 中     | 保護者に注意喚起、セッションタイムアウト         |
