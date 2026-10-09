using GamifiedMathDrill.Api.DTOs;
using GamifiedMathDrill.Core.Models;
using GamifiedMathDrill.Tests.Integration.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GamifiedMathDrill.Tests.Integration;

/// <summary>
/// エッジケーステスト（仕様書120-150行目参照）
/// </summary>
public class EdgeCaseTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public EdgeCaseTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        // 子供ログインには登録端末のトークンが必要
        AuthenticationHelper.EnsureTestUsersExistAsync(factory.Services).Wait();
        AuthenticationHelper.UseParentDeviceAsync(_client, factory).Wait();
    }

    [Fact]
    public async Task StockManagement_ConcurrentRequests_CorrectHandling()
    {
        // 認証
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);

        // Arrange: 保護者でログイン
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        // 在庫1の景品を作成
        var formData = new MultipartFormDataContent
        {
            { new StringContent("Limited Item"), "Name" },
            { new StringContent("Test Description"), "Description" },
            { new StringContent("50"), "RequiredPoints" },
            { new StringContent("Snack"), "Category" },
            { new StringContent("true"), "IsPhysical" },
            { new StringContent("1"), "Stock" }
        };
        var createRewardResponse = await _client.PostAsync("/api/rewards", formData);

        // デバッグ: レスポンスを確認
        if (!createRewardResponse.IsSuccessStatusCode)
        {
            var errorContent = await createRewardResponse.Content.ReadAsStringAsync();
            throw new Exception($"Reward creation failed: {createRewardResponse.StatusCode} - {errorContent}");
        }

        var rewardDto = await createRewardResponse.Content.ReadFromJsonAsync<RewardDto>();
        Assert.NotNull(rewardDto);

        // 子供でログイン
        var childId = AuthenticationHelper.GetTestChildId(_factory.Services);
        var childLoginResponse = await _client.PostAsJsonAsync("/api/auth/child-login", new
        {
            ChildId = childId,
            PIN = "1234"
        });
        var childLoginResult = await childLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult!.Token);

        // デバッグ: 認証状態を確認
        if (!childLoginResponse.IsSuccessStatusCode)
        {
            var loginError = await childLoginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Child login failed: {childLoginResponse.StatusCode} - {loginError}");
        }

        // ポイントを追加（50ポイント必要）
        var studentId = AuthenticationHelper.GetTestStudentId(_factory.Services);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GamifiedMathDrill.Infrastructure.Data.ApplicationDbContext>();
            var student = await db.Students.FindAsync(studentId);
            if (student != null)
            {
                student.TotalPoints = 100; // 十分なポイントを付与
                await db.SaveChangesAsync();
            }
        }

        // Act: 交換申請（成功するはず）
        var exchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });

        // デバッグ: エラー内容を確認
        if (!exchangeResponse.IsSuccessStatusCode)
        {
            var errorContent = await exchangeResponse.Content.ReadAsStringAsync();
            var statusCode = exchangeResponse.StatusCode;
            var headers = string.Join(", ", exchangeResponse.Headers.Select(h => $"{h.Key}={string.Join(";", h.Value)}"));
            throw new Exception($"Exchange request failed: {statusCode} - Content: '{errorContent}' - Headers: {headers}");
        }

        // 在庫が0になったので、2回目は失敗するはず
        var secondExchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, exchangeResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondExchangeResponse.StatusCode);
    }

    [Fact]
    public async Task PointInsufficiency_ApprovalTimeCheck()
    {
        // 認証
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);

        // Arrange: 保護者でログイン
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        // 景品を作成（100ポイント必要）
        var formData1 = new MultipartFormDataContent
        {
            { new StringContent("Expensive Item"), "Name" },
            { new StringContent("Test Description"), "Description" },
            { new StringContent("100"), "RequiredPoints" },
            { new StringContent("Toy"), "Category" },
            { new StringContent("true"), "IsPhysical" },
            { new StringContent("10"), "Stock" }
        };
        var createRewardResponse = await _client.PostAsync("/api/rewards", formData1);
        var rewardDto = await createRewardResponse.Content.ReadFromJsonAsync<RewardDto>();
        Assert.NotNull(rewardDto);

        // 子供でログイン（初期ポイント: 0）
        var childId = AuthenticationHelper.GetTestChildId(_factory.Services);
        var childLoginResponse = await _client.PostAsJsonAsync("/api/auth/child-login", new
        {
            ChildId = childId,
            PIN = "1234"
        });

        // ポイント不足時の申請は拒否されるべき
        var childLoginResult = await childLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult!.Token);

        var exchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });

        // Assert: ポイント不足でエラー
        Assert.Equal(HttpStatusCode.BadRequest, exchangeResponse.StatusCode);
        var errorContent = await exchangeResponse.Content.ReadAsStringAsync();
        Assert.Contains("ポイントが不足しています", errorContent);
    }

    [Fact]
    public async Task ImageUpload_OversizedFile_Rejected()
    {
        // 認証
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);

        // Arrange: 保護者でログイン
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        // 5MBを超える画像をシミュレート（実際は6MB）
        var oversizedContent = new byte[6 * 1024 * 1024]; // 6MB
        var fileContent = new ByteArrayContent(oversizedContent);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var formData = new MultipartFormDataContent
        {
            { new StringContent("Test Reward"), "Name" },
            { new StringContent("Test Description"), "Description" },
            { new StringContent("100"), "RequiredPoints" },
            { new StringContent("Toy"), "Category" },
            { new StringContent("true"), "IsPhysical" },
            { new StringContent("10"), "Stock" },
            { fileContent, "Image", "large.jpg" }
        };

        // Act
        var response = await _client.PostAsync("/api/rewards", formData);

        // デバッグ: レスポンスを確認
        var errorContent = await response.Content.ReadAsStringAsync();

        // Assert: ファイルサイズ超過でエラー
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("ファイルサイズが大きすぎます", errorContent);
    }

    [Fact]
    public async Task RequestCancellation_OnlyPendingStatusAllowed()
    {
        // 認証
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);

        // Arrange: 保護者でログイン
        var parentLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var parentLoginResult = await parentLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // 景品を作成
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentLoginResult!.Token);
        var formData2 = new MultipartFormDataContent
        {
            { new StringContent("Test Item"), "Name" },
            { new StringContent("Test Description"), "Description" },
            { new StringContent("10"), "RequiredPoints" },
            { new StringContent("Snack"), "Category" },
            { new StringContent("true"), "IsPhysical" },
            { new StringContent("5"), "Stock" }
        };
        var createRewardResponse = await _client.PostAsync("/api/rewards", formData2);
        var rewardDto = await createRewardResponse.Content.ReadFromJsonAsync<RewardDto>();
        Assert.NotNull(rewardDto);

        // 子供でログイン
        var childId = AuthenticationHelper.GetTestChildId(_factory.Services);
        var childLoginResponse = await _client.PostAsJsonAsync("/api/auth/child-login", new
        {
            ChildId = childId,
            PIN = "1234"
        });
        var childLoginResult = await childLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // ポイントを追加（10ポイント必要）
        var studentId = AuthenticationHelper.GetTestStudentId(_factory.Services);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GamifiedMathDrill.Infrastructure.Data.ApplicationDbContext>();
            var student = await db.Students.FindAsync(studentId);
            if (student != null)
            {
                student.TotalPoints = 50; // 十分なポイントを付与
                await db.SaveChangesAsync();
            }
        }

        // 交換申請
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult!.Token);
        var exchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });
        var exchangeRequest = await exchangeResponse.Content.ReadFromJsonAsync<ExchangeRequestDto>();
        Assert.NotNull(exchangeRequest);

        // Act: Pendingステータスのキャンセル（成功するはず）
        var cancelResponse = await _client.PutAsync($"/api/exchange-requests/{exchangeRequest.Id}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        // キャンセル後、再度キャンセル試行（失敗するはず）
        var secondCancelResponse = await _client.PutAsync($"/api/exchange-requests/{exchangeRequest.Id}/cancel", null);
        Assert.Equal(HttpStatusCode.BadRequest, secondCancelResponse.StatusCode);
    }

    [Fact]
    public async Task ApprovedRequest_CannotBeCancelled()
    {
        // 認証
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);

        // Arrange: 保護者でログイン
        var parentLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var parentLoginResult = await parentLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // 景品を作成
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentLoginResult!.Token);
        var formData3 = new MultipartFormDataContent
        {
            { new StringContent("Test Item"), "Name" },
            { new StringContent("Test Description"), "Description" },
            { new StringContent("10"), "RequiredPoints" },
            { new StringContent("Snack"), "Category" },
            { new StringContent("true"), "IsPhysical" },
            { new StringContent("5"), "Stock" }
        };
        var createRewardResponse = await _client.PostAsync("/api/rewards", formData3);
        var rewardDto = await createRewardResponse.Content.ReadFromJsonAsync<RewardDto>();
        Assert.NotNull(rewardDto);

        // 子供でログイン・申請
        var childId = AuthenticationHelper.GetTestChildId(_factory.Services);
        var childLoginResponse = await _client.PostAsJsonAsync("/api/auth/child-login", new
        {
            ChildId = childId,
            PIN = "1234"
        });
        var childLoginResult = await childLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult!.Token);

        // ポイントを追加（10ポイント必要）
        var studentId = AuthenticationHelper.GetTestStudentId(_factory.Services);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GamifiedMathDrill.Infrastructure.Data.ApplicationDbContext>();
            var student = await db.Students.FindAsync(studentId);
            if (student != null)
            {
                student.TotalPoints = 50; // 十分なポイントを付与
                await db.SaveChangesAsync();
            }
        }

        var exchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });
        var exchangeRequest = await exchangeResponse.Content.ReadFromJsonAsync<ExchangeRequestDto>();
        Assert.NotNull(exchangeRequest);

        // 保護者が承認
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentLoginResult!.Token);
        var approveResponse = await _client.PutAsJsonAsync($"/api/exchange-requests/{exchangeRequest.Id}/approve", new { ParentNote = "承認します" });
        Assert.True(approveResponse.IsSuccessStatusCode, $"Approval failed: {await approveResponse.Content.ReadAsStringAsync()}");

        // Act: 承認済み申請をキャンセル試行（失敗するはず）
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult!.Token);
        var cancelResponse = await _client.PutAsync($"/api/exchange-requests/{exchangeRequest.Id}/cancel", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, cancelResponse.StatusCode);
        var errorContent = await cancelResponse.Content.ReadAsStringAsync();
        Assert.Contains("承認済みまたは却下された申請はキャンセルできません", errorContent);
    }

    [Fact]
    public async Task StockReturnsOnRejection()
    {
        // 認証
        await AuthenticationHelper.EnsureTestUsersExistAsync(_factory.Services);

        // Arrange: 保護者でログイン
        var parentLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "parent@example.com",
            Password = "Parent123!"
        });
        var parentLoginResult = await parentLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // 在庫5の景品を作成
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentLoginResult!.Token);
        var formData4 = new MultipartFormDataContent
        {
            { new StringContent("Stock Test Item"), "Name" },
            { new StringContent("Test Description"), "Description" },
            { new StringContent("10"), "RequiredPoints" },
            { new StringContent("Snack"), "Category" },
            { new StringContent("true"), "IsPhysical" },
            { new StringContent("5"), "Stock" }
        };
        var createRewardResponse = await _client.PostAsync("/api/rewards", formData4);
        var rewardDto = await createRewardResponse.Content.ReadFromJsonAsync<RewardDto>();
        Assert.NotNull(rewardDto);

        // 子供でログイン・申請
        var childId = AuthenticationHelper.GetTestChildId(_factory.Services);
        var childLoginResponse = await _client.PostAsJsonAsync("/api/auth/child-login", new
        {
            ChildId = childId,
            PIN = "1234"
        });
        var childLoginResult = await childLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", childLoginResult!.Token);

        // ポイントを追加（10ポイント必要）
        var studentId = AuthenticationHelper.GetTestStudentId(_factory.Services);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GamifiedMathDrill.Infrastructure.Data.ApplicationDbContext>();
            var student = await db.Students.FindAsync(studentId);
            if (student != null)
            {
                student.TotalPoints = 50; // 十分なポイントを付与
                await db.SaveChangesAsync();
            }
        }

        var exchangeResponse = await _client.PostAsJsonAsync("/api/exchange-requests", new
        {
            RewardId = rewardDto.Id
        });
        var exchangeRequest = await exchangeResponse.Content.ReadFromJsonAsync<ExchangeRequestDto>();
        Assert.NotNull(exchangeRequest);

        // 在庫確認（申請後は4になっているはず）
        var afterRequestResponse = await _client.GetAsync($"/api/rewards/{rewardDto.Id}");
        var afterRequestReward = await afterRequestResponse.Content.ReadFromJsonAsync<RewardDto>();
        Assert.NotNull(afterRequestReward);
        Assert.Equal(4, afterRequestReward.Stock);

        // Act: 保護者が却下
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parentLoginResult!.Token);
        await _client.PutAsJsonAsync($"/api/exchange-requests/{exchangeRequest.Id}/reject", new
        {
            Reason = "Test rejection"
        });

        // Assert: 在庫が戻っているはず（5に戻る）
        var afterRejectionResponse = await _client.GetAsync($"/api/rewards/{rewardDto.Id}");
        var afterRejectionReward = await afterRejectionResponse.Content.ReadFromJsonAsync<RewardDto>();
        Assert.NotNull(afterRejectionReward);
        Assert.Equal(5, afterRejectionReward.Stock);
    }
}
