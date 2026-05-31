using Microsoft.Extensions.Caching.Memory;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Services;

public class CaptchaService : ICaptchaService
{
    private readonly IMemoryCache _cache;

    public CaptchaService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public (string CaptchaId, string Question) Generate()
    {
        int a = Random.Shared.Next(1, 20);
        int b = Random.Shared.Next(1, 20);
        int answer = a + b;
        string question = $"{a} + {b} = ?";

        var captchaId = Guid.NewGuid().ToString("N");
        _cache.Set(captchaId, answer.ToString(), TimeSpan.FromMinutes(5));

        return (captchaId, question);
    }

    public bool Verify(string captchaId, string userAnswer)
    {
        if (string.IsNullOrWhiteSpace(captchaId) || string.IsNullOrWhiteSpace(userAnswer))
            return false;

        if (!_cache.TryGetValue(captchaId, out string? stored))
            return false;

        _cache.Remove(captchaId);

        return string.Equals(stored, userAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
