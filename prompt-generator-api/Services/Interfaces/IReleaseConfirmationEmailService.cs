using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Services.Interfaces;

/// <summary>
/// Sends the tagged butterfly release confirmation email to the submitter.
/// </summary>
public interface IReleaseConfirmationEmailService
{
    /// <summary>
    /// Sends the release confirmation email when <see cref="ReleaseSubmission.Email"/> is set.
    /// </summary>
    /// <param name="submission">Saved release submission (tag, date, location, email).</param>
    /// <param name="uniqueId">Webhook / record identifier shown as UniqueID in the email body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendReleaseConfirmationIfEmailPresentAsync(
        ReleaseSubmission submission,
        long uniqueId,
        CancellationToken cancellationToken = default);
}
