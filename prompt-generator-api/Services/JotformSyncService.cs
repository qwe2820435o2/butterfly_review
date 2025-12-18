using System.Globalization;
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

    public JotformSyncService(
        IJotformApiService apiService,
        IReleaseSubmissionRepository releaseRepository,
        ISightingSubmissionRepository sightingRepository,
        IOptions<JotformSettings> options)
    {
        _apiService = apiService;
        _releaseRepository = releaseRepository;
        _sightingRepository = sightingRepository;
        _settings = options.Value;
    }

    public async Task<int> SyncReleaseSubmissionsAsync(
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        var totalUpserted = 0;
        var offset = 0;
        var limit = _settings.PageSize > 0 ? _settings.PageSize : 100;

        while (true)
        {
            var page = await _apiService.GetReleaseSubmissionsPageAsync(offset, limit, cancellationToken);

            if (page.Content == null || page.Content.Count == 0)
            {
                break;
            }

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

                await _releaseRepository.UpsertBySubmissionIdAsync(entity);
                totalUpserted++;
            }

            if (page.Content.Count < limit)
            {
                break;
            }

            offset += limit;
        }

        return totalUpserted;
    }

    public async Task<int> SyncSightingSubmissionsAsync(
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        var totalUpserted = 0;
        var offset = 0;
        var limit = _settings.PageSize > 0 ? _settings.PageSize : 100;

        while (true)
        {
            var page = await _apiService.GetSightingSubmissionsPageAsync(offset, limit, cancellationToken);

            if (page.Content == null || page.Content.Count == 0)
            {
                break;
            }

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

                await _sightingRepository.UpsertBySubmissionIdAsync(entity);
                totalUpserted++;
            }

            if (page.Content.Count < limit)
            {
                break;
            }

            offset += limit;
        }

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