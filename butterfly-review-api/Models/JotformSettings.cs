namespace tennis_wave_api.Models;

/// <summary>
/// Jotform configuration settings
/// </summary>
public class JotformSettings
{
    public const string SectionName = "Jotform";

    public string ApiKey { get; set; } = string.Empty;

    public string ReleaseFormId { get; set; } = string.Empty;

    public string SightingFormId { get; set; } = string.Empty;

    public int PageSize { get; set; } = 100;

    /// <summary>
    /// Gmail OAuth redirect URI
    /// </summary>
    public string GmailRedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Gmail OAuth client ID
    /// </summary>
    public string GmailClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gmail OAuth client secret
    /// </summary>
    public string GmailClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gmail OAuth refresh token
    /// </summary>
    public string GmailRefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Optional Reply-To for outbound mail (e.g. admin mailbox for public sighting emails).
    /// </summary>
    public string GmailReplyTo { get; set; } = string.Empty;
}