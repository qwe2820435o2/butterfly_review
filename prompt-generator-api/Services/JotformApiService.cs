using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using tennis_wave_api.Models;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Services;

/// <summary>
/// Concrete Jotform API client.
/// </summary>
public class JotformApiService : IJotformApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JotformSettings _settings;
    private readonly ILogger<JotformApiService>? _logger;

    public JotformApiService(
        IHttpClientFactory httpClientFactory, 
        IOptions<JotformSettings> options,
        ILogger<JotformApiService>? logger = null)
    {
        _httpClientFactory = httpClientFactory;
        _settings = options.Value;
        _logger = logger;
    }

    public Task<JotformApiResponseDto> GetReleaseSubmissionsPageAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        return GetSubmissionsPageAsync(_settings.ReleaseFormId, offset, limit, cancellationToken);
    }

    public Task<JotformApiResponseDto> GetSightingSubmissionsPageAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        return GetSubmissionsPageAsync(_settings.SightingFormId, offset, limit, cancellationToken);
    }

    private async Task<JotformApiResponseDto> GetSubmissionsPageAsync(string formId, int offset, int limit, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"https://api.jotform.com/form/{formId}/submissions";

        var query = new Dictionary<string, string>
        {
            ["apiKey"] = _settings.ApiKey,
            ["offset"] = offset.ToString(),
            ["limit"] = limit.ToString()
        };

        var uriBuilder = new UriBuilder(url)
        {
            Query = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"))
        };

        using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
        request.Headers.Accept.ParseAdd("application/json");

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<JotformApiResponseDto>(json);
        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize Jotform response.");
        }

        return result;
    }

    public async Task<JotformSubmissionRawDto?> FindReleaseSubmissionByTagNumberAsync(string tagNumber, CancellationToken cancellationToken = default)
    {
        return await FindSubmissionByTagNumberAsync(_settings.ReleaseFormId, tagNumber, "27", cancellationToken);
    }

    public async Task<JotformSubmissionRawDto?> FindSightingSubmissionByTagNumberAsync(string tagNumber, CancellationToken cancellationToken = default)
    {
        return await FindSubmissionByTagNumberAsync(_settings.SightingFormId, tagNumber, "25", cancellationToken);
    }

    private async Task<JotformSubmissionRawDto?> FindSubmissionByTagNumberAsync(string formId, string tagNumber, string tagFieldId, CancellationToken cancellationToken)
    {
        var offset = 0;
        const int limit = 100;

        _logger?.LogInformation("开始查询表单 {FormId}，标签号: {TagNumber}, 字段ID: {FieldId}", formId, tagNumber, tagFieldId);

        while (true)
        {
            var page = await GetSubmissionsPageAsync(formId, offset, limit, cancellationToken);

            if (page.Content == null || page.Content.Count == 0)
            {
                break;
            }

            foreach (var submission in page.Content)
            {
                if (submission.Answers.TryGetValue(tagFieldId, out var tagField))
                {
                    // Check if field name is 'tagNo' (for Release form) or matches tag number
                    var fieldName = tagField.Name;
                    var answerValue = GetStringAnswer(tagField);
                    
                    if ((fieldName == "tagNo" || !string.IsNullOrWhiteSpace(answerValue)) && 
                        string.Equals(answerValue, tagNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger?.LogInformation("找到匹配的提交记录: {SubmissionId}, 标签号: {TagNumber}", submission.Id, tagNumber);
                        return submission;
                    }
                }
            }

            if (page.Content.Count < limit)
            {
                break;
            }

            offset += limit;
        }

        return null;
    }

    public async Task UpdateSubmissionFieldAsync(string submissionId, string fieldId, string value, CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"https://api.jotform.com/submission/{submissionId}";

        var formData = new Dictionary<string, string>
        {
            ["apiKey"] = _settings.ApiKey,
            [$"submission[{fieldId}]"] = value
        };

        var content = new FormUrlEncodedContent(formData);
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        };

        var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger?.LogInformation("成功更新提交记录 {SubmissionId} 的字段 {FieldId} 为 {Value}", submissionId, fieldId, value);
    }

    /// <summary>
    /// Get string answer from JotformAnswerRawDto.
    /// </summary>
    private static string? GetStringAnswer(JotformAnswerRawDto answer)
    {
        if (answer.AnswerJson is null)
        {
            return null;
        }

        var value = answer.AnswerJson.Value;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }
}