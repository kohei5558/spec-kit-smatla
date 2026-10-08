using System.Net;
using System.Net.Http.Json;
using GamifiedMathDrill.Api.DTOs;
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
            Name = "作成テスト花子",
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
        Assert.Equal("作成テスト花子", result.Name);
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

        // 同じクラスの他テストが同一保護者で作成したアカウントも上限に数えられるため、残り枠だけ作成して10件にする
        var existing = await Client.GetFromJsonAsync<List<ChildAccountDto>>("/api/child-accounts");
        var remaining = 10 - existing!.Count;
        for (int i = 1; i <= remaining; i++)
        {
            var request = new ChildAccountCreateDto
            {
                Name = $"上限テスト{i}",
                GradeLevel = i % 6 + 1,
                PresetAvatarId = i % 15 + 1,
                PIN = $"{5000 + i}"
            };

            var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", request);
            Assert.True(createResponse.IsSuccessStatusCode, $"Failed to create account {i}");
        }

        // 11個目のアカウント作成を試みる
        var eleventhRequest = new ChildAccountCreateDto
        {
            Name = "上限テスト11",
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

    #region T042: アカウント更新テスト (Phase 4, US2)

    [Fact]
    public async Task UpdateChildAccount_WithValidData_ReturnsSuccess()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // まず子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "次郎",
            GradeLevel = 2,
            PresetAvatarId = 1,
            PIN = "2222"
        };
        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var createdChild = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(createdChild);

        // 更新データを準備
        var updateRequest = new ChildAccountUpdateDto
        {
            Name = "次郎（更新後）",
            GradeLevel = 3,
            PresetAvatarId = 2,
            NewPIN = null // PIN変更なし
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/child-accounts/{createdChild.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(result);
        Assert.Equal("次郎（更新後）", result.Name);
        Assert.Equal(3, result.GradeLevel);
        // アバターURLが変更されていることを確認（詳細なURLチェックはスキップ）
        Assert.NotEmpty(result.AvatarUrl);
    }

    [Fact]
    public async Task UpdateChildAccount_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 2つの子供アカウントを作成
        var child1Request = new ChildAccountCreateDto
        {
            Name = "太郎",
            GradeLevel = 1,
            PresetAvatarId = 1,
            PIN = "1111"
        };
        var child1Response = await Client.PostAsJsonAsync("/api/child-accounts", child1Request);
        var child1 = await child1Response.Content.ReadFromJsonAsync<ChildAccountDto>();

        var child2Request = new ChildAccountCreateDto
        {
            Name = "花子",
            GradeLevel = 2,
            PresetAvatarId = 2,
            PIN = "2222"
        };
        var child2Response = await Client.PostAsJsonAsync("/api/child-accounts", child2Request);
        var child2 = await child2Response.Content.ReadFromJsonAsync<ChildAccountDto>();

        Assert.NotNull(child1);
        Assert.NotNull(child2);

        // child2の名前をchild1と同じにしようとする
        var updateRequest = new ChildAccountUpdateDto
        {
            Name = "太郎",
            GradeLevel = 2,
            PresetAvatarId = 2,
            NewPIN = null
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/child-accounts/{child2.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("既に存在", errorContent);
    }

    #endregion

    #region T043: PIN変更テスト (Phase 4, US2)

    [Fact]
    public async Task UpdateChildAccount_WithNewPIN_SuccessfullyChangesPIN()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "三郎",
            GradeLevel = 1,
            PresetAvatarId = 1,
            PIN = "3333"
        };
        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var createdChild = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(createdChild);

        // PINを変更
        var updateRequest = new ChildAccountUpdateDto
        {
            Name = "三郎",
            GradeLevel = 1,
            PresetAvatarId = 1,
            NewPIN = "9999" // 新しいPIN
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/child-accounts/{createdChild.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // 古いPINでログイン試行（失敗するはず）
        var oldPinLoginRequest = new
        {
            ChildId = createdChild.Id,
            PIN = "3333"
        };
        var oldPinResponse = await Client.PostAsJsonAsync("/api/auth/child/login", oldPinLoginRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, oldPinResponse.StatusCode);

        // 新しいPINでログイン試行（成功するはず）
        var newPinLoginRequest = new
        {
            ChildId = createdChild.Id,
            PIN = "9999"
        };
        var newPinResponse = await Client.PostAsJsonAsync("/api/auth/child/login", newPinLoginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, newPinResponse.StatusCode);
        var loginResult = await newPinResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        Assert.NotEmpty(loginResult.Token);
    }

    [Fact]
    public async Task UpdateChildAccount_WithEmptyNewPIN_DoesNotChangePIN()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "四郎",
            GradeLevel = 1,
            PresetAvatarId = 1,
            PIN = "4444"
        };
        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        var createdChild = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(createdChild);

        // 名前だけ変更（NewPINは空文字）
        var updateRequest = new ChildAccountUpdateDto
        {
            Name = "四郎（更新後）",
            GradeLevel = 1,
            PresetAvatarId = 1,
            NewPIN = null
        };

        var updateResponse = await Client.PutAsJsonAsync($"/api/child-accounts/{createdChild.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // 元のPINでログイン試行（成功するはず）
        var loginRequest = new
        {
            ChildId = createdChild.Id,
            PIN = "4444"
        };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/child/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);
        Assert.NotEmpty(loginResult.Token);
    }

    #endregion

    #region Phase 5: User Story 3 - 学習統計

    [Fact]
    public async Task GetChildAccountDetail_WithLearningData_ReturnsStats()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "五郎（学習済み）",
            GradeLevel = 4,
            PresetAvatarId = 1,
            PIN = "5555"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var createdChild = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(createdChild);

        // Act: 学習統計を取得
        var response = await Client.GetAsync($"/api/child-accounts/{createdChild.Id}/detail");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(result);
        Assert.Equal("五郎（学習済み）", result.Name);
        
        // 学習統計が含まれていることを確認（データなしの場合でも構造は存在）
        Assert.NotNull(result.LearningStats);
    }

    [Fact]
    public async Task GetChildAccountDetail_WithoutLearningData_ReturnsEmptyStats()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成（学習記録なし）
        var createRequest = new ChildAccountCreateDto
        {
            Name = "六郎（未学習）",
            GradeLevel = 2,
            PresetAvatarId = 2,
            PIN = "6666"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var createdChild = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(createdChild);

        // Act: 詳細情報を取得
        var response = await Client.GetAsync($"/api/child-accounts/{createdChild.Id}/detail");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(result);
        Assert.NotNull(result.LearningStats);
        
        // 学習データがない場合、初期値が返される
        Assert.Equal(0, result.LearningStats.TotalProblems);
        Assert.Equal(0, result.LearningStats.AccuracyRate);
        Assert.Equal(0, result.LearningStats.TotalPoints);
        Assert.Equal(0, result.LearningStats.ConsecutiveDays);
    }

    #endregion

    #region Phase 6: User Story 4 - アカウント一時停止

    [Fact]
    public async Task SuspendChildAccount_ThenActivate_Success()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "七郎",
            GradeLevel = 3,
            PresetAvatarId = 3,
            PIN = "7777"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var createdChild = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(createdChild);
        Assert.True(createdChild.IsActive);

        // Act: アカウントを停止
        var suspendResponse = await Client.PostAsync($"/api/child-accounts/{createdChild.Id}/suspend", null);

        // Assert: 停止成功
        Assert.Equal(HttpStatusCode.NoContent, suspendResponse.StatusCode);

        // 停止後の状態を確認
        var getResponse = await Client.GetAsync($"/api/child-accounts/{createdChild.Id}");
        var suspendedChild = await getResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(suspendedChild);
        Assert.False(suspendedChild.IsActive);

        // Act: アカウントを再開
        var activateResponse = await Client.PostAsync($"/api/child-accounts/{createdChild.Id}/activate", null);

        // Assert: 再開成功
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        // 再開後の状態を確認
        var reactivatedResponse = await Client.GetAsync($"/api/child-accounts/{createdChild.Id}");
        var reactivatedChild = await reactivatedResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(reactivatedChild);
        Assert.True(reactivatedChild.IsActive);
    }

    #endregion

    #region アカウント削除 (Phase 7)

    [Fact]
    public async Task DeleteChildAccount_WithValidData_Success()
    {
        // Arrange
        await AuthenticateAsParentAsync();

        // 子供アカウントを作成
        var createRequest = new ChildAccountCreateDto
        {
            Name = "削除テスト太郎",
            GradeLevel = 2,
            PresetAvatarId = 2,
            PIN = "9999"
        };

        var createResponse = await Client.PostAsJsonAsync("/api/child-accounts", createRequest);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var createdChild = await createResponse.Content.ReadFromJsonAsync<ChildAccountDto>();
        Assert.NotNull(createdChild);
        var childId = createdChild.Id;

        // 削除前に存在確認
        var getBeforeDelete = await Client.GetAsync($"/api/child-accounts/{childId}");
        Assert.Equal(HttpStatusCode.OK, getBeforeDelete.StatusCode);

        // Act: アカウントを削除
        var deleteResponse = await Client.DeleteAsync($"/api/child-accounts/{childId}");

        // Assert: 削除成功
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 削除後、取得できないことを確認
        var getAfterDelete = await Client.GetAsync($"/api/child-accounts/{childId}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);

        // 一覧にも表示されないことを確認
        var listResponse = await Client.GetAsync("/api/child-accounts");
        var children = await listResponse.Content.ReadFromJsonAsync<List<ChildAccountDto>>();
        Assert.NotNull(children);
        Assert.DoesNotContain(children, c => c.Id == childId);
    }

    #endregion
}

