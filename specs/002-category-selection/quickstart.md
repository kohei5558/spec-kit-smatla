# Quickstart: 問題カテゴリ選択機能

**Feature**: 002-category-selection | **Date**: 2026-01-18  
**Purpose**: 開発者が機能を理解し、素早く実装・テストできるようにするガイド

## Overview

この機能は、ホーム画面で計算カテゴリ（足し算、引き算、掛け算、割り算、すべて）を選択し、選択したカテゴリの問題のみを連続して解けるようにします。

**実装の範囲**:

- ✅ Backend: API 拡張（カテゴリフィルタ追加）
- ✅ Frontend: カテゴリ選択 UI、状態管理、カテゴリ表示
- ✅ Testing: 単体テスト、統合テスト、E2E テスト

---

## Prerequisites

開発環境がセットアップ済みであることを確認:

```bash
# .NET SDK 8.0 以上
dotnet --version  # 8.0.x

# Node.js（Blazor WASM のビルドに必要）
node --version  # 18.x 以上

# リポジトリクローン済み
cd /path/to/spec-kit-smatla
git checkout 002-category-selection
```

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────────┐
│  Frontend (Blazor WebAssembly)                               │
├──────────────────────────────────────────────────────────────┤
│  Home.razor                                                  │
│    ├─ CategorySelector.razor  (新規)                         │
│    │   └─ カード形式のUI（MudCard + アイコン）               │
│    │                                                          │
│    └─ CategoryStateService     (新規)                         │
│        ├─ GetCategory()                                       │
│        ├─ SetCategory(type)                                   │
│        └─ sessionStorage 管理                                 │
│                                                              │
│  ProblemPage.razor                                           │
│    ├─ カテゴリ名+アイコン表示  (修正)                        │
│    └─ ProblemApiClient.GetNextProblem(studentId, category)   │
│                                                              │
│  ResultsPage.razor                                           │
│    └─ カテゴリ別統計表示       (修正)                        │
└──────────────────────────────────────────────────────────────┘
                          ↓ HTTP
┌──────────────────────────────────────────────────────────────┐
│  Backend (ASP.NET Core)                                      │
├──────────────────────────────────────────────────────────────┤
│  ProblemsController                                          │
│    └─ GetNext(studentId, category?, excludeRecentIds?)      │
│                          ↓                                    │
│  IProblemService                                             │
│    └─ GetNextProblemAsync(studentId, category?, excludeIds?) │
│                          ↓                                    │
│  ProblemRepository                                           │
│    └─ FindByCategoryAsync(category, excludeIds)             │
│                          ↓                                    │
│  ApplicationDbContext                                        │
│    └─ Problems.Where(p => p.CalculationType == category)    │
│                          ↓                                    │
│  SQLite / PostgreSQL                                         │
│    └─ SELECT * FROM Problems WHERE CalculationType = ?      │
│        (インデックス使用: IX_Problems_CalculationType)      │
└──────────────────────────────────────────────────────────────┘
```

---

## Key Files to Modify/Create

### Backend

| File                                                                             | Action | Description                                      |
| -------------------------------------------------------------------------------- | ------ | ------------------------------------------------ |
| `backend/src/GamifiedMathDrill.Core/Interfaces/IProblemService.cs`               | 修正   | `GetNextProblemAsync` に category パラメータ追加 |
| `backend/src/GamifiedMathDrill.Core/Services/ProblemService.cs`                  | 修正   | カテゴリフィルタリングロジック実装               |
| `backend/src/GamifiedMathDrill.Infrastructure/Repositories/ProblemRepository.cs` | 修正   | カテゴリ検索メソッド追加                         |
| `backend/src/GamifiedMathDrill.Api/Controllers/ProblemsController.cs`            | 修正   | category クエリパラメータ追加                    |

### Frontend

| File                                                                  | Action | Description                           |
| --------------------------------------------------------------------- | ------ | ------------------------------------- |
| `frontend/GamifiedMathDrill.Client/Services/CategoryStateService.cs`  | 新規   | sessionStorage 管理サービス           |
| `frontend/GamifiedMathDrill.Client/Components/CategorySelector.razor` | 新規   | カテゴリ選択 UI コンポーネント        |
| `frontend/GamifiedMathDrill.Client/Pages/Home.razor`                  | 修正   | CategorySelector 追加、バリデーション |
| `frontend/GamifiedMathDrill.Client/Pages/ProblemPage.razor`           | 修正   | カテゴリ表示追加                      |
| `frontend/GamifiedMathDrill.Client/Pages/ResultsPage.razor`           | 修正   | カテゴリ別統計表示                    |
| `frontend/GamifiedMathDrill.Client/Services/ProblemApiClient.cs`      | 修正   | category パラメータ追加               |

### Tests

| File                                                                          | Action | Description |
| ----------------------------------------------------------------------------- | ------ | ----------- |
| `backend/tests/GamifiedMathDrill.Tests.Unit/CategorySelectionTests.cs`        | 新規   | 単体テスト  |
| `backend/tests/GamifiedMathDrill.Tests.Integration/CategoryFilteringTests.cs` | 新規   | 統合テスト  |
| `frontend/tests/E2E/CategorySelectionFlow.cs`                                 | 新規   | E2E テスト  |

---

## Implementation Steps

### Step 1: Backend - Service Layer

#### 1.1 Update Interface

`backend/src/GamifiedMathDrill.Core/Interfaces/IProblemService.cs`

```csharp
public interface IProblemService
{
    // 修正: categoryパラメータ追加（デフォルトnull）
    Task<Problem?> GetNextProblemAsync(
        int studentId,
        CalculationType? category = null,  // 追加
        List<int>? excludeRecentIds = null);

