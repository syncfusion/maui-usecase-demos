using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;
using Syncfusion.Maui.Chips;
using Syncfusion.Maui.ListView;

namespace SmartMovieBooking.Views;

public partial class MovieHomeView : ContentPage
{
    private readonly HomeViewModel _vm;
    private readonly BookingSession _session;

    public MovieHomeView(HomeViewModel vm, BookingSession session)
    {
        InitializeComponent();
        _vm = vm;
        _session = session;
        BindingContext = _vm;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _vm.LoadAsync();
        BuildGenreChips();
        LoadingSpinner.IsRunning = false;
        LoadingSpinner.IsVisible = false;
    }

    // Build the genre chip strip imperatively. SfChipGroup's ChipLayout
    // child has a known runtime-inflation issue (InflationException on
    // some platforms), so we build chips directly into a
    // HorizontalStackLayout instead.
    private void BuildGenreChips()
    {
        GenreChipStrip.Children.Clear();
        foreach (var genre in _vm.Genres)
        {
            var chip = new SfChip
            {
                Text = genre,
                ChipType = SfChipType.Choice,
                IsCheckable = true,
                ChipBackground = genre == _vm.SelectedGenre ? Color.FromArgb("#4F46E5") : Color.FromArgb("#F1F2F8"),
                ChipTextColor = genre == _vm.SelectedGenre ? Colors.White : Color.FromArgb("#49454F"),
                FontFamily = "OpenSansSemibold",
                FontSize = 11,
            };

            chip.CheckedChanged += (s, e) =>
            {
                if (e.IsChecked)
                    _vm.SelectedGenre = genre;
            };

            GenreChipStrip.Children.Add(chip);
        }
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
        => _vm.SearchText = e.NewTextValue ?? string.Empty;

    private async void OnMovieTapped(object? sender, ItemSelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not Movie m) return;

        // Store selection and navigate to the Theaters tab
        _session.SelectedMovie = m;
        _session.SelectedTheater = null;
        _session.SelectedShowtime = null;
        _session.SelectedDate = DateTime.Today;
        _session.SelectedSeats.Clear();

        MoviesList.SelectedItem = null; // reset so the same movie can be tapped again
        await Shell.Current.GoToAsync("//theaters");
    }
}
