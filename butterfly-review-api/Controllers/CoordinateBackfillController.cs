using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Controllers;

/// <summary>
/// One-time maintenance endpoint: re-derives Latitude/Longitude for existing submissions using
/// the corrected gpsLocationRaw-first / mapLocatorRaw-fallback priority, and reports the records
/// still pinned at the map widget's default position that only a human can recover.
///
/// Call /preview first: check TopStoredCoordinateClusters to confirm the sentinel is the right
/// one, review the Updated list, then call /apply with the same sentinel.
/// Decision logic lives in CoordinateBackfillDecider.
/// </summary>
[ApiController]
[Route("api/coordinate-backfill")]
[Authorize(Roles = "Admin")]
public class CoordinateBackfillController : ControllerBase
{
    /// <summary>
    /// The Jotform map widget's default pin position (downtown Auckland). Overridable per request
    /// so a second default can be cleaned up without a code change.
    /// </summary>
    private const double DefaultSentinelLatitude = -36.8485;
    private const double DefaultSentinelLongitude = 174.7635;

    private const int ClusterSampleSize = 5;
    private const int ClusterCount = 10;

    private readonly IReleaseSubmissionRepository _releaseRepository;
    private readonly ISightingSubmissionRepository _sightingRepository;

    public CoordinateBackfillController(
        IReleaseSubmissionRepository releaseRepository,
        ISightingSubmissionRepository sightingRepository)
    {
        _releaseRepository = releaseRepository;
        _sightingRepository = sightingRepository;
    }

    [HttpGet("preview")]
    public async Task<IActionResult> Preview(
        [FromQuery] double sentinelLat = DefaultSentinelLatitude,
        [FromQuery] double sentinelLng = DefaultSentinelLongitude)
    {
        var report = await BuildReportAsync(apply: false, sentinelLat, sentinelLng);
        return Ok(ApiResponseHelper.Success(report,
            $"Preview only, no changes written. {report.Updated.Count} record(s) would be updated, " +
            $"{report.StuckAtSentinel.Count} stuck at the default and need manual follow-up."));
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply(
        [FromQuery] double sentinelLat = DefaultSentinelLatitude,
        [FromQuery] double sentinelLng = DefaultSentinelLongitude)
    {
        var report = await BuildReportAsync(apply: true, sentinelLat, sentinelLng);
        return Ok(ApiResponseHelper.Success(report,
            $"Applied. {report.Updated.Count} record(s) updated, " +
            $"{report.StuckAtSentinel.Count} stuck at the default and need manual follow-up."));
    }

    private async Task<CoordinateBackfillReportDto> BuildReportAsync(bool apply, double sentinelLat, double sentinelLng)
    {
        var releases = await _releaseRepository.GetAllAsync();
        var sightings = await _sightingRepository.GetAllAsync();

        var report = new CoordinateBackfillReportDto
        {
            Applied = apply,
            SentinelLatitude = sentinelLat,
            SentinelLongitude = sentinelLng,
            TotalScanned = releases.Count + sightings.Count,
            TopStoredCoordinateClusters = BuildClusters(
                releases.Select(ToItem).Concat(sightings.Select(ToItem)),
                sentinelLat,
                sentinelLng)
        };

        foreach (var release in releases)
        {
            var decision = CoordinateBackfillDecider.Decide(
                new CoordinateBackfillInput(release.GpsLocationRaw, release.MapLocatorRaw, release.Latitude, release.Longitude),
                sentinelLat,
                sentinelLng);

            if (!Record(report, ToItem(release), decision))
            {
                continue;
            }

            if (apply)
            {
                release.Latitude = decision.NewLatitude;
                release.Longitude = decision.NewLongitude;
                release.UpdatedAtUtc = DateTime.UtcNow;
                await _releaseRepository.UpdateAsync(release);
            }
        }

        foreach (var sighting in sightings)
        {
            var decision = CoordinateBackfillDecider.Decide(
                new CoordinateBackfillInput(sighting.GpsLocationRaw, sighting.MapLocatorRaw, sighting.Latitude, sighting.Longitude),
                sentinelLat,
                sentinelLng);

            if (!Record(report, ToItem(sighting), decision))
            {
                continue;
            }

            if (apply)
            {
                sighting.Latitude = decision.NewLatitude;
                sighting.Longitude = decision.NewLongitude;
                sighting.UpdatedAtUtc = DateTime.UtcNow;
                await _sightingRepository.UpdateAsync(sighting);
            }
        }

        return report;
    }

    /// <summary>
    /// Files the item under the right list. Returns true when the caller should write the record back.
    /// </summary>
    private static bool Record(
        CoordinateBackfillReportDto report,
        CoordinateBackfillItemDto item,
        CoordinateBackfillDecision decision)
    {
        item.Reason = decision.Reason;

        switch (decision.Action)
        {
            case CoordinateBackfillAction.Update:
                item.NewLatitude = decision.NewLatitude;
                item.NewLongitude = decision.NewLongitude;
                report.Updated.Add(item);
                return true;

            case CoordinateBackfillAction.StuckAtSentinel:
                report.StuckAtSentinel.Add(item);
                return false;

            default:
                return false;
        }
    }

    private static List<CoordinateClusterDto> BuildClusters(
        IEnumerable<CoordinateBackfillItemDto> items,
        double sentinelLat,
        double sentinelLng)
    {
        return items
            .Where(x => x.OldLatitude.HasValue && x.OldLongitude.HasValue)
            .GroupBy(x => (Lat: x.OldLatitude!.Value, Lng: x.OldLongitude!.Value))
            .Select(g => new CoordinateClusterDto
            {
                Latitude = g.Key.Lat,
                Longitude = g.Key.Lng,
                Count = g.Count(),
                IsConfiguredSentinel = CoordinateBackfillDecider.IsSentinel(g.Key.Lat, g.Key.Lng, sentinelLat, sentinelLng),
                SampleTagNumbers = g.Select(x => x.TagNumber).Take(ClusterSampleSize).ToList()
            })
            .OrderByDescending(x => x.Count)
            .Take(ClusterCount)
            .ToList();
    }

    private static CoordinateBackfillItemDto ToItem(ReleaseSubmission release) => new()
    {
        CollectionType = "Release",
        Id = release.Id ?? string.Empty,
        SubmissionId = release.SubmissionId,
        TagNumber = release.TagNumber,
        Email = release.Email,
        Address = release.Address,
        OldLatitude = release.Latitude,
        OldLongitude = release.Longitude
    };

    private static CoordinateBackfillItemDto ToItem(SightingSubmission sighting) => new()
    {
        CollectionType = "Sighting",
        Id = sighting.Id ?? string.Empty,
        SubmissionId = sighting.SubmissionId,
        TagNumber = sighting.TagNumber,
        Email = sighting.Email,
        Address = sighting.Address,
        OldLatitude = sighting.Latitude,
        OldLongitude = sighting.Longitude
    };
}
