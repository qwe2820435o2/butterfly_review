using System.Linq;
using Microsoft.AspNetCore.Mvc;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrajectoriesController : ControllerBase
{
    private readonly IReleaseSubmissionRepository _releaseRepository;
    private readonly ISightingSubmissionRepository _sightingRepository;

    public TrajectoriesController(
        IReleaseSubmissionRepository releaseRepository,
        ISightingSubmissionRepository sightingRepository)
    {
        _releaseRepository = releaseRepository;
        _sightingRepository = sightingRepository;
    }

    /// <summary>
    /// Get all distinct tag numbers from both release and sighting submissions that have coordinates
    /// </summary>
    /// <returns>List of unique tag numbers</returns>
    [HttpGet("tagNumbers")]
    public async Task<IActionResult> GetAllTagNumbers()
    {
        try
        {
            // Get all release and sighting submissions with coordinates in parallel
            var releaseTask = _releaseRepository.GetAllWithCoordinatesAsync();
            var sightingTask = _sightingRepository.GetAllWithCoordinatesAsync();
            
            await Task.WhenAll(releaseTask, sightingTask);
            
            var releases = await releaseTask;
            var sightings = await sightingTask;

            // Extract distinct tag numbers from releases
            var releaseTagNumbers = releases
                .Where(r => !string.IsNullOrWhiteSpace(r.TagNumber))
                .Select(r => r.TagNumber)
                .Distinct()
                .ToList();

            // Extract distinct tag numbers from sightings
            var sightingTagNumbers = sightings
                .Where(s => !string.IsNullOrWhiteSpace(s.TagNumber))
                .Select(s => s.TagNumber)
                .Distinct()
                .ToList();

            // Combine and get distinct tag numbers
            var allTagNumbers = releaseTagNumbers
                .Union(sightingTagNumbers)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            return Ok(ApiResponseHelper.Success(allTagNumbers, $"Found {allTagNumbers.Count} unique tag number(s) with coordinates"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseHelper.Fail<object>(ex.Message));
        }
    }

    /// <summary>
    /// Get all trajectories with coordinates for overview map
    /// Returns all release and sighting points grouped by tagNumber
    /// </summary>
    /// <returns>List of trajectory overview data</returns>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllTrajectories([FromQuery] int? year)
    {
        try
        {
            Task<IReadOnlyList<ReleaseSubmission>> releaseTask;
            Task<IReadOnlyList<SightingSubmission>> sightingTask;

            if (year.HasValue)
            {
                // Build UTC range for the selected year
                var startUtc = new DateTime(year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var endUtc = startUtc.AddYears(1).AddTicks(-1);

                releaseTask = _releaseRepository.GetByCreatedRangeAsync(startUtc, endUtc);
                sightingTask = _sightingRepository.GetByCreatedRangeAsync(startUtc, endUtc);
            }
            else
            {
                // Get all release and sighting submissions with coordinates
                releaseTask = _releaseRepository.GetAllWithCoordinatesAsync();
                sightingTask = _sightingRepository.GetAllWithCoordinatesAsync();
            }
            
            await Task.WhenAll(releaseTask, sightingTask);
            
            var releases = (await releaseTask)
                .Where(r => r.Latitude.HasValue && r.Longitude.HasValue)
                .ToList();
            var sightings = (await sightingTask)
                .Where(s => s.Latitude.HasValue && s.Longitude.HasValue)
                .ToList();

            // Group releases by tagNumber
            // Business logic: A butterfly is only released once, so each tagNumber should have only one release record
            // If multiple release records exist for the same tagNumber, take the latest one (by ReleaseDateTimeUtc)
            var releasesByTag = releases
                .Where(r => !string.IsNullOrWhiteSpace(r.TagNumber))
                .GroupBy(r => r.TagNumber)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var releaseList = g.OrderByDescending(r => r.ReleaseDateTimeUtc ?? DateTime.MinValue).ToList();
                        // Log warning if multiple releases found for the same tagNumber (data quality issue)
                        if (releaseList.Count > 1)
                        {
                            // Note: In production, you might want to log this to a logging service
                            // For now, we'll just take the latest one
                        }
                        return releaseList.First();
                    }
                );

            // Group sightings by tagNumber and sort by date
            // Business logic: A tagNumber can be sighted multiple times, so keep all sighting records
            var sightingsByTag = sightings
                .Where(s => !string.IsNullOrWhiteSpace(s.TagNumber))
                .GroupBy(s => s.TagNumber)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(s => s.SightingDateTimeUtc ?? DateTime.MinValue).ToList()
                );

            // Get all unique tagNumbers
            var allTagNumbers = releasesByTag.Keys
                .Union(sightingsByTag.Keys)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            // Build trajectory points (flat structure for easy querying)
            // Filter out coordinates that are clearly outside New Zealand bounds
            // New Zealand approximate bounds: Lat: -34 to -47, Lng: 166 to 179
            var allPoints = new List<TrajectoryPointDto>();

            foreach (var tagNumber in allTagNumbers)
            {
                // Add release point if exists (Type = 1)
                if (releasesByTag.TryGetValue(tagNumber, out var release))
                {
                    var lat = release.Latitude!.Value;
                    var lng = release.Longitude!.Value;
                    
                    // Check if coordinates are within New Zealand bounds
                    // If not, try swapping lat/lng (common data error)
                    if (!IsWithinNewZealandBounds(lat, lng))
                    {
                        // Try swapping coordinates
                        if (IsWithinNewZealandBounds(lng, lat))
                        {
                            // Coordinates were swapped, correct them
                            lat = release.Longitude!.Value;
                            lng = release.Latitude!.Value;
                        }
                        else
                        {
                            // Coordinates are outside NZ bounds even after swap, skip this point
                            continue;
                        }
                    }
                    
                    allPoints.Add(new TrajectoryPointDto
                    {
                        TagNumber = tagNumber,
                        Type = 1, // Release
                        Latitude = lat,
                        Longitude = lng,
                        Address = release.Address
                    });
                }

                // Add sighting points if exist (Type = 2)
                if (sightingsByTag.TryGetValue(tagNumber, out var tagSightings))
                {
                    var sightingPoints = tagSightings
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
                            
                            return new TrajectoryPointDto
                            {
                                TagNumber = tagNumber,
                                Type = 2, // Sighting
                                Latitude = lat,
                                Longitude = lng,
                                Address = s.Address
                            };
                        })
                        .Where(p => p != null)
                        .ToList();
                    
                    allPoints.AddRange(sightingPoints!);
                }
            }

            // Sort by tagNumber, then by type (release first), then by date if available
            var sortedPoints = allPoints
                .OrderBy(p => p.TagNumber)
                .ThenBy(p => p.Type) // Release (1) before Sighting (2)
                .ToList();

            return Ok(ApiResponseHelper.Success(sortedPoints, $"Found {sortedPoints.Count} trajectory point(s) with coordinates"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseHelper.Fail<object>(ex.Message));
        }
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

