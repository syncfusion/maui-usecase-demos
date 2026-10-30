using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.Maui.ProgressBar;
using VisitorPass.Models;
using VisitorPass.Services;

namespace VisitorPass.ViewModels;

/// <summary>
/// VP-060 — ViewModel for the Digital Pass / Visitor Details screen
/// (Screen 3).
///
/// Lifecycle:
///   1. The page constructs the VM via DI.
///   2. The Shell pushes a <c>details</c> route with
///      <c>?passId={…}</c> as a query parameter.
///   3. <see cref="ApplyQueryAttributes"/> (from
///      <see cref="IQueryAttributable"/>) captures the passId; the
///      page calls <see cref="LoadAsync"/> from its
///      <c>OnAppearing</c> hook.
///   4. <see cref="LoadAsync"/> resolves the visitor from
///      <see cref="IVisitorStore.FindByPassId"/> and populates
///      <see cref="Visitor"/> + recomputes <see cref="Timeline"/>.
///   5. The user taps <see cref="CheckInCommand"/> /
///      <see cref="CheckOutCommand"/> / <see cref="CancelVisitCommand"/>;
///      each delegates to the store (which raises
///      <see cref="IVisitorStore.VisitorsChanged"/>) and the VM
///      rebuilds the timeline.
///
/// The timeline always has 4 steps by default. When the visitor is
/// <c>Cancelled</c> the last step becomes "Cancelled"; when the
/// visitor is <c>Overdue</c> the second step gets an "Overdue" badge.
///
/// Button-visibility logic per HARNESS §9 (Screen 3 notes):
///   - CheckIn    — only when status is Expected / Overdue.
///   - CheckOut   — only when status is CheckedIn.
///   - Cancel     — only when status is Expected / Overdue.
///   - No buttons when the visit is terminal (CheckedOut / Cancelled).
/// </summary>
public partial class VisitorDetailsViewModel : BaseViewModel, IQueryAttributable
{
    /// <summary>Shell route name registered in <c>AppShell.xaml</c>.</summary>
    public const string Route = "details";

    /// <summary>
    /// Query-parameter name used by both the Dashboard
    /// (TodayGrid cell-tap) and the Directory (directory cell-tap)
    /// navigation handlers. Must stay in sync with
    /// <c>DashboardPage.VisitorIdQueryKey</c>; re-asserted here so
    /// the consumer has a single source of truth on both sides.
    /// </summary>
    public const string PassIdQueryKey = "passId";

    private readonly IVisitorStore _store;
    private string? _pendingPassId;

