using System.Linq;
using Microsoft.AspNetCore.Mvc;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Helpers;
using tennis_wave_api.Models;

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
    
}

