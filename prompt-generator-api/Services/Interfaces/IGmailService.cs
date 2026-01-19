namespace tennis_wave_api.Services.Interfaces;

/// <summary>
/// Service for sending emails via Gmail API.
/// </summary>
public interface IGmailService
{
    /// <summary>
    /// Send an email to one or more recipients.
    /// </summary>
    /// <param name="to">Recipient email address(es)</param>
    /// <param name="subject">Email subject</param>
    /// <param name="bodyHtml">Email body in HTML format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailAsync(string[] to, string subject, string bodyHtml, CancellationToken cancellationToken = default);
}
