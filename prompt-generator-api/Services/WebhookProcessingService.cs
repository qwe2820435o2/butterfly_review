using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Models.Entities;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Services;

/// <summary>
/// Service for processing webhook data from JotForm.
/// Handles tag number processing, submission lookup, and email sending.
/// </summary>
public class WebhookProcessingService : IWebhookProcessingService
{
    private readonly IJotformApiService _jotformApiService;
    private readonly IReleaseSubmissionRepository _releaseSubmissionRepository;
    private readonly ISightingSubmissionRepository _sightingSubmissionRepository;
    private readonly IGmailService _gmailService;
    private readonly JotformSettings _jotformSettings;
    private readonly ILogger<WebhookProcessingService> _logger;

    // Notification email addresses
    private static readonly string[] NotificationEmails = 
    {
        "hi.travis.nong@gmail.com",
        "jacqui@nzbutterflies.org.nz",
        "devangi1008@gmail.com"
    };

    public WebhookProcessingService(
        IJotformApiService jotformApiService,
        IReleaseSubmissionRepository releaseSubmissionRepository,
        ISightingSubmissionRepository sightingSubmissionRepository,
        IGmailService gmailService,
        IOptions<JotformSettings> jotformOptions,
        ILogger<WebhookProcessingService> logger)
    {
        _jotformApiService = jotformApiService;
        _releaseSubmissionRepository = releaseSubmissionRepository;
        _sightingSubmissionRepository = sightingSubmissionRepository;
        _gmailService = gmailService;
        _jotformSettings = jotformOptions.Value;
        _logger = logger;
    }

