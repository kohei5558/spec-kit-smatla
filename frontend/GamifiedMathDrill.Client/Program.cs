using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
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

// Configure HttpClient with API base address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5242") });

// Register authentication services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

// Register API clients
builder.Services.AddScoped<ProblemApiClient>();
builder.Services.AddScoped<StudentApiClient>();
builder.Services.AddScoped<LearningRecordApiClient>();
builder.Services.AddScoped<DailyChallengeApiClient>();
builder.Services.AddScoped<RewardApiClient>();
builder.Services.AddScoped<ExchangeRequestApiClient>();
builder.Services.AddScoped<ParentDashboardApiClient>();
builder.Services.AddScoped<ChildAccountApiClient>();

// Register state management services
builder.Services.AddScoped<CategoryStateService>();

await builder.Build().RunAsync();