    /// <summary>
    /// Currently loaded visitor. XAML binds Status, CheckInTime,
    /// CheckOutTime, etc. through this property; <c>null</c> means
    /// "no visitor resolved yet" (the page shows
    /// <see cref="Controls.EmptyState"/>).
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasVisitor))]
    [NotifyPropertyChangedFor(nameof(IsCheckInVisible))]
    [NotifyPropertyChangedFor(nameof(IsCheckOutVisible))]
    [NotifyPropertyChangedFor(nameof(IsCancelVisible))]
    private Visitor? _visitor;

    /// <summary>
    /// Derived 4-step timeline. Always 4 entries
    /// (Registered / Expected / Checked In / Checked Out) — except
    /// when the visit is cancelled, in which case the last entry
    /// is replaced by a "Cancelled" step. Bound to the timeline
    /// CollectionView in the page.
    /// </summary>
    public ObservableCollection<TimelineStep> Timeline { get; } = new();

    /// <summary>
    /// 4-entry step list bound to the <see cref="Syncfusion.Maui.ProgressBar.SfStepProgressBar"/>.
    /// Each entry carries its own <c>Status</c> (NotStarted /
    /// InProgress / Completed) so the Syncfusion step progress bar
    /// can render the journey in the design's "completed check /
    /// current check / pending check" pattern.
    /// </summary>
    public ObservableCollection<VisitStepItem> StepItems { get; } = new();

    /// <summary>
    /// Rich 4-checkpoint journey bound to the Digital Pass "Visit
    /// Journey Progression" panel. Each entry is a full checkpoint card
    /// (title + timestamp + description + two meta chips) whose node
    /// state (Completed / InProgress / NotStarted) is driven by the
    /// visitor's current status.
    /// </summary>
    public ObservableCollection<JourneyCheckpoint> Journey { get; } = new();

    /// <summary>
    /// Parallel collection of <see cref="Syncfusion.Maui.ProgressBar.StepProgressBarItem"/>
    /// entries the <c>SfStepProgressBar</c> binds to. The control accepts only
    /// <see cref="Syncfusion.Maui.ProgressBar.StepProgressBarItem"/> (or a
    /// collection wired through <c>StepProgressBarMapping</c>) — see the
    /// control's <c>OnItemsSourceChanged</c> path — so a custom-typed
    /// collection such as <see cref="Journey"/> would silently render no
    /// steps. We mirror <see cref="Journey"/> here so the bar's nodes
    /// line up with the rich checkpoint cards.
    /// </summary>
    public ObservableCollection<StepProgressBarItem> JourneySteps { get; } = new();

    /// <summary>Number of checkpoints cleared (0-4), shown in the "3 Checkpoints Cleared" chip.</summary>
    public int CheckpointsCleared { get; private set; }

    /// <summary>True when the page is currently resolving a store load.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>True when <see cref="Visitor"/> has been resolved.</summary>
    public bool HasVisitor => Visitor is not null;

    /// <summary>TRUE when the Check In button should be shown.</summary>
    public bool IsCheckInVisible =>
        Visitor is { } v
        && (v.Status == VisitorStatus.Expected || v.Status == VisitorStatus.Overdue);

    /// <summary>TRUE when the Check Out button should be shown.</summary>
    public bool IsCheckOutVisible => Visitor is { Status: VisitorStatus.CheckedIn };

    /// <summary>TRUE when the Cancel Visit button should be shown.</summary>
    public bool IsCancelVisible =>
        Visitor is { } v
        && (v.Status == VisitorStatus.Expected || v.Status == VisitorStatus.Overdue);

    // ----- Identity / facility card (left column, derived from Visitor) -----

    /// <summary>Facility headline shown in the identity card.</summary>
    public string FacilityName => "Central Facility";

    /// <summary>Clearance tier derived from the visit purpose.</summary>
    public string ClearanceTier => Visitor?.Purpose switch
    {
        "Business Meeting" => "Tier 2 Enterprise Clearance",
        "Interview" => "Tier 1 Personnel Clearance",
        "Package Delivery" => "Tier 0 Perimeter Clearance",
        "Maintenance Visit" => "Tier 3 Facilities Clearance",
        _ => "Tier 2 Enterprise Clearance",
    };

    /// <summary>Role / caption line under the visitor name.</summary>
    public string VisitorRole => Visitor?.Purpose switch
    {
        "Business Meeting" => "VP of Supply Chain",
        "Interview" => "Candidate — Engineering",
        "Package Delivery" => "Certified Courier",
        "Maintenance Visit" => "Facilities Contractor",
        _ => "Visitor",
    };

    /// <summary>Host caption line (designated host organisation). Shown with a building icon.</summary>
    public string HostOrganization => Visitor?.Host ?? string.Empty;

    /// <summary>Visitor category shown in the identity card (e.g. "Executive Vendor").</summary>
    public string VisitorCategory => Visitor?.Purpose switch
    {
        "Business Meeting" => "Executive Vendor",
        "Interview" => "Candidate",
        "Package Delivery" => "Courier",
        "Maintenance Visit" => "Facilities",
        _ => "Visitor",
    };

    /// <summary>Badge protocol derived from current status.</summary>
    public string BadgeProtocol => Visitor?.Status switch
    {
        VisitorStatus.Expected => "Escorted Access Pending",
        VisitorStatus.CheckedIn => "Escorted Access Active",
        VisitorStatus.CheckedOut => "Access Deactivated",
        VisitorStatus.Cancelled => "Access Revoked",
        VisitorStatus.Overdue => "Access Review Required",
        _ => "Escorted Access Only",
    };

    /// <summary>Designated host initials (avatar tile on the host block).</summary>
    public string HostInitials
    {
        get
        {
            var host = Visitor?.Host ?? string.Empty;
            var parts = host.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "H";
            if (parts.Length == 1) return parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant();
            return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpperInvariant();
        }
    }

    /// <summary>Static host department caption (presentation detail).</summary>
    public string HostDepartment => "Engineering • Floor 4, Suite 402B";

    /// <summary>Host extension, derived deterministically from the pass id (stable per visitor).</summary>
    public string HostExtension => "x" + PassSeed(4, 4);

    /// <summary>Host phone line, derived deterministically from the pass id.</summary>
    public string HostPhone => $"(555) 234-{PassSeed(4, 1000 + 8999)}";

    /// <summary>Zone permissions shown as a dot list on the identity card.</summary>
    public string[] ZonePermissions =>
    [
        "Building A",
        "North Tower Floor 4",
        "Exec Conference B",
    ];

    /// <summary>Long identifier under the barcode (e.g. "VP-8491-2024-ACME").</summary>
    public string PassIdentifier => Visitor is { } v
        ? $"{v.PassId}-{v.VisitDate:yyyy}-ACME"
        : string.Empty;

    public string BarcodeValue =>
        Visitor?.PassCode ?? "VP-EMPTY";

    /// <summary>NFC caption line (e.g. "NFC Active • UID: 04:A3:F2:89").</summary>
    public string NfcLine => Visitor?.Status == VisitorStatus.Cancelled
        ? "NFC Disabled • UID: 04:A3:F2:89"
        : $"NFC Active • UID: {HexPair(1)}:{HexPair(2)}:{HexPair(3)}";

    /// <summary>Escort policy notice paragraph using the host name.</summary>
    public string EscortNotice => Visitor is { } v
        ? $"Tier 2 non-employee guests must remain within designated visual line of sight with {v.Host} or an authorized facility escort in all Building A engineering labs."
        : "Non-employee guests must remain within designated escort zones.";

    // ----- Visit Summary & Telemetry (right column, derived from Visitor) -----

    /// <summary>TRUE when the visit is currently active (checked in or checked out).</summary>
    public bool IsActiveSession =>
        Visitor?.Status is VisitorStatus.CheckedIn or VisitorStatus.CheckedOut;

    /// <summary>Scheduled start, "h:mm tt".</summary>
    public string ScheduledDisplay => Visitor?.ScheduledStart.ToString("h:mm tt") ?? "—:— AM";

    /// <summary>Arrival time (check-in when available, else scheduled).</summary>
    public string ArrivalDisplay =>
        Visitor?.CheckInTime?.ToString("h:mm tt")
        ?? Visitor?.ScheduledStart.ToString("h:mm tt")
        ?? "—:— AM";

    /// <summary>Arrival caption ("+15m Early" / "On Time" / "—").</summary>
    public string ArrivalCaption => Visitor is { } v
        ? ArriveCaption(v.ScheduledStart - v.CheckInTime ?? TimeSpan.Zero)
        : "On Time";

    /// <summary>Departure estimate (scheduled end).</summary>
    public string DepartureDisplay => Visitor?.ScheduledEnd.ToString("h:mm tt") ?? "—:— AM";

    /// <summary>Departure caption ("1h 45m left" / "Completed").</summary>
    public string DepartureCaption => Visitor is { } v
        ? v.Status == VisitorStatus.CheckedOut
            ? "Completed"
            : DurationCaption(v.ScheduledStart, v.ScheduledEnd)
        : "—";

    /// <summary>Planned duration ("2h 00m").</summary>
    public string DurationDisplay => DurationCaption(
        VisitDateOrNow, VisitDateOrNow.AddMinutes(Visitor?.ExpectedDurationMinutes ?? 0));

    /// <summary>Elapsed time since arrival for the Duration tile caption ("35m elapsed" / "—").</summary>
    public string ElapsedDisplay => Visitor is { CheckInTime: { } ci }
        ? $"{Math.Max(0, (int)(DateTime.Now - ci).TotalMinutes)}m elapsed"
        : "—";

    /// <summary>Current zone shown in the telemetry grid.</summary>
    public string CurrentZone => "Floor 4";

    /// <summary>Current zone caption.</summary>
    public string CurrentZoneCaption => "Exec Conf B";

    /// <summary>Escort status caption (host + active state).</summary>
    public string EscortStatus => Visitor is { } v
        ? $"{v.Host} ({(v.Status == VisitorStatus.Cancelled ? "Inactive" : "Active")})"
        : "—";

    /// <summary>Wi-Fi guest token, derived deterministically from the pass id.</summary>
    public string WifiToken => $"PASS-GUEST-{PassSeed(4, 100 + 899)}";

    /// <summary>Parking bay, derived from the vehicle number when present.</summary>
    public string ParkingBay => Visitor?.VehicleNumber is { } vehicle
        ? $"Bay P2-{vehicle[^4..]} (Validated)"
        : "Bay P2-B14 (Validated)";

    /// <summary>Authorised visit window text, e.g. "09:30 AM – 11:30 AM".</summary>
    public string AuthorizedWindow => Visitor is { } v
        ? $"{v.ScheduledStart:h:mm tt} – {v.ScheduledEnd:h:mm tt}"
        : "—:— AM – —:— AM";

    /// <summary>Authorised window date caption, e.g. "Today, Oct 24".</summary>
    public string AuthorizedWindowDate =>
        Visitor?.VisitDate.ToString("MMM d") ?? "—";

    /// <summary>Expected duration display, e.g. "2 Hours (Standard)".</summary>
    public string ExpectedDurationDisplay =>
        Visitor?.ExpectedDurationMinutes is int minutes
        ? $"{DurationCaption(DateTime.MinValue, DateTime.MinValue.AddMinutes(minutes))} (Standard)"
        : "—";

    /// <summary>"Live Synchronized" header pill label (always live in this app).</summary>
    public string SyncPill => "Live Synchronized";

    /// <summary>Today's date, e.g. "Thursday, Oct 24" (OS bar date chip).</summary>
    public string TodayDisplay => Visitor?.VisitDate.ToString("dddd, MMM d") ?? DateTime.Now.ToString("dddd, MMM d");

    /// <summary>Revision caption under the header actions.</summary>
    public string Revision => "REV-4022.B";

    private static DateTime VisitDateOrNow => DateTime.Today;

    /// <summary>Stable integer derived from the pass id (used for deterministic detail fields).</summary>
    private int PassSeed(int salt, int modulus = int.MaxValue)
    {
        var id = Visitor?.PassId ?? string.Empty;
        var seed = salt;
        foreach (var c in id)
            seed = unchecked(seed * 31 + c);
        return Math.Abs(seed) % modulus;
    }

    private string HexPair(int salt)
    {
        var n = PassSeed(salt * 7, 0x100);
        return n.ToString("X2");
    }

    private static string ArriveCaption(TimeSpan delta)
    {
        var mins = (int)delta.TotalMinutes;
        if (mins <= 0) return "On Time";
        if (mins <= 59) return $"+{mins}m Early";
        return $"+{mins / 60}h {mins % 60}m Early";
    }

    private static string DurationCaption(DateTime start, DateTime end)
    {
        var total = (int)Math.Max(0, (end - start).TotalMinutes);
        return $"{total / 60}h {total % 60:00}m";
    }

    public VisitorDetailsViewModel(IVisitorStore store)
    {
        _store = store;
        // Keep the timeline in sync with store mutations — when another
        // screen (e.g. the Dashboard's today-grid) flips a row's
        // status, this screen's timeline reflects the new state the
        // next time the page is shown.
        _store.VisitorsChanged += OnVisitorsChanged;
    }

    /// <summary>
    /// Shell hook: called by <c>Shell</c> when navigation pushes a new
    /// details route. We stash the <c>passId</c> in
    /// <see cref="_pendingPassId"/> so <see cref="LoadAsync"/> can
    /// retrieve it (we don't load synchronously here because
    /// <c>FindByPassId</c> should run on the UI thread or after the
    /// shell transition completes).
    /// </summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(PassIdQueryKey, out var raw)
            && raw is string passId
            && !string.IsNullOrWhiteSpace(passId))
        {
            _pendingPassId = passId;
        }
    }

    /// <summary>
    /// Called from the Details page's OnAppearing handler. Resolves
    /// the pending pass-id (set by ApplyQueryAttributes or
    /// <see cref="SetPassId"/>) and rebuilds the timeline.
    /// </summary>
    public async Task LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(_pendingPassId))
            return;

        IsLoading = true;
        try
        {
            var visitor = _store.FindByPassId(_pendingPassId);
            Visitor = visitor;
            RebuildTimeline();
        }
        finally
        {
            IsLoading = false;
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Direct setter for the pending pass-id. Used by the page
    /// code-behind when navigation is via plain MAUI Navigation
    /// (no Shell query parameters).
    /// </summary>
    public void SetPassId(string passId)
    {
        if (!string.IsNullOrWhiteSpace(passId))
            _pendingPassId = passId;
    }

    /// <summary>
    /// Build the 4-step timeline from the visitor's current status.
    /// Steps 1..3 are always "Registered" / "Expected" /
    /// "Checked In"; step 4 is "Checked Out" (default) or
    /// "Cancelled" (when the visit is cancelled). Also rebuilds
    /// <see cref="StepItems"/> for the Syncfusion step progress bar and
    /// <see cref="Journey"/> for the full checkpoint cards.
    /// </summary>
    private void RebuildTimeline()
    {
        Timeline.Clear();
        StepItems.Clear();
        Journey.Clear();
        JourneySteps.Clear();
        if (Visitor is not { } v)
        {
            CheckpointsCleared = 0;
            NotifyDerived();
            return;
        }

        var s = v.Status;
        var checkedIn = v.CheckInTime ?? v.ScheduledStart;
        var scheduledEnd = v.ScheduledEnd;

        // Determine which checkpoint is the active one for each status.
        // Expected / Overdue  -> checkpoint 1 (pre-registration approved) is current
        // CheckedIn           -> checkpoint 3 (physical check-in verified) is current
        // CheckedOut / Cancelled -> all relevant checkpoints resolved.

        var regCompleted = true; // registration is always done once we exist here.
        var perimeterCompleted = s is VisitorStatus.CheckedIn or VisitorStatus.CheckedOut or VisitorStatus.Cancelled;
        var checkInCompleted = s is VisitorStatus.CheckedIn or VisitorStatus.CheckedOut;
        var departedCompleted = s == VisitorStatus.CheckedOut;

        var perimeterCurrent = s is VisitorStatus.Expected or VisitorStatus.Overdue;
        var checkInCurrent = s == VisitorStatus.CheckedIn;

        Journey.Add(new JourneyCheckpoint(
            1,
            "Pre-Registration Approved",
            $"{v.VisitDate:MMM d, h:mm tt}",
            $"Corporate host portal submission by {v.Host}. Identity pre-check confirmed with background accreditation.",
            "Self-Service Portal",
            "Auto-Cleared",
            StepStatus.Completed));

        Journey.Add(new JourneyCheckpoint(
            2,
            "Expected Perimeter Detection",
            $"Today, {v.ScheduledStart:h:mm tt}",
            "Approached Perimeter Gate B sensor. License plate recognized and dispatched to guest parking bay.",
            "Gate B Exterior",
            "ALPR Sensor #04",
            perimeterCompleted ? StepStatus.Completed
                : (perimeterCurrent ? StepStatus.InProgress : StepStatus.NotStarted)));

        Journey.Add(new JourneyCheckpoint(
            3,
            "Physical Check-In Verified",
            $"{(s is VisitorStatus.CheckedIn or VisitorStatus.CheckedOut ? "Today" : "Est.")}, {checkedIn:h:mm tt}",
            "Ingress logged at Desk #2 turnstiles. Badge token issued, escort notification sent via MS Teams.",
            "Physical SmartBadge #448",
            "Officer R. Sterling",
            checkInCompleted ? StepStatus.Completed
                : (checkInCurrent ? StepStatus.InProgress : StepStatus.NotStarted)));

        var departedTitle = s == VisitorStatus.Cancelled ? "Visit Cancelled" : "Departure Check-Out";
        var departedTime = s == VisitorStatus.Cancelled
            ? $"Today, {v.ScheduledStart:h:mm tt}"
            : $"{(s == VisitorStatus.CheckedOut ? "Today" : "Est.")}, {scheduledEnd:h:mm tt}";
        var departedDesc = s == VisitorStatus.Cancelled
            ? "Visit was cancelled by the host before ingress. All guest credentials were revoked and the badge deactivated."
            : "Awaiting departure egress turnstile tap or facility receptionist dispatch. Guest credentials will self-revoke.";
        Journey.Add(new JourneyCheckpoint(
            4,
            departedTitle,
            departedTime,
            departedDesc,
            "Turnstile Array A/B",
            "1h 45m Remaining",
            departedCompleted ? StepStatus.Completed
                : StepStatus.NotStarted));

        // Mirror the rich checkpoint list as StepProgressBarItem entries.
        // The SfStepProgressBar only renders items of type
        // StepProgressBarItem (or a collection wired through
        // StepProgressBarMapping); passing a JourneyCheckpoint collection
        // directly results in zero steps being drawn. Syncfusion's
        // StepProgressBarItem.Status setter is internal in 33.2.15 — we
        // reach it through reflection so the bar's node colours reflect
        // each checkpoint's Completed / InProgress / NotStarted state.
        foreach (var checkpoint in Journey)
        {
            var item = new StepProgressBarItem();
            StepItemStatusAccessor.Set(item, checkpoint.Status);
            JourneySteps.Add(item);
        }

        CheckpointsCleared = (int)(
            (regCompleted ? 1 : 0)
            + (perimeterCompleted ? 1 : 0)
            + (checkInCompleted ? 1 : 0)
            + (departedCompleted ? 1 : 0));

        // Keep the legacy 4-step Timeline + StepItems in sync (still bound elsewhere).
        Timeline.Add(new TimelineStep(
            Order: 1,
            Label: "Registered",
            SubLabel: v.PassId,
            IsCompleted: true,
            IsCurrent: false));

        var expectedOverdue = s == VisitorStatus.Overdue;
        Timeline.Add(new TimelineStep(
            Order: 2,
            Label: "Expected",
            SubLabel: v.ScheduledStart.ToString("t"),
            IsCompleted: !expectedOverdue && (s is VisitorStatus.CheckedIn or VisitorStatus.CheckedOut),
            IsCurrent: s == VisitorStatus.Expected || expectedOverdue,
            IsOverdue: expectedOverdue));

        Timeline.Add(new TimelineStep(
            Order: 3,
            Label: "Checked In",
            SubLabel: v.CheckInTime?.ToString("t") ?? "—",
            IsCompleted: s == VisitorStatus.CheckedOut,
            IsCurrent: s == VisitorStatus.CheckedIn));

        var step4Label = s == VisitorStatus.Cancelled ? "Cancelled" : "Checked Out";
        Timeline.Add(new TimelineStep(
            Order: 4,
            Label: step4Label,
            SubLabel: s == VisitorStatus.Cancelled
                ? "Visit did not proceed"
                : (v.CheckOutTime?.ToString("t") ?? "—"),
            IsCompleted: s == VisitorStatus.Cancelled || s == VisitorStatus.CheckedOut,
            IsCurrent: s == VisitorStatus.CheckedOut || s == VisitorStatus.Cancelled));

        StepItems.Add(new VisitStepItem("Pre-Registration Approved",
            $"{v.VisitDate:MMM d, h:mm tt}", StepStatus.Completed));
        StepItems.Add(new VisitStepItem("Expected Perimeter Detection",
            $"Today, {v.ScheduledStart:h:mm tt}",
            perimeterCompleted ? StepStatus.Completed : (perimeterCurrent ? StepStatus.InProgress : StepStatus.NotStarted)));
        StepItems.Add(new VisitStepItem("Physical Check-In Verified",
            $"{checkedIn:h:mm tt}",
            checkInCompleted ? StepStatus.Completed : (checkInCurrent ? StepStatus.InProgress : StepStatus.NotStarted)));
        StepItems.Add(new VisitStepItem(departedTitle,
            departedTime,
            departedCompleted ? StepStatus.Completed : StepStatus.NotStarted));

        OnPropertyChanged(nameof(CheckpointsCleared));
        NotifyDerived();
    }

    /// <summary>
    /// Raise change notifications for every derived, read-only property
    /// that depends on the current <see cref="Visitor"/> (identity card,
    /// telemetry, badge, host, escort). Called whenever the visitor is
    /// (re)resolved or its status transitions so the bound labels refresh.
    /// </summary>
    private void NotifyDerived()
    {
        OnPropertyChanged(nameof(FacilityName));
        OnPropertyChanged(nameof(ClearanceTier));
        OnPropertyChanged(nameof(VisitorRole));
        OnPropertyChanged(nameof(HostOrganization));
        OnPropertyChanged(nameof(VisitorCategory));
        OnPropertyChanged(nameof(BadgeProtocol));
        OnPropertyChanged(nameof(BarcodeValue));
        OnPropertyChanged(nameof(HostInitials));
        OnPropertyChanged(nameof(HostDepartment));
        OnPropertyChanged(nameof(HostExtension));
        OnPropertyChanged(nameof(HostPhone));
        OnPropertyChanged(nameof(ZonePermissions));
        OnPropertyChanged(nameof(PassIdentifier));
        OnPropertyChanged(nameof(NfcLine));
        OnPropertyChanged(nameof(EscortNotice));
        OnPropertyChanged(nameof(IsActiveSession));
        OnPropertyChanged(nameof(ScheduledDisplay));
        OnPropertyChanged(nameof(ArrivalDisplay));
        OnPropertyChanged(nameof(ArrivalCaption));
        OnPropertyChanged(nameof(DepartureDisplay));
        OnPropertyChanged(nameof(DepartureCaption));
        OnPropertyChanged(nameof(DurationDisplay));
        OnPropertyChanged(nameof(CurrentZone));
        OnPropertyChanged(nameof(CurrentZoneCaption));
        OnPropertyChanged(nameof(EscortStatus));
        OnPropertyChanged(nameof(WifiToken));
        OnPropertyChanged(nameof(ParkingBay));
        OnPropertyChanged(nameof(AuthorizedWindow));
        OnPropertyChanged(nameof(AuthorizedWindowDate));
        OnPropertyChanged(nameof(ExpectedDurationDisplay));
        OnPropertyChanged(nameof(SyncPill));
        OnPropertyChanged(nameof(Revision));
    }

    /// <summary>
    /// Called by the page when navigating away — clears the visitor
    /// so the next visit to <c>details</c> doesn't briefly show the
    /// old visitor.
    /// </summary>
    public void Clear()
    {
        Visitor = null;
        Timeline.Clear();
        StepItems.Clear();
        Journey.Clear();
        JourneySteps.Clear();
        CheckpointsCleared = 0;
        _pendingPassId = null;
        NotifyDerived();
    }

    [RelayCommand(CanExecute = nameof(IsCheckInVisible))]
    private void CheckIn()
    {
        if (Visitor is { } v)
            _store.CheckIn(v);
    }

    [RelayCommand(CanExecute = nameof(IsCheckOutVisible))]
    private void CheckOut()
    {
        if (Visitor is { } v)
            _store.CheckOut(v);
    }

    [RelayCommand(CanExecute = nameof(IsCancelVisible))]
    private void CancelVisit()
    {
        if (Visitor is { } v)
            _store.Cancel(v);
    }

    /// <summary>
    /// Subscribed to <see cref="IVisitorStore.VisitorsChanged"/> — when
    /// the visitor's status changes (whether from this screen's actions
    /// or another screen), rebuild the timeline and re-evaluate the
    /// button visibilities.
    /// </summary>
    private void OnVisitorsChanged(object? sender, Visitor? changed)
    {
        // If the visitor we host has been replaced with a new instance
        // (status transition creates a new wrapper object in the
        // IVisitorStore implementation), or its data changed, refresh.
        if (Visitor is null)
            return;
        if (changed is not null && changed != Visitor)
        {
            // Some other visitor changed; ignore.
            return;
        }
        RebuildTimeline();
        // Refresh derived button-visibility commands. We re-raise the
        // change notifications manually so [RelayCommand] attributes
        // re-evaluate CanExecute.
        OnPropertyChanged(nameof(IsCheckInVisible));
        OnPropertyChanged(nameof(IsCheckOutVisible));
        OnPropertyChanged(nameof(IsCancelVisible));
        CheckInCommand.NotifyCanExecuteChanged();
        CheckOutCommand.NotifyCanExecuteChanged();
        CancelVisitCommand.NotifyCanExecuteChanged();
    }
}

