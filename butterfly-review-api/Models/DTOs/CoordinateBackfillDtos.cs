namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// One-time coordinate backfill report (see CoordinateBackfillController).
/// </summary>
public class CoordinateBackfillReportDto
{
    public bool Applied { get; set; }

    public double SentinelLatitude { get; set; }

    public double SentinelLongitude { get; set; }

    public int TotalScanned { get; set; }

    /// <summary>
    /// The most-repeated stored coordinates across both collections, before any changes.
    /// A widget default shows up here as an implausibly large pile of records on one exact point,
    /// which is how you confirm the configured sentinel is the right one.
    /// </summary>
    public List<CoordinateClusterDto> TopStoredCoordinateClusters { get; set; } = new();

    /// <summary>
    /// Records that were (or, in preview mode, would be) updated.
    /// </summary>
    public List<CoordinateBackfillItemDto> Updated { get; set; } = new();

    /// <summary>
    /// Records pinned at the sentinel whose raw location text holds no usable coordinates.
    /// Left untouched — these can only be recovered by contacting the submitter.
    /// </summary>
    public List<CoordinateBackfillItemDto> StuckAtSentinel { get; set; } = new();
}

public class CoordinateClusterDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Count { get; set; }
    public bool IsConfiguredSentinel { get; set; }
    public List<string> SampleTagNumbers { get; set; } = new();
}

public class CoordinateBackfillItemDto
{
    public string CollectionType { get; set; } = string.Empty; // "Release" or "Sighting"
    public string Id { get; set; } = string.Empty;
    public string SubmissionId { get; set; } = string.Empty;
    public string TagNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public double? OldLatitude { get; set; }
    public double? OldLongitude { get; set; }
    public double? NewLatitude { get; set; }
    public double? NewLongitude { get; set; }
    public string Reason { get; set; } = string.Empty;
}
