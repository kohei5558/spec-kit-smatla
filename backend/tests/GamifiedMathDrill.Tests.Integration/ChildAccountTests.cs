using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Core.DTOs;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// 子供アカウント管理機能の統合テスト (Phase 3, US1)
/// </summary>
public class ChildAccountTests : AuthenticatedTestBase
{
    public ChildAccountTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    #region T032: 作成・一覧取得テスト

    [Fact]
    public async Task CreateChildAccount_WithValidData_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        var createRequest = new ChildAccountCreateDto
        {
            Name = "花子",
            GradeLevel = 3,
            PresetAvatarId = 1,
            PIN = "5678"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Id);
        Assert.Equal("花子", result.Name);
        Assert.Equal(3, result.GradeLevel);
        Assert.True(result.IsActive);
        Assert.NotEmpty(result.AvatarUrl);
    }

    [Fact]
    public async Task GetChildAccounts_ReturnsListOfChildren()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // Act
        var response = await Client.GetAsync("/api/child-accounts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<ChildAccountDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result); // 少なくとも1つの子供アカウント（シード済み）が存在
    }

    [Fact]
    public async Task CreateChildAccount_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange - 認証なし
        var createRequest = new ChildAccountCreateDto
        {
            Name = "次郎",
            GradeLevel = 2,
            PresetAvatarId = 1,
            PIN = "4321"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region T035: バリデーションテスト

    [Fact]
    public async Task CreateChildAccount_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        var firstRequest = new ChildAccountCreateDto
        {
            Name = "重複テスト",
            GradeLevel = 1,
            PresetAvatarId = 1,
            PIN = "1111"
        };

        // 最初のアカウントを作成
        await Client.PostAsJsonAsync("/api/child-accounts", firstRequest);

        // 同じ名前で再度作成を試みる
        var duplicateRequest = new ChildAccountCreateDto
        {
            Name = "重複テスト",
            GradeLevel = 2,
            PresetAvatarId = 2,
            PIN = "2222"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/child-accounts", duplicateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("既に存在", errorContent);
    }

    [Fact]
    public async Task CreateChildAccount_WithInvalidPinFormat_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        var invalidPinRequest = new ChildAccountCreateDto
        {
            Name = "PIN無効",
            GradeLevel = 1,
            PresetAvatarId = 1,
            PIN = "123" // 3桁（4桁が必要）
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/child-accounts", invalidPinRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateChildAccount_WithNonNumericPin_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        var invalidPinRequest = new ChildAccountCreateDto
        {
            Name = "PIN英字",
            GradeLevel = 1,
            PresetAvatarId = 1,
            PIN = "abcd" // 英字（数字が必要）
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/child-accounts", invalidPinRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateChildAccount_WithInvalidGradeLevel_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        var invalidGradeRequest = new ChildAccountCreateDto
        {
            Name = "学年無効",
            GradeLevel = 7, // 7年生（1-6が有効）
            PresetAvatarId = 1,
            PIN = "1234"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/child-accounts", invalidGradeRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateChildAccount_ExceedingTenAccountLimit_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 10個のアカウントを作成
        for (int i = 1; i <= 10; i++)
        {
            var request = new ChildAccountCreateDto
            {
                Name = $"子供{i}",
                GradeLevel = i % 6 + 1,
                PresetAvatarId = i % 15 + 1,
                PIN = $"{i:D4}"
            };

            var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", request);
            if (i <= 10)
            {
                Assert.True(createResponse.IsSuccessStatusCode, $"Failed to create account {i}");
            }
        }

        // 11個目のアカウント作成を試みる
        var eleventhRequest = new ChildAccountCreateDto
        {
            Name = "子供11",
            GradeLevel = 1,
            PresetAvatarId = 1,
            PIN = "1111"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/child-accounts", eleventhRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("10件", errorContent);
    }

    #endregion
}
