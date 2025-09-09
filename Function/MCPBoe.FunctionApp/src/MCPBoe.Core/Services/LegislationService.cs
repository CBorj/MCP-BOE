using Microsoft.Extensions.Logging;
using MCPBoe.Core.Clients;
using MCPBoe.Core.Models;

namespace MCPBoe.Core.Services;

/// <summary>
/// Interface for legislation service
/// </summary>
public interface ILegislationService
{
    Task<SearchLegislationResponse> SearchConsolidatedLegislationAsync(
        SearchLegislationRequest request, CancellationToken cancellationToken = default);
    
    Task<GetLawResponse> GetConsolidatedLawAsync(
        GetLawRequest request, CancellationToken cancellationToken = default);
    
    Task<GetLawStructureResponse> GetLawStructureAsync(
        GetLawStructureRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for legislation operations
/// </summary>
public class LegislationService : ILegislationService
{
    private readonly IBoeApiClient _boeApiClient;
    private readonly ILogger<LegislationService> _logger;

    public LegislationService(IBoeApiClient boeApiClient, ILogger<LegislationService> logger)
    {
        _boeApiClient = boeApiClient;
        _logger = logger;
    }

    public async Task<SearchLegislationResponse> SearchConsolidatedLegislationAsync(
        SearchLegislationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing legislation search request: {Query}", request.Query);
            
            var startTime = DateTime.UtcNow;
            var results = await _boeApiClient.SearchConsolidatedLegislationAsync(
                request.Query, request.Limit, request.Offset, cancellationToken);
            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation("Found {Count} legislation results for query: {Query} in {ExecutionTime}ms", 
                results.Count, request.Query, executionTime);

            return new SearchLegislationResponse
            {
                Results = results,
                TotalResults = results.Count, // BOE API doesn't provide total count
                Query = request.Query,
                ExecutionTimeMs = executionTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing legislation search request: {Query}", request.Query);
            throw;
        }
    }

    public async Task<GetLawResponse> GetConsolidatedLawAsync(
        GetLawRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing get law request: {LawId}", request.LawId);

            var law = await _boeApiClient.GetConsolidatedLawAsync(
                request.LawId, 
                request.IncludeMetadata, 
                request.IncludeAnalysis, 
                request.IncludeFullText, 
                cancellationToken);

            if (law == null)
            {
                _logger.LogWarning("Law not found: {LawId}", request.LawId);
                return new GetLawResponse { Law = null };
            }

            _logger.LogInformation("Successfully retrieved law: {LawId}", request.LawId);

            // TODO: Extract metadata and analysis if requested
            var metadata = request.IncludeMetadata ? ExtractMetadata(law) : null;
            var analysis = request.IncludeAnalysis ? ExtractAnalysis(law) : null;

            return new GetLawResponse
            {
                Law = law,
                Metadata = metadata,
                Analysis = analysis
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get law request: {LawId}", request.LawId);
            throw;
        }
    }

    public async Task<GetLawStructureResponse> GetLawStructureAsync(
        GetLawStructureRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing get law structure request: {LawId}", request.LawId);

            var structure = await _boeApiClient.GetLawStructureAsync(request.LawId, cancellationToken);

            if (structure == null)
            {
                _logger.LogWarning("Law structure not found: {LawId}", request.LawId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved law structure: {LawId}", request.LawId);
            }

            return new GetLawStructureResponse
            {
                Structure = structure,
                LawId = request.LawId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get law structure request: {LawId}", request.LawId);
            throw;
        }
    }

    private Dictionary<string, object>? ExtractMetadata(BoeLegislationModel law)
    {
        try
        {
            return new Dictionary<string, object>
            {
                ["id"] = law.Id,
                ["title"] = law.Title,
                ["date"] = law.Date,
                ["norm_type"] = law.NormType,
                ["number"] = law.Number,
                ["department"] = law.Department,
                ["range"] = law.Range,
                ["is_active"] = law.IsActive,
                ["url"] = law.Url,
                ["extracted_at"] = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting metadata for law: {LawId}", law.Id);
            return null;
        }
    }

    private Dictionary<string, object>? ExtractAnalysis(BoeLegislationModel law)
    {
        try
        {
            // Basic text analysis
            var textLength = law.Text?.Length ?? 0;
            var wordCount = string.IsNullOrEmpty(law.Text) ? 0 : law.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            
            return new Dictionary<string, object>
            {
                ["text_length"] = textLength,
                ["word_count"] = wordCount,
                ["has_structure"] = law.Structure != null,
                ["structure_complexity"] = CalculateStructureComplexity(law.Structure),
                ["analysis_date"] = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting analysis for law: {LawId}", law.Id);
            return null;
        }
    }

    private int CalculateStructureComplexity(BoeStructureModel? structure)
    {
        if (structure == null) return 0;
        
        return structure.Titles.Count + structure.Chapters.Count + structure.Articles.Count;
    }
}
