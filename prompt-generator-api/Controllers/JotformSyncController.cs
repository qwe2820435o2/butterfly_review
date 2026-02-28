using Microsoft.AspNetCore.Mvc;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JotformSyncController : ControllerBase
{
    private readonly IJotformSyncService _syncService;

    public JotformSyncController(IJotformSyncService syncService)
    {
        _syncService = syncService;
    }

    /// <summary>
    /// Trigger sync of release form submissions within a time range.
    /// </summary>
    [HttpPost("release")]
    public async Task<IActionResult> SyncRelease([FromBody] JotformSyncRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ReleaseFormId))
        {
            return BadRequest(ApiResponseHelper.Fail<object>("ReleaseFormId is required"));
        }

        if (request.EndUtc < request.StartUtc)
        {
            return BadRequest(ApiResponseHelper.Fail<object>("EndUtc must be greater than or equal to StartUtc"));
        }

        var count = await _syncService.SyncReleaseSubmissionsAsync(request.ReleaseFormId, request.StartUtc, request.EndUtc);

        var result = new
        {
            request.ReleaseFormId,
            request.StartUtc,
            request.EndUtc,
            UpsertedCount = count
        };

        return Ok(ApiResponseHelper.Success(result, "Release submissions synced successfully"));
    }

    /// <summary>
    /// Trigger sync of sighting form submissions within a time range.
    /// </summary>
    [HttpPost("sighting")]
    public async Task<IActionResult> SyncSighting([FromBody] JotformSyncRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SightFormId))
        {
            return BadRequest(ApiResponseHelper.Fail<object>("SightFormId is required"));
        }

        if (request.EndUtc < request.StartUtc)
        {
            return BadRequest(ApiResponseHelper.Fail<object>("EndUtc must be greater than or equal to StartUtc"));
        }

        var count = await _syncService.SyncSightingSubmissionsAsync(request.SightFormId, request.StartUtc, request.EndUtc);

        var result = new
        {
            request.SightFormId,
            request.StartUtc,
            request.EndUtc,
            UpsertedCount = count
        };

        return Ok(ApiResponseHelper.Success(result, "Sighting submissions synced successfully"));
    }
}