namespace tennis_wave_api.Models;

/// <summary>
/// API response model
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Business status code (0: success, other: error)
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Message for client
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Actual data
    /// </summary>
    public T? Data { get; set; }
}