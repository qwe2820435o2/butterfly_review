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
}