using Microsoft.Extensions.Logging;
using MCPBoe.Core.Clients;
using MCPBoe.Core.Models;

namespace MCPBoe.Core.Services;

/// <summary>
/// Interface for auxiliary service
/// </summary>
public interface IAuxiliaryService
{
    // New methods for Azure Functions
    Task<DepartmentsResponse> GetDepartmentsAsync(CancellationToken cancellationToken = default);
    Task<LegalRangesResponse> GetLegalRangesAsync(GetLegalRangesRequest request, CancellationToken cancellationToken = default);
    Task<CodeDescriptionsResponse> GetCodeDescriptionsAsync(GetCodeDescriptionsRequest request, CancellationToken cancellationToken = default);

    // Legacy methods for compatibility
    Task<AuxiliaryDataResponse> GetDepartmentsTableAsync(
        GetDepartmentsRequest request, CancellationToken cancellationToken = default);
    
    Task<AuxiliaryDataResponse> GetLegalRangesTableAsync(
        GetLegalRangesRequest request, CancellationToken cancellationToken = default);
    
    Task<CodeDescriptionResponse> GetCodeDescriptionAsync(
        GetCodeDescriptionRequest request, CancellationToken cancellationToken = default);
    
    Task<AuxiliaryDataResponse> SearchAuxiliaryDataAsync(
        SearchAuxiliaryRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for auxiliary data operations (departments, ranges, codes, etc.)
/// </summary>
public class AuxiliaryService : IAuxiliaryService
{
    private readonly IBoeApiClient _boeApiClient;
    private readonly ILogger<AuxiliaryService> _logger;

    public AuxiliaryService(IBoeApiClient boeApiClient, ILogger<AuxiliaryService> logger)
    {
        _boeApiClient = boeApiClient;
        _logger = logger;
    }

    /// <summary>
    /// Get list of departments
    /// </summary>
    public async Task<DepartmentsResponse> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting departments list");

            // Call BOE API to get departments
            var departments = await _boeApiClient.GetDepartmentsTableAsync("", 1000, cancellationToken);

            return new DepartmentsResponse
            {
                Departments = departments,
                TotalCount = departments.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments");
            throw;
        }
    }

    /// <summary>
    /// Get legal ranges for a specific date
    /// </summary>
    public async Task<LegalRangesResponse> GetLegalRangesAsync(GetLegalRangesRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting legal ranges for date: {Date}", request.Date);

            // Call BOE API to get legal ranges (note: the API doesn't use date, so we get all ranges)
            var ranges = await _boeApiClient.GetLegalRangesTableAsync(request.Limit, cancellationToken);

            return new LegalRangesResponse
            {
                Ranges = ranges,
                Date = request.Date,
                TotalCount = ranges.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting legal ranges for date: {Date}", request.Date);
            throw;
        }
    }

    /// <summary>
    /// Get descriptions for specific codes
    /// </summary>
    public async Task<CodeDescriptionsResponse> GetCodeDescriptionsAsync(GetCodeDescriptionsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting code descriptions for {CodeCount} codes", request.Codes.Count());

            var descriptions = new List<CodeDescriptionResponse>();
            var foundCodes = 0;

            foreach (var code in request.Codes)
            {
                try
                {
                    var description = await _boeApiClient.GetCodeDescriptionAsync(code, cancellationToken);
                    if (description != null)
                    {
                        descriptions.Add(new CodeDescriptionResponse
                        {
                            Code = code,
                            Description = description.Description ?? string.Empty,
                            Type = description.Type ?? string.Empty,
                            AdditionalInfo = ExtractAdditionalInfo(description)
                        });
                        foundCodes++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get description for code: {Code}", code);
                    // Continue with other codes
                }
            }

            return new CodeDescriptionsResponse
            {
                Descriptions = descriptions,
                TotalCodes = request.Codes.Count(),
                FoundCodes = foundCodes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code descriptions");
            throw;
        }
    }

    public async Task<AuxiliaryDataResponse> GetDepartmentsTableAsync(
        GetDepartmentsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing departments table request: SearchTerm: {SearchTerm}, Limit: {Limit}", 
                request.SearchTerm, request.Limit);

            var departments = await _boeApiClient.GetDepartmentsTableAsync(
                request.SearchTerm, request.Limit, cancellationToken);

            _logger.LogInformation("Found {Count} departments for search term: {SearchTerm}", 
                departments.Count, request.SearchTerm);

            return new AuxiliaryDataResponse
            {
                Data = departments,
                Type = "departments",
                TotalItems = departments.Count,
                SearchTerm = request.SearchTerm
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing departments table request: {SearchTerm}", request.SearchTerm);
            throw;
        }
    }

    public async Task<AuxiliaryDataResponse> GetLegalRangesTableAsync(
        GetLegalRangesRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing legal ranges table request: Limit: {Limit}", request.Limit);

            var ranges = await _boeApiClient.GetLegalRangesTableAsync(request.Limit, cancellationToken);

            _logger.LogInformation("Found {Count} legal ranges", ranges.Count);

            return new AuxiliaryDataResponse
            {
                Data = ranges,
                Type = "legal_ranges",
                TotalItems = ranges.Count,
                SearchTerm = string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing legal ranges table request");
            throw;
        }
    }

    public async Task<CodeDescriptionResponse> GetCodeDescriptionAsync(
        GetCodeDescriptionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing code description request: {Code}", request.Code);

            var codeData = await _boeApiClient.GetCodeDescriptionAsync(request.Code, cancellationToken);

            if (codeData == null)
            {
                _logger.LogWarning("Code not found: {Code}", request.Code);
                
                return new CodeDescriptionResponse
                {
                    Code = request.Code,
                    Description = "Code not found",
                    Type = "unknown"
                };
            }

            _logger.LogInformation("Successfully retrieved code description: {Code}", request.Code);

            return new CodeDescriptionResponse
            {
                Code = request.Code,
                Description = codeData.Description,
                Type = codeData.Type,
                AdditionalInfo = ExtractAdditionalInfo(codeData)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing code description request: {Code}", request.Code);
            throw;
        }
    }

    public async Task<AuxiliaryDataResponse> SearchAuxiliaryDataAsync(
        SearchAuxiliaryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing auxiliary data search request: {Query}", request.Query);

            var results = await _boeApiClient.SearchAuxiliaryDataAsync(request.Query, cancellationToken);

            _logger.LogInformation("Found {Count} auxiliary data items for query: {Query}", 
                results.Count, request.Query);

            return new AuxiliaryDataResponse
            {
                Data = results,
                Type = "search_results",
                TotalItems = results.Count,
                SearchTerm = request.Query
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auxiliary data search request: {Query}", request.Query);
            throw;
        }
    }

    private Dictionary<string, object>? ExtractAdditionalInfo(BoeAuxiliaryModel codeData)
    {
        try
        {
            var info = new Dictionary<string, object>
            {
                ["code"] = codeData.Code,
                ["type"] = codeData.Type,
                ["retrieved_at"] = DateTime.UtcNow
            };

            // Add type-specific information
            switch (codeData.Type.ToLowerInvariant())
            {
                case "department":
                case "departamento":
                    info["category"] = "government_department";
                    break;
                case "range":
                case "rango":
                    info["category"] = "legal_range";
                    break;
                default:
                    info["category"] = "general";
                    break;
            }

            return info;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting additional info for code: {Code}", codeData.Code);
            return null;
        }
    }
}