    public async Task ProcessWebhookDataAsync(WebhookRawRequestDto rawRequest, long timestamp, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始处理 webhook 数据，Timestamp: {Timestamp}", timestamp);

            // Extract and validate tag number
            var tagNumber = rawRequest.TagNumber;
            if (string.IsNullOrWhiteSpace(tagNumber))
            {
                _logger.LogWarning("标签号缺失，发送通知邮件. Timestamp: {Timestamp}", timestamp);
                await SendNotificationEmailAsync(
                    NotificationEmails,
                    "Tag has not been entered",
                    $"Tag has not been entered yet. UniqueID: {timestamp}",
                    cancellationToken);
                return;
            }

            _logger.LogInformation("收到的标签号: {TagNumber}", tagNumber);

            // Normalize tag number to uppercase
            tagNumber = tagNumber.ToUpperInvariant();
            _logger.LogInformation("标准化后的标签号: {TagNumber}", tagNumber);

            // Find release submission from MongoDB
            _logger.LogInformation("开始从 MongoDB 查找 Release submission, TagNumber: {TagNumber}", tagNumber);
            var releaseSubmissions = await _releaseSubmissionRepository.GetByTagNumberAsync(tagNumber);
            
            // Filter by case-insensitive match (MongoDB query is case-sensitive)
            var releaseSubmission = releaseSubmissions
                .FirstOrDefault(s => string.Equals(s.TagNumber, tagNumber, StringComparison.OrdinalIgnoreCase));

            if (releaseSubmission == null)
            {
                _logger.LogWarning("未找到匹配的 Release submission，发送通知邮件. TagNumber: {TagNumber}, Timestamp: {Timestamp}", 
                    tagNumber, timestamp);
                await SendNotificationEmailAsync(
                    NotificationEmails,
                    "Tag has not been entered",
                    $"{tagNumber} has not been entered yet. UniqueID: {timestamp}",
                    cancellationToken);
                return;
            }

            _logger.LogInformation("找到匹配的 Release submission: SubmissionId={SubmissionId}, MongoDB Id={Id}", 
                releaseSubmission.SubmissionId, releaseSubmission.Id);

            // Validate date data
            if (rawRequest.Date == null)
            {
                _logger.LogError("时间数据缺失. Timestamp: {Timestamp}", timestamp);
                throw new InvalidOperationException("Seen time data is missing");
            }

            // Format seen time
            var seenOfTime = $"{rawRequest.Date.Day}-{rawRequest.Date.Month}-{rawRequest.Date.Year} {rawRequest.Date.TimeInput} {rawRequest.Date.AmPm}";
            var seenOfTimeAddress = rawRequest.Address ?? "Unknown location";
            _logger.LogInformation("观察时间和地点: Time={Time}, Address={Address}", seenOfTime, seenOfTimeAddress);

            // Extract release submission data from MongoDB entity
            // ReleaseSubmission entity already has these fields mapped from JotForm
            var dateOfRelease = releaseSubmission.ReleaseDatePretty ?? "Unknown";
            var addressOfRelease = releaseSubmission.Address ?? "Unknown";
            var email = releaseSubmission.Email ?? "Unknown";

            _logger.LogInformation("提交信息: DateOfRelease={Date}, AddressOfRelease={Address}, Email={Email}", 
                dateOfRelease, addressOfRelease, email);

            // Build email content
            _logger.LogInformation("准备发送邮件");
            var emailContent = BuildEmailContent(tagNumber, dateOfRelease, addressOfRelease, seenOfTime, seenOfTimeAddress, timestamp);

            // Prepare recipient list (matching original TS logic)
            var recipientEmails = new List<string>();
            
            // Add email from webhook request (q17_email)
            if (!string.IsNullOrWhiteSpace(rawRequest.Email))
            {
                recipientEmails.Add(rawRequest.Email);
            }
            
            // Add fixed notification emails (jacqui and devangi, but NOT hi.travis.nong)
            recipientEmails.Add("jacqui@nzbutterflies.org.nz");
            recipientEmails.Add("devangi1008@gmail.com");
            
            // Add email from release submission (if valid and not already in list)
            if (!string.IsNullOrWhiteSpace(email) && email != "Unknown" && !recipientEmails.Contains(email))
            {
                recipientEmails.Add(email);
            }

            _logger.LogInformation("收件人列表: {Recipients}", string.Join(", ", recipientEmails));

            // Send email
            await _gmailService.SendEmailAsync(
                recipientEmails.ToArray(),
                "Re: Tagged Butterfly Sighting",
                emailContent,
                cancellationToken);

            // Save sighting submission to MongoDB
            try
            {
                _logger.LogInformation("开始保存 Sighting submission 到 MongoDB, TagNumber: {TagNumber}", tagNumber);
                var sightingSubmission = CreateSightingSubmissionFromWebhook(rawRequest, tagNumber, timestamp);
                await _sightingSubmissionRepository.InsertAsync(sightingSubmission);
                _logger.LogInformation("成功保存 Sighting submission 到 MongoDB, SubmissionId: {SubmissionId}", sightingSubmission.SubmissionId);
            }
            catch (Exception saveEx)
            {
                // Log error but don't fail the entire process
                _logger.LogError(saveEx, "保存 Sighting submission 到 MongoDB 失败, TagNumber: {TagNumber}, Timestamp: {Timestamp}", 
                    tagNumber, timestamp);
            }

            _logger.LogInformation("成功处理 webhook 数据，标签号: {TagNumber}", tagNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理 webhook 数据时出错. Timestamp: {Timestamp}", timestamp);
            
            // Send error notification email
            try
            {
                await SendNotificationEmailAsync(
                    new[] { "hi.travis.nong@gmail.com" },
                    "Webhook Processing Error",
                    $"Error processing webhook data: {ex.Message}\n\nTimestamp: {timestamp}",
                    cancellationToken);
            }
            catch (Exception emailError)
            {
                _logger.LogError(emailError, "发送错误通知邮件失败. Timestamp: {Timestamp}", timestamp);
            }
            
            throw;
        }
    }

