using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Models.Entities;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Services;

/// <summary>
/// Syncs Jotform submissions into MongoDB.
/// </summary>
public class JotformSyncService : IJotformSyncService
{
    private readonly IJotformApiService _apiService;
    private readonly IReleaseSubmissionRepository _releaseRepository;
    private readonly ISightingSubmissionRepository _sightingRepository;
    private readonly JotformSettings _settings;
    private readonly ILogger<JotformSyncService> _logger;

    public JotformSyncService(
        IJotformApiService apiService,
        IReleaseSubmissionRepository releaseRepository,
        ISightingSubmissionRepository sightingRepository,
        IOptions<JotformSettings> options,
        ILogger<JotformSyncService> logger)
    {
        _apiService = apiService;
        _releaseRepository = releaseRepository;
        _sightingRepository = sightingRepository;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<int> SyncReleaseSubmissionsAsync(
        string releaseFormId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        var totalUpserted = 0;
        var offset = 0;
        var limit = _settings.PageSize > 0 ? _settings.PageSize : 100;
        var pageNumber = 0;

        _logger.LogInformation("Starting Release form sync. FormId: {ReleaseFormId}, Time range: {StartUtc} to {EndUtc}", releaseFormId, startUtc, endUtc);

        while (true)
        {
            pageNumber++;
            var page = await _apiService.GetReleaseSubmissionsPageAsync(releaseFormId, offset, limit, cancellationToken);

            if (page.Content == null || page.Content.Count == 0)
            {
                _logger.LogInformation("Page {PageNumber}: No more data, sync completed", pageNumber);
                break;
            }

            // Collect matched records and their dates
            var matchedRecords = new List<DateTime>();
            var matchedEntities = new List<ReleaseSubmission>();

            foreach (var raw in page.Content)
            {
                if (!TryParseToUtc(raw.CreatedAt, out var createdUtc))
                {
                    continue;
                }

                if (createdUtc < startUtc || createdUtc > endUtc)
                {
                    continue;
                }

                var entity = JotformMappingHelper.ToReleaseSubmission(raw);
                entity.CreatedAtUtc = createdUtc;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                matchedRecords.Add(createdUtc);
                matchedEntities.Add(entity);
            }

            // Upsert matched records
            foreach (var entity in matchedEntities)
            {
                await _releaseRepository.UpsertBySubmissionIdAsync(entity);
                totalUpserted++;
            }

            // Log progress for this page
            if (matchedRecords.Count > 0)
            {
                var minDate = matchedRecords.Min();
                var maxDate = matchedRecords.Max();
                _logger.LogInformation(
                    "Page {PageNumber} (offset {Offset}): Matched {MatchedCount} records. Date range: {MinDate:yyyy-MM-dd HH:mm:ss} to {MaxDate:yyyy-MM-dd HH:mm:ss}",
                    pageNumber, offset, matchedRecords.Count, minDate, maxDate);
            }
            else
            {
                _logger.LogInformation("Page {PageNumber} (offset {Offset}): No records matched in time range", pageNumber, offset);
            }

            if (page.Content.Count < limit)
            {
                _logger.LogInformation("Page {PageNumber}: Last page reached, sync completed", pageNumber);
                break;
            }

            offset += limit;
        }

        _logger.LogInformation("Release form sync completed. Total upserted: {TotalUpserted}", totalUpserted);
        return totalUpserted;
    }

    public async Task<int> SyncSightingSubmissionsAsync(
        string sightFormId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        var totalUpserted = 0;
        var offset = 0;
        var limit = _settings.PageSize > 0 ? _settings.PageSize : 100;
        var pageNumber = 0;

        _logger.LogInformation("Starting Sighting form sync. FormId: {SightFormId}, Time range: {StartUtc} to {EndUtc}", sightFormId, startUtc, endUtc);

        while (true)
        {
            pageNumber++;
            var page = await _apiService.GetSightingSubmissionsPageAsync(sightFormId, offset, limit, cancellationToken);

            if (page.Content == null || page.Content.Count == 0)
            {
                _logger.LogInformation("Page {PageNumber}: No more data, sync completed", pageNumber);
                break;
            }

            // Collect matched records and their dates
            var matchedRecords = new List<DateTime>();
            var matchedEntities = new List<SightingSubmission>();

            foreach (var raw in page.Content)
            {
                if (!TryParseToUtc(raw.CreatedAt, out var createdUtc))
                {
                    continue;
                }

                if (createdUtc < startUtc || createdUtc > endUtc)
                {
                    continue;
                }

                var entity = JotformMappingHelper.ToSightingSubmission(raw);
                entity.CreatedAtUtc = createdUtc;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                matchedRecords.Add(createdUtc);
                matchedEntities.Add(entity);
            }

            // Upsert matched records
            foreach (var entity in matchedEntities)
            {
                await _sightingRepository.UpsertBySubmissionIdAsync(entity);
                totalUpserted++;
            }

            // Log progress for this page
            if (matchedRecords.Count > 0)
            {
                var minDate = matchedRecords.Min();
                var maxDate = matchedRecords.Max();
                _logger.LogInformation(
                    "Page {PageNumber} (offset {Offset}): Matched {MatchedCount} records. Date range: {MinDate:yyyy-MM-dd HH:mm:ss} to {MaxDate:yyyy-MM-dd HH:mm:ss}",
                    pageNumber, offset, matchedRecords.Count, minDate, maxDate);
            }
            else
            {
                _logger.LogInformation("Page {PageNumber} (offset {Offset}): No records matched in time range", pageNumber, offset);
            }

            if (page.Content.Count < limit)
            {
                _logger.LogInformation("Page {PageNumber}: Last page reached, sync completed", pageNumber);
                break;
            }

            offset += limit;
        }

        _logger.LogInformation("Sighting form sync completed. Total upserted: {TotalUpserted}", totalUpserted);
        return totalUpserted;
    }

    /// <summary>
    /// Parse a date string like "2025-12-14 16:43:00" to UTC DateTime.
    /// </summary>
    private static bool TryParseToUtc(string? value, out DateTime result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out result);
    }
}