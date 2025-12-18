using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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

    public JotformApiService(IHttpClientFactory httpClientFactory, IOptions<JotformSettings> options)
    {
        _httpClientFactory = httpClientFactory;
        _settings = options.Value;
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
}