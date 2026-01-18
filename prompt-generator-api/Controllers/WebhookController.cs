using System.Text.Json;
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
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(ILogger<WebhookController> logger)
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
}
