namespace MCPBoe.Core.Configuration;

/// <summary>
/// Configuration options for BOE API client
/// </summary>
public class BoeApiOptions
{
    public const string SectionName = "BoeApi";

    /// <summary>
    /// Base URL for the BOE API
    /// </summary>
    public string BaseUrl { get; set; } = "https://www.boe.es/datosabiertos/api";

    /// <summary>
    /// Timeout for HTTP requests in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Delay between retries in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Maximum concurrent requests
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 10;

    /// <summary>
    /// User agent string for HTTP requests
    /// </summary>
    public string UserAgent { get; set; } = "MCPBoe.FunctionApp/1.0";

    /// <summary>
    /// Enable request/response logging
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Cache TTL in minutes for static data (departments, ranges, etc.)
    /// </summary>
    public int CacheTtlMinutes { get; set; } = 60;
}

/// <summary>
/// Configuration options for the application
/// </summary>
public class AppOptions
{
    public const string SectionName = "App";

    /// <summary>
    /// Application name
    /// </summary>
    public string Name { get; set; } = "MCPBoe Function App";

    /// <summary>
    /// Application version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Environment (Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; } = "Development";

    /// <summary>
    /// Enable detailed error responses
    /// </summary>
    public bool DetailedErrors { get; set; } = false;

    /// <summary>
    /// Enable CORS
    /// </summary>
    public bool EnableCors { get; set; } = true;

    /// <summary>
    /// Allowed CORS origins
    /// </summary>
    public string[] AllowedOrigins { get; set; } = ["*"];
}
