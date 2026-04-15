using System.Globalization;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tennis_wave_api.Models;
using tennis_wave_api.Models.Entities;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Services;

/// <summary>
/// Builds and sends the MBNZT release confirmation email (tag, date, release location, UniqueID).
/// </summary>
public class ReleaseConfirmationEmailService : IReleaseConfirmationEmailService
{
    private const string Subject = "Re: Tagged Butterfly Release";
    private const string AdminEmail = "admin@nzbutterflies.org.nz";

    private readonly IGmailService _gmailService;
    private readonly JotformSettings _jotformSettings;
    private readonly ILogger<ReleaseConfirmationEmailService> _logger;

    public ReleaseConfirmationEmailService(
        IGmailService gmailService,
        IOptions<JotformSettings> jotformOptions,
        ILogger<ReleaseConfirmationEmailService> logger)
    {
        _gmailService = gmailService;
        _jotformSettings = jotformOptions.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendReleaseConfirmationIfEmailPresentAsync(
        ReleaseSubmission submission,
        long uniqueId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(submission.Email))
        {
            return;
        }

        //var to = submission.Email.Trim();
        var to = "hi.travis.nong@gmail.com";
        
        var tagNumber = submission.TagNumber ?? string.Empty;
        var tagDate = submission.ReleaseDatePretty ?? "Unknown";
        var releaseLocation = submission.Address ?? "Unknown";

        var bodyHtml = BuildEmailHtml(
            WebUtility.HtmlEncode(tagNumber),
            WebUtility.HtmlEncode(tagDate),
            WebUtility.HtmlEncode(releaseLocation),
            WebUtility.HtmlEncode(uniqueId.ToString(CultureInfo.InvariantCulture)));

        try
        {
            await _gmailService.SendEmailAsync(
                new[] { to },
                Subject,
                bodyHtml,
                GetGmailReplyToOrNull(),
                cancellationToken);

            _logger.LogInformation(
                "Release confirmation email sent. To={To}, TagNumber={TagNumber}, UniqueId={UniqueId}",
                to,
                tagNumber,
                uniqueId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send release confirmation email. To={To}, TagNumber={TagNumber}, UniqueId={UniqueId}",
                to,
                tagNumber,
                uniqueId);
        }
    }

    private string? GetGmailReplyToOrNull()
    {
        var v = _jotformSettings.GmailReplyTo;
        return string.IsNullOrWhiteSpace(v) ? null : v.Trim();
    }

    private static string BuildEmailHtml(
        string tagNumberEncoded,
        string tagDateEncoded,
        string releaseLocationEncoded,
        string uniqueIdEncoded)
    {
        return $@"
<html>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #222;"">
    <p>Thank you for being part of our <strong>MBNZT tagging programme</strong>!</p>
    <p>Tagged Butterfly {tagNumberEncoded} was tagged on {tagDateEncoded} and released at: {releaseLocationEncoded}</p>
    <hr />
    <p>If any of the details are incorrect please FORWARD this email immediately to <a href=""mailto:{AdminEmail}"">{AdminEmail}</a> with correct information so the records can be updated.</p>
    <p>UniqueID: {uniqueIdEncoded}</p>
</body>
</html>";
    }
}