    /// <summary>
    /// Build HTML email content for butterfly sighting notification.
    /// </summary>
    private static string BuildEmailContent(
        string tagNumber,
        string dateOfRelease,
        string addressOfRelease,
        string seenOfTime,
        string seenOfTimeAddress,
        long timestamp)
    {
        return $@"
            <html>
            <body style=""font-family: Arial, sans-serif; line-height: 1.6;"">
                <p>Thank you for being part of our <b>MBNZT tagging programme</b>! Here's some exciting news:</p>
                
                <p>
                    Tagged Butterfly <span style=""color: blue; font-weight: bold;"">{tagNumber}</span> was tagged on 
                    <span style=""color: blue; font-weight: bold;"">{dateOfRelease}</span> and released at:
                </p>
                <pre style=""color: green; margin-left: 20px;"">{addressOfRelease}</pre>
                
                <p>
                    The butterfly was then seen at:
                </p>
                <pre style=""color: red; margin-left: 20px;"">{seenOfTimeAddress}</pre>
                
                <p>on <span style=""margin-left: 20px; font-weight: bold; color: red;"">{seenOfTime}</span></p>
                
                <p>You can see a map of its journey here:</p>
                           
                <a href=""https://butterfly.up.railway.app/map/{tagNumber}"" style=""color: blue;"">https://butterfliestrace.ts.r.appspot.com/butterfly/{tagNumber}</a>
                
                <p>We are raising awareness about our monarch butterflies in NZ and conservation issues for both the monarch and other Lepidoptera species. What we learn will be shared with anyone, especially the scientific community, to help with conservation and addressing climate change.</p>
                
                <p>We would love your further involvement. Check out <a href=""http://www.nzbutterflies.org.nz"" style=""color: blue;"">www.nzbutterflies.org.nz</a> for more information about our work.</p>
                
                <p>This is a very exciting project and we thank you for your part in it.</p>
                
                <p style=""font-size: 10px; color: gray;"">UniqueID: {timestamp}</p>
            </body>
            </html>
        ";
    }

    /// <summary>
    /// Send a simple notification email.
    /// </summary>
    private async Task SendNotificationEmailAsync(
        string[] to,
        string subject,
        string message,
        CancellationToken cancellationToken)
    {
        var emailContent = $@"
            <html>
            <body style=""font-family: Arial, sans-serif; line-height: 1.6;"">
                <p>{message}</p>
            </body>
            </html>
        ";

        await _gmailService.SendEmailAsync(to, subject, emailContent, cancellationToken);
    }

    /// <summary>
    /// Create SightingSubmission entity from webhook data.
    /// </summary>
    private SightingSubmission CreateSightingSubmissionFromWebhook(
        WebhookRawRequestDto rawRequest, 
        string tagNumber, 
        long timestamp)
    {
        var now = DateTime.UtcNow;
        
        // Generate a unique submission ID based on timestamp
        // Format: "webhook_{timestamp}"
        var submissionId = $"webhook_{timestamp}";

        // Format sighting date/time from webhook data
        string? sightingDatePretty = null;
        DateTime? sightingDateTimeUtc = null;
        
        if (rawRequest.Date != null)
        {
            sightingDatePretty = $"{rawRequest.Date.Day}-{rawRequest.Date.Month}-{rawRequest.Date.Year} {rawRequest.Date.TimeInput} {rawRequest.Date.AmPm}";
            
            // Try to parse datetime
            if (!string.IsNullOrWhiteSpace(rawRequest.Date.Day) &&
                !string.IsNullOrWhiteSpace(rawRequest.Date.Month) &&
                !string.IsNullOrWhiteSpace(rawRequest.Date.Year) &&
                !string.IsNullOrWhiteSpace(rawRequest.Date.TimeInput))
            {
                var dateStr = $"{rawRequest.Date.Year}-{rawRequest.Date.Month.PadLeft(2, '0')}-{rawRequest.Date.Day.PadLeft(2, '0')} {rawRequest.Date.TimeInput} {rawRequest.Date.AmPm}";
                if (DateTime.TryParse(dateStr, out var parsedDate))
                {
                    sightingDateTimeUtc = parsedDate.ToUniversalTime();
                }
            }
        }

        // Parse name from Name object
        string? name = null;
        if (rawRequest.Name != null)
        {
            var nameParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(rawRequest.Name.First))
                nameParts.Add(rawRequest.Name.First);
            if (!string.IsNullOrWhiteSpace(rawRequest.Name.Last))
                nameParts.Add(rawRequest.Name.Last);
            name = nameParts.Count > 0 ? string.Join(" ", nameParts) : null;
        }

