using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;

namespace tennis_wave_api.Controllers;

/// <summary>
/// Controller for handling JotForm webhook callbacks.
/// Receives form submission notifications and processes them asynchronously.
/// </summary>
[ApiController]
[Route("api/sight-webhook")]
public class SightWebhookController : ControllerBase
{
    private readonly ILogger<SightWebhookController> _logger;

    public SightWebhookController(ILogger<SightWebhookController> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Handles Sight Form webhook POST requests.
    /// Returns 200 immediately and processes the request asynchronously.
    /// </summary>
    /// <returns>Success response indicating the webhook was received</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public IActionResult SightCallback()
    {
        _logger.LogInformation("收到 webhook 请求: Method={Method}, Path={Path}, QueryString={QueryString}",
            Request.Method,
            Request.Path,
            Request.QueryString);

        // Read form data before returning response (to avoid Request disposal)
        string? rawRequestJson = null;
        try
        {
            // Check if request has form content
            if (Request.HasFormContentType)
            {
                // Extract rawRequest field from form
                if (Request.Form.TryGetValue("rawRequest", out var rawRequestValues) && 
                    rawRequestValues.Count > 0 && 
                    !string.IsNullOrWhiteSpace(rawRequestValues[0]))
                {
                    rawRequestJson = rawRequestValues[0];
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取表单数据时发生错误");
        }

        // Immediately return success response
        var response = new
        {
            success = true,
            timestamp = DateTime.UtcNow
        };

        // Start asynchronous processing after response is sent
        if (!string.IsNullOrWhiteSpace(rawRequestJson))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessWebhookAsync(rawRequestJson);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理 webhook 数据时发生错误");
                }
            });
        }
        else
        {
            _logger.LogWarning("rawRequest 字段缺失，跳过异步处理");
        }

