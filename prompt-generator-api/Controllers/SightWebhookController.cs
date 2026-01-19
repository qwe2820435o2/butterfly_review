using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Services.Interfaces;

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
    private readonly IWebhookProcessingService _webhookProcessingService;

    public SightWebhookController(
        ILogger<SightWebhookController> logger,
        IWebhookProcessingService webhookProcessingService)
    {
        _logger = logger;
        _webhookProcessingService = webhookProcessingService;
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
    /// Parses JSON from rawRequest field and validates required fields.
    /// </summary>
    /// <param name="rawRequestJson">JSON string from rawRequest field</param>
    private async Task ProcessWebhookAsync(string rawRequestJson)
    {
        // Generate timestamp for this webhook request
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        try
        {
            _logger.LogInformation("开始处理 webhook 数据，rawRequest 长度: {Length}, Timestamp: {Timestamp}", 
                rawRequestJson.Length, 
                timestamp);

            // Parse JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsedRequest = JsonSerializer.Deserialize<WebhookRawRequestDto>(rawRequestJson, options);
            
            if (parsedRequest == null)
            {
                _logger.LogError("解析 rawRequest JSON 失败，返回 null. Timestamp: {Timestamp}", timestamp);
                return;
            }

            _logger.LogInformation("解析的请求数据: TagNumber={TagNumber}, HasDate={HasDate}, HasAddress={HasAddress}, HasEmail={HasEmail}",
                parsedRequest.TagNumber,
                parsedRequest.Date != null,
                !string.IsNullOrWhiteSpace(parsedRequest.Address),
                !string.IsNullOrWhiteSpace(parsedRequest.Email));

            // Validate required fields
            var validationResult = ValidateWebhookData(parsedRequest, timestamp);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Webhook 数据验证失败: {Reason}. Timestamp: {Timestamp}", 
                    validationResult.ErrorMessage, 
                    timestamp);
                return;
            }

            _logger.LogInformation("Webhook 数据验证成功，开始处理. Timestamp: {Timestamp}", timestamp);

            // Process webhook data using service
            await _webhookProcessingService.ProcessWebhookDataAsync(parsedRequest, timestamp);

            _logger.LogInformation("Webhook 数据处理完成. Timestamp: {Timestamp}", timestamp);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "解析 rawRequest JSON 时发生错误: {Message}. Timestamp: {Timestamp}", 
                ex.Message, 
                timestamp);
            // Error handling is done in WebhookProcessingService
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 webhook 数据时发生未预期的错误: {Message}. Timestamp: {Timestamp}", 
                ex.Message, 
                timestamp);
            // Error handling is done in WebhookProcessingService
        }
    }

    /// <summary>
    /// Validate webhook data for required fields.
    /// </summary>
    /// <param name="request">Parsed webhook request data</param>
    /// <param name="timestamp">Timestamp for this webhook request</param>
    /// <returns>Validation result</returns>
    private (bool IsValid, string? ErrorMessage) ValidateWebhookData(WebhookRawRequestDto request, long timestamp)
    {
        // Validate tagNumber (q25_tagNumber) - required
        if (string.IsNullOrWhiteSpace(request.TagNumber))
        {
            _logger.LogWarning("标签号缺失. Timestamp: {Timestamp}", timestamp);
            return (false, "Tag number (q25_tagNumber) is required but missing");
        }

        // Validate date (q30_date) - required
        if (request.Date == null)
        {
            _logger.LogError("时间数据缺失. Timestamp: {Timestamp}", timestamp);
            return (false, "Date (q30_date) is required but missing");
        }

        // Validate date components
        if (string.IsNullOrWhiteSpace(request.Date.Day) ||
            string.IsNullOrWhiteSpace(request.Date.Month) ||
            string.IsNullOrWhiteSpace(request.Date.Year))
        {
            _logger.LogError("时间数据不完整. Timestamp: {Timestamp}", timestamp);
            return (false, "Date components (day, month, year) are incomplete");
        }

        _logger.LogInformation("Webhook 数据验证通过. TagNumber: {TagNumber}, Timestamp: {Timestamp}", 
            request.TagNumber, 
            timestamp);

        return (true, null);
    }
}
