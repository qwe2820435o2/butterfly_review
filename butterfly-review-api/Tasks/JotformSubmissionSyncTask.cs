using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using tennis_wave_api.Services.Interfaces;
 
namespace tennis_wave_api.Tasks;
 
/// <summary>
/// Background service that periodically syncs Jotform submissions into MongoDB.
/// Runs every 2 hours for a fixed time range.
/// </summary>
public class JotformSubmissionSyncTask : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<JotformSubmissionSyncTask> _logger;
 
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(2);
 
    // Fixed sync window (UTC)
    private static readonly DateTime StartUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime EndUtc = new(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);
 
    // Form IDs
    private static readonly string[] ReleaseFormIds = new[]
    {
        "260190981381863",
        "260197297922871"
    };
 
    private const string SightFormId = "260190812224852";
 
    public JotformSubmissionSyncTask(
        IServiceProvider serviceProvider,
        ILogger<JotformSubmissionSyncTask> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Jotform Sync 后台服务已启动，检查间隔: {Interval} 小时；时间范围(UTC): {StartUtc} - {EndUtc}",
            _checkInterval.TotalHours,
            StartUtc,
            EndUtc);
 
        // Wait a bit before first run to allow application to fully start
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
 
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("开始执行 Jotform Sync，时间: {Time}", DateTime.UtcNow);
 
                using var scope = _serviceProvider.CreateScope();
                var syncService = scope.ServiceProvider.GetRequiredService<IJotformSyncService>();
 
                var totalReleaseUpserted = 0;
                foreach (var releaseFormId in ReleaseFormIds)
                {
                    try
                    {
                        var count = await syncService.SyncReleaseSubmissionsAsync(releaseFormId, StartUtc, EndUtc, stoppingToken);
                        totalReleaseUpserted += count;
                        _logger.LogInformation("Release sync 完成：FormId={ReleaseFormId}，Upserted={Count}", releaseFormId, count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Release sync 失败：FormId={ReleaseFormId}", releaseFormId);
                        // Continue other form ids
                    }
                }
 
                var sightingUpserted = await syncService.SyncSightingSubmissionsAsync(SightFormId, StartUtc, EndUtc, stoppingToken);
                _logger.LogInformation("Sighting sync 完成：FormId={SightFormId}，Upserted={Count}", SightFormId, sightingUpserted);
 
                _logger.LogInformation(
                    "Jotform Sync 完成：ReleaseTotalUpserted={ReleaseTotal}，SightingUpserted={SightingTotal}；下次执行时间: {NextRun}",
                    totalReleaseUpserted,
                    sightingUpserted,
                    DateTime.UtcNow.Add(_checkInterval));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 Jotform Sync 时发生错误");
                // Continue running even if there's an error
            }
 
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
 
        _logger.LogInformation("Jotform Sync 后台服务已停止");
    }
}
