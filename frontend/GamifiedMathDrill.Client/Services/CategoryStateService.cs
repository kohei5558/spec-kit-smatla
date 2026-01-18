using Microsoft.JSInterop;
using System.Text.Json;

namespace GamifiedMathDrill.Client.Services;

/// <summary>
/// カテゴリ選択状態をsessionStorageで管理するサービス
/// </summary>
public class CategoryStateService
{
    private readonly IJSRuntime _jsRuntime;
    private const string StorageKey = "category-selection";

    public CategoryStateService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// カテゴリ選択状態
    /// </summary>
    public class CategorySelection
    {
        public string? SelectedCategory { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// 選択されたカテゴリを設定
    /// </summary>
    /// <param name="category">カテゴリ名（null=すべて）</param>
    public async Task SetSelectedCategoryAsync(string? category)
    {
        var selection = new CategorySelection
        {
            SelectedCategory = category,
            LastUpdated = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(selection);
        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", StorageKey, json);
    }

    /// <summary>
    /// 選択されたカテゴリを取得
    /// </summary>
    /// <returns>カテゴリ名（null=未選択または「すべて」）</returns>
    public async Task<string?> GetSelectedCategoryAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var selection = JsonSerializer.Deserialize<CategorySelection>(json);
            return selection?.SelectedCategory;
        }
        catch
        {
            // デシリアライズエラーの場合はnullを返す
            return null;
        }
    }

    /// <summary>
    /// カテゴリ選択状態をクリア
    /// </summary>
    public async Task ClearSelectedCategoryAsync()
    {
        await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
    }

    /// <summary>
    /// カテゴリが選択されているかチェック
    /// </summary>
    /// <returns>true=選択済み、false=未選択</returns>
    public async Task<bool> HasSelectedCategoryAsync()
    {
        var category = await GetSelectedCategoryAsync();
        // nullの場合は「すべて」が選択されているとみなす（明示的な選択がない場合は未選択）
        // ただし、sessionStorageに値が存在する場合は選択済みとする
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
            return !string.IsNullOrEmpty(json);
        }
        catch
        {
            return false;
        }
    }
}
