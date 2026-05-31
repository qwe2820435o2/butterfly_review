namespace tennis_wave_api.Models.DTOs;

public class CaptchaDto
{
    public string CaptchaId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
}
