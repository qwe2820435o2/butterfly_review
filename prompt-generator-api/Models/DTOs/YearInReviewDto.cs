namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// Year in Review DTO - Main response for annual report
/// </summary>
public class YearInReviewDto
{
    /// <summary>
    /// Year of the report
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Overview statistics
    /// </summary>
    public OverviewStatisticsDto Overview { get; set; } = new();

    /// <summary>
    /// Geographic distribution data
    /// </summary>
    public GeographicDistributionDto GeographicDistribution { get; set; } = new();
}

/// <summary>
/// Overview statistics for the year
/// </summary>
public class OverviewStatisticsDto
{
    /// <summary>
    /// Total number of butterflies released
    /// </summary>
    public int TotalReleases { get; set; }

    /// <summary>
    /// Total number of sightings
    /// </summary>
    public int TotalSightings { get; set; }

    /// <summary>
    /// Number of unique volunteers (by email)
    /// </summary>
    public int UniqueVolunteers { get; set; }

    /// <summary>
    /// Number of unique geographic regions covered
    /// </summary>
    public int UniqueRegions { get; set; }

    /// <summary>
    /// Average survival days across all butterflies
    /// </summary>
    public double? AverageSurvivalDays { get; set; }

    /// <summary>
    /// Total flight distance in kilometers (sum of all trajectories)
    /// </summary>
    public double TotalFlightDistanceKm { get; set; }

    /// <summary>
    /// Survival rate percentage (alive butterflies / total released)
    /// </summary>
    public double? SurvivalRate { get; set; }
}

/// <summary>
/// Geographic distribution data
/// </summary>
public class GeographicDistributionDto
{
    /// <summary>
    /// All release locations
    /// </summary>
    public List<LocationPointDto> ReleaseLocations { get; set; } = new();

    /// <summary>
    /// All sighting locations
    /// </summary>
    public List<LocationPointDto> SightingLocations { get; set; } = new();

    /// <summary>
    /// Most active release location (with count)
    /// </summary>
    public LocationWithCountDto? MostActiveReleaseLocation { get; set; }

    /// <summary>
    /// Most active sighting location (with count)
    /// </summary>
    public LocationWithCountDto? MostActiveSightingLocation { get; set; }

    /// <summary>
    /// Geographic bounds (min/max lat/lng)
    /// </summary>
    public GeographicBoundsDto? Bounds { get; set; }
}

/// <summary>
/// Location point with coordinates
/// </summary>
public class LocationPointDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public DateTime? Date { get; set; }
}

/// <summary>
/// Location with count (for most active locations)
/// </summary>
public class LocationWithCountDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Geographic bounds
/// </summary>
public class GeographicBoundsDto
{
    public double MinLatitude { get; set; }
    public double MaxLatitude { get; set; }
    public double MinLongitude { get; set; }
    public double MaxLongitude { get; set; }
}

