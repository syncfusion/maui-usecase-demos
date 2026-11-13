using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VisitorPass.Models;

/// <summary>
/// One visitor record in the in-memory <c>IVisitorStore</c>.
///
/// The status transitions happen at runtime (Expected → CheckedIn →
/// CheckedOut, or Expected → Cancelled, or Expected → Overdue), so
/// this type is an <see cref="ObservableObject"/> rather than a
/// plain POCO. The store raises its own <c>VisitorsChanged</c>
/// event after a mutation; this object raises per-property change
/// notifications so XAML bindings to <c>Status</c>,
/// <c>CheckInTime</c>, <c>CheckOutTime</c>, <c>StatusBrush</c>
/// etc. update without a full grid rebuild.
///
/// Fields are per SPEC §5:
///   PassId                — VP-yyyyMMdd-#### unique local id
///   VisitorName           — visitor full name
///   Host                  — from the predefined list
///   Purpose               — from the predefined list
///   ContactNumber         — phone reference
///   VehicleNumber         — optional
///   VisitDate             — scheduled date
///   VisitTime             — scheduled time (time-of-day)
///   ExpectedDurationMinutes — planned duration
///   Status                — Expected / Checked In / Checked Out /
///                           Cancelled / Overdue
///   CheckInTime           — arrival timestamp (null until checked in)
///   CheckOutTime          — departure timestamp (null until checked out)
///   PassCode              — barcode text (same as PassId by default)
///   AvatarColor           — hex used by SfAvatarView background
///   Notes                 — optional free-form notes
/// </summary>
public partial class Visitor : ObservableObject
{
    /// <summary>Unique local identifier (format: <c>VP-yyyyMMdd-####</c>).</summary>
    [ObservableProperty]
    private string _passId = string.Empty;

    /// <summary>Visitor full name.</summary>
    [ObservableProperty]
    private string _visitorName = string.Empty;

    /// <summary>Employee being visited (from the predefined host list).</summary>
    [ObservableProperty]
    private string _host = string.Empty;

    /// <summary>Visit purpose (from the predefined purpose list).</summary>
    [ObservableProperty]
    private string _purpose = string.Empty;

    /// <summary>Reference contact phone.</summary>
    [ObservableProperty]
    private string _contactNumber = string.Empty;

    /// <summary>Optional vehicle information.</summary>
    [ObservableProperty]
    private string? _vehicleNumber;

    /// <summary>Scheduled date (date-only component is what consumers show).</summary>
    [ObservableProperty]
    private DateTime _visitDate;

    /// <summary>Scheduled time-of-day.</summary>
    [ObservableProperty]
    private TimeSpan _visitTime;

    /// <summary>Planned duration in minutes.</summary>
    [ObservableProperty]
    private int _expectedDurationMinutes = 60;

    /// <summary>
    /// Current lifecycle status. Setting this also re-fires
    /// <c>OnPropertyChanged</c> for the derived helpers
    /// <c>HasArrived</c>, <c>StatusDisplayLabel</c>,
    /// <c>StatusShortLabel</c>, <c>StatusBrushKey</c>, and
    /// <c>StatusBackgroundBrushKey</c>, so XAML bindings on the
    /// visitor-details screen react without extra plumbing.
    /// </summary>
    [ObservableProperty]
    private VisitorStatus _status;

    /// <summary>Arrival timestamp; null until <see cref="Status"/> becomes <see cref="VisitorStatus.CheckedIn"/>.</summary>
    [ObservableProperty]
    private DateTime? _checkInTime;

    /// <summary>Departure timestamp; null until <see cref="Status"/> becomes <see cref="VisitorStatus.CheckedOut"/>.</summary>
    [ObservableProperty]
    private DateTime? _checkOutTime;

    /// <summary>Barcode text (same value as <see cref="PassId"/> by default).</summary>
    [ObservableProperty]
    private string _passCode = string.Empty;

    /// <summary>Hex colour used by the SfAvatarView background on the directory rows + digital pass.</summary>
    [ObservableProperty]
    private string _avatarColor = "#5146E5";

    /// <summary>Optional free-form notes attached to the visitor.</summary>
    [ObservableProperty]
    private string? _notes;

    /// <summary>Derived: scheduled start = <see cref="VisitDate"/> + <see cref="VisitTime"/>.</summary>
    public DateTime ScheduledStart => VisitDate.Date + VisitTime;

    /// <summary>Derived: scheduled end = <see cref="ScheduledStart"/> + expected duration.</summary>
    public DateTime ScheduledEnd => ScheduledStart.AddMinutes(ExpectedDurationMinutes);

    /// <summary>
    /// Friendly check-in caption for the Active Dossier preview
    /// card. Shows the time when checked in, falls back to
    /// "—:— AM" before check-in.
    /// </summary>
    public string CheckInTimeDisplay =>
        CheckInTime?.ToString("h:mm tt") ?? "—:— AM";

    /// <summary>True when status is <see cref="VisitorStatus.CheckedIn"/> or <see cref="VisitorStatus.CheckedOut"/>.</summary>
    public bool HasArrived => Status is VisitorStatus.CheckedIn or VisitorStatus.CheckedOut;

    /// <summary>Two-letter monogram for the avatar tile. Falls back to <c>?</c> when the name has no letters.</summary>
    public string Initials
    {
        get
        {
            var joined = string.Join(string.Empty,
                (VisitorName ?? string.Empty)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Take(2)
                    .Select(part => part.Length > 0 ? part[0].ToString() : string.Empty));
            return joined.Length switch
            {
                0 => "?",
                _ => joined.Length > 2 ? joined[..2].ToUpperInvariant() : joined.ToUpperInvariant()
            };
        }
    }

    /// <summary>Full status label (e.g. <c>Checked In</c>); consumers on the Details screen bind this directly.</summary>
    public string StatusDisplayLabel => Status.Display();

    /// <summary>Compact status label (e.g. <c>In</c>); used by phone pills + chip filters.</summary>
    public string StatusShortLabel => Status.Short();

    /// <summary>Resource key of the foreground brush matching the current status (see <see cref="VisitorStatusExtensions.BrushKey"/>).</summary>
    public string StatusBrushKey => Status.BrushKey();

    /// <summary>Resource key of the pale background brush matching the current status (see <see cref="VisitorStatusExtensions.BackgroundBrushKey"/>).</summary>
    public string StatusBackgroundBrushKey => Status.BackgroundBrushKey();

    /// <summary>
    /// Raise derived-property change notifications whenever <see cref="Status"/> changes
    /// so XAML bindings on <see cref="HasArrived"/>,
    /// <see cref="StatusDisplayLabel"/>, <see cref="StatusShortLabel"/>,
    /// <see cref="StatusBrushKey"/>, and <see cref="StatusBackgroundBrushKey"/>
    /// refresh.
    /// </summary>
    partial void OnStatusChanged(VisitorStatus value)
    {
        OnPropertyChanged(nameof(HasArrived));
        OnPropertyChanged(nameof(StatusDisplayLabel));
        OnPropertyChanged(nameof(StatusShortLabel));
        OnPropertyChanged(nameof(StatusBrushKey));
        OnPropertyChanged(nameof(StatusBackgroundBrushKey));
    }

    partial void OnVisitorNameChanged(string value) => OnPropertyChanged(nameof(Initials));
}
