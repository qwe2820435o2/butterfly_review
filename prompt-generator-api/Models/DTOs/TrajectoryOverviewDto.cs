namespace tennis_wave_api.Models.DTOs;

/// <summary>
/// A single trajectory point
/// Each point is a flat object with tagNumber and type for easy querying
/// </summary>
public class TrajectoryPointDto
{
    public string TagNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Point type: 1 = Release, 2 = Sighting
    /// </summary>
    public int Type { get; set; }
    
    public double Latitude { get; set; }
    
    public double Longitude { get; set; }
    
    public string? Address { get; set; }
}

