using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MCPBoe.Core.Models;
using MCPBoe.Core.Services;

namespace MCPBoe.FunctionApp.Functions;

/// <summary>
/// Azure Functions for legislation operations
/// </summary>
public class LegislationFunction
{
    private readonly ILegislationService _legislationService;
    private readonly ILogger<LegislationFunction> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public LegislationFunction(ILegislationService legislationService, ILogger<LegislationFunction> logger)
    {
        _legislationService = legislationService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Search consolidated legislation
    /// </summary>
    [Function("SearchLegislation")]
    public async Task<HttpResponseData> SearchLegislation(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "legislation/search")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing search legislation request");

            // Parse request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var request = JsonSerializer.Deserialize<SearchLegislationRequest>(requestBody, _jsonOptions);

            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Validate request
            var validationErrors = ValidateSearchRequest(request);
            if (validationErrors.Any())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                    $"Validation errors: {string.Join(", ", validationErrors)}");
            }

            // Process request
            var response = await _legislationService.SearchConsolidatedLegislationAsync(request, cancellationToken);
            var apiResponse = ApiResponse<SearchLegislationResponse>.Ok(response);

            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing search legislation request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    /// <summary>
    /// Get consolidated law by ID
    /// </summary>
    [Function("GetLaw")]
    public async Task<HttpResponseData> GetLaw(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "legislation/{lawId}")] HttpRequestData req,
        string lawId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing get law request for ID: {LawId}", lawId);

            if (string.IsNullOrWhiteSpace(lawId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Law ID is required");
            }

            // Parse query parameters
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var request = new GetLawRequest
            {
                LawId = lawId,
                IncludeMetadata = bool.TryParse(query["includeMetadata"], out var includeMetadata) && includeMetadata,
                IncludeAnalysis = bool.TryParse(query["includeAnalysis"], out var includeAnalysis) && includeAnalysis,
                IncludeFullText = bool.TryParse(query["includeFullText"], out var includeFullText) && includeFullText
            };

            // Process request
            var response = await _legislationService.GetConsolidatedLawAsync(request, cancellationToken);
            
            if (response.Law == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, $"Law not found: {lawId}");
            }

            var apiResponse = ApiResponse<GetLawResponse>.Ok(response);
            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get law request for ID: {LawId}", lawId);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    /// <summary>
    /// Get law structure by ID
    /// </summary>
    [Function("GetLawStructure")]
    public async Task<HttpResponseData> GetLawStructure(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "legislation/{lawId}/structure")] HttpRequestData req,
        string lawId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing get law structure request for ID: {LawId}", lawId);

            if (string.IsNullOrWhiteSpace(lawId))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Law ID is required");
            }

            var request = new GetLawStructureRequest { LawId = lawId };

            // Process request
            var response = await _legislationService.GetLawStructureAsync(request, cancellationToken);
            
            if (response.Structure == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, $"Law structure not found: {lawId}");
            }

            var apiResponse = ApiResponse<GetLawStructureResponse>.Ok(response);
            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get law structure request for ID: {LawId}", lawId);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private List<string> ValidateSearchRequest(SearchLegislationRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Query))
            errors.Add("Query is required");

        if (request.Query?.Length > 500)
            errors.Add("Query cannot exceed 500 characters");

        if (request.Limit < 1 || request.Limit > 100)
            errors.Add("Limit must be between 1 and 100");

        if (request.Offset < 0)
            errors.Add("Offset must be non-negative");

        return errors;
    }

    private async Task<HttpResponseData> CreateSuccessResponse<T>(HttpRequestData req, T data)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await response.WriteStringAsync(json);
        
        return response;
    }

    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        
        var errorResponse = ApiResponse<object>.Fail(message);
        var json = JsonSerializer.Serialize(errorResponse, _jsonOptions);
        await response.WriteStringAsync(json);
        
        return response;
    }
}
