using System.Linq;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using tennis_wave_api.Data;
using tennis_wave_api.Data.Interfaces;
using tennis_wave_api.Models.Entities;
using tennis_wave_api.Services.Interfaces;

namespace tennis_wave_api.Services;

/// <summary>
/// Service for normalizing tag numbers (converting lowercase to uppercase) in MongoDB collections.
/// </summary>
public class TagNumberNormalizationService : ITagNumberNormalizationService
{
    private readonly MongoDbHelper _dbHelper;
    private readonly ILogger<TagNumberNormalizationService> _logger;

    public TagNumberNormalizationService(
        MongoDbHelper dbHelper,
        ILogger<TagNumberNormalizationService> logger)
    {
        _dbHelper = dbHelper;
        _logger = logger;
    }

    public async Task<int> NormalizeReleaseTagNumbersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始检查 Release submissions 中的小写 tagNumber");

            var collection = _dbHelper.GetConfiguredCollection<ReleaseSubmission>();

            // Find all documents with tagNumber containing lowercase letters
            // Using regex to find tagNumber that contains at least one lowercase letter
            var filter = Builders<ReleaseSubmission>.Filter.Regex(
                x => x.TagNumber,
                new MongoDB.Bson.BsonRegularExpression("[a-z]"));

            var documentsToUpdate = await collection.Find(filter).ToListAsync(cancellationToken);

            if (documentsToUpdate.Count == 0)
            {
                _logger.LogInformation("Release submissions 中没有发现小写 tagNumber");
                return 0;
            }

            _logger.LogInformation("发现 {Count} 个 Release submissions 需要更新 tagNumber", documentsToUpdate.Count);

            var updateCount = 0;

            // Update each document
            foreach (var doc in documentsToUpdate)
            {
                var originalTagNumber = doc.TagNumber;
                var normalizedTagNumber = originalTagNumber.ToUpperInvariant();

                if (originalTagNumber != normalizedTagNumber)
                {
                    var updateFilter = Builders<ReleaseSubmission>.Filter.Eq(x => x.Id, doc.Id);
                    var update = Builders<ReleaseSubmission>.Update
                        .Set(x => x.TagNumber, normalizedTagNumber)
                        .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

                    var result = await collection.UpdateOneAsync(updateFilter, update, cancellationToken: cancellationToken);

                    if (result.ModifiedCount > 0)
                    {
                        updateCount++;
                        _logger.LogInformation(
                            "更新 Release submission {Id}: TagNumber {Original} -> {Normalized}",
                            doc.Id,
                            originalTagNumber,
                            normalizedTagNumber);
                    }
                }
            }

            _logger.LogInformation("完成 Release submissions 更新，共更新 {Count} 条记录", updateCount);
            return updateCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新 Release submissions tagNumber 时发生错误");
            throw;
        }
    }

    public async Task<int> NormalizeSightingTagNumbersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始检查 Sighting submissions 中的小写 tagNumber");

            var collection = _dbHelper.GetConfiguredCollection<SightingSubmission>();

            // Find all documents with tagNumber containing lowercase letters
            // Using regex to find tagNumber that contains at least one lowercase letter
            var filter = Builders<SightingSubmission>.Filter.Regex(
                x => x.TagNumber,
                new MongoDB.Bson.BsonRegularExpression("[a-z]"));

            var documentsToUpdate = await collection.Find(filter).ToListAsync(cancellationToken);

            if (documentsToUpdate.Count == 0)
            {
                _logger.LogInformation("Sighting submissions 中没有发现小写 tagNumber");
                return 0;
            }

            _logger.LogInformation("发现 {Count} 个 Sighting submissions 需要更新 tagNumber", documentsToUpdate.Count);

            var updateCount = 0;

            // Update each document
            foreach (var doc in documentsToUpdate)
            {
                var originalTagNumber = doc.TagNumber;
                var normalizedTagNumber = originalTagNumber.ToUpperInvariant();

                if (originalTagNumber != normalizedTagNumber)
                {
                    var updateFilter = Builders<SightingSubmission>.Filter.Eq(x => x.Id, doc.Id);
                    var update = Builders<SightingSubmission>.Update
                        .Set(x => x.TagNumber, normalizedTagNumber)
                        .Set(x => x.UpdatedAtUtc, DateTime.UtcNow);

                    var result = await collection.UpdateOneAsync(updateFilter, update, cancellationToken: cancellationToken);

                    if (result.ModifiedCount > 0)
                    {
                        updateCount++;
                        _logger.LogInformation(
                            "更新 Sighting submission {Id}: TagNumber {Original} -> {Normalized}",
                            doc.Id,
                            originalTagNumber,
                            normalizedTagNumber);
                    }
                }
            }

            _logger.LogInformation("完成 Sighting submissions 更新，共更新 {Count} 条记录", updateCount);
            return updateCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新 Sighting submissions tagNumber 时发生错误");
            throw;
        }
    }
}
