using HotelManagerSample.Controls;
using HotelManagerSample;
using Syncfusion.Maui.DataGrid;
using Syncfusion.Maui.DataPager;

namespace HotelManagerSample;

/// <summary>
/// Code-behind for the desktop Bookings page. Wires the inline search
/// field, the SfDataPager reset hook, and the Add-Booking popup so the
/// view-model can stay pure-data. All view-model work happens through
/// the shared singleton <see cref="BookingsViewModel"/> resolved
/// from <see cref="App.Services"/>.
/// </summary>
public partial class BookingsPage : ContentPage
{
    private readonly BookingsViewModel _vm;

    /// <summary>Cached host for the AddBookingPopupContent control so we can
    /// re-subscribe its events every time the popup is re-opened.</summary>
    private AddBookingPopupContent? _addBookingContent;

    /// <summary>Initialises the XAML-inflated content tree, resolves
    /// the shared view-model, wires the Add-Booking popup lazily, and
    /// subscribes to the pager-reset event the VM raises whenever a
    /// filter pass changes the visible row count.</summary>
    public BookingsPage()
    {
        InitializeComponent();

        // TitleBarController.Apply() (App.xaml.cs OnLaunched) collapses the
        // OS title bar and paints the chrome in #F6F8FD, so the original
        // XAML padding of 24,10,24,28 stays unchanged on every platform.

        // Resolve the SHARED view-model from the DI container — it owns
        // its own subscription to the SHARED IHotelStore so the moment a
        // new booking is committed through the popup, this page's grid
        // and resulting pager index refresh from the same notifications
        // the DashboardPage and RoomsPage also listen on.
        App.CaptureServices();
        _vm = App.Services!.GetRequiredService<BookingsViewModel>();
        BindingContext = _vm;

        // Reset the pager to page 1 whenever the VM's filter rebuild lands.
        _vm.PagerResetRequested += OnPagerResetRequested;

        // Wire the Add-Booking popup's content control lazily so the
        // AddBookingViewModel inside it doesn't have to be constructed at
        // page-build time. The user activating the popup triggers inflation.
        AddBookingPopup.ContentTemplate = new DataTemplate(() =>
        {
            _addBookingContent = new AddBookingPopupContent();
            _addBookingContent.Committed += OnAddBookingCommitted;
            _addBookingContent.Cancelled += OnAddBookingCancelled;
            return _addBookingContent;
        });
    }

    /// <summary>Opens the Add-Booking popup bound to the
    /// <c>AddBookingPopup</c> Syncfusion control.</summary>
    /// <param name="sender">The control the user tapped. Unused.</param>
    /// <param name="e">Tapped event payload. Unused.</param>
    private void OnNewBookingTapped(object? sender, TappedEventArgs e)
    {
        if (AddBookingPopup is null) return;
        AddBookingPopup.IsOpen = true;
    }

    /// <summary>Closes the Add-Booking popup without committing a
    /// reservation. Raised by the popup's "Cancel" button.</summary>
    /// <param name="sender">The popup content. Unused.</param>
    /// <param name="e">Cancellation payload. Unused.</param>
    private void OnAddBookingCancelled(object? sender, EventArgs e)
    {
        AddBookingPopup.IsOpen = false;
    }

    /// <summary>Closes the Add-Booking popup once the popup content
    /// has finished committing the booking through the shared store.
    /// All KPI / pager refreshes ride on the BookingsChanged /
    /// RoomsChanged events the store fires — this handler only
    /// dismisses the popup so the page doesn't double-mutate.</summary>
    /// <param name="sender">The popup content. Unused.</param>
    /// <param name="result">The booking payload the popup
    /// constructed. Unused — kept for the contract.</param>
    private void OnAddBookingCommitted(object? sender, AddBookingFormResult result)
    {
        // The popup's view-model already called _store.AddBooking before
        // raising Committed, so the shared inventory + BookingsChanged +
        // RoomsChanged events have fired by the time we got here. Just
        // close the popup and let the data flow through every subscriber
        // (BookingsPage grid, RoomsPage tiles, DashboardPage KPIs).
        // Keeping a thin host reduces the risk of double-booking the
        // room if AddBooking runs on both sides.
        AddBookingPopup.IsOpen = false;
    }

