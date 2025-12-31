using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GamifiedMathDrill.Client;
using GamifiedMathDrill.Client.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// MudBlazor services
builder.Services.AddMudServices();

// Configure HttpClient with API base address
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5242") });

// Register API clients
builder.Services.AddScoped<ProblemApiClient>();
builder.Services.AddScoped<StudentApiClient>();
builder.Services.AddScoped<LearningRecordApiClient>();

await builder.Build().RunAsync();
