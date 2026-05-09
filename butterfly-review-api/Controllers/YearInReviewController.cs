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

            // Calculate geographic distribution first (needed for statistics)
            var geographicDistribution = CalculateGeographicDistribution(releases, sightings);

            // Calculate all statistics
            var statistics = CalculateOverviewStatistics(releases, sightings, geographicDistribution);

            // Build the flattened year in review DTO
            var yearInReview = new YearInReviewDto
            {
                Year = year,
                TotalReleases = statistics.TotalReleases,
                TotalSightings = statistics.TotalSightings,
                UniqueVolunteers = statistics.UniqueVolunteers,
                UniqueRegions = statistics.UniqueRegions,
                AverageFlightDistanceKm = statistics.AverageFlightDistanceKm,
                AverageDaysToFirstSighting = statistics.AverageDaysToFirstSighting,
                ReleaseLocationsCount = statistics.ReleaseLocationsCount,
                SightingLocationsCount = statistics.SightingLocationsCount,
                MostActiveReleaseLocationAddress = statistics.MostActiveReleaseLocationAddress,
                MostActiveReleaseLocationCount = statistics.MostActiveReleaseLocationCount,
                MostActiveSightingLocationAddress = statistics.MostActiveSightingLocationAddress,
                MostActiveSightingLocationCount = statistics.MostActiveSightingLocationCount,
                GeographicDistribution = geographicDistribution,
                Overview = null // Explicitly set to null to avoid serialization
            };

            return Ok(ApiResponseHelper.Success(yearInReview, $"Year {year} in review data generated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseHelper.Fail<YearInReviewDto>(ex.Message));
        }
    }

    /// <summary>
    /// Calculate overview statistics (internal helper, returns flattened structure)
    /// </summary>
    private OverviewStatisticsDto CalculateOverviewStatistics(
        IReadOnlyList<Models.Entities.ReleaseSubmission> releases,
        IReadOnlyList<Models.Entities.SightingSubmission> sightings,
        GeographicDistributionDto geographicDistribution)
    {
        var uniqueVolunteers = new HashSet<string>();
        var uniqueRegions = new HashSet<string>();
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

        // Calculate distances and days to first sighting for each tag number
        var uniqueTagNumbers = releases.Select(r => r.TagNumber).Distinct().ToHashSet();
        var daysToFirstSightingList = new List<int>();
        
        foreach (var tagNumber in uniqueTagNumbers)
        {
            var tagReleases = releases.Where(r => r.TagNumber == tagNumber).ToList();
            var tagSightings = sightings.Where(s => s.TagNumber == tagNumber)
                .OrderBy(s => s.SightingDateTimeUtc ?? DateTime.MinValue)
                .ToList();

            // Calculate days to first sighting
            if (tagReleases.Count > 0 && tagSightings.Count > 0)
            {
                var latestRelease = tagReleases
                    .OrderByDescending(r => r.ReleaseDateTimeUtc ?? DateTime.MinValue)
                    .First();

                var firstSightingForDays = tagSightings.First();

                if (latestRelease.ReleaseDateTimeUtc.HasValue && firstSightingForDays.SightingDateTimeUtc.HasValue)
                {
                    var days = (int)(firstSightingForDays.SightingDateTimeUtc.Value - latestRelease.ReleaseDateTimeUtc.Value).TotalDays;
                    if (days >= 0)
                    {
                        daysToFirstSightingList.Add(days);
                    }
                }

                // Calculate total distance for this butterfly
                // Trajectory: Release -> First Sighting -> Second Sighting -> ...
                if (latestRelease.Latitude.HasValue && latestRelease.Longitude.HasValue)
                {
                    var butterflyDistance = 0.0;
                    var validSightings = tagSightings
                        .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
                        .OrderBy(s => s.SightingDateTimeUtc ?? DateTime.MinValue)
                        .ToList();
                    
                    if (validSightings.Count > 0)
                    {
                        // Calculate distance from release to first sighting
                        var firstSightingPoint = validSightings[0];
                        butterflyDistance += CalculateHaversineDistance(
                            latestRelease.Latitude.Value,
                            latestRelease.Longitude.Value,
                            firstSightingPoint.Latitude!.Value,
                            firstSightingPoint.Longitude!.Value);
                        
                        // Calculate distances between consecutive sightings
                        for (int i = 0; i < validSightings.Count - 1; i++)
                        {
                            var current = validSightings[i];
                            var next = validSightings[i + 1];
                            
                            butterflyDistance += CalculateHaversineDistance(
                                current.Latitude!.Value,
                                current.Longitude!.Value,
                                next.Latitude!.Value,
                                next.Longitude!.Value);
                        }
                        
                        totalDistance += butterflyDistance;
                    }
                }
            }
        }

        // Calculate average days to first sighting
        double? averageDaysToFirstSighting = daysToFirstSightingList.Count > 0
            ? daysToFirstSightingList.Average()
            : null;

        // Calculate average flight distance (total distance / butterflies released count)
        double? averageFlightDistanceKm = releases.Count > 0
            ? totalDistance / releases.Count
            : null;

        return new OverviewStatisticsDto
        {
            TotalReleases = releases.Count,
            TotalSightings = sightings.Count,
            UniqueVolunteers = uniqueVolunteers.Count,
            UniqueRegions = uniqueRegions.Count,
            AverageFlightDistanceKm = averageFlightDistanceKm,
            AverageDaysToFirstSighting = averageDaysToFirstSighting,
            ReleaseLocationsCount = geographicDistribution.ReleaseLocations.Count,
            SightingLocationsCount = geographicDistribution.SightingLocations.Count,
            MostActiveReleaseLocationAddress = geographicDistribution.MostActiveReleaseLocation?.Address,
            MostActiveReleaseLocationCount = geographicDistribution.MostActiveReleaseLocation?.Count ?? 0,
            MostActiveSightingLocationAddress = geographicDistribution.MostActiveSightingLocation?.Address,
            MostActiveSightingLocationCount = geographicDistribution.MostActiveSightingLocation?.Count ?? 0
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
            .Select(r =>
            {
                var lat = r.Latitude!.Value;
                var lng = r.Longitude!.Value;
                
                // Check if coordinates are within New Zealand bounds
                // If not, try swapping lat/lng (common data error)
                if (!IsWithinNewZealandBounds(lat, lng))
                {
                    // Try swapping coordinates
                    if (IsWithinNewZealandBounds(lng, lat))
                    {
                        // Coordinates were swapped, correct them
                        lat = r.Longitude!.Value;
                        lng = r.Latitude!.Value;
                    }
                    else
                    {
                        // Coordinates are outside NZ bounds even after swap, return null to filter out
                        return null;
                    }
                }
                
                return new LocationPointDto
                {
                    Latitude = lat,
                    Longitude = lng,
                    Address = r.Address,
                    Date = r.ReleaseDateTimeUtc
                };
            })
            .Where(l => l != null)
            .ToList();

        var sightingLocations = sightings
            .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
            .Select(s =>
            {
                var lat = s.Latitude!.Value;
                var lng = s.Longitude!.Value;
                
                // Check if coordinates are within New Zealand bounds
                // If not, try swapping lat/lng (common data error)
                if (!IsWithinNewZealandBounds(lat, lng))
                {
                    // Try swapping coordinates
                    if (IsWithinNewZealandBounds(lng, lat))
                    {
                        // Coordinates were swapped, correct them
                        lat = s.Longitude!.Value;
                        lng = s.Latitude!.Value;
                    }
                    else
                    {
                        // Coordinates are outside NZ bounds even after swap, return null to filter out
                        return null;
                    }
                }
                
                return new LocationPointDto
                {
                    Latitude = lat,
                    Longitude = lng,
                    Address = s.Address,
                    Date = s.SightingDateTimeUtc
                };
            })
            .Where(l => l != null)
            .ToList();

        // Find most active release location (only from valid NZ coordinates)
        var releaseLocationGroups = releases
            .Where(r => r.Latitude.HasValue && r.Longitude.HasValue && !string.IsNullOrWhiteSpace(r.Address))
            .Select(r =>
            {
                var lat = r.Latitude!.Value;
                var lng = r.Longitude!.Value;
                
                // Check if coordinates are within New Zealand bounds
                if (!IsWithinNewZealandBounds(lat, lng))
                {
                    // Try swapping coordinates
                    if (IsWithinNewZealandBounds(lng, lat))
                    {
                        lat = r.Longitude!.Value;
                        lng = r.Latitude!.Value;
                    }
                    else
                    {
                        return null;
                    }
                }
                
                return new { r.Address, Latitude = lat, Longitude = lng };
            })
            .Where(r => r != null)
            .GroupBy(r => r!.Address)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        LocationWithCountDto? mostActiveRelease = null;
        if (releaseLocationGroups != null)
        {
            var firstRelease = releaseLocationGroups.First();
            mostActiveRelease = new LocationWithCountDto
            {
                Latitude = firstRelease.Latitude,
                Longitude = firstRelease.Longitude,
                Address = firstRelease.Address,
                Count = releaseLocationGroups.Count()
            };
        }

        // Find most active sighting location (only from valid NZ coordinates)
        var sightingLocationGroups = sightings
            .Where(s => s.Latitude.HasValue && s.Longitude.HasValue && !string.IsNullOrWhiteSpace(s.Address))
            .Select(s =>
            {
                var lat = s.Latitude!.Value;
                var lng = s.Longitude!.Value;
                
                // Check if coordinates are within New Zealand bounds
                if (!IsWithinNewZealandBounds(lat, lng))
                {
                    // Try swapping coordinates
                    if (IsWithinNewZealandBounds(lng, lat))
                    {
                        lat = s.Longitude!.Value;
                        lng = s.Latitude!.Value;
                    }
                    else
                    {
                        return null;
                    }
                }
                
                return new { s.Address, Latitude = lat, Longitude = lng };
            })
            .Where(s => s != null)
            .GroupBy(s => s!.Address)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        LocationWithCountDto? mostActiveSighting = null;
        if (sightingLocationGroups != null)
        {
            var firstSighting = sightingLocationGroups.First();
            mostActiveSighting = new LocationWithCountDto
            {
                Latitude = firstSighting.Latitude,
                Longitude = firstSighting.Longitude,
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

    /// <summary>
    /// Check if coordinates are within New Zealand bounds
    /// New Zealand approximate bounds: Latitude: -34 to -47, Longitude: 166 to 179
    /// Strict bounds to ensure only New Zealand locations are included
    /// Also checks that coordinates are NOT in other countries (e.g., Australia)
    /// </summary>
    private static bool IsWithinNewZealandBounds(double latitude, double longitude)
    {
        // New Zealand is in the southern hemisphere (negative latitude) and eastern hemisphere (positive longitude)
        // Main islands bounds: Lat: -34.0 to -47.3, Lng: 166.4 to 178.6
        // Using strict bounds - only include coordinates clearly within New Zealand
        
        // First check: Must be within New Zealand bounds
        bool inNZBounds = latitude >= -47.5 && latitude <= -33.5 &&
                          longitude >= 166.0 && longitude <= 179.0;
        
        if (!inNZBounds)
        {
            return false;
        }
        
        // Second check: Must NOT be in Australia or other nearby countries
        // Australia bounds: Lat: -10 to -44, Lng: 113 to 154
        // If latitude is in Australia range AND longitude is in Australia range, reject it
        bool inAustraliaBounds = latitude >= -44.0 && latitude <= -10.0 &&
                                 longitude >= 113.0 && longitude <= 154.0;
        
        if (inAustraliaBounds)
        {
            return false;
        }
        
        return true;
    }
}

