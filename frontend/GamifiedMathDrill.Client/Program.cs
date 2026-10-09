using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using GamifiedMathDrill.Client;
using GamifiedMathDrill.Client.Services;
using MudBlazor.Services;
using Blazored.LocalStorage;
using Blazored.SessionStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// MudBlazor services
builder.Services.AddMudServices();

// Blazored LocalStorage and SessionStorage
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

// Register TokenService first
builder.Services.AddScoped<TokenService>();

// Configure HttpClient with API base address and authorization handler
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient(new AuthorizationMessageHandler(sp))
    {
        BaseAddress = new Uri("http://localhost:5242")
    };
    return httpClient;
});

// Register authentication services
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<AuthService>(provider =>
{
    var httpClient = provider.GetRequiredService<HttpClient>();
    var tokenService = provider.GetRequiredService<TokenService>();
    return new AuthService(httpClient, tokenService, () => provider.GetRequiredService<AuthenticationStateProvider>());
});
builder.Services.AddAuthorizationCore();

// Register API clients
builder.Services.AddScoped<ProblemApiClient>();
builder.Services.AddScoped<StudentApiClient>();
builder.Services.AddScoped<LearningRecordApiClient>();
builder.Services.AddScoped<DailyChallengeApiClient>();
builder.Services.AddScoped<RewardApiClient>();
builder.Services.AddScoped<ExchangeRequestApiClient>();
builder.Services.AddScoped<ParentDashboardApiClient>();
builder.Services.AddScoped<ChildAccountApiClient>();
builder.Services.AddScoped<DeviceApiClient>();

// Register state management services
builder.Services.AddScoped<CategoryStateService>();

await builder.Build().RunAsync();
