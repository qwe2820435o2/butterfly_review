using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReleaseSubmissionsController : ControllerBase
{
    private readonly IReleaseSubmissionRepository _repository;

    public ReleaseSubmissionsController(IReleaseSubmissionRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get release submissions with optional filters.
    /// Supports filtering by email and/or tagNumber.
    /// </summary>
    /// <param name="email">Filter by email address (optional)</param>
    /// <param name="tagNumber">Filter by tag number (optional)</param>
    /// <param name="includeAnswers">Whether to include answers field in response (default: true)</param>
    /// <returns>List of release submissions matching the filters</returns>
    [HttpGet]
    public async Task<IActionResult> GetReleaseSubmissions(
        [FromQuery] string? email = null,
        [FromQuery] string? tagNumber = null,
        [FromQuery] bool includeAnswers = true)
    {
        try
        {
            // Validate that at least one filter is provided
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(tagNumber))
            {
                return BadRequest(ApiResponseHelper.Fail<object>("At least one filter parameter (email or tagNumber) is required"));
            }

            // If both filters are provided, we need to filter by both
            // For simplicity, we'll prioritize email if both are provided
            // You can enhance this to support AND logic if needed
            IReadOnlyList<tennis_wave_api.Models.Entities.ReleaseSubmission> submissions;

            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(tagNumber))
            {
                // Both filters provided: get by email first, then filter by tagNumber in memory
                // Note: For better performance, you might want to add a combined query method in repository
                var emailResults = await _repository.GetByEmailAsync(email);
                submissions = emailResults.Where(s => s.TagNumber.Equals(tagNumber, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else if (!string.IsNullOrWhiteSpace(email))
            {
                submissions = await _repository.GetByEmailAsync(email);
            }
            else
            {
                submissions = await _repository.GetByTagNumberAsync(tagNumber!);
            }

            submissions = submissions
                .Where(s => !ReleaseSubmissionSoftDeleteHelper.IsSoftDeleted(s.Status))
                .ToList();

            // Remove answers field if not requested
            if (!includeAnswers)
            {
                foreach (var submission in submissions)
                {
                    submission.Answers = new Dictionary<string, tennis_wave_api.Models.DTOs.JotformAnswerRawDto>();
                }
            }

            return Ok(ApiResponseHelper.Success(submissions, $"Found {submissions.Count} record(s)"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseHelper.Fail<object>(ex.Message));
        }
    }

    /// <summary>
    /// Admin: paginated list of all release submissions.
    /// </summary>
    [HttpGet("paginated")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool sortDescending = true,
        [FromQuery] bool includeAnswers = false,
        [FromQuery] string? search = null)
    {
        try
        {
            var (items, totalCount) = await _repository.GetPaginatedAsync(page, pageSize, sortDescending, search);

            if (!includeAnswers)
            {
                foreach (var item in items)
                {
                    item.Answers = new Dictionary<string, JotformAnswerRawDto>();
                }
            }

            var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
            var result = new PaginatedResultDto<ReleaseSubmission>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };

            return Ok(ApiResponseHelper.Success(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponseHelper.Fail<object>(ex.Message));
        }
    }

    /// <summary>
    /// Admin: soft-delete a release submission.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _repository.SoftDeleteByIdAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponseHelper.Fail<object>("Submission not found"));
        }
        return Ok(ApiResponseHelper.Success<object>(null, "Submission deleted"));
    }

    /// <summary>
    /// Admin: get a single release submission by id.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(string id)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null)
        {
            return NotFound(ApiResponseHelper.Fail<object>("Submission not found"));
        }
        return Ok(ApiResponseHelper.Success(item));
    }

    /// <summary>
    /// Admin: create a new release submission.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ReleaseSubmissionInputDto dto)
    {
        var now = DateTime.UtcNow;
        var entity = new ReleaseSubmission
        {
            SubmissionId = $"admin-{Guid.NewGuid():N}",
            FormId = "admin",
            Status = string.Empty,
            CreatedAtUtc = now,
            InsertedAtUtc = now,
            TagNumber = dto.TagNumber,
            Email = dto.Email,
            ReleaseDateTimeUtc = dto.ReleaseDateTimeUtc,
            ReleaseDatePretty = dto.ReleaseDateTimeUtc?.ToString("yyyy-MM-dd HH:mm"),
            Notes = dto.Notes,
            Wind = dto.Wind,
            Sex = dto.Sex,
            Sun = dto.Sun,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Address = dto.Address
        };

        await _repository.InsertAsync(entity);
        return Ok(ApiResponseHelper.Success(entity, "Submission created"));
    }

    /// <summary>
    /// Admin: update an existing release submission.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string id, [FromBody] ReleaseSubmissionInputDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
        {
            return NotFound(ApiResponseHelper.Fail<object>("Submission not found"));
        }

        existing.TagNumber = dto.TagNumber;
        existing.Email = dto.Email;
        existing.ReleaseDateTimeUtc = dto.ReleaseDateTimeUtc;
        existing.ReleaseDatePretty = dto.ReleaseDateTimeUtc?.ToString("yyyy-MM-dd HH:mm");
        existing.Notes = dto.Notes;
        existing.Wind = dto.Wind;
        existing.Sex = dto.Sex;
        existing.Sun = dto.Sun;
        existing.Latitude = dto.Latitude;
        existing.Longitude = dto.Longitude;
        existing.Address = dto.Address;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(existing);
        return Ok(ApiResponseHelper.Success(existing, "Submission updated"));
    }
}

