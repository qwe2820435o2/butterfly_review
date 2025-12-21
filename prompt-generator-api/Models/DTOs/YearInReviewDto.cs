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
    /// Monthly statistics
    /// </summary>
    public List<MonthlyStatisticsDto> MonthlyStats { get; set; } = new();

    /// <summary>
    /// Geographic distribution data
    /// </summary>
    public GeographicDistributionDto GeographicDistribution { get; set; } = new();

    /// <summary>
    /// Top contributors (volunteers)
    /// </summary>
    public List<ContributorDto> TopContributors { get; set; } = new();

    /// <summary>
    /// Achievement highlights
    /// </summary>
    public AchievementsDto Achievements { get; set; } = new();
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
/// Monthly statistics
/// </summary>
public class MonthlyStatisticsDto
{
    /// <summary>
    /// Month number (1-12)
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// Month name (e.g., "January", "February")
    /// </summary>
    public string MonthName { get; set; } = string.Empty;

    /// <summary>
    /// Number of releases in this month
    /// </summary>
    public int Releases { get; set; }

    /// <summary>
    /// Number of sightings in this month
    /// </summary>
    public int Sightings { get; set; }

    /// <summary>
    /// Number of unique tag numbers sighted in this month
    /// </summary>
    public int UniqueTagNumbersSighted { get; set; }
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

/// <summary>
/// Contributor (volunteer) information
/// </summary>
public class ContributorDto
{
    /// <summary>
    /// Email address (may be masked for privacy)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Number of sightings reported by this volunteer
    /// </summary>
    public int SightingCount { get; set; }

    /// <summary>
    /// Number of unique tag numbers reported by this volunteer
    /// </summary>
    public int UniqueTagNumbersReported { get; set; }

    /// <summary>
    /// Rank in the leaderboard (1-based)
    /// </summary>
    public int Rank { get; set; }
}

/// <summary>
/// Achievement highlights
/// </summary>
public class AchievementsDto
{
    /// <summary>
    /// Longest flight distance record
    /// </summary>
    public FlightRecordDto? LongestFlight { get; set; }

    /// <summary>
    /// Longest survival record
    /// </summary>
    public SurvivalRecordDto? LongestSurvival { get; set; }

    /// <summary>
    /// Most sighted butterfly
    /// </summary>
    public MostSightedDto? MostSighted { get; set; }

    /// <summary>
    /// Farthest sighting from release point
    /// </summary>
    public FarthestSightingDto? FarthestSighting { get; set; }
}

/// <summary>
/// Flight distance record
/// </summary>
public class FlightRecordDto
{
    public string TagNumber { get; set; } = string.Empty;
    public double TotalDistanceKm { get; set; }
    public int SightingCount { get; set; }
    public int? SurvivalDays { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? LastSightingDate { get; set; }
}

/// <summary>
/// Survival record
/// </summary>
public class SurvivalRecordDto
{
    public string TagNumber { get; set; } = string.Empty;
    public int SurvivalDays { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? LastSightingDate { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Most sighted butterfly
/// </summary>
public class MostSightedDto
{
    public string TagNumber { get; set; } = string.Empty;
    public int SightingCount { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? FirstSightingDate { get; set; }
    public DateTime? LastSightingDate { get; set; }
}

/// <summary>
/// Farthest sighting from release point
/// </summary>
public class FarthestSightingDto
{
    public string TagNumber { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public LocationPointDto? ReleaseLocation { get; set; }
    public LocationPointDto? SightingLocation { get; set; }
    public DateTime? SightingDate { get; set; }
}