    Task<(bool IsCorrect, int PointsEarned, bool LeveledUp, Level? NewLevel)>
        SubmitAnswerAsync(int studentId, int problemId, int answer);
}
```

#### 1.2 Update Service Implementation

`backend/src/GamifiedMathDrill.Core/Services/ProblemService.cs`

```csharp
public async Task<Problem?> GetNextProblemAsync(
    int studentId,
    CalculationType? category = null,  // 追加
    List<int>? excludeRecentIds = null)
{
    // 児童の存在確認
    var student = await _studentRepository.GetByIdAsync(studentId);
    if (student == null)
    {
        throw new KeyNotFoundException($"Student with ID {studentId} not found.");
    }

    // 問題を取得（カテゴリフィルタ適用）
    var problems = await _problemRepository.FindAvailableAsync(
        category,  // 追加: categoryを渡す
        excludeRecentIds ?? new List<int>());

    if (!problems.Any())
    {
        return null;
    }

    // ランダムに1問選択
    var random = new Random();
    return problems[random.Next(problems.Count)];
}
```

#### 1.3 Update Repository

`backend/src/GamifiedMathDrill.Infrastructure/Repositories/ProblemRepository.cs`

```csharp
public async Task<List<Problem>> FindAvailableAsync(
    CalculationType? category = null,  // 追加
    List<int> excludeIds = null)
{
    var query = _context.Problems.AsQueryable();

    // カテゴリフィルタ（追加）
    if (category.HasValue)
    {
        query = query.Where(p => p.CalculationType == category.Value);
    }

    // 除外IDフィルタ（既存）
    if (excludeIds != null && excludeIds.Any())
    {
        query = query.Where(p => !excludeIds.Contains(p.Id));
    }

    return await query.ToListAsync();
}
```

---

### Step 2: Backend - API Layer

`backend/src/GamifiedMathDrill.Api/Controllers/ProblemsController.cs`

```csharp
[HttpGet("next")]
public async Task<ActionResult<ApiResponse<ProblemDto>>> GetNext(
    [FromQuery] int studentId,
    [FromQuery] CalculationType? category = null,  // 追加
    [FromQuery] List<int>? excludeRecentIds = null)
{
    if (studentId <= 0)
    {
        return BadRequest(ApiResponse<ProblemDto>.ErrorResponse(
            "Valid student ID is required."));
    }

    try
    {
        var problem = await _problemService.GetNextProblemAsync(
            studentId, category, excludeRecentIds);  // categoryを渡す

        if (problem == null)
        {
            return NotFound(ApiResponse<ProblemDto>.ErrorResponse(
                "No available problems found."));
        }

        var problemDto = new ProblemDto
        {
            Id = problem.Id,
            Question = problem.Question,
            DifficultyLevel = problem.DifficultyLevel,
            CalculationTypeText = problem.CalculationType.ToString()
        };

        return Ok(ApiResponse<ProblemDto>.SuccessResponse(problemDto));
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(ApiResponse<ProblemDto>.ErrorResponse(ex.Message));
    }
}
```

---

### Step 3: Frontend - State Management

#### 3.1 Create CategoryStateService

`frontend/GamifiedMathDrill.Client/Services/CategoryStateService.cs`

```csharp
using GamifiedMathDrill.Client.Models;
using System.Text.Json;

