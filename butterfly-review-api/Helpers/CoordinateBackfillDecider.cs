namespace tennis_wave_api.Helpers;

public enum CoordinateBackfillAction
{
    /// <summary>Leave the record alone and don't report it.</summary>
    None,

    /// <summary>Stored coordinates are pinned exactly at the sentinel; a real location is available.</summary>
    Update,

    /// <summary>
    /// Pinned at the map widget's default position, and the raw location text holds nothing
    /// better. Only a human can recover the real location, so report it and don't touch it.
    /// </summary>
    StuckAtSentinel
}

public record CoordinateBackfillInput(
    string? GpsLocationRaw,
    string? MapLocatorRaw,
    double? Latitude,
    double? Longitude);

public record CoordinateBackfillDecision(
    CoordinateBackfillAction Action,
    double? NewLatitude,
    double? NewLongitude,
    string Reason);

/// <summary>
/// Decides how a stored submission's coordinates should be handled by the one-time backfill
/// (see CoordinateBackfillController).
///
/// Background: the Jotform "Address Map Locator" widget submits a fixed default position when
/// the user never drags the pin. That text parses as valid coordinates, so submissions ingested
/// under the old mapLocatorRaw-first order were stored at that default instead of the real
/// location carried by gpsLocationRaw.
///
/// The ONLY proven signature of that bug is the stored coordinate being exactly the sentinel.
/// Do not treat "gpsLocationRaw and mapLocatorRaw parse to slightly different values" as evidence
/// of the bug on its own — two raw location fields on the same legitimate submission routinely
/// disagree by a few meters (different capture method, rounding), and that is not a defect. A
/// record whose stored coordinate is any real, non-sentinel value is left alone unconditionally,
/// even if a fresher parse would produce a marginally different number.
/// </summary>
public static class CoordinateBackfillDecider
{
    private const double CoordEpsilon = 1e-6;

    public static CoordinateBackfillDecision Decide(
        CoordinateBackfillInput input,
        double sentinelLatitude,
        double sentinelLongitude)
    {
        var hasNew = TryResolve(input, preferGps: true, out var newLat, out var newLng);

        // Resolving to the sentinel is the same as resolving to nothing: it's not a real location.
        var newIsUsable = hasNew && !IsSentinel(newLat!.Value, newLng!.Value, sentinelLatitude, sentinelLongitude);

        var currentIsNull = !input.Latitude.HasValue || !input.Longitude.HasValue;

        if (currentIsNull)
        {
            return newIsUsable
                ? Update(newLat, newLng, "was-null-now-resolvable")
                : None();
        }

        var currentIsSentinel = IsSentinel(input.Latitude!.Value, input.Longitude!.Value, sentinelLatitude, sentinelLongitude);

        if (!currentIsSentinel)
        {
            // Stored value is a real, specific location — not proof of the known bug. Leave it,
            // even if gps/map disagree by a hair.
            return None();
        }

        return newIsUsable
            ? Update(newLat, newLng, "matched-sentinel-bug-pattern")
            : new CoordinateBackfillDecision(
                CoordinateBackfillAction.StuckAtSentinel, null, null, "stuck-at-default-no-usable-source");
    }

    public static bool IsSentinel(double latitude, double longitude, double sentinelLatitude, double sentinelLongitude)
    {
        return CoordsEqual(latitude, sentinelLatitude) && CoordsEqual(longitude, sentinelLongitude);
    }

    private static CoordinateBackfillDecision Update(double? lat, double? lng, string reason) =>
        new(CoordinateBackfillAction.Update, lat, lng, reason);

    private static CoordinateBackfillDecision None() =>
        new(CoordinateBackfillAction.None, null, null, string.Empty);

    private static bool TryResolve(
        CoordinateBackfillInput input,
        bool preferGps,
        out double? latitude,
        out double? longitude)
    {
        var primary = preferGps ? input.GpsLocationRaw : input.MapLocatorRaw;
        var fallback = preferGps ? input.MapLocatorRaw : input.GpsLocationRaw;

        if (JotformMappingHelper.TryParseCoordinates(primary, out latitude, out longitude))
        {
            return true;
        }

        return JotformMappingHelper.TryParseCoordinates(fallback, out latitude, out longitude);
    }

    private static bool CoordsEqual(double a, double b) => Math.Abs(a - b) < CoordEpsilon;
}