/// <summary>
/// VP-060 — one row in the Digital Pass timeline.
/// Plain POCO — the derived collection is rebuilt wholesale on every
/// status mutation, so we don't need INPC here.
/// </summary>
/// <param name="Order">1-based ordinal.</param>
/// <param name="Label">Step title ("Registered", "Expected", …).</param>
/// <param name="SubLabel">Time / status caption shown under the title.</param>
/// <param name="IsCompleted">TRUE when this step already happened.</param>
/// <param name="IsCurrent">TRUE for the active step (highlights).</param>
/// <param name="IsOverdue">TRUE only for the Expected step when the visitor is Overdue.</param>
public sealed record TimelineStep(
    int Order,
    string Label,
    string SubLabel,
    bool IsCompleted,
    bool IsCurrent,
    bool IsOverdue = false);

/// <summary>
/// VP-UI StepProgressBar — one entry in the
/// <see cref="Syncfusion.Maui.ProgressBar.SfStepProgressBar"/>.
/// Mapped onto <see cref="Syncfusion.Maui.ProgressBar.StepStatus"/>
/// (NotStarted / InProgress / Completed) so the Syncfusion control
/// renders the design's completed / current / pending pattern.
/// </summary>
/// <param name="Label">Step title (e.g. "Pre-Registration Approved").</param>
/// <param name="SubLabel">Time / status caption shown next to the title.</param>
/// <param name="Status">Drives the visual state on the step progress bar.</param>
public sealed record VisitStepItem(
    string Label,
    string SubLabel,
    StepStatus Status);

