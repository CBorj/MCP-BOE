using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MCPBoe.Core.Models;

/// <summary>
/// Request DTOs for BOE API operations
/// </summary>

/// <summary>
/// Request for searching consolidated legislation
/// </summary>
public record SearchLegislationRequest
{
    [JsonPropertyName("query")]
    [Required(ErrorMessage = "Query is required")]
    [StringLength(500, ErrorMessage = "Query cannot exceed 500 characters")]
    public string Query { get; init; } = string.Empty;

    [JsonPropertyName("limit")]
    [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100")]
    public int Limit { get; init; } = 10;

    [JsonPropertyName("offset")]
    [Range(0, int.MaxValue, ErrorMessage = "Offset must be non-negative")]
    public int Offset { get; init; } = 0;
}

/// <summary>
/// Request for getting a consolidated law
/// </summary>
public record GetLawRequest
{
    [JsonPropertyName("law_id")]
    [Required(ErrorMessage = "Law ID is required")]
    public string LawId { get; init; } = string.Empty;

    [JsonPropertyName("include_metadata")]
    public bool IncludeMetadata { get; init; } = true;

    [JsonPropertyName("include_analysis")]
    public bool IncludeAnalysis { get; init; } = false;

    [JsonPropertyName("include_full_text")]
    public bool IncludeFullText { get; init; } = false;
}

/// <summary>
/// Request for getting law structure
/// </summary>
public record GetLawStructureRequest
{
    [JsonPropertyName("law_id")]
    [Required(ErrorMessage = "Law ID is required")]
    public string LawId { get; init; } = string.Empty;
}

/// <summary>
/// Request for getting BOE summary
/// </summary>
public record GetBoeSummaryRequest
{
    [JsonPropertyName("date")]
    [Required(ErrorMessage = "Date is required")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "Date must be in YYYYMMDD format")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("max_items")]
    [Range(1, 1000, ErrorMessage = "Max items must be between 1 and 1000")]
    public int MaxItems { get; init; } = 50;
}

/// <summary>
/// Request for getting BORME summary
/// </summary>
public record GetBormeSummaryRequest
{
    [JsonPropertyName("date")]
    [Required(ErrorMessage = "Date is required")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "Date must be in YYYYMMDD format")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("max_items")]
    [Range(1, 1000, ErrorMessage = "Max items must be between 1 and 1000")]
    public int MaxItems { get; init; } = 50;
}

/// <summary>
/// Request for searching recent BOE
/// </summary>
public record SearchRecentBoeRequest
{
    [JsonPropertyName("days_back")]
    [Range(1, 30, ErrorMessage = "Days back must be between 1 and 30")]
    public int DaysBack { get; init; } = 7;

    [JsonPropertyName("search_terms")]
    [Required(ErrorMessage = "Search terms are required")]
    [MinLength(1, ErrorMessage = "At least one search term is required")]
    public List<string> SearchTerms { get; init; } = new();
}

/// <summary>
/// Request for getting departments table
/// </summary>
public record GetDepartmentsRequest
{
    [JsonPropertyName("search_term")]
    public string SearchTerm { get; init; } = string.Empty;

    [JsonPropertyName("limit")]
    [Range(1, 1000, ErrorMessage = "Limit must be between 1 and 1000")]
    public int Limit { get; init; } = 100;
}

/// <summary>
/// Request for getting legal ranges table
/// </summary>
public record GetLegalRangesRequest
{
    [JsonPropertyName("date")]
    [Required(ErrorMessage = "Date is required")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("limit")]
    [Range(1, 1000, ErrorMessage = "Limit must be between 1 and 1000")]
    public int Limit { get; init; } = 100;
}

/// <summary>
/// Request for getting code description
/// </summary>
public record GetCodeDescriptionRequest
{
    [JsonPropertyName("code")]
    [Required(ErrorMessage = "Code is required")]
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Request for getting multiple code descriptions
/// </summary>
public record GetCodeDescriptionsRequest
{
    [JsonPropertyName("codes")]
    [Required(ErrorMessage = "Codes are required")]
    public IEnumerable<string> Codes { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Request for searching auxiliary data
/// </summary>
public record SearchAuxiliaryRequest
{
    [JsonPropertyName("query")]
    [Required(ErrorMessage = "Query is required")]
    [StringLength(200, ErrorMessage = "Query cannot exceed 200 characters")]
    public string Query { get; init; } = string.Empty;
}
