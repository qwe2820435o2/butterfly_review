namespace tennis_wave_api.Helpers;

public enum CoordinateBackfillAction
{
    /// <summary>Leave the record alone and don't report it.</summary>
    None,

    /// <summary>Stored coordinates provably came from the old parse order; safe to re-derive.</summary>
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
/// Two invariants keep this safe to run against live data:
///   1. Only records whose stored coordinates exactly match what the old parse order would have
///      produced are updated. Anything else is assumed to be a manual correction and left alone.
///   2. The sentinel value is never written back into a record.
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
        var hasOld = TryResolve(input, preferGps: false, out var oldLat, out var oldLng);

        // Resolving to the sentinel is the same as resolving to nothing: it's not a real location.
        var newIsUsable = hasNew && !IsSentinel(newLat!.Value, newLng!.Value, sentinelLatitude, sentinelLongitude);

        var currentIsNull = !input.Latitude.HasValue || !input.Longitude.HasValue;

        if (currentIsNull)
        {
            return newIsUsable
                ? Update(newLat, newLng, "was-null-now-resolvable")
                : None();
        }

        var currentMatchesOldComputed = hasOld
            && CoordsEqual(input.Latitude!.Value, oldLat!.Value)
            && CoordsEqual(input.Longitude!.Value, oldLng!.Value);

        var newDiffersFromOld = !hasOld
            || !CoordsEqual(newLat!.Value, oldLat!.Value)
            || !CoordsEqual(newLng!.Value, oldLng!.Value);

        if (newIsUsable && currentMatchesOldComputed && newDiffersFromOld)
        {
            return Update(newLat, newLng, "matched-old-bug-pattern");
        }

        if (IsSentinel(input.Latitude!.Value, input.Longitude!.Value, sentinelLatitude, sentinelLongitude))
        {
            return new CoordinateBackfillDecision(
                CoordinateBackfillAction.StuckAtSentinel, null, null, "stuck-at-default-no-usable-source");
        }

        return None();
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
