using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Tasks;

/// <summary>
/// Background service that periodically normalizes tag numbers in MongoDB collections.
/// Runs every 2 hours to check and convert lowercase tag numbers to uppercase.
/// </summary>
public class TagNumberNormalizationTask : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TagNumberNormalizationTask> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(2);

    public TagNumberNormalizationTask(
        IServiceProvider serviceProvider,
        ILogger<TagNumberNormalizationTask> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TagNumber 标准化后台服务已启动，检查间隔: {Interval} 小时", _checkInterval.TotalHours);

        // Wait a bit before first run to allow application to fully start
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("开始执行 TagNumber 标准化检查，时间: {Time}", DateTime.UtcNow);

                // Create a scope for this iteration to get scoped services
                using var scope = _serviceProvider.CreateScope();
                var normalizationService = scope.ServiceProvider.GetRequiredService<ITagNumberNormalizationService>();

                // Normalize Release submissions
                var releaseCount = await normalizationService.NormalizeReleaseTagNumbersAsync(stoppingToken);
                _logger.LogInformation("Release submissions 标准化完成，更新了 {Count} 条记录", releaseCount);

                // Normalize Sighting submissions
                var sightingCount = await normalizationService.NormalizeSightingTagNumbersAsync(stoppingToken);
                _logger.LogInformation("Sighting submissions 标准化完成，更新了 {Count} 条记录", sightingCount);

                _logger.LogInformation("TagNumber 标准化检查完成，下次检查时间: {NextRun}", DateTime.UtcNow.Add(_checkInterval));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 TagNumber 标准化检查时发生错误");
                // Continue running even if there's an error
            }

            // Wait for the next check interval
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
        }

        _logger.LogInformation("TagNumber 标准化后台服务已停止");
    }
}
