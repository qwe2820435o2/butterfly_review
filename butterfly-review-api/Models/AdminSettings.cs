namespace tennis_wave_api.Models;

/// <summary>
/// Configuration for bootstrapping admin accounts. Any user whose email appears
/// in <see cref="AdminEmails"/> is promoted to the "Admin" role on login.
/// </summary>
public class AdminSettings
{
    public const string SectionName = "AdminSettings";

    public List<string> AdminEmails { get; set; } = new();

    /// <summary>
    /// Optional shared secret that locks the manual <c>POST /api/auth/admin-register</c>
    /// endpoint. When non-empty, callers must send a matching <c>X-Setup-Key</c> header.
    /// Leave empty in development to allow the endpoint without a key.
    /// </summary>
    public string SetupKey { get; set; } = string.Empty;
}
