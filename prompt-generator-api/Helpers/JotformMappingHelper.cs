using System.Globalization;
using System.Text.Json;
using tennis_wave_api.Models.DTOs;
using tennis_wave_api.Models.Entities;

namespace tennis_wave_api.Helpers;

public class JotformMappingHelper
{
    /// <summary>
    /// Map a release form submission to ReleaseSubmission entity.
    /// </summary>
    public static ReleaseSubmission ToReleaseSubmission(JotformSubmissionRawDto raw)
    {
        var entity = new ReleaseSubmission
        {
            SubmissionId = raw.Id,
            FormId = raw.FormId,
            Ip = raw.Ip,
            Status = raw.Status,
            CreatedAtRaw = raw.CreatedAt,
            UpdatedAtRaw = raw.UpdatedAt,
            InsertedAtUtc = DateTime.UtcNow
        };

        entity.CreatedAtUtc = ParseToUtc(raw.CreatedAt, out var createdUtc)
            ? createdUtc
            : DateTime.UtcNow;

        // Email: field "17"
        entity.Email = GetStringAnswer(raw, "17");

        // Tag number: field "27" (name: tagNo)
        entity.TagNumber = GetStringAnswer(raw, "27") ?? string.Empty;

        // Release date/time: field "20" -> answer.datetime
        var dateAnswer = GetAnswerObject(raw, "20");
        if (TryGetStringProperty(dateAnswer, "datetime", out var dtString) &&
            ParseToUtc(dtString, out var releaseUtc))
        {
            entity.ReleaseDateTimeUtc = releaseUtc;
        }

        entity.ReleaseDatePretty = GetAnswerPrettyFormat(raw, "20");

        // Notes: field "22"
        entity.Notes = GetStringAnswer(raw, "22");

        // Weather and other simple radios
        entity.Wind = GetStringAnswer("32", raw);   // estimateThe32
        entity.Sex = GetStringAnswer("33", raw);    // malefemale
        entity.Sun = GetStringAnswer("34", raw);    // wasIt34

        // Location from widgets: 35 (Address Map Locator), 36 (GPS Location)
        var mapText = GetStringAnswer(raw, "35");
        var gpsText = GetStringAnswer(raw, "36");
        entity.MapLocatorRaw = mapText;
        entity.GpsLocationRaw = gpsText;

        // Parse coordinates from either widget text
        if (TryParseCoordinates(mapText, out var lat1, out var lng1))
        {
            entity.Latitude = lat1;
            entity.Longitude = lng1;
        }

        if ((!entity.Latitude.HasValue || !entity.Longitude.HasValue) &&
            TryParseCoordinates(gpsText, out var lat2, out var lng2))
        {
            entity.Latitude = entity.Latitude ?? lat2;
            entity.Longitude = entity.Longitude ?? lng2;
        }

        // Address is usually the first line of GPS widget text
        if (!string.IsNullOrWhiteSpace(gpsText))
        {
            var lines = gpsText.Split('\n');
            entity.Address = lines.Length > 0 ? lines[0].Trim() : null;
        }

        return entity;
    }

    /// <summary>
    /// Map a sighting form submission to SightingSubmission entity.
    /// </summary>
    public static SightingSubmission ToSightingSubmission(JotformSubmissionRawDto raw)
    {
        var entity = new SightingSubmission
        {
            SubmissionId = raw.Id,
            FormId = raw.FormId,
            Ip = raw.Ip,
            Status = raw.Status,
            CreatedAtRaw = raw.CreatedAt,
            UpdatedAtRaw = raw.UpdatedAt,
            InsertedAtUtc = DateTime.UtcNow
        };

        entity.CreatedAtUtc = ParseToUtc(raw.CreatedAt, out var createdUtc)
            ? createdUtc
            : DateTime.UtcNow;

        // Basic contact info
        entity.Email = GetStringAnswer(raw, "17");

        var nameAnswer = GetAnswer(raw, "19");
        if (!string.IsNullOrWhiteSpace(nameAnswer?.PrettyFormat))
        {
            entity.Name = nameAnswer!.PrettyFormat;
        }

        var phoneObj = GetAnswerObject(raw, "20");
        if (TryGetStringProperty(phoneObj, "full", out var phone))
        {
            entity.Phone = phone;
        }

        // Tag number: field "25"
        entity.TagNumber = GetStringAnswer("25", raw) ?? string.Empty;

        // Sighting date/time: field "30" -> answer.datetime
        var dateAnswer = GetAnswerObject(raw, "30");
        if (TryGetStringProperty(dateAnswer, "datetime", out var dtString) &&
            ParseToUtc(dtString, out var sightingUtc))
        {
            entity.SightingDateTimeUtc = sightingUtc;
        }

        entity.SightingDatePretty = GetAnswerPrettyFormat(raw, "30");

        // Condition, dead/alive, nearby butterflies, plants
        entity.Condition = GetStringAnswer("24", raw);
        entity.DeadOrAlive = GetStringAnswer("28", raw);
        entity.NearbyButterflies = GetStringAnswer("27", raw);
        entity.NearbyPlants = GetStringAnswer("14", raw);

        // Weather info (sun, wind)
        entity.HowSunny = GetStringAnswer("31", raw);    // howSunny
        entity.HowWindy = GetStringAnswer("32", raw);   // wasIt32

        // Location from widgets: 42 (Address Map), 43 (GPS)
        var mapText = GetStringAnswer("42", raw);
        var gpsText = GetStringAnswer("43", raw);
        entity.MapLocatorRaw = mapText;
        entity.GpsLocationRaw = gpsText;

        if (TryParseCoordinates(mapText, out var lat1, out var lng1))
        {
            entity.Latitude = lat1;
            entity.Longitude = lng1;
        }

        if ((!entity.Latitude.HasValue || !entity.Longitude.HasValue) &&
            TryParseCoordinates(gpsText, out var lat2, out var lng2))
        {
            entity.Latitude = entity.Latitude ?? lat2;
            entity.Longitude = entity.Longitude ?? lng2;
        }

        if (!string.IsNullOrWhiteSpace(gpsText))
        {
            var lines = gpsText.Split('\n');
            entity.Address = lines.Length > 0 ? lines[0].Trim() : null;
        }

        return entity;
    }

