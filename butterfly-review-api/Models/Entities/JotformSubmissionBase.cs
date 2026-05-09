using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using tennis_wave_api.Models.DTOs;

namespace tennis_wave_api.Models.Entities;

/// <summary>
/// Base entity for Jotform submissions
/// </summary>
public abstract class JotformSubmissionBase
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("submissionId")]
    public string SubmissionId { get; set; } = string.Empty;

    [BsonElement("formId")]
    public string FormId { get; set; } = string.Empty;

    [BsonElement("ip")]
    public string? Ip { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;

    [BsonElement("createdAtRaw")]
    public string CreatedAtRaw { get; set; } = string.Empty;

    [BsonElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; }

    [BsonElement("updatedAtRaw")]
    public string? UpdatedAtRaw { get; set; }

    [BsonElement("insertedAtUtc")]
    public DateTime InsertedAtUtc { get; set; }

    [BsonElement("updatedAtUtc")]
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Full raw answers from Jotform (key is field id like "20", "27")
    /// </summary>
    [BsonElement("answers")]
    public Dictionary<string, JotformAnswerRawDto> Answers { get; set; } = new();
}