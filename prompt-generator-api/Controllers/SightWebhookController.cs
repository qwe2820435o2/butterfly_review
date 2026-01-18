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
        Console.WriteLine("=== Test Endpoint 1: IFormCollection ===");
        Console.WriteLine($"Content-Type: {Request.ContentType}");
        Console.WriteLine($"Form Count: {form?.Count ?? 0}");

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
                Console.WriteLine($"Form Field: {field.Key} = {value}");
                _logger.LogInformation("Form Field: {Key} = {Value}", field.Key, value);
                ((Dictionary<string, string>)result["formFields"]!)[field.Key] = value ?? string.Empty;
            }

            // Check for rawRequest specifically
            if (form.TryGetValue("rawRequest", out var rawRequestValues))
            {
                var rawRequest = rawRequestValues.ToString();
                Console.WriteLine("=== Found rawRequest ===");
                Console.WriteLine($"rawRequest Length: {rawRequest?.Length ?? 0} characters");
                
                _logger.LogInformation("=== Found rawRequest ===");
                _logger.LogInformation("rawRequest Length: {Length} characters", rawRequest?.Length ?? 0);
                
                if (!string.IsNullOrWhiteSpace(rawRequest))
                {
                    // Print full rawRequest content
                    Console.WriteLine("=== rawRequest Full Content ===");
                    Console.WriteLine(rawRequest);
                    Console.WriteLine("=== End of rawRequest ===");

                    _logger.LogInformation("=== rawRequest Full Content ===");
                    _logger.LogInformation("{Content}", rawRequest);
                    _logger.LogInformation("=== End of rawRequest ===");
                    
                    // Try to parse as JSON and log structure
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(rawRequest);
                        Console.WriteLine("=== rawRequest is valid JSON ===");
                        Console.WriteLine($"JSON Root Element Type: {jsonDoc.RootElement.ValueKind}");
                        
                        _logger.LogInformation("=== rawRequest is valid JSON ===");
                        _logger.LogInformation("JSON Root Element Type: {Type}", jsonDoc.RootElement.ValueKind);
                        
                        // Log all top-level keys
                        if (jsonDoc.RootElement.ValueKind == JsonValueKind.Object)
                        {
                            Console.WriteLine("=== JSON Top-Level Keys ===");
                            _logger.LogInformation("=== JSON Top-Level Keys ===");
                            foreach (var prop in jsonDoc.RootElement.EnumerateObject())
                            {
                                var propValue = prop.Value.ValueKind == JsonValueKind.String 
                                    ? prop.Value.GetString() 
                                    : prop.Value.ToString();
                                
                                Console.WriteLine($"Key: {prop.Name}, Value Type: {prop.Value.ValueKind}, Value: {propValue}");
                                _logger.LogInformation("Key: {Key}, Value Type: {Type}, Value: {Value}", 
                                    prop.Name, 
                                    prop.Value.ValueKind,
                                    propValue);
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"rawRequest is not valid JSON: {ex.Message}");
                        _logger.LogWarning(ex, "rawRequest is not valid JSON: {Message}", ex.Message);
                    }
                }
                
                result["rawRequest"] = rawRequest;
                result["rawRequestLength"] = rawRequest?.Length ?? 0;
            }
            else
            {
                Console.WriteLine("rawRequest field NOT FOUND in form data!");
                _logger.LogWarning("rawRequest field NOT FOUND in form data!");
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
                _logger.LogInformation("=== Found rawRequest ===");
                _logger.LogInformation("rawRequest Length: {Length} characters", rawRequest?.Length ?? 0);
                
                if (!string.IsNullOrWhiteSpace(rawRequest))
                {
                    // Print full rawRequest content
                    _logger.LogInformation("=== rawRequest Full Content ===");
                    _logger.LogInformation("{Content}", rawRequest);
                    _logger.LogInformation("=== End of rawRequest ===");
                    
                    // Try to parse as JSON
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(rawRequest);
                        _logger.LogInformation("rawRequest is valid JSON, Root Type: {Type}", jsonDoc.RootElement.ValueKind);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "rawRequest is not valid JSON: {Message}", ex.Message);
                    }
                }
                
                result["rawRequest"] = rawRequest;
                result["rawRequestLength"] = rawRequest?.Length ?? 0;
            }
            else
            {
                _logger.LogWarning("rawRequest field NOT FOUND in Request.Form!");
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
            _logger.LogInformation("=== rawRequest Full Content ===");
            _logger.LogInformation("{Content}", rawRequest);
            _logger.LogInformation("=== End of rawRequest ===");
            
            // Try to parse as JSON
            try
            {
                var jsonDoc = JsonDocument.Parse(rawRequest);
                _logger.LogInformation("rawRequest is valid JSON, Root Type: {Type}", jsonDoc.RootElement.ValueKind);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "rawRequest is not valid JSON: {Message}", ex.Message);
            }
        }
        else
        {
            _logger.LogWarning("rawRequest is null or empty!");
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
            _logger.LogInformation("Form Field Count: {Count}", Request.Form.Count);
            
            foreach (var field in Request.Form)
            {
                var value = field.Value.ToString();
                
                // Special handling for rawRequest - print full content
                if (field.Key == "rawRequest" && !string.IsNullOrWhiteSpace(value))
                {
                    _logger.LogInformation("=== Form Field: {Key} (Length: {Length}) ===", field.Key, value.Length);
                    _logger.LogInformation("Full Content: {Content}", value);
                    _logger.LogInformation("=== End of {Key} ===", field.Key);
                    
                    // Try to parse as JSON
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(value);
                        _logger.LogInformation("rawRequest is valid JSON, Root Type: {Type}", jsonDoc.RootElement.ValueKind);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "rawRequest is not valid JSON: {Message}", ex.Message);
                    }
                }
                else
                {
                    _logger.LogInformation("Form Field: {Key} = {Value}", field.Key, value);
                }
                
                ((Dictionary<string, string>)result["formFields"]!)[field.Key] = value ?? string.Empty;
            }
            
            // Check specifically for rawRequest
            if (!Request.Form.ContainsKey("rawRequest"))
            {
                _logger.LogWarning("rawRequest field NOT FOUND in Request.Form!");
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
    /// Debug endpoint: Print rawRequest content in detail
    /// Route: POST /api/sight-webhook/debug-raw-request
    /// </summary>
    [HttpPost("debug-raw-request")]
    [Consumes("multipart/form-data", "application/x-www-form-urlencoded")]
    public IActionResult DebugRawRequest()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("=== DEBUG: rawRequest Endpoint ===");
        Console.WriteLine("========================================");
        Console.WriteLine($"Content-Type: {Request.ContentType}");
        Console.WriteLine($"HasFormContentType: {Request.HasFormContentType}");
        Console.WriteLine($"Form Count: {Request.Form?.Count ?? 0}");
        Console.WriteLine("");

        _logger.LogInformation("========================================");
        _logger.LogInformation("=== DEBUG: rawRequest Endpoint ===");
        _logger.LogInformation("========================================");
        _logger.LogInformation("Content-Type: {ContentType}", Request.ContentType);
        _logger.LogInformation("HasFormContentType: {HasForm}", Request.HasFormContentType);
        _logger.LogInformation("Form Count: {Count}", Request.Form?.Count ?? 0);
        _logger.LogInformation("");

        var result = new Dictionary<string, object?>
        {
            ["endpoint"] = "debug-raw-request",
            ["contentType"] = Request.ContentType,
            ["hasFormContentType"] = Request.HasFormContentType,
            ["formFieldCount"] = Request.Form?.Count ?? 0,
            ["allFormFields"] = new Dictionary<string, string>(),
            ["rawRequest"] = new Dictionary<string, object?>()
        };

        // List all form fields
        if (Request.HasFormContentType && Request.Form != null)
        {
            Console.WriteLine("--- All Form Fields ---");
            _logger.LogInformation("--- All Form Fields ---");
            foreach (var field in Request.Form)
            {
                var value = field.Value.ToString();
                var displayValue = value?.Length > 100 ? value.Substring(0, 100) + "..." : value;
                Console.WriteLine($"  [{field.Key}] = {displayValue}");
                _logger.LogInformation("  [{Key}] = {Value}", field.Key, displayValue);
                ((Dictionary<string, string>)result["allFormFields"]!)[field.Key] = value ?? string.Empty;
            }
            Console.WriteLine("");
            _logger.LogInformation("");
        }

        // Extract and analyze rawRequest
        if (Request.HasFormContentType && Request.Form != null && Request.Form.TryGetValue("rawRequest", out var rawRequestValues))
        {
            var rawRequest = rawRequestValues.ToString();
            
            Console.WriteLine("========================================");
            Console.WriteLine("=== rawRequest Found ===");
            Console.WriteLine("========================================");
            Console.WriteLine($"Length: {rawRequest?.Length ?? 0} characters");
            Console.WriteLine("");

            _logger.LogInformation("========================================");
            _logger.LogInformation("=== rawRequest Found ===");
            _logger.LogInformation("========================================");
            _logger.LogInformation("Length: {Length} characters", rawRequest?.Length ?? 0);
            _logger.LogInformation("");

            if (!string.IsNullOrWhiteSpace(rawRequest))
            {
                // Print full content
                Console.WriteLine("--- Full rawRequest Content ---");
                Console.WriteLine(rawRequest);
                Console.WriteLine("--- End of rawRequest Content ---");
                Console.WriteLine("");

                _logger.LogInformation("--- Full rawRequest Content ---");
                _logger.LogInformation("{Content}", rawRequest);
                _logger.LogInformation("--- End of rawRequest Content ---");
                _logger.LogInformation("");

                // Try to parse as JSON
                try
                {
                    var jsonDoc = JsonDocument.Parse(rawRequest);
                    Console.WriteLine("--- JSON Analysis ---");
                    Console.WriteLine("Valid JSON: Yes");
                    Console.WriteLine($"Root Element Type: {jsonDoc.RootElement.ValueKind}");
                    Console.WriteLine("");

                    _logger.LogInformation("--- JSON Analysis ---");
                    _logger.LogInformation("Valid JSON: Yes");
                    _logger.LogInformation("Root Element Type: {Type}", jsonDoc.RootElement.ValueKind);
                    _logger.LogInformation("");

                    if (jsonDoc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        Console.WriteLine("--- Top-Level Properties ---");
                        _logger.LogInformation("--- Top-Level Properties ---");
                        foreach (var prop in jsonDoc.RootElement.EnumerateObject())
                        {
                            var propValue = prop.Value.ValueKind == JsonValueKind.String 
                                ? prop.Value.GetString() 
                                : prop.Value.ToString();
                            
                            var displayValue = propValue?.Length > 200 ? propValue.Substring(0, 200) + "..." : propValue;
                            
                            Console.WriteLine($"  [{prop.Name}]");
                            Console.WriteLine($"    Type: {prop.Value.ValueKind}");
                            Console.WriteLine($"    Value: {displayValue}");
                            Console.WriteLine("");

                            _logger.LogInformation("  [{Key}]", prop.Name);
                            _logger.LogInformation("    Type: {Type}", prop.Value.ValueKind);
                            _logger.LogInformation("    Value: {Value}", displayValue);
                            _logger.LogInformation("");
                        }
                    }

                    ((Dictionary<string, object?>)result["rawRequest"]!)["isValidJson"] = true;
                    ((Dictionary<string, object?>)result["rawRequest"]!)["rootElementType"] = jsonDoc.RootElement.ValueKind.ToString();
                    ((Dictionary<string, object?>)result["rawRequest"]!)["fullContent"] = rawRequest;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("--- JSON Analysis ---");
                    Console.WriteLine("Valid JSON: No");
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine("");

                    _logger.LogWarning("--- JSON Analysis ---");
                    _logger.LogWarning("Valid JSON: No");
                    _logger.LogWarning("Error: {Message}", ex.Message);
                    _logger.LogWarning("");

                    ((Dictionary<string, object?>)result["rawRequest"]!)["isValidJson"] = false;
                    ((Dictionary<string, object?>)result["rawRequest"]!)["parseError"] = ex.Message;
                    ((Dictionary<string, object?>)result["rawRequest"]!)["fullContent"] = rawRequest;
                }
            }
            else
            {
                Console.WriteLine("rawRequest is null or empty!");
                _logger.LogWarning("rawRequest is null or empty!");
                ((Dictionary<string, object?>)result["rawRequest"]!)["error"] = "rawRequest is null or empty";
            }
        }
        else
        {
            Console.WriteLine("========================================");
            Console.WriteLine("=== rawRequest NOT FOUND ===");
            Console.WriteLine("========================================");

            _logger.LogWarning("========================================");
            _logger.LogWarning("=== rawRequest NOT FOUND ===");
            _logger.LogWarning("========================================");
            ((Dictionary<string, object?>)result["rawRequest"]!)["error"] = "rawRequest field not found in form data";
        }

        Console.WriteLine("========================================");
        Console.WriteLine("=== End of DEBUG ===");
        Console.WriteLine("========================================");

        _logger.LogInformation("========================================");
        _logger.LogInformation("=== End of DEBUG ===");
        _logger.LogInformation("========================================");

        return Ok(ApiResponseHelper.Success(result, "Debug endpoint - rawRequest analysis complete"));
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
