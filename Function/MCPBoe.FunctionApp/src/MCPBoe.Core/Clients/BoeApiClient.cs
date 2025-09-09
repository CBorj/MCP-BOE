using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MCPBoe.Core.Configuration;
using MCPBoe.Core.Models;

namespace MCPBoe.Core.Clients;

/// <summary>
/// Interface for BOE API client
/// </summary>
public interface IBoeApiClient
{
    Task<List<BoeLegislationModel>> SearchConsolidatedLegislationAsync(
        string query, int limit = 10, int offset = 0, CancellationToken cancellationToken = default);
    
    Task<BoeLegislationModel?> GetConsolidatedLawAsync(
        string lawId, bool includeMetadata = true, bool includeAnalysis = false, 
        bool includeFullText = false, CancellationToken cancellationToken = default);
    
    Task<BoeStructureModel?> GetLawStructureAsync(
        string lawId, CancellationToken cancellationToken = default);
    
    Task<List<BoeSummaryModel>> GetBoeSummaryAsync(
        string date, int maxItems = 50, CancellationToken cancellationToken = default);
    
    Task<List<BoeSummaryModel>> GetBormeSummaryAsync(
        string date, int maxItems = 50, CancellationToken cancellationToken = default);
    
    Task<List<BoeSummaryModel>> SearchRecentBoeAsync(
        int daysBack, List<string> searchTerms, CancellationToken cancellationToken = default);
    
    Task<List<BoeAuxiliaryModel>> GetDepartmentsTableAsync(
        string searchTerm = "", int limit = 100, CancellationToken cancellationToken = default);
    
    Task<List<BoeAuxiliaryModel>> GetLegalRangesTableAsync(
        int limit = 100, CancellationToken cancellationToken = default);
    
    Task<BoeAuxiliaryModel?> GetCodeDescriptionAsync(
        string code, CancellationToken cancellationToken = default);
    
    Task<List<BoeAuxiliaryModel>> SearchAuxiliaryDataAsync(
        string query, CancellationToken cancellationToken = default);
}

