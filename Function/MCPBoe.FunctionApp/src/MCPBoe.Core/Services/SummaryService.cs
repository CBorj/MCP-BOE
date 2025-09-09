using Microsoft.Extensions.Logging;
using MCPBoe.Core.Clients;
using MCPBoe.Core.Models;

namespace MCPBoe.Core.Services;

/// <summary>
/// Interface for summary service
/// </summary>
public interface ISummaryService
{
    Task<SummaryResponse> GetBoeSummaryAsync(
        GetBoeSummaryRequest request, CancellationToken cancellationToken = default);
    
    Task<SummaryResponse> GetBormeSummaryAsync(
        GetBormeSummaryRequest request, CancellationToken cancellationToken = default);
    
    Task<SearchRecentBoeResponse> SearchRecentBoeAsync(
        SearchRecentBoeRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for summary operations
/// </summary>
public class SummaryService : ISummaryService
{
    private readonly IBoeApiClient _boeApiClient;
    private readonly ILogger<SummaryService> _logger;

    public SummaryService(IBoeApiClient boeApiClient, ILogger<SummaryService> logger)
    {
        _boeApiClient = boeApiClient;
        _logger = logger;
    }

    public async Task<SummaryResponse> GetBoeSummaryAsync(
        GetBoeSummaryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing BOE summary request for date: {Date}", request.Date);

            var summaries = await _boeApiClient.GetBoeSummaryAsync(
                request.Date, request.MaxItems, cancellationToken);

            _logger.LogInformation("Found {Count} BOE summary items for date: {Date}", 
                summaries.Count, request.Date);

            return new SummaryResponse
            {
                Summaries = summaries,
                Date = request.Date,
                Type = "BOE",
                TotalItems = summaries.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing BOE summary request for date: {Date}", request.Date);
            throw;
        }
    }

    public async Task<SummaryResponse> GetBormeSummaryAsync(
        GetBormeSummaryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing BORME summary request for date: {Date}", request.Date);

            var summaries = await _boeApiClient.GetBormeSummaryAsync(
                request.Date, request.MaxItems, cancellationToken);

            _logger.LogInformation("Found {Count} BORME summary items for date: {Date}", 
                summaries.Count, request.Date);

            return new SummaryResponse
            {
                Summaries = summaries,
                Date = request.Date,
                Type = "BORME",
                TotalItems = summaries.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing BORME summary request for date: {Date}", request.Date);
            throw;
        }
    }

    public async Task<SearchRecentBoeResponse> SearchRecentBoeAsync(
        SearchRecentBoeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing recent BOE search request: DaysBack: {DaysBack}, Terms: {SearchTerms}", 
                request.DaysBack, string.Join(", ", request.SearchTerms));

            var results = await _boeApiClient.SearchRecentBoeAsync(
                request.DaysBack, request.SearchTerms, cancellationToken);

            // Filter results based on search terms (client-side filtering as backup)
            var filteredResults = FilterResultsBySearchTerms(results, request.SearchTerms);

            _logger.LogInformation("Found {Count} recent BOE items (filtered from {OriginalCount}) for terms: {SearchTerms}", 
                filteredResults.Count, results.Count, string.Join(", ", request.SearchTerms));

            return new SearchRecentBoeResponse
            {
                Results = filteredResults,
                SearchTerms = request.SearchTerms,
                DaysSearched = request.DaysBack,
                TotalMatches = filteredResults.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing recent BOE search request: DaysBack: {DaysBack}", request.DaysBack);
            throw;
        }
    }

    private List<BoeSummaryModel> FilterResultsBySearchTerms(List<BoeSummaryModel> results, List<string> searchTerms)
    {
        if (!searchTerms.Any())
            return results;

        try
        {
            return results.Where(result =>
            {
                var searchableText = $"{result.Title} {result.Section} {result.Issuer}".ToLowerInvariant();
                
                return searchTerms.Any(term =>
                    searchableText.Contains(term.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error filtering results by search terms, returning original results");
            return results;
        }
    }
}
