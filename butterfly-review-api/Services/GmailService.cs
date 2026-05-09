using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tennis_wave_api.Models;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Services;

/// <summary>
/// Service for sending emails via Gmail API.
/// Uses HttpClient to call Gmail API directly.
/// </summary>
public class GmailService : IGmailService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JotformSettings _settings;
    private readonly ILogger<GmailService> _logger;

    public GmailService(
        IHttpClientFactory httpClientFactory,
        IOptions<JotformSettings> options,
        ILogger<GmailService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string[] to,
        string subject,
        string bodyHtml,
        string? replyTo = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var recipients = string.Join(", ", to);
            _logger.LogInformation("准备发送邮件到: {Recipients}, 主题: {Subject}", recipients, subject);

            // Get access token using refresh token
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            // Build email message (matching original TS format with \r\n)
            var email = new StringBuilder();
            email.Append($"To: {recipients}\r\n");
            email.Append($"Subject: {subject}\r\n");
            if (!string.IsNullOrWhiteSpace(replyTo))
            {
                email.Append($"Reply-To: {replyTo.Trim()}\r\n");
            }

            email.Append("MIME-Version: 1.0\r\n");
            email.Append("Content-Type: text/html; charset=utf-8\r\n");
            email.Append("\r\n");
            email.Append(bodyHtml);

            // Encode message (RFC 4648 URL-safe base64)
            // Match original TS logic: replace + with -, / with _, and remove trailing = signs
            var emailBytes = Encoding.UTF8.GetBytes(email.ToString());
            var encodedMessage = Convert.ToBase64String(emailBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            // Send email via Gmail API
            var client = _httpClientFactory.CreateClient();
            var url = "https://gmail.googleapis.com/gmail/v1/users/me/messages/send";

            var requestBody = new
            {
                raw = encodedMessage
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("邮件发送成功到: {Recipients}", recipients);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送邮件失败到: {Recipients}, 主题: {Subject}", string.Join(", ", to), subject);
            throw;
        }
    }

    /// <summary>
    /// Get access token using refresh token.
    /// </summary>
    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var url = "https://oauth2.googleapis.com/token";

        var requestBody = new Dictionary<string, string>
        {
            ["client_id"] = _settings.GmailClientId,
            ["client_secret"] = _settings.GmailClientSecret,
            ["refresh_token"] = _settings.GmailRefreshToken,
            ["grant_type"] = "refresh_token"
        };

        var content = new FormUrlEncodedContent(requestBody);
        var response = await client.PostAsync(url, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);

        if (tokenResponse.TryGetProperty("access_token", out var accessTokenElement))
        {
            return accessTokenElement.GetString() ?? throw new InvalidOperationException("Access token is null");
        }

        throw new InvalidOperationException("Failed to get access token from response");
    }
}