/// <summary>
/// HTTP client for BOE API
/// </summary>
public class BoeApiClient : IBoeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BoeApiClient> _logger;
    private readonly BoeApiOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public BoeApiClient(HttpClient httpClient, ILogger<BoeApiClient> logger, IOptions<BoeApiOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", _options.UserAgent);
    }

    public async Task<List<BoeLegislationModel>> SearchConsolidatedLegislationAsync(
        string query, int limit = 10, int offset = 0, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching consolidated legislation: {Query}, Limit: {Limit}, Offset: {Offset}", 
                query, limit, offset);

            var url = $"/legislacion/consolidada?q={Uri.EscapeDataString(query)}&limit={limit}&offset={offset}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BOE API returned {StatusCode} for legislation search: {Query}", 
                    response.StatusCode, query);
                return new List<BoeLegislationModel>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<BoeApiLegislationResponse>(content, _jsonOptions);
            
            return apiResponse?.Results ?? new List<BoeLegislationModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching consolidated legislation: {Query}", query);
            throw;
        }
    }

    public async Task<BoeLegislationModel?> GetConsolidatedLawAsync(
        string lawId, bool includeMetadata = true, bool includeAnalysis = false, 
        bool includeFullText = false, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting consolidated law: {LawId}", lawId);

            var url = $"/legislacion/consolidada/{lawId}?metadata={includeMetadata}&analysis={includeAnalysis}&fulltext={includeFullText}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BOE API returned {StatusCode} for law: {LawId}", response.StatusCode, lawId);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<BoeLegislationModel>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consolidated law: {LawId}", lawId);
            throw;
        }
    }

    public async Task<BoeStructureModel?> GetLawStructureAsync(string lawId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting law structure: {LawId}", lawId);

            var url = $"/legislacion/consolidada/{lawId}/estructura";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BOE API returned {StatusCode} for law structure: {LawId}", response.StatusCode, lawId);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<BoeStructureModel>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting law structure: {LawId}", lawId);
            throw;
        }
    }

    public async Task<List<BoeSummaryModel>> GetBoeSummaryAsync(string date, int maxItems = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting BOE summary for date: {Date}, MaxItems: {MaxItems}", date, maxItems);

            var url = $"/sumario/boe/{date}?limit={maxItems}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BOE API returned {StatusCode} for BOE summary: {Date}", response.StatusCode, date);
                return new List<BoeSummaryModel>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<BoeSummaryApiResponse>(content, _jsonOptions);
            
            return apiResponse?.Items ?? new List<BoeSummaryModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting BOE summary for date: {Date}", date);
            throw;
        }
    }

    public async Task<List<BoeSummaryModel>> GetBormeSummaryAsync(string date, int maxItems = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting BORME summary for date: {Date}, MaxItems: {MaxItems}", date, maxItems);

            var url = $"/sumario/borme/{date}?limit={maxItems}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BOE API returned {StatusCode} for BORME summary: {Date}", response.StatusCode, date);
                return new List<BoeSummaryModel>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<BoeSummaryApiResponse>(content, _jsonOptions);
            
            return apiResponse?.Items ?? new List<BoeSummaryModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting BORME summary for date: {Date}", date);
            throw;
        }
    }

    public async Task<List<BoeSummaryModel>> SearchRecentBoeAsync(int daysBack, List<string> searchTerms, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching recent BOE: DaysBack: {DaysBack}, Terms: {SearchTerms}", 
                daysBack, string.Join(", ", searchTerms));

            var terms = string.Join(",", searchTerms.Select(Uri.EscapeDataString));
            var url = $"/buscar/reciente?dias={daysBack}&terminos={terms}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BOE API returned {StatusCode} for recent search", response.StatusCode);
                return new List<BoeSummaryModel>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<BoeSummaryApiResponse>(content, _jsonOptions);
            
            return apiResponse?.Items ?? new List<BoeSummaryModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching recent BOE: DaysBack: {DaysBack}", daysBack);
            throw;
        }
    }

    public async Task<List<BoeAuxiliaryModel>> GetDepartmentsTableAsync(string searchTerm = "", int limit = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting departments table: SearchTerm: {SearchTerm}, Limit: {Limit}", searchTerm, limit);

            var url = $"/tablas/departamentos?q={Uri.EscapeDataString(searchTerm)}&limit={limit}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BOE API returned {StatusCode} for departments table", response.StatusCode);
                return new List<BoeAuxiliaryModel>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<BoeAuxiliaryApiResponse>(content, _jsonOptions);
            
            return apiResponse?.Items ?? new List<BoeAuxiliaryModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments table: {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<List<BoeAuxiliaryModel>> GetLegalRangesTableAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting legal ranges table: Limit: {Limit}", limit);

            var url = $"/tablas/rangos?limit={limit}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BOE API returned {StatusCode} for legal ranges table", response.StatusCode);
                return new List<BoeAuxiliaryModel>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<BoeAuxiliaryApiResponse>(content, _jsonOptions);
            
            return apiResponse?.Items ?? new List<BoeAuxiliaryModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting legal ranges table");
            throw;
        }
    }

    public async Task<BoeAuxiliaryModel?> GetCodeDescriptionAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting code description: {Code}", code);

            var url = $"/codigo/{code}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BOE API returned {StatusCode} for code: {Code}", response.StatusCode, code);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<BoeAuxiliaryModel>(content, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting code description: {Code}", code);
            throw;
        }
    }

    public async Task<List<BoeAuxiliaryModel>> SearchAuxiliaryDataAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Searching auxiliary data: {Query}", query);

            var url = $"/tablas/buscar?q={Uri.EscapeDataString(query)}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BOE API returned {StatusCode} for auxiliary search: {Query}", response.StatusCode, query);
                return new List<BoeAuxiliaryModel>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<BoeAuxiliaryApiResponse>(content, _jsonOptions);
            
            return apiResponse?.Items ?? new List<BoeAuxiliaryModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching auxiliary data: {Query}", query);
            throw;
        }
    }

    // Internal API response models
    private record BoeApiLegislationResponse
    {
        [JsonPropertyName("resultados")]
        public List<BoeLegislationModel> Results { get; init; } = new();
    }

    private record BoeSummaryApiResponse
    {
        [JsonPropertyName("items")]
        public List<BoeSummaryModel> Items { get; init; } = new();
    }

    private record BoeAuxiliaryApiResponse
    {
        [JsonPropertyName("items")]
        public List<BoeAuxiliaryModel> Items { get; init; } = new();
    }
}