        return Ok(ApiResponseHelper.Success(response, "Webhook received successfully"));
    }

    /// <summary>
    /// Process webhook data asynchronously.
    /// Parses JSON from rawRequest field.
    /// </summary>
    /// <param name="rawRequestJson">JSON string from rawRequest field</param>
    private async Task ProcessWebhookAsync(string rawRequestJson)
    {
        try
        {
            _logger.LogInformation("开始处理 webhook 数据，rawRequest 长度: {Length}", rawRequestJson.Length);

            // Parse JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsedRequest = JsonSerializer.Deserialize<WebhookRawRequestDto>(rawRequestJson, options);
            
            if (parsedRequest == null)
            {
                _logger.LogError("解析 rawRequest JSON 失败，返回 null");
                return;
            }

            _logger.LogInformation("解析的请求数据: TagNumber={TagNumber}, HasDate={HasDate}, HasAddress={HasAddress}, HasEmail={HasEmail}",
                parsedRequest.TagNumber,
                parsedRequest.Date != null,
                !string.IsNullOrWhiteSpace(parsedRequest.Address),
                !string.IsNullOrWhiteSpace(parsedRequest.Email));

            // TODO: Add to processing queue (will be implemented in step 5)
            // For now, just log the parsed data
            _logger.LogInformation("Webhook 数据解析成功，等待后续处理");
            
            await Task.CompletedTask;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "解析 rawRequest JSON 时发生错误: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 webhook 数据时发生未预期的错误: {Message}", ex.Message);
        }
    }
    
    /// <summary>
    /// Test endpoint 1: Receive using IFormCollection parameter
    /// Route: POST /api/sight-webhook/test-form-collection
    /// </summary>
    [HttpPost("test-form-collection")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    public IActionResult TestFormCollection([FromForm] IFormCollection form)
    {
        _logger.LogInformation("=== Test Endpoint 1: IFormCollection ===");
        _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
        _logger.LogInformation("Form Count: {Count}", form?.Count ?? 0);

        var result = new Dictionary<string, object?>
        {
            ["method"] = "IFormCollection",
            ["contentType"] = Request.ContentType,
            ["formFields"] = new Dictionary<string, string>()
        };

        if (form != null)
        {
            foreach (var field in form)
            {
                var value = field.Value.ToString();
                _logger.LogInformation("Form Field: {Key} = {Value}", field.Key, value);
                ((Dictionary<string, string>)result["formFields"]!)[field.Key] = value ?? string.Empty;
            }

            // Check for rawRequest specifically
            if (form.TryGetValue("rawRequest", out var rawRequestValues))
            {
                var rawRequest = rawRequestValues.ToString();
                _logger.LogInformation("Found rawRequest: {Length} characters", rawRequest?.Length ?? 0);
                result["rawRequest"] = rawRequest;
            }
        }

        return Ok(ApiResponseHelper.Success(result, "Test endpoint 1 - IFormCollection received"));
    }
    
    /// <summary>
    /// Test endpoint 2: Receive using Request.Form directly
    /// Route: POST /api/sight-webhook/test-request-form
    /// </summary>
    [HttpPost("test-request-form")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    public IActionResult TestRequestForm()
    {
        _logger.LogInformation("=== Test Endpoint 2: Request.Form ===");
        _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
        _logger.LogInformation("HasFormContentType: {HasForm}", Request.HasFormContentType);

        var result = new Dictionary<string, object?>
        {
            ["method"] = "Request.Form",
            ["contentType"] = Request.ContentType,
            ["hasFormContentType"] = Request.HasFormContentType,
            ["formFields"] = new Dictionary<string, string>()
        };

        if (Request.HasFormContentType && Request.Form != null)
        {
            _logger.LogInformation("Form Count: {Count}", Request.Form.Count);

            foreach (var field in Request.Form)
            {
                var value = field.Value.ToString();
                _logger.LogInformation("Form Field: {Key} = {Value}", field.Key, value);
                ((Dictionary<string, string>)result["formFields"]!)[field.Key] = value ?? string.Empty;
            }

            // Check for rawRequest specifically
            if (Request.Form.TryGetValue("rawRequest", out var rawRequestValues))
            {
                var rawRequest = rawRequestValues.ToString();
                _logger.LogInformation("Found rawRequest: {Length} characters", rawRequest?.Length ?? 0);
                result["rawRequest"] = rawRequest;
            }
        }

        return Ok(ApiResponseHelper.Success(result, "Test endpoint 2 - Request.Form received"));
    }

    /// <summary>
    /// Test endpoint 3: Receive using [FromForm] with specific field name
    /// Route: POST /api/sight-webhook/test-from-form-field
    /// </summary>
    [HttpPost("test-from-form-field")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    public IActionResult TestFromFormField([FromForm(Name = "rawRequest")] string? rawRequest)
    {
        _logger.LogInformation("=== Test Endpoint 3: [FromForm] with field name ===");
        _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
        _logger.LogInformation("rawRequest received: {HasValue}, Length: {Length}", 
            !string.IsNullOrWhiteSpace(rawRequest), 
            rawRequest?.Length ?? 0);

        if (!string.IsNullOrWhiteSpace(rawRequest))
        {
            _logger.LogInformation("rawRequest content: {Content}", rawRequest);
        }

        var result = new Dictionary<string, object?>
        {
            ["method"] = "[FromForm] with field name",
            ["contentType"] = Request.ContentType,
            ["rawRequest"] = rawRequest ?? "null or empty",
            ["rawRequestLength"] = rawRequest?.Length ?? 0
        };

        return Ok(ApiResponseHelper.Success(result, "Test endpoint 3 - [FromForm] field received"));
    }

    /// <summary>
    /// Test endpoint 4: Read raw request body as stream
    /// Route: POST /api/sight-webhook/test-raw-body
    /// </summary>
    [HttpPost("test-raw-body")]
    public async Task<IActionResult> TestRawBody()
    {
        _logger.LogInformation("=== Test Endpoint 4: Raw Request Body ===");
        _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
        _logger.LogInformation("Content-Length: {Length}", Request.ContentLength);

        var result = new Dictionary<string, object?>
        {
            ["method"] = "Raw Request Body",
            ["contentType"] = Request.ContentType,
            ["contentLength"] = Request.ContentLength
        };

        try
        {
            Request.EnableBuffering();
            Request.Body.Position = 0;

            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var bodyContent = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            _logger.LogInformation("Raw body content length: {Length}", bodyContent.Length);
            _logger.LogInformation("Raw body content (first 500 chars): {Content}", 
                bodyContent.Length > 500 ? bodyContent.Substring(0, 500) + "..." : bodyContent);

            result["bodyLength"] = bodyContent.Length;
            result["bodyPreview"] = bodyContent.Length > 500 
                ? bodyContent.Substring(0, 500) + "..." 
                : bodyContent;
            result["fullBody"] = bodyContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading raw body");
            result["error"] = ex.Message;
        }

        return Ok(ApiResponseHelper.Success(result, "Test endpoint 4 - Raw body received"));
    }

    /// <summary>
    /// Test endpoint 5: Read all headers and form data
    /// Route: POST /api/sight-webhook/test-all-data
    /// </summary>
    [HttpPost("test-all-data")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded", "application/json")]
    public async Task<IActionResult> TestAllData()
    {
        _logger.LogInformation("=== Test Endpoint 5: All Request Data ===");

        var result = new Dictionary<string, object?>
        {
            ["method"] = "All Request Data",
            ["headers"] = new Dictionary<string, string>(),
            ["query"] = new Dictionary<string, string>(),
            ["formFields"] = new Dictionary<string, string>(),
            ["rawBody"] = string.Empty
        };

        // Log all headers
        _logger.LogInformation("--- Headers ---");
        foreach (var header in Request.Headers)
        {
            var headerValue = header.Value.ToString();
            _logger.LogInformation("Header: {Key} = {Value}", header.Key, headerValue);
            ((Dictionary<string, string>)result["headers"]!)[header.Key] = headerValue;
        }

        // Log query string
        _logger.LogInformation("--- Query String ---");
        foreach (var query in Request.Query)
        {
            _logger.LogInformation("Query: {Key} = {Value}", query.Key, query.Value.ToString());
            ((Dictionary<string, string>)result["query"]!)[query.Key] = query.Value.ToString();
        }

        // Log form data
        if (Request.HasFormContentType && Request.Form != null)
        {
            _logger.LogInformation("--- Form Data ---");
            foreach (var field in Request.Form)
            {
                var value = field.Value.ToString();
                _logger.LogInformation("Form Field: {Key} = {Value}", field.Key, value);
                ((Dictionary<string, string>)result["formFields"]!)[field.Key] = value ?? string.Empty;
            }
        }

        // Try to read body
        try
        {
            Request.EnableBuffering();
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var bodyContent = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
            
            _logger.LogInformation("--- Raw Body ---");
            _logger.LogInformation("Body length: {Length}", bodyContent.Length);
            _logger.LogInformation("Body content: {Content}", bodyContent);
            
            result["rawBody"] = bodyContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading body");
            result["bodyError"] = ex.Message;
        }

        return Ok(ApiResponseHelper.Success(result, "Test endpoint 5 - All data received"));
    }

    /// <summary>
    /// Test endpoint 6: Receive as JSON (if JotForm sends JSON)
    /// Route: POST /api/sight-webhook/test-json
    /// </summary>
    [HttpPost("test-json")]
    [Consumes("application/json")]
    public async Task<IActionResult> TestJson()
    {
        _logger.LogInformation("=== Test Endpoint 6: JSON Body ===");
        _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);

        var result = new Dictionary<string, object?>
        {
            ["method"] = "JSON Body",
            ["contentType"] = Request.ContentType
        };

        try
        {
            Request.EnableBuffering();
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            var jsonContent = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            _logger.LogInformation("JSON content length: {Length}", jsonContent.Length);
            _logger.LogInformation("JSON content: {Content}", jsonContent);

            result["jsonLength"] = jsonContent.Length;
            result["jsonContent"] = jsonContent;

            // Try to parse as JSON
            try
            {
                var jsonDoc = JsonDocument.Parse(jsonContent);
                result["isValidJson"] = true;
                result["parsedJson"] = JsonSerializer.Serialize(jsonDoc);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Content is not valid JSON");
                result["isValidJson"] = false;
                result["parseError"] = ex.Message;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading JSON body");
            result["error"] = ex.Message;
        }

        return Ok(ApiResponseHelper.Success(result, "Test endpoint 6 - JSON received"));
    }
}
