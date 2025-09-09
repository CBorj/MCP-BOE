using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MCPBoe.Core.Models;
using MCPBoe.Core.Services;

namespace MCPBoe.FunctionApp.Functions;

/// <summary>
/// Azure Functions for auxiliary operations (departments, legal ranges, codes)
/// </summary>
public class AuxiliaryFunction
{
    private readonly IAuxiliaryService _auxiliaryService;
    private readonly ILogger<AuxiliaryFunction> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuxiliaryFunction(IAuxiliaryService auxiliaryService, ILogger<AuxiliaryFunction> logger)
    {
        _auxiliaryService = auxiliaryService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Get list of departments
    /// </summary>
    [Function("GetDepartments")]
    public async Task<HttpResponseData> GetDepartments(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "auxiliary/departments")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing get departments request");

            var response = await _auxiliaryService.GetDepartmentsAsync(cancellationToken);
            var apiResponse = ApiResponse<DepartmentsResponse>.Ok(response);

            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get departments request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    /// <summary>
    /// Get legal ranges for a specific date
    /// </summary>
    [Function("GetLegalRanges")]
    public async Task<HttpResponseData> GetLegalRanges(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "auxiliary/legal-ranges")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing get legal ranges request");

            // Parse request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var request = JsonSerializer.Deserialize<GetLegalRangesRequest>(requestBody, _jsonOptions);

            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Validate request
            var validationErrors = ValidateLegalRangesRequest(request);
            if (validationErrors.Any())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                    $"Validation errors: {string.Join(", ", validationErrors)}");
            }

            // Process request
            var response = await _auxiliaryService.GetLegalRangesAsync(request, cancellationToken);
            var apiResponse = ApiResponse<LegalRangesResponse>.Ok(response);

            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get legal ranges request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    /// <summary>
    /// Get descriptions for specific codes
    /// </summary>
    [Function("GetCodeDescriptions")]
    public async Task<HttpResponseData> GetCodeDescriptions(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "auxiliary/codes")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing get code descriptions request");

            // Parse request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);
            var request = JsonSerializer.Deserialize<GetCodeDescriptionsRequest>(requestBody, _jsonOptions);

            if (request == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");
            }

            // Validate request
            var validationErrors = ValidateCodeDescriptionsRequest(request);
            if (validationErrors.Any())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 
                    $"Validation errors: {string.Join(", ", validationErrors)}");
            }

            // Process request
            var response = await _auxiliaryService.GetCodeDescriptionsAsync(request, cancellationToken);
            var apiResponse = ApiResponse<CodeDescriptionsResponse>.Ok(response);

            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing get code descriptions request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [Function("HealthCheck")]
    public async Task<HttpResponseData> HealthCheck(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("Health check requested");

            var health = new
            {
                Status = "Healthy",
                Timestamp = DateTimeOffset.UtcNow,
                Version = typeof(AuxiliaryFunction).Assembly.GetName().Version?.ToString() ?? "Unknown",
                Environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Unknown"
            };

            var apiResponse = ApiResponse<object>.Ok(health);
            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in health check");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Health check failed");
        }
    }

    /// <summary>
    /// Get API information and available endpoints
    /// </summary>
    [Function("GetApiInfo")]
    public async Task<HttpResponseData> GetApiInfo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "info")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("API info requested");

            var apiInfo = new
            {
                Name = "MCPBoe Function App",
                Description = "Azure Functions API for Spanish BOE (Boletín Oficial del Estado) operations",
                Version = typeof(AuxiliaryFunction).Assembly.GetName().Version?.ToString() ?? "1.0.0",
                Endpoints = new
                {
                    Legislation = new[]
                    {
                        "POST /legislation/search - Search legislation",
                        "GET /legislation/{lawId} - Get specific law",
                        "GET /legislation/{lawId}/structure - Get law structure"
                    },
                    Summary = new[]
                    {
                        "POST /summary/boe - Get BOE summary for date",
                        "POST /summary/borme - Get BORME summary for date",
                        "POST /summary/search - Search recent BOE publications"
                    },
                    Auxiliary = new[]
                    {
                        "GET /auxiliary/departments - Get departments list",
                        "POST /auxiliary/legal-ranges - Get legal ranges for date",
                        "POST /auxiliary/codes - Get code descriptions"
                    },
                    System = new[]
                    {
                        "GET /health - Health check",
                        "GET /info - API information"
                    }
                },
                Documentation = "https://github.com/your-org/mcpboe-function-app"
            };

            var apiResponse = ApiResponse<object>.Ok(apiInfo);
            return await CreateSuccessResponse(req, apiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting API info");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Failed to get API info");
        }
    }

    private List<string> ValidateLegalRangesRequest(GetLegalRangesRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Date))
            errors.Add("Date is required");
        else if (!System.Text.RegularExpressions.Regex.IsMatch(request.Date, @"^\d{8}$"))
            errors.Add("Date must be in YYYYMMDD format");

        return errors;
    }

    private List<string> ValidateCodeDescriptionsRequest(GetCodeDescriptionsRequest request)
    {
        var errors = new List<string>();

        if (request.Codes == null || !request.Codes.Any())
            errors.Add("At least one code is required");

        if (request.Codes?.Any(code => string.IsNullOrWhiteSpace(code)) == true)
            errors.Add("All codes must be non-empty");

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