    /// <summary>Reacts to the SfDataPager page-changed event by
    /// forwarding the new index + page size to the view-model so the
    /// range label and dynamic grid height refresh.</summary>
    /// <param name="sender">The SfDataPager. Unused.</param>
    /// <param name="e">Page-changed payload supplied by Syncfusion.</param>
    private void OnPagerPageChanged(object? sender, PageChangedEventArgs e)
    {
        if (BindingContext is not BookingsViewModel vm)
            return;

        vm.UpdatePaging(e.NewPageIndex, BookingsPager.PageSize);
    }

    /// <summary>Moves the SfDataPager back to page 1 whenever the
    /// VM's <see cref="BookingsViewModel.PagerResetRequested"/>
    /// event fires (e.g. after a chip-search narrows the visible
    /// row count).</summary>
    /// <param name="sender">The VM raising the event. Unused.</param>
    /// <param name="e">Empty event payload. Unused.</param>
    private void OnPagerResetRequested(object? sender, EventArgs e)
    {
        if (BookingsPager is null)
            return;

        BookingsPager.MoveToFirstPage();
    }

    /// <summary>Toggles the inline "×" clear-search label visibility
    /// based on whether the search entry holds any text. Driven by
    /// the entry's <c>TextChanged</c> event.</summary>
    /// <param name="sender">The Entry whose text changed. Unused.</param>
    /// <param name="e">Standard <see cref="TextChangedEventArgs"/>
    /// supplied by MAUI bindings.</param>
    private void OnBookingsSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (this.FindByName<Label>("BookingsSearchClear") is { } clear)
            clear.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);
    }

    /// <summary>Clears the search-entry text on tap of the trailing
    /// "×" glyph, which propagates through the TwoWay binding back
    /// into the view-model's filter.</summary>
    /// <param name="sender">The tapped glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnBookingsSearchClearTapped(object? sender, TappedEventArgs e)
    {
        if (BookingsSearchEntry is not null)
            BookingsSearchEntry.Text = string.Empty;
    }

    // ============== SfDataGrid interaction ==============

    /// <summary>Forwards the grid's selection into
    /// <see cref="BookingsViewModel.SelectedBooking"/>. The XAML already
    /// declares a two-way <c>SelectedItem</c> binding for the same
    /// property; this handler covers the case where the grid raises
    /// <c>SelectionChanged</c> without writing the binding source back
    /// (e.g. when a selection is cleared programmatically or shifted
    /// between paged sources).</summary>
    /// <param name="sender">The bookings SfDataGrid. Unused.</param>
    /// <param name="e">Syncfusion grid selection payload.</param>
    private void OnBookingsGridSelectionChanged(object? sender, DataGridSelectionChangedEventArgs e)
    {
        if (BindingContext is not BookingsViewModel vm)
            return;

        var added = e.AddedRows;
        var removed = e.RemovedRows;
        object? selected = added is { Count: > 0 } ? added[0] : null;
        if (selected is null && sender is SfDataGrid grid)
            selected = grid.SelectedRow;

        // Treat a non-empty removal as a deselection.
        if (selected is null && removed is { Count: > 0 })
            selected = null;

        if (!ReferenceEquals(vm.SelectedBooking, selected))
            vm.SelectedBooking = selected;
    }

    /// <summary>Row-cell-tap handler. Demonstrates the grid's built-in
    /// <c>CellTapped</c> API — tapping anywhere in a row (not just the
    /// header) surfaces the reservation in the detail strip below the
    /// pager. The two-way <c>SelectedItem</c> binding does the same
    /// thing; this handler is here so the sample shows both entry
    /// points a customer would reach for.</summary>
    /// <param name="sender">The bookings SfDataGrid. Unused.</param>
    /// <param name="e">Cell-tapped payload carrying the tapped row.</param>
    private void OnBookingsGridCellTapped(object? sender, DataGridCellTappedEventArgs e)
    {
        if (e.RowData is SelectableBooking row)
            _vm.SelectedBooking = row;
    }

    /// <summary>Drops the grid selection and hides the detail strip when
    /// the user taps the "×" in the strip's trailing cell.</summary>
    /// <param name="sender">The tapped glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnClearSelectionTapped(object? sender, TappedEventArgs e)
    {
        _vm.ClearBookingSelection();
        if (BookingsGrid is not null)
            BookingsGrid.SelectedRow = null!;
    }
}
