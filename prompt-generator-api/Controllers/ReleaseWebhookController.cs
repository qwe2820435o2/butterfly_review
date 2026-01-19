using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Controllers;

/// <summary>
/// Controller for handling Release Form JotForm webhook callbacks.
/// Receives form submission notifications and saves them to MongoDB.
/// </summary>
[ApiController]
[Route("api/release-webhook")]
public class ReleaseWebhookController : ControllerBase
{
    private readonly ILogger<ReleaseWebhookController> _logger;
    private readonly IReleaseSubmissionRepository _releaseSubmissionRepository;
    private readonly JotformSettings _jotformSettings;

    public ReleaseWebhookController(
        ILogger<ReleaseWebhookController> logger,
        IReleaseSubmissionRepository releaseSubmissionRepository,
        IOptions<JotformSettings> jotformSettings)
    {
        _logger = logger;
        _releaseSubmissionRepository = releaseSubmissionRepository;
        _jotformSettings = jotformSettings.Value;
    }

    /// <summary>
    /// Handles Release Form webhook POST requests.
    /// Returns 200 immediately and processes the request asynchronously.
    /// </summary>
    /// <returns>Success response indicating the webhook was received</returns>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public IActionResult ReleaseCallback()
    {
        _logger.LogInformation("收到 Release webhook 请求: Method={Method}, Path={Path}, QueryString={QueryString}",
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
    /// Parses JSON from rawRequest field and saves to MongoDB.
    /// </summary>
    /// <param name="rawRequestJson">JSON string from rawRequest field</param>
    private async Task ProcessWebhookAsync(string rawRequestJson)
    {
        // Generate timestamp for this webhook request
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        try
        {
            _logger.LogInformation("开始处理 Release webhook 数据，rawRequest 长度: {Length}, Timestamp: {Timestamp}", 
                rawRequestJson.Length, 
                timestamp);

            // Fix unescaped newlines in JSON string values before parsing
            var fixedJson = FixUnescapedNewlinesInJson(rawRequestJson);

            // Parse JSON
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var parsedRequest = JsonSerializer.Deserialize<ReleaseWebhookRawRequestDto>(fixedJson, options);
            
            if (parsedRequest == null)
            {
                _logger.LogError("解析 rawRequest JSON 失败，返回 null. Timestamp: {Timestamp}", timestamp);
                return;
            }

            _logger.LogInformation("解析的请求数据: TagNumber={TagNumber}, HasDate={HasDate}, HasEmail={HasEmail}",
                parsedRequest.TagNumber,
                parsedRequest.Date != null,
                !string.IsNullOrWhiteSpace(parsedRequest.Email));

            // Create ReleaseSubmission entity from webhook data
            var releaseSubmission = CreateReleaseSubmissionFromWebhook(parsedRequest, timestamp);

            // Save to MongoDB
            await _releaseSubmissionRepository.InsertAsync(releaseSubmission);

            _logger.LogInformation("成功保存 Release submission 到 MongoDB, SubmissionId: {SubmissionId}, TagNumber: {TagNumber}, Timestamp: {Timestamp}", 
                releaseSubmission.SubmissionId, 
                releaseSubmission.TagNumber,
                timestamp);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "解析 rawRequest JSON 时发生错误: {Message}. Timestamp: {Timestamp}", 
                ex.Message, 
                timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 webhook 数据时发生未预期的错误: {Message}. Timestamp: {Timestamp}", 
                ex.Message, 
                timestamp);
        }
    }

    /// <summary>
    /// Create ReleaseSubmission entity from webhook raw request data.
    /// </summary>
    /// <param name="rawRequest">Parsed webhook request data</param>
    /// <param name="timestamp">Timestamp for this webhook request</param>
    /// <returns>ReleaseSubmission entity</returns>
    private ReleaseSubmission CreateReleaseSubmissionFromWebhook(
        ReleaseWebhookRawRequestDto rawRequest, 
        long timestamp)
    {
        var now = DateTime.UtcNow;
        
        // Generate a unique submission ID based on timestamp
        var submissionId = $"webhook_{timestamp}";

        // Normalize tag number to uppercase
        var tagNumber = string.IsNullOrWhiteSpace(rawRequest.TagNumber) 
            ? string.Empty 
            : rawRequest.TagNumber.ToUpperInvariant();

        // Format release date/time from webhook data
        string? releaseDatePretty = null;
        DateTime? releaseDateTimeUtc = null;
        
        if (rawRequest.Date != null)
        {
            releaseDatePretty = $"{rawRequest.Date.Day}-{rawRequest.Date.Month}-{rawRequest.Date.Year} {rawRequest.Date.TimeInput} {rawRequest.Date.AmPm}";
            
            // Try to parse datetime
            if (!string.IsNullOrWhiteSpace(rawRequest.Date.Day) &&
                !string.IsNullOrWhiteSpace(rawRequest.Date.Month) &&
                !string.IsNullOrWhiteSpace(rawRequest.Date.Year) &&
                !string.IsNullOrWhiteSpace(rawRequest.Date.TimeInput))
            {
                var dateStr = $"{rawRequest.Date.Year}-{rawRequest.Date.Month.PadLeft(2, '0')}-{rawRequest.Date.Day.PadLeft(2, '0')} {rawRequest.Date.TimeInput} {rawRequest.Date.AmPm}";
                if (DateTime.TryParse(dateStr, out var parsedDate))
                {
                    releaseDateTimeUtc = parsedDate.ToUniversalTime();
                }
            }
        }

        // Parse coordinates from GPS location or Map Locator
        double? latitude = null;
        double? longitude = null;
        string? address = null;

        if (TryParseCoordinates(rawRequest.GpsLocation, out var lat1, out var lng1))
        {
            latitude = lat1;
            longitude = lng1;
        }

        if ((!latitude.HasValue || !longitude.HasValue) &&
            TryParseCoordinates(rawRequest.MapLocator, out var lat2, out var lng2))
        {
            latitude = latitude ?? lat2;
            longitude = longitude ?? lng2;
        }

        // Extract address from GPS location (first line)
        if (!string.IsNullOrWhiteSpace(rawRequest.GpsLocation))
        {
            var lines = rawRequest.GpsLocation.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                address = lines[0].Trim();
            }
        }

        var entity = new ReleaseSubmission
        {
            SubmissionId = submissionId,
            FormId = _jotformSettings.ReleaseFormId,
            Status = "ACTIVE",
            CreatedAtRaw = now.ToString("yyyy-MM-dd HH:mm:ss"),
            CreatedAtUtc = now,
            InsertedAtUtc = now,
            UpdatedAtUtc = now,
            
            // Map webhook fields
            Email = rawRequest.Email,
            TagNumber = tagNumber,
            ReleaseDatePretty = releaseDatePretty,
            ReleaseDateTimeUtc = releaseDateTimeUtc,
            Notes = rawRequest.Notes,
            Wind = rawRequest.Wind,
            Sex = rawRequest.Sex,
            Sun = rawRequest.Sun,
            MapLocatorRaw = rawRequest.MapLocator,
            GpsLocationRaw = rawRequest.GpsLocation,
            Latitude = latitude,
            Longitude = longitude,
            Address = address,
            
            // Initialize empty Answers dictionary (webhook doesn't provide full answer structure)
            Answers = new Dictionary<string, JotformAnswerRawDto>()
        };

        return entity;
    }

