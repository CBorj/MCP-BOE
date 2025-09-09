using System.Text.Json.Serialization;

namespace MCPBoe.Core.Models;

/// <summary>
/// Response DTOs for BOE API operations
/// </summary>

/// <summary>
/// Base response wrapper
/// </summary>
public record ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(string error) => new() { Success = false, Error = error };
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public record PaginatedResponse<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; init; } = new();

    [JsonPropertyName("total_count")]
    public int TotalCount { get; init; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; init; }

    [JsonPropertyName("page_number")]
    public int PageNumber { get; init; }

    [JsonPropertyName("has_next")]
    public bool HasNext { get; init; }

    [JsonPropertyName("has_previous")]
    public bool HasPrevious { get; init; }
}

/// <summary>
/// Response for legislation search
/// </summary>
public record SearchLegislationResponse
{
    [JsonPropertyName("results")]
    public List<BoeLegislationModel> Results { get; init; } = new();

    [JsonPropertyName("total_results")]
    public int TotalResults { get; init; }

    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    [JsonPropertyName("execution_time_ms")]
    public double ExecutionTimeMs { get; init; }
}

/// <summary>
/// Response for getting consolidated law
/// </summary>
public record GetLawResponse
{
    [JsonPropertyName("law")]
    public BoeLegislationModel? Law { get; init; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    [JsonPropertyName("analysis")]
    public Dictionary<string, object>? Analysis { get; init; }
}

/// <summary>
/// Response for getting law structure
/// </summary>
public record GetLawStructureResponse
{
    [JsonPropertyName("structure")]
    public BoeStructureModel? Structure { get; init; }

    [JsonPropertyName("law_id")]
    public string LawId { get; init; } = string.Empty;
}

/// <summary>
/// Response for BOE/BORME summaries
/// </summary>
public record SummaryResponse
{
    [JsonPropertyName("summaries")]
    public List<BoeSummaryModel> Summaries { get; init; } = new();

    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty; // "BOE" or "BORME"

    [JsonPropertyName("total_items")]
    public int TotalItems { get; init; }
}

/// <summary>
/// Response for recent BOE search
/// </summary>
public record SearchRecentBoeResponse
{
    [JsonPropertyName("results")]
    public List<BoeSummaryModel> Results { get; init; } = new();

    [JsonPropertyName("search_terms")]
    public List<string> SearchTerms { get; init; } = new();

    [JsonPropertyName("days_searched")]
    public int DaysSearched { get; init; }

    [JsonPropertyName("total_matches")]
    public int TotalMatches { get; init; }
}

/// <summary>
/// Response for auxiliary data (departments, ranges, etc.)
/// </summary>
public record AuxiliaryDataResponse
{
    [JsonPropertyName("data")]
    public List<BoeAuxiliaryModel> Data { get; init; } = new();

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty; // "departments", "ranges", etc.

    [JsonPropertyName("total_items")]
    public int TotalItems { get; init; }

    [JsonPropertyName("search_term")]
    public string SearchTerm { get; init; } = string.Empty;
}

/// <summary>
/// Response for code description
/// </summary>
public record CodeDescriptionResponse
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("additional_info")]
    public Dictionary<string, object>? AdditionalInfo { get; init; }
}

/// <summary>
/// Response for multiple code descriptions
/// </summary>
public record CodeDescriptionsResponse
{
    [JsonPropertyName("descriptions")]
    public List<CodeDescriptionResponse> Descriptions { get; init; } = new();

    [JsonPropertyName("total_codes")]
    public int TotalCodes { get; init; }

    [JsonPropertyName("found_codes")]
    public int FoundCodes { get; init; }
}

/// <summary>
/// Response for departments list
/// </summary>
public record DepartmentsResponse
{
    [JsonPropertyName("departments")]
    public List<BoeAuxiliaryModel> Departments { get; init; } = new();

    [JsonPropertyName("total_count")]
    public int TotalCount { get; init; }
}

/// <summary>
/// Response for legal ranges
/// </summary>
public record LegalRangesResponse
{
    [JsonPropertyName("ranges")]
    public List<BoeAuxiliaryModel> Ranges { get; init; } = new();

    [JsonPropertyName("date")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("total_count")]
    public int TotalCount { get; init; }
}

/// <summary>
/// Error response
/// </summary>
public record ErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; init; } = string.Empty;

    [JsonPropertyName("error_code")]
    public string ErrorCode { get; init; } = string.Empty;

    [JsonPropertyName("details")]
    public string? Details { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