namespace GamifiedMathDrill.Client.Services;

public class CategoryStateService
{
    private const string StorageKey = "category-selection";

    public CalculationType? GetCategory()
    {
        var json = SessionStorage.GetItem(StorageKey);
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        var state = JsonSerializer.Deserialize<CategoryState>(json);
        return state?.SelectedCategory;
    }

    public void SetCategory(CalculationType? category)
    {
        var state = new CategoryState
        {
            SelectedCategory = category,
            LastUpdated = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(state);
        SessionStorage.SetItem(StorageKey, json);
    }

    public void Clear()
    {
        SessionStorage.RemoveItem(StorageKey);
    }

    private class CategoryState
    {
        public CalculationType? SelectedCategory { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
```

**Note**: SessionStorage のラッパーは既存のものを使用、または Blazor の IJSRuntime で実装

---

### Step 4: Frontend - Category Selector UI

`frontend/GamifiedMathDrill.Client/Components/CategorySelector.razor`

```razor
@using GamifiedMathDrill.Client.Models

<MudGrid Spacing="3">
    <MudItem xs="12" sm="6" md="4">
        <MudCard Class="category-card @GetSelectedClass(CalculationType.Addition)"
                 @onclick="() => OnCategorySelected(CalculationType.Addition)">
            <MudCardContent Class="text-center">
                <MudIcon Icon="@Icons.Material.Filled.Add" Size="Size.Large" Color="Color.Success" />
                <MudText Typo="Typo.h5" Class="mt-2">たしざん</MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>

    <MudItem xs="12" sm="6" md="4">
        <MudCard Class="category-card @GetSelectedClass(CalculationType.Subtraction)"
                 @onclick="() => OnCategorySelected(CalculationType.Subtraction)">
            <MudCardContent Class="text-center">
                <MudIcon Icon="@Icons.Material.Filled.Remove" Size="Size.Large" Color="Color.Info" />
                <MudText Typo="Typo.h5" Class="mt-2">ひきざん</MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>

    <MudItem xs="12" sm="6" md="4">
        <MudCard Class="category-card @GetSelectedClass(CalculationType.Multiplication)"
                 @onclick="() => OnCategorySelected(CalculationType.Multiplication)">
            <MudCardContent Class="text-center">
                <MudIcon Icon="@Icons.Material.Filled.Close" Size="Size.Large" Color="Color.Warning" />
                <MudText Typo="Typo.h5" Class="mt-2">かけざん</MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>

    <MudItem xs="12" sm="6" md="4">
        <MudCard Class="category-card @GetSelectedClass(CalculationType.Division)"
                 @onclick="() => OnCategorySelected(CalculationType.Division)">
            <MudCardContent Class="text-center">
                <MudIcon Icon="@Icons.Material.Filled.MoreHoriz" Size="Size.Large" Color="Color.Error" />
                <MudText Typo="Typo.h5" Class="mt-2">わりざん</MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>

    <MudItem xs="12" sm="6" md="4">
        <MudCard Class="category-card @GetSelectedClass(null)"
                 @onclick="() => OnCategorySelected(null)">
            <MudCardContent Class="text-center">
                <MudIcon Icon="@Icons.Material.Filled.AllInclusive" Size="Size.Large" Color="Color.Primary" />
                <MudText Typo="Typo.h5" Class="mt-2">すべて</MudText>
            </MudCardContent>
        </MudCard>
    </MudItem>
</MudGrid>

@code {
    [Parameter]
    public CalculationType? SelectedCategory { get; set; }

    [Parameter]
    public EventCallback<CalculationType?> OnCategoryChanged { get; set; }

    private async Task OnCategorySelected(CalculationType? category)
    {
        SelectedCategory = category;
        await OnCategoryChanged.InvokeAsync(category);
    }

    private string GetSelectedClass(CalculationType? category)
    {
        return SelectedCategory == category ? "selected" : "";
    }
}
```

**CSS** (`CategorySelector.razor.css`):

```css
.category-card {
  cursor: pointer;
  transition: all 0.3s ease;
}

.category-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.2);
}

.category-card.selected {
  border: 3px solid var(--mud-palette-primary);
  background-color: var(--mud-palette-primary-lighten);
}
```

---

### Step 5: Frontend - Home Page Integration

`frontend/GamifiedMathDrill.Client/Pages/Home.razor` (部分的な修正)

```razor
@page "/"
@inject CategoryStateService CategoryState
@inject NavigationManager Navigation
@inject ISnackbar Snackbar

<!-- 既存のコンテンツ -->

<MudText Typo="Typo.h6" Class="mb-3">カテゴリを選んでね！</MudText>

<CategorySelector
    SelectedCategory="@selectedCategory"
    OnCategoryChanged="@OnCategoryChanged" />

<MudButton Variant="Variant.Filled"
           Color="Color.Primary"
           Size="Size.Large"
           Class="mt-4"
           OnClick="StartProblem">
    もんだいをとく
</MudButton>

@code {
    private CalculationType? selectedCategory;

    protected override void OnInitialized()
    {
        // sessionStorageから復元
        selectedCategory = CategoryState.GetCategory();
    }

    private void OnCategoryChanged(CalculationType? category)
    {
        selectedCategory = category;
        CategoryState.SetCategory(category);
    }

    private void StartProblem()
    {
        if (selectedCategory == null)
        {
            Snackbar.Add("カテゴリを選んでね！", Severity.Warning);
            return;
        }

        Navigation.NavigateTo("/problem");
    }
}
```

---

## Testing

### Unit Tests

`backend/tests/GamifiedMathDrill.Tests.Unit/CategorySelectionTests.cs`

```csharp
[Fact]
public async Task GetNextProblemAsync_WithCategory_ReturnsFilteredProblem()
{
    // Arrange
    var mockRepo = new Mock<IProblemRepository>();
    mockRepo.Setup(r => r.FindAvailableAsync(CalculationType.Addition, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Problem>
            {
                new Problem { Id = 1, CalculationType = CalculationType.Addition }
            });

    var service = new ProblemService(mockRepo.Object, ...);

    // Act
    var result = await service.GetNextProblemAsync(studentId: 1, category: CalculationType.Addition);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(CalculationType.Addition, result.CalculationType);
}
```

### Integration Tests

`backend/tests/GamifiedMathDrill.Tests.Integration/CategoryFilteringTests.cs`

```csharp
[Fact]
public async Task GetNextProblem_WithCategoryParameter_ReturnsOnlyThatCategory()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/problems/next?studentId=1&category=Addition");

    // Assert
    response.EnsureSuccessStatusCode();
    var json = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<ApiResponse<ProblemDto>>(json);

    Assert.Equal("Addition", result.Data.CalculationTypeText);
}
```

---

## Running the Application

### Backend

```bash
cd backend/src/GamifiedMathDrill.Api
dotnet run
```

API が起動: `https://localhost:7001`

### Frontend

```bash
cd frontend/GamifiedMathDrill.Client
dotnet run
```

アプリが起動: `https://localhost:7002`

---

## Manual Testing Checklist

- [ ] ホーム画面でカテゴリ選択 UI が表示される
- [ ] カテゴリをクリックすると選択状態が変わる（ハイライト）
- [ ] カテゴリ未選択で「もんだいをとく」を押すと警告メッセージが表示される
- [ ] カテゴリ選択後、問題画面に遷移できる
- [ ] 問題画面でカテゴリ名とアイコンが表示される
- [ ] 選択したカテゴリの問題のみが出題される（5 問連続で確認）
- [ ] 「すべて」選択時、複数の異なるカテゴリの問題が出題される
- [ ] ページ再読み込み後も選択状態が保持される
- [ ] ホーム画面に戻ると再選択できる
- [ ] 結果画面でカテゴリ別統計が表示される

---

## Troubleshooting

### Issue: sessionStorage が動作しない

**Solution**: IJSRuntime を使用して sessionStorage にアクセス:

```csharp
await JSRuntime.InvokeVoidAsync("sessionStorage.setItem", key, value);
```

### Issue: カテゴリフィルタが効かない

**Check**:

1. API パラメータが正しく渡されているか（ブラウザの開発者ツールでネットワークタブを確認）
2. バックエンドの Repository メソッドでカテゴリフィルタが適用されているか

---

## Next Steps

実装完了後:

1. `/speckit.tasks` コマンドを実行してタスク分解
2. 各タスクを実装
3. テストを実行して成功基準（SC-001〜SC-006）を満たすことを確認