    /// <summary>
    /// Fix unescaped newlines in JSON string values.
    /// JotForm may send JSON with unescaped \r\n in string values, which causes JSON parsing errors.
    /// </summary>
    private static string FixUnescapedNewlinesInJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return json;
        }

        // Use regex to escape newlines within string values
        var result = Regex.Replace(
            json,
            @"""([^""\\]*(\\.[^""\\]*)*)""",
            match =>
            {
                var value = match.Groups[1].Value;
                // Escape unescaped newlines and carriage returns
                value = value.Replace("\r\n", "\\r\\n")
                            .Replace("\r", "\\r")
                            .Replace("\n", "\\n");
                return $"\"{value}\"";
            },
            RegexOptions.None);

        return result;
    }

    /// <summary>
    /// Try parse coordinates from text (GPS location or Map Locator).
    /// Supports two patterns:
    /// 1. "Longitude: 176.9123\nLatitude: -39.4926"
    /// 2. "... \n-39.09176, 174.10500"
    /// </summary>
    private static bool TryParseCoordinates(string? text, out double? latitude, out double? longitude)
    {
        latitude = null;
        longitude = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        // Pattern 1: "Longitude: 176.9123\nLatitude: -39.4926"
        var lower = text.ToLowerInvariant();
        if (lower.Contains("longitude") && lower.Contains("latitude"))
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains("Longitude", StringComparison.OrdinalIgnoreCase) &&
                    TryParseDoubleFromLine(line, "Longitude", out var lon))
                {
                    longitude = lon;
                }

                if (line.Contains("Latitude", StringComparison.OrdinalIgnoreCase) &&
                    TryParseDoubleFromLine(line, "Latitude", out var lat))
                {
                    latitude = lat;
                }
            }

            return latitude.HasValue && longitude.HasValue;
        }

        // Pattern 2: "... \n-39.09176, 174.10500"
        var parts = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var last = parts[^1].Trim();
        var coordParts = last.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (coordParts.Length == 2 &&
            double.TryParse(coordParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat2) &&
            double.TryParse(coordParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon2))
        {
            latitude = lat2;
            longitude = lon2;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Try parse a double value from a line containing a label.
    /// </summary>
    private static bool TryParseDoubleFromLine(string line, string label, out double? value)
    {
        value = null;
        var idx = line.IndexOf(label, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return false;
        }

        var part = line[(idx + label.Length)..].Trim(' ', ':');
        if (double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }
}
