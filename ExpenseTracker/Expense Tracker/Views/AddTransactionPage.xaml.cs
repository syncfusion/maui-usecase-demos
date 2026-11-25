using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Calendar;

namespace SmartExpense;

/// <summary>
/// A page that allows users to add a new transaction, including selecting a category and entering transaction details.
/// </summary>
public partial class AddTransactionPage : ContentPage
{
    private AddTransactionViewModel? ViewModel => BindingContext as AddTransactionViewModel;
    private bool _initialized;
    // Track scroll positions so we can restore after keyboard hides
    private double _lastScrollY = 0;
    private double _savedScrollY = 0;

    public AddTransactionPage()
    {
        InitializeComponent();
        // Only pick up a DI-provided viewmodel when no BindingContext was pre-assigned
        if (BindingContext is null)
            BindingContext = IPlatformApplication.Current?.Services.GetService<AddTransactionViewModel>();

        // Track scrolling so we can restore position when the keyboard is dismissed
        try
        {
            MainScroll.Scrolled += MainScroll_Scrolled;
        }
        catch (Exception ignoredEx) { System.Diagnostics.Debug.WriteLine(ignoredEx); }
    }

    /// <summary>
    /// Handler is set when the page is displayed. This method ensures that the categories are loaded only once when the page is first shown, preventing unnecessary reloads on subsequent displays. It uses the Dispatcher to execute the loading of categories asynchronously, allowing the UI to remain responsive while the data is being fetched.
    /// </summary>
    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler is not null && !_initialized)
        {
            _initialized = true;

            Dispatcher.Dispatch(async () =>
            {
                try
                {
                    if (ViewModel?.LoadCategoriesCommand is not null)
                        await ViewModel.LoadCategoriesCommand.ExecuteAsync(null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AddTransactionPage init error: {ex}");
                }
            });
        }
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        calendar.IsOpen = true;
    }

    private void Calendar_SelectionChanged(object sender,
     CalendarSelectionChangedEventArgs e)
    {
        // ? DO NOTHING HERE
        // Selection is temporary until OK is pressed
    }

    private void Calendar_ActionButtonClicked(object? sender, EventArgs e)
    {
        if (BindingContext is AddTransactionViewModel vm)
            vm.DateSelectedCommand.Execute(calendar.SelectedDate);

        calendar.IsOpen = false;
    }

    private void Calendar_ActionButtonCanceled(object? sender, EventArgs e)
    {
        // ? Just close, no commit
        calendar.IsOpen = false;
    }

    private void  SfButton_Clicked(object sender, EventArgs e)
    {
        // Try to pop the current navigation stack. Use awaiting
        // to ensure smooth async navigation and handle fallbacks.
        _ = ClosePageAsync();
    }


    private void NotesEditor_Focused(object sender, FocusEventArgs e)
    {
        // Save the current scroll position so we can restore it on unfocus
        _savedScrollY = _lastScrollY;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // Wait for keyboard to appear and layout to resize.
                var delay = 200;
                if (Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.Android)
                    delay = 300;

                await Task.Delay(delay);

                // Try to scroll the NotesEditor to the top of the visible area.
                await MainScroll.ScrollToAsync(NotesEditor, ScrollToPosition.Start, true);

                // Sometimes the first scroll happens before final resize; try again shortly after.
                await Task.Delay(200);
                await MainScroll.ScrollToAsync(NotesEditor, ScrollToPosition.Start, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NotesEditor_Focused scroll error: {ex}");
            }
        });
    }

    private void NotesEditor_Unfocused(object sender, FocusEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // Allow time for keyboard to hide and layout to settle
                var delay = 200;
                if (Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.Android)
                    delay = 350;

                await Task.Delay(delay);

                // Restore the scroll position saved before focus
                await MainScroll.ScrollToAsync(0, _savedScrollY, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NotesEditor_Unfocused restore error: {ex}");
            }
        });
    }

    private void MainScroll_Scrolled(object? sender, ScrolledEventArgs e)
    {
        _lastScrollY = e.ScrollY;
    }


    private async Task ClosePageAsync()
    {
        try
        {
            if (Navigation is not null && Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
                return;
            }

            var page = this.GetParentPage();
            if (page?.Navigation is not null && page.Navigation.NavigationStack.Count > 1)
            {
                await page.Navigation.PopAsync();
                return;
            }

            var mainNav = Application.Current?.MainPage?.Navigation;
            if (mainNav is not null && mainNav.NavigationStack.Count > 1)
            {
                await mainNav.PopAsync();
                return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Close navigation error: {ex}");
        }
    }
}

public static class ViewExtensions
{
    public static Page? GetParentPage(this Element element)
    {
        while (element != null)
        {
            if (element is Page page)
                return page;

            element = element.Parent;
        }
        return null;
    }
}