using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MCPBoe.Core.Extensions;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Register Core services
builder.Services.AddMCPBoeCore(builder.Configuration);

// Configure logging and telemetry
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configure logging levels
builder.Services.Configure<LoggerFilterOptions>(options =>
{
    options.Rules.Clear();
    options.AddFilter("Microsoft.Azure.Functions.Worker", LogLevel.Information);
    options.AddFilter("MCPBoe", LogLevel.Information);
    options.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
});

// Configure HTTP clients
builder.Services.AddHttpClient();

builder.Build().Run();