/// <summary>
/// Digital Pass "Visit Journey Progression" — one checkpoint card.
/// Carries everything a full checkpoint row needs: ordinal, title,
/// timestamp, description, two meta chips, and the node state that
/// drives the coloured circle on the vertical <see cref="SfStepProgressBar"/>.
/// </summary>
/// <param name="Order">1-based ordinal.</param>
/// <param name="Label">Checkpoint title.</param>
/// <param name="Timestamp">Right-aligned time caption.</param>
/// <param name="Description">Two-line body text.</param>
/// <param name="MetaLeft">First meta chip text.</param>
/// <param name="MetaRight">Second meta chip text.</param>
/// <param name="Status">Node state (Completed / InProgress / NotStarted).</param>
public sealed record JourneyCheckpoint(
    int Order,
    string Label,
    string Timestamp,
    string Description,
    string MetaLeft,
    string MetaRight,
    StepStatus Status)
{
    /// <summary>TRUE when this checkpoint is the active one (drives the "CURRENT STAGE" pill).</summary>
    public bool IsCurrent => Status == StepStatus.InProgress;

    /// <summary>TRUE when this checkpoint has completed.</summary>
    public bool IsCompleted => Status == StepStatus.Completed;
}

/// <summary>
/// Reflection shim for <see cref="StepProgressBarItem"/>.
/// Syncfusion 33.2.15's <c>Status</c> setter is <c>internal</c>; the
/// public <c>StepProgressBarItemMapping</c> only forwards
/// <c>PrimaryText</c> / <c>SecondaryText</c> / <c>ImageSource</c>. To
/// drive the bar's Completed / InProgress / NotStarted visuals from a
/// custom checkpoint list, we set <c>Status</c> through the property's
/// auto-generated setter. The setter is a plain CLR property — the
/// reflection cost is a one-off cached lookup.
/// </summary>
internal static class StepItemStatusAccessor
{
    private static readonly PropertyInfo StatusProperty =
        typeof(StepProgressBarItem).GetProperty(
            "Status",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException(
            "StepProgressBarItem.Status property not found; Syncfusion API changed.");

    public static void Set(StepProgressBarItem item, StepStatus status)
        => StatusProperty.SetValue(item, status);
}
