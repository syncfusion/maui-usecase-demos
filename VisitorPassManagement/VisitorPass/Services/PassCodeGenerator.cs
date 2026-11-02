using System.Globalization;

namespace VisitorPass.Services;

/// <summary>
/// Generates visitor pass codes in the canonical
/// <c>VP-{yyyyMMdd}-{####}</c> format documented in SPEC §5.
///
/// The zero-padded 4-digit sequence restarts at 0001 every day. The
/// in-process counter is held in a per-day map so two registrations
/// in the same day receive consecutive codes while registrations
/// across two different days restart independently.
///
/// This generator is a singleton (registered in
/// <c>MauiProgram.cs</c>) and is intentionally thread-safe so
/// registration from a background thread doesn't produce a collision.
/// </summary>
public sealed class PassCodeGenerator
{
    /// <summary>Per-day running counter; protected by <see cref="_lock"/>.</summary>
    private readonly Dictionary<string, int> _perDayCounter = new();

    /// <summary>Lock object that protects <see cref="_perDayCounter"/>.</summary>
    private readonly object _lock = new();

    /// <summary>Injectable clock so tests can pin "today".</summary>
    internal Func<DateTime> Clock { get; set; } = () => DateTime.Now;

    /// <summary>
    /// Generates the next pass code for the supplied date (defaults
    /// to today). Format: <c>VP-yyyyMMdd-####</c> where <c>####</c> is
    /// a zero-padded 4-digit sequence starting at 0001 per day.
    /// </summary>
    /// <param name="date">Date the visit is registered for; defaults to <c>DateTime.Now</c>.</param>
    /// <returns>The new pass code, e.g. <c>VP-20251102-0001</c>.</returns>
    public string Next(DateTime? date = null, int visitorCount = 0)
    {
        var stamp = (date ?? Clock()).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        lock (_lock)
        {
            _perDayCounter.TryGetValue(stamp, out var seq);
            seq++;
            _perDayCounter[stamp] = seq;

            return string.Format(
                CultureInfo.InvariantCulture,
                "VP-{0}-{1:D4}",
                stamp,
                seq);
        }
    }
}
