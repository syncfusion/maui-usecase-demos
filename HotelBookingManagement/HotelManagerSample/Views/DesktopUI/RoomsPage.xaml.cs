using HotelManagerSample;

namespace HotelManagerSample;

/// <summary>
/// Code-behind for the desktop Rooms page. Wires the inline search
/// field's clear-glyph behaviour. The dynamic grid height and the
/// store-event refreshes flow through the shared singleton
/// <see cref="RoomsViewModel"/> resolved from DI — there is no
/// imperative height math left in this file.
/// </summary>
public partial class RoomsPage : ContentPage
{
    /// <summary>Initialises the XAML-inflated tree, resolves the
    /// shared view-model from DI, and binds it as the page's
    /// <see cref="BindableObject.BindingContext"/>.</summary>
    public RoomsPage()
    {
        InitializeComponent();

        App.CaptureServices();
        var vm = App.Services!.GetRequiredService<RoomsViewModel>();
        BindingContext = vm;
    }

    /// <summary>Toggles the inline "×" clear-search label visibility
    /// for the rooms-page search entry.</summary>
    /// <param name="sender">The search entry. Unused.</param>
    /// <param name="e">Text-changed payload from MAUI bindings.</param>
    private void OnRoomsSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (this.FindByName<Label>("RoomsSearchClear") is { } clear)
            clear.IsVisible = !string.IsNullOrEmpty(e.NewTextValue);
    }

    /// <summary>Clears the rooms-page search entry on tap of the
    /// trailing "×" glyph.</summary>
    /// <param name="sender">The tapped glyph. Unused.</param>
    /// <param name="e">Tapped-event payload. Unused.</param>
    private void OnRoomsSearchClearTapped(object? sender, TappedEventArgs e)
    {
        if (RoomsSearchEntry is not null)
            RoomsSearchEntry.Text = string.Empty;
    }
}