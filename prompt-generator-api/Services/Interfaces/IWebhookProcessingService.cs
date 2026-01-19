using tennis_wave_api.Models.DTOs;

namespace tennis_wave_api.Services.Interfaces;

/// <summary>
/// Service for processing webhook data from JotForm.
/// </summary>
public interface IWebhookProcessingService
{
    /// <summary>
    /// Process webhook data asynchronously.
    /// </summary>
    /// <param name="rawRequest">Parsed webhook request data</param>
    /// <param name="timestamp">Timestamp for this webhook request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ProcessWebhookDataAsync(WebhookRawRequestDto rawRequest, long timestamp, CancellationToken cancellationToken = default);
}
