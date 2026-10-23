using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HotelManagerSample;

/// <summary>
/// View-model for the mobile shell. The shell is a single SfTabView
/// container that hosts the three mobile pages (Dashboard, Bookings,
/// Rooms). It exposes a single <see cref="SelectedTabIndex"/> property
/// the tab bar can two-way bind to, plus a
/// <see cref="NewBookingCommand"/> that the Floating Action Button
/// invokes. The shell raises <see cref="NewBookingRequested"/> so any
/// page (or shell) can react to the request.
/// </summary>
public sealed class MobileShellViewModel : INotifyPropertyChanged
{
    /// <summary>Index of the Dashboard tab in the SfTabView.</summary>
    public const int DashboardTabIndex = 0;
    /// <summary>Index of the Bookings tab in the SfTabView.</summary>
    public const int BookingsTabIndex = 1;
    /// <summary>Index of the Rooms tab in the SfTabView.</summary>
    public const int RoomsTabIndex = 2;
    /// <summary>Kept for legacy callers but the More tab was removed —
    /// pinned to the same index as Rooms so any stray navigation silently
    /// degrades to the Rooms surface.</summary>
    public const int MoreTabIndex = RoomsTabIndex;

    /// <summary>Standard <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
    /// event the MAUI binding engine observes to keep XAML bindings in sync
    /// with the VM's derived properties.</summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <summary>Raises <see cref="PropertyChanged"/> for the supplied
    /// property name. Caller-supplied so derived helpers can name
    /// properties by string.</summary>
    /// <param name="name">Name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private int _selectedTabIndex = DashboardTabIndex;
    /// <summary>Two-way bound to the SfTabView's
    /// <c>SelectedIndex</c> / <c>SelectedItem</c> property. Pages can set
    /// it to programmatically switch tabs (e.g. after creating a new
    /// booking the wizard pops back to the Bookings list).</summary>
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            // Three tabs: Dashboard (0), Bookings (1), Rooms (2).
            var clamped = Math.Clamp(value, 0, 2);
            // Guard against the SfTabView two-way binding bouncing the
            // value back to the getter before the backing field is
            // updated — and against re-notifying when nothing changed
            // (which causes a visible re-selection flicker).
            if (_selectedTabIndex == clamped)
                return;
            _selectedTabIndex = clamped;
            OnPropertyChanged();
        }
    }

    /// <summary>True while the New-Booking wizard is on top of the tab.
    /// Pages bind IsEnabled off this so they don't capture taps while the
    /// modal is alive.</summary>
    private bool _isNewBookingOpen;
    /// <summary>True while the New-Booking wizard is on top of the tab.
    /// Pages bind <c>IsEnabled</c> off this so they don't capture taps
    /// while the modal is alive.</summary>
    public bool IsNewBookingOpen
    {
        get => _isNewBookingOpen;
        set
        {
            if (_isNewBookingOpen == value) return;
            _isNewBookingOpen = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsTabBarEnabled));
        }
    }

    /// <summary>SfTabView.IsEnabled binding source.</summary>
    public bool IsTabBarEnabled => !_isNewBookingOpen;

    /// <summary>Raised when the user taps the floating "New Booking"
    /// button on the dashboard / bookings page. The shell listens for it
    /// to push the modal wizard.</summary>
    public event EventHandler? NewBookingRequested;

    /// <summary>Command the Floating Action Button binds to.</summary>
    public ICommand NewBookingCommand { get; }
    /// <summary>Command bound to the tab bar's selected-index change.</summary>
    public ICommand SwitchToTabCommand { get; }

    /// <summary>Constructs the shell VM, wires the NewBookingCommand to
    /// raise <see cref="NewBookingRequested"/>, and wires
    /// <see cref="SwitchToTabCommand"/> to update
    /// <see cref="SelectedTabIndex"/>.</summary>
    public MobileShellViewModel()
    {
        NewBookingCommand = new Command(() => NewBookingRequested?.Invoke(this, EventArgs.Empty));
        SwitchToTabCommand = new Command<int>(idx => SelectedTabIndex = idx);
    }
}
