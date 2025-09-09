using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MCPBoe.Core.Models;
using MCPBoe.Core.Services;

namespace MCPBoe.FunctionApp.Functions;

/// <summary>
/// Azure Functions for summary operations
/// </summary>
public class SummaryFunction
{
    private readonly ISummaryService _summaryService;
    private readonly ILogger<SummaryFunction> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public SummaryFunction(ISummaryService summaryService, ILogger<SummaryFunction> logger)
    {
        _summaryService = summaryService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Get BOE summary for a specific date
    /// </summary>
    [Function("GetBoeSummary")]
    public async Task<HttpResponseData> GetBoeSummary(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "summary/boe")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing BOE summary request");

            // Parse request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var request = JsonSerializer.Deserialize<GetBoeSummaryRequest>(requestBody, _jsonOptions);

            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Validate request
            var validationErrors = ValidateBoeSummaryRequest(request);
            if (validationErrors.Any())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                    $"Validation errors: {string.Join(", ", validationErrors)}");
            }

            // Process request
            var response = await _summaryService.GetBoeSummaryAsync(request, cancellationToken);
            var apiResponse = ApiResponse<SummaryResponse>.Ok(response);

            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing BOE summary request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    /// <summary>
    /// Get BORME summary for a specific date
    /// </summary>
    [Function("GetBormeSummary")]
    public async Task<HttpResponseData> GetBormeSummary(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "summary/borme")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing BORME summary request");

            // Parse request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var request = JsonSerializer.Deserialize<GetBormeSummaryRequest>(requestBody, _jsonOptions);

            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Validate request
            var validationErrors = ValidateBormeSummaryRequest(request);
            if (validationErrors.Any())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                    $"Validation errors: {string.Join(", ", validationErrors)}");
            }

            // Process request
            var response = await _summaryService.GetBormeSummaryAsync(request, cancellationToken);
            var apiResponse = ApiResponse<SummaryResponse>.Ok(response);

            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing BORME summary request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    /// <summary>
    /// Search recent BOE publications
    /// </summary>
    [Function("SearchRecentBoe")]
    public async Task<HttpResponseData> SearchRecentBoe(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "summary/search")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing recent BOE search request");

            // Parse request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var request = JsonSerializer.Deserialize<SearchRecentBoeRequest>(requestBody, _jsonOptions);

            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Validate request
            var validationErrors = ValidateSearchRecentBoeRequest(request);
            if (validationErrors.Any())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                    $"Validation errors: {string.Join(", ", validationErrors)}");
            }

            // Process request
            var response = await _summaryService.SearchRecentBoeAsync(request, cancellationToken);
            var apiResponse = ApiResponse<SearchRecentBoeResponse>.Ok(response);

            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing recent BOE search request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private List<string> ValidateBoeSummaryRequest(GetBoeSummaryRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Date))
            errors.Add("Date is required");
        else if (!System.Text.RegularExpressions.Regex.IsMatch(request.Date, @"^\d{8}$"))
            errors.Add("Date must be in YYYYMMDD format");

        if (request.MaxItems < 1 || request.MaxItems > 1000)
            errors.Add("Max items must be between 1 and 1000");

        return errors;
    }

    private List<string> ValidateBormeSummaryRequest(GetBormeSummaryRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Date))
            errors.Add("Date is required");
        else if (!System.Text.RegularExpressions.Regex.IsMatch(request.Date, @"^\d{8}$"))
            errors.Add("Date must be in YYYYMMDD format");

        if (request.MaxItems < 1 || request.MaxItems > 1000)
            errors.Add("Max items must be between 1 and 1000");

        return errors;
    }

    private List<string> ValidateSearchRecentBoeRequest(SearchRecentBoeRequest request)
    {
        var errors = new List<string>();

        if (request.DaysBack < 1 || request.DaysBack > 30)
            errors.Add("Days back must be between 1 and 30");

        if (request.SearchTerms == null || !request.SearchTerms.Any())
            errors.Add("At least one search term is required");

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
