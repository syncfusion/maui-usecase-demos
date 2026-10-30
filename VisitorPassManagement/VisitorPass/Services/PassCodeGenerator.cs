using System.Globalization;

namespace VisitorPass.Services;

/// <summary>
/// Generates visitor pass codes in the canonical format documented in
/// SPEC §5 + HARNESS §6 rule 8:
///
///     VP-{####}
///
/// The zero-padded 4-digit sequence restarts at 0001 every day. The
/// in-process counter is held in a per-day map so two registrations in
/// the same day receive consecutive codes while registrations across
/// two different days restart independently.
///
/// This generator is a singleton (registered in VP-012 as
/// <see cref="PassCodeGenerator"/>) and is intentionally thread-safe so
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
    /// Generates the next pass code for the supplied date (defaults to today).
    /// Format: <c>VP-####</c> where <c>####</c> is a zero-padded 4-digit
    /// sequence starting at 0001.
    /// </summary>
    /// <param name="date">Date the visit is registered for; defaults to <c>DateTime.Now</c>.</param>
    /// <returns>The new pass code, e.g. <c>VP-0001</c>.</returns>
    public string Next(DateTime? date = null, int visitorCount = 0)
    {
        lock (_lock)
        {
            _perDayCounter.TryGetValue("Counter", out var seq);
            seq = visitorCount + 1;
            _perDayCounter["Counter"] = seq;

            return string.Format(
                CultureInfo.InvariantCulture,
                "VP-{0:D4}",
                seq);
        }
    }
}
