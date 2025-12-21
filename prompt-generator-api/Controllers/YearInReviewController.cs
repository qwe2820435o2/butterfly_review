using Microsoft.AspNetCore.Mvc;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;

namespace tennis_wave_api.Controllers;

/// <summary>
/// Controller for Year in Review (Annual Report) functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class YearInReviewController : ControllerBase
{
    private readonly IReleaseSubmissionRepository _releaseRepository;
    private readonly ISightingSubmissionRepository _sightingRepository;

    public YearInReviewController(
        IReleaseSubmissionRepository releaseRepository,
        ISightingSubmissionRepository sightingRepository)
    {
        _releaseRepository = releaseRepository;
        _sightingRepository = sightingRepository;
    }

    /// <summary>
    /// Get year in review data for a specific year
    /// </summary>
    /// <param name="year">Year to generate report for (e.g., 2024)</param>
    /// <returns>Complete year in review data</returns>
    [HttpGet("{year}")]
    public async Task<IActionResult> GetYearInReview(int year)
    {
        try
        {
            // Validate year
            if (year < 2000 || year > 2100)
            {
                return BadRequest(ApiResponseHelper.Fail<YearInReviewDto>($"Invalid year: {year}. Year must be between 2000 and 2100."));
            }

            // Calculate date range for the year
            var startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            // Get all releases and sightings for this year
            var releases = await _releaseRepository.GetByCreatedRangeAsync(startDate, endDate);
            var sightings = await _sightingRepository.GetByCreatedRangeAsync(startDate, endDate);

            // Build the year in review DTO
            var yearInReview = new YearInReviewDto
            {
                Year = year,
                Overview = CalculateOverviewStatistics(releases, sightings),
                GeographicDistribution = CalculateGeographicDistribution(releases, sightings)
            };

            return Ok(ApiResponseHelper.Success(yearInReview, $"Year {year} in review data generated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseHelper.Fail<YearInReviewDto>(ex.Message));
        }
    }

    /// <summary>
    /// Calculate overview statistics
    /// </summary>
    private OverviewStatisticsDto CalculateOverviewStatistics(
        IReadOnlyList<Models.Entities.ReleaseSubmission> releases,
        IReadOnlyList<Models.Entities.SightingSubmission> sightings)
    {
        var uniqueVolunteers = new HashSet<string>();
        var uniqueRegions = new HashSet<string>();
        var survivalDaysList = new List<int>();
        var totalDistance = 0.0;

        // Process releases
        foreach (var release in releases)
        {
            if (!string.IsNullOrWhiteSpace(release.Email))
            {
                uniqueVolunteers.Add(release.Email.ToLowerInvariant());
            }

            if (!string.IsNullOrWhiteSpace(release.Address))
            {
                uniqueRegions.Add(release.Address);
            }
        }

        // Process sightings
        foreach (var sighting in sightings)
        {
            if (!string.IsNullOrWhiteSpace(sighting.Email))
            {
                uniqueVolunteers.Add(sighting.Email.ToLowerInvariant());
            }

            if (!string.IsNullOrWhiteSpace(sighting.Address))
            {
                uniqueRegions.Add(sighting.Address);
            }
        }

        // Calculate survival days and distances for each tag number
        var tagNumbers = releases.Select(r => r.TagNumber).Distinct().ToList();
        foreach (var tagNumber in tagNumbers)
        {
            var tagReleases = releases.Where(r => r.TagNumber == tagNumber).ToList();
            var tagSightings = sightings.Where(s => s.TagNumber == tagNumber)
                .OrderBy(s => s.SightingDateTimeUtc ?? DateTime.MinValue)
                .ToList();

            if (tagReleases.Count > 0 && tagSightings.Count > 0)
            {
                var latestRelease = tagReleases
                    .OrderByDescending(r => r.ReleaseDateTimeUtc ?? DateTime.MinValue)
                    .First();

                var lastSighting = tagSightings.Last();

                if (latestRelease.ReleaseDateTimeUtc.HasValue && lastSighting.SightingDateTimeUtc.HasValue)
                {
                    var days = (int)(lastSighting.SightingDateTimeUtc.Value - latestRelease.ReleaseDateTimeUtc.Value).TotalDays;
                    if (days > 0)
                    {
                        survivalDaysList.Add(days);
                    }
                }

                // Calculate total distance
                if (latestRelease.Latitude.HasValue && latestRelease.Longitude.HasValue)
                {
                    foreach (var sighting in tagSightings)
                    {
                        if (sighting.Latitude.HasValue && sighting.Longitude.HasValue)
                        {
                            var distance = CalculateHaversineDistance(
                                latestRelease.Latitude.Value,
                                latestRelease.Longitude.Value,
                                sighting.Latitude.Value,
                                sighting.Longitude.Value);
                            totalDistance += distance;
                        }
                    }
                }
            }
        }

        // Calculate survival rate
        var aliveCount = sightings.Count(s => 
            s.DeadOrAlive?.Equals("Alive", StringComparison.OrdinalIgnoreCase) == true ||
            s.DeadOrAlive?.Equals("alive", StringComparison.OrdinalIgnoreCase) == true);
        double? survivalRate = releases.Count > 0 ? (double)aliveCount / releases.Count * 100 : null;

        return new OverviewStatisticsDto
        {
            TotalReleases = releases.Count,
            TotalSightings = sightings.Count,
            UniqueVolunteers = uniqueVolunteers.Count,
            UniqueRegions = uniqueRegions.Count,
            AverageSurvivalDays = survivalDaysList.Count > 0 ? survivalDaysList.Average() : null,
            TotalFlightDistanceKm = totalDistance,
            SurvivalRate = survivalRate
        };
    }

    /// <summary>
    /// Calculate geographic distribution
    /// </summary>
    private GeographicDistributionDto CalculateGeographicDistribution(
        IReadOnlyList<Models.Entities.ReleaseSubmission> releases,
        IReadOnlyList<Models.Entities.SightingSubmission> sightings)
    {
        var releaseLocations = releases
            .Where(r => r.Latitude.HasValue && r.Longitude.HasValue)
            .Select(r => new LocationPointDto
            {
                Latitude = r.Latitude!.Value,
                Longitude = r.Longitude!.Value,
                Address = r.Address,
                Date = r.ReleaseDateTimeUtc
            })
            .ToList();

        var sightingLocations = sightings
            .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
            .Select(s => new LocationPointDto
            {
                Latitude = s.Latitude!.Value,
                Longitude = s.Longitude!.Value,
                Address = s.Address,
                Date = s.SightingDateTimeUtc
            })
            .ToList();

        // Find most active release location
        var releaseLocationGroups = releases
            .Where(r => r.Latitude.HasValue && r.Longitude.HasValue && !string.IsNullOrWhiteSpace(r.Address))
            .GroupBy(r => r.Address)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        LocationWithCountDto? mostActiveRelease = null;
        if (releaseLocationGroups != null)
        {
            var firstRelease = releaseLocationGroups.First();
            mostActiveRelease = new LocationWithCountDto
            {
                Latitude = firstRelease.Latitude!.Value,
                Longitude = firstRelease.Longitude!.Value,
                Address = firstRelease.Address,
                Count = releaseLocationGroups.Count()
            };
        }

        // Find most active sighting location
        var sightingLocationGroups = sightings
            .Where(s => s.Latitude.HasValue && s.Longitude.HasValue && !string.IsNullOrWhiteSpace(s.Address))
            .GroupBy(s => s.Address)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        LocationWithCountDto? mostActiveSighting = null;
        if (sightingLocationGroups != null)
        {
            var firstSighting = sightingLocationGroups.First();
            mostActiveSighting = new LocationWithCountDto
            {
                Latitude = firstSighting.Latitude!.Value,
                Longitude = firstSighting.Longitude!.Value,
                Address = firstSighting.Address,
                Count = sightingLocationGroups.Count()
            };
        }

        // Calculate bounds
        GeographicBoundsDto? bounds = null;
        var allLatitudes = releaseLocations.Select(l => l.Latitude)
            .Concat(sightingLocations.Select(l => l.Latitude))
            .ToList();
        var allLongitudes = releaseLocations.Select(l => l.Longitude)
            .Concat(sightingLocations.Select(l => l.Longitude))
            .ToList();

        if (allLatitudes.Any() && allLongitudes.Any())
        {
            bounds = new GeographicBoundsDto
            {
                MinLatitude = allLatitudes.Min(),
                MaxLatitude = allLatitudes.Max(),
                MinLongitude = allLongitudes.Min(),
                MaxLongitude = allLongitudes.Max()
            };
        }

        return new GeographicDistributionDto
        {
            ReleaseLocations = releaseLocations,
            SightingLocations = sightingLocations,
            MostActiveReleaseLocation = mostActiveRelease,
            MostActiveSightingLocation = mostActiveSighting,
            Bounds = bounds
        };
    }

    /// <summary>
    /// Calculate distance between two points using Haversine formula
    /// </summary>
    private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in km
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}

