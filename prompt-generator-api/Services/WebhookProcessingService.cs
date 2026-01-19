using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Services;

/// <summary>
/// Service for processing webhook data from JotForm.
/// Handles tag number processing, submission lookup, and email sending.
/// </summary>
public class WebhookProcessingService : IWebhookProcessingService
{
    private readonly IJotformApiService _jotformApiService;
    private readonly IGmailService _gmailService;
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
        IGmailService gmailService,
        ILogger<WebhookProcessingService> logger)
    {
        _jotformApiService = jotformApiService;
        _gmailService = gmailService;
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

            // Find release submission
            _logger.LogInformation("开始查找 Release submission");
            var releaseSubmission = await _jotformApiService.FindReleaseSubmissionByTagNumberAsync(tagNumber, cancellationToken);

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

            _logger.LogInformation("找到匹配的 Release submission: {SubmissionId}", releaseSubmission.Id);

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

            // Extract release submission data
            var dateOfRelease = GetAnswerPrettyFormat(releaseSubmission, "20") ?? "Unknown";
            var addressOfRelease = GetStringAnswer(releaseSubmission, "36") ?? "Unknown";
            var email = GetStringAnswer(releaseSubmission, "17") ?? "Unknown";

            _logger.LogInformation("提交信息: DateOfRelease={Date}, AddressOfRelease={Address}, Email={Email}", 
                dateOfRelease, addressOfRelease, email);

            // Build email content
            _logger.LogInformation("准备发送邮件");
            var emailContent = BuildEmailContent(tagNumber, dateOfRelease, addressOfRelease, seenOfTime, seenOfTimeAddress, timestamp);

            // Prepare recipient list
            var recipientEmails = new List<string>();
            if (!string.IsNullOrWhiteSpace(rawRequest.Email))
            {
                recipientEmails.Add(rawRequest.Email);
            }
            recipientEmails.AddRange(NotificationEmails);
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
                           
                <a href=""https://butterfliestrace.ts.r.appspot.com/butterfly/{tagNumber}"" style=""color: blue;"">https://butterfliestrace.ts.r.appspot.com/butterfly/{tagNumber}</a>
                
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
    /// Get string answer from submission by field ID.
    /// </summary>
    private static string? GetStringAnswer(JotformSubmissionRawDto submission, string fieldId)
    {
        if (!submission.Answers.TryGetValue(fieldId, out var answer) || answer.AnswerJson is null)
        {
            return null;
        }

        var value = answer.AnswerJson.Value;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    /// <summary>
    /// Get pretty format from submission by field ID.
    /// </summary>
    private static string? GetAnswerPrettyFormat(JotformSubmissionRawDto submission, string fieldId)
    {
        if (!submission.Answers.TryGetValue(fieldId, out var answer))
        {
            return null;
        }

        return answer.PrettyFormat;
    }
}