        // Parse phone from Phone object
        string? phone = rawRequest.Phone?.Full;

        // Parse coordinates from Address field (q43_typeA43 contains GPS location)
        double? latitude = null;
        double? longitude = null;
        string? address = null;

        if (TryParseCoordinates(rawRequest.Address, out var lat1, out var lng1))
        {
            latitude = lat1;
            longitude = lng1;
        }

        // Extract address from GPS location (first line)
        if (!string.IsNullOrWhiteSpace(rawRequest.Address))
        {
            var lines = rawRequest.Address.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                address = lines[0].Trim();
            }
        }

        // Construct MapLocatorRaw from latitude and longitude (format: "Longitude: {lng}\nLatitude: {lat}")
        string? mapLocatorRaw = null;
        if (latitude.HasValue && longitude.HasValue)
        {
            mapLocatorRaw = $"Longitude: {longitude.Value}\nLatitude: {latitude.Value}";
        }

        var entity = new SightingSubmission
        {
            SubmissionId = submissionId,
            FormId = _jotformSettings.SightingFormId,
            Status = "ACTIVE",
            CreatedAtRaw = now.ToString("yyyy-MM-dd HH:mm:ss"),
            CreatedAtUtc = now,
            InsertedAtUtc = now,
            UpdatedAtUtc = now,
            
            // Map webhook fields
            Email = rawRequest.Email,
            Name = name,
            Phone = phone,
            TagNumber = tagNumber,
            Address = address,
            Condition = rawRequest.Condition,
            DeadOrAlive = rawRequest.DeadOrAlive,
            HowSunny = rawRequest.HowSunny,
            HowWindy = rawRequest.HowWindy,
            NearbyButterflies = rawRequest.NearbyButterflies,
            NearbyPlants = rawRequest.NearbyPlants,
            Latitude = latitude,
            Longitude = longitude,
            MapLocatorRaw = mapLocatorRaw, // Constructed from latitude and longitude
            GpsLocationRaw = rawRequest.Address, // Store raw GPS location data
            SightingDatePretty = sightingDatePretty,
            SightingDateTimeUtc = sightingDateTimeUtc,
            
            // Initialize empty Answers dictionary (webhook doesn't provide full answer structure)
            Answers = new Dictionary<string, JotformAnswerRawDto>()
        };

        // Add basic answers if available (for consistency with JotForm structure)
        if (!string.IsNullOrWhiteSpace(rawRequest.TagNumber))
        {
            entity.Answers["25"] = new JotformAnswerRawDto
            {
                Name = "tagNumber",
                AnswerJson = System.Text.Json.JsonSerializer.SerializeToElement(rawRequest.TagNumber),
                PrettyFormat = tagNumber
            };
        }

        if (rawRequest.Date != null)
        {
            var dateJson = System.Text.Json.JsonSerializer.SerializeToElement(rawRequest.Date);
            entity.Answers["30"] = new JotformAnswerRawDto
            {
                Name = "date",
                AnswerJson = dateJson,
                PrettyFormat = sightingDatePretty
            };
        }

        if (!string.IsNullOrWhiteSpace(rawRequest.Address))
        {
            entity.Answers["43"] = new JotformAnswerRawDto
            {
                Name = "typeA43",
                AnswerJson = System.Text.Json.JsonSerializer.SerializeToElement(rawRequest.Address),
                PrettyFormat = rawRequest.Address
            };
        }

        if (!string.IsNullOrWhiteSpace(rawRequest.Email))
        {
            entity.Answers["48"] = new JotformAnswerRawDto
            {
                Name = "email48",
                AnswerJson = System.Text.Json.JsonSerializer.SerializeToElement(rawRequest.Email),
                PrettyFormat = rawRequest.Email
            };
        }

        return entity;
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
