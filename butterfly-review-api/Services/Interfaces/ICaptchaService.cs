namespace tennis_wave_api.Services.Interfaces;

public interface ICaptchaService
{
    (string CaptchaId, string Question) Generate();
    bool Verify(string captchaId, string userAnswer);
}