    /// <summary>
    /// Try parse a date string like "2025-12-14 16:43:00" to UTC.
    /// </summary>
    private static bool ParseToUtc(string? value, out DateTime result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dt))
        {
            result = dt;
            return true;
        }

        return false;
    }

    private static JotformAnswerRawDto? GetAnswer(JotformSubmissionRawDto raw, string fieldId)
    {
        return raw.Answers.TryGetValue(fieldId, out var answer) ? answer : null;
    }

    private static string? GetStringAnswer(string fieldId, JotformSubmissionRawDto raw)
    {
        return GetStringAnswer(raw, fieldId);
    }

    private static string? GetStringAnswer(JotformSubmissionRawDto raw, string fieldId)
    {
        if (!raw.Answers.TryGetValue(fieldId, out var answer) || answer.Answer is null)
        {
            return null;
        }

        var value = answer.Answer.Value;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }

    private static JsonElement? GetAnswerObject(JotformSubmissionRawDto raw, string fieldId)
    {
        if (!raw.Answers.TryGetValue(fieldId, out var answer) || answer.Answer is null)
        {
            return null;
        }

        var value = answer.Answer.Value;
        return value.ValueKind == JsonValueKind.Object ? value : null;
    }

    private static bool TryGetStringProperty(JsonElement? element, string propertyName, out string? value)
    {
        value = null;
        if (element is null || element.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (element.Value.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
        {
            value = prop.GetString();
            return true;
        }

        return false;
    }

    private static bool TryParseCoordinates(string? text, out double? latitude, out double? longitude)
    {
        latitude = null;
        longitude = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        // Pattern 1: "Longitude: 176.9123\nLatitude: -39.4926"
        var lower = text.ToLowerInvariant();
        if (lower.Contains("longitude") && lower.Contains("latitude"))
        {
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains("Longitude", StringComparison.OrdinalIgnoreCase) &&
                    TryParseDoubleFromLine(line, "Longitude", out var lon))
                {
                    longitude = lon;
                }

                if (line.Contains("Latitude", StringComparison.OrdinalIgnoreCase) &&
                    TryParseDoubleFromLine(line, "Latitude", out var lat))
                {
                    latitude = lat;
                }
            }

            return latitude.HasValue && longitude.HasValue;
        }

        // Pattern 2: "... \n-39.09176, 174.10500"
        var parts = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var last = parts[^1].Trim();
        var coordParts = last.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (coordParts.Length == 2 &&
            double.TryParse(coordParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat2) &&
            double.TryParse(coordParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lon2))
        {
            latitude = lat2;
            longitude = lon2;
            return true;
        }

        return false;
    }

    private static bool TryParseDoubleFromLine(string line, string label, out double? value)
    {
        value = null;
        var idx = line.IndexOf(label, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return false;
        }

        var part = line[(idx + label.Length)..].Trim(' ', ':');
        if (double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }

    private static string? GetAnswerPrettyFormat(JotformSubmissionRawDto raw, string fieldId)
    {
        var answer = GetAnswer(raw, fieldId);
        return string.IsNullOrWhiteSpace(answer?.PrettyFormat) ? null : answer!.PrettyFormat;
    }
}