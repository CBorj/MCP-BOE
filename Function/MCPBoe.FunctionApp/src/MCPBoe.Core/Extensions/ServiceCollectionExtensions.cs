using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using MCPBoe.Core.Clients;
using MCPBoe.Core.Configuration;
using MCPBoe.Core.Services;

namespace MCPBoe.Core.Extensions;

/// <summary>
/// Service collection extensions for MCPBoe.Core
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add MCPBoe core services
    /// </summary>
    public static IServiceCollection AddMCPBoeCore(this IServiceCollection services, IConfiguration configuration)
    {
        // Configuration
        services.Configure<BoeApiOptions>(configuration.GetSection(BoeApiOptions.SectionName));
        services.Configure<AppOptions>(configuration.GetSection(AppOptions.SectionName));

        // HTTP clients
        services.AddHttpClients(configuration);

        // Services
        services.AddScoped<ILegislationService, LegislationService>();
        services.AddScoped<ISummaryService, SummaryService>();
        services.AddScoped<IAuxiliaryService, AuxiliaryService>();

        return services;
    }

    private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var boeApiOptions = configuration.GetSection(BoeApiOptions.SectionName).Get<BoeApiOptions>() ?? new BoeApiOptions();

        // Configure retry policy
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: boeApiOptions.RetryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(
                    boeApiOptions.RetryDelayMs * Math.Pow(2, retryAttempt - 1)), // Exponential backoff
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
                });

        // Configure timeout policy
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(boeApiOptions.TimeoutSeconds));

        // Combine policies
        var combinedPolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy);

        // Add typed HTTP client for BOE API
        services.AddHttpClient<IBoeApiClient, BoeApiClient>(client =>
        {
            client.BaseAddress = new Uri(boeApiOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(boeApiOptions.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("User-Agent", boeApiOptions.UserAgent);
        })
        .AddPolicyHandler(combinedPolicy);

        return services;
    }

    /// <summary>
    /// Add validation services
    /// </summary>
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        // Add FluentValidation if needed
        // services.AddValidatorsFromAssemblyContaining<RequestDtoValidator>();
        
        return services;
    }
}
