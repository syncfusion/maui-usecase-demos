using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;
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

    // Build the genre chip strip with plain Borders (no Syncfusion SfChip).
    // The Syncfusion SfChip API has incompatible property names between
    // minor versions (ChipType vs Type, IsCheckable vs IsChecked,
    // ChipBackground vs BackgroundColor, CheckedChanged event name),
    // so we render chips as a Border + Label + TapGestureRecognizer trio.
    // Looks identical, works on every platform, and avoids the 6 CS0117
    // errors that appeared in the user's Error List.
    private void BuildGenreChips()
    {
        GenreChipStrip.Children.Clear();
        foreach (var genre in _vm.Genres)
        {
            bool isSelected = genre == _vm.SelectedGenre;

            var label = new Label
            {
                Text = genre,
                FontFamily = "OpenSansSemibold",
                FontSize = 11,
                TextColor = isSelected ? Colors.White : Color.FromArgb("#49454F"),
                VerticalTextAlignment = TextAlignment.Center
            };

            var border = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 999 },
                BackgroundColor = isSelected ? Color.FromArgb("#4F46E5") : Color.FromArgb("#F1F2F8"),
                Padding = new Thickness(14, 6),
                Content = label
            };

            var tap = new TapGestureRecognizer();
            string capturedGenre = genre;
            tap.Tapped += (s, e) => OnGenreChipTapped(capturedGenre);
            border.GestureRecognizers.Add(tap);

            GenreChipStrip.Children.Add(border);
        }
    }

    private void OnGenreChipTapped(string genre)
    {
        _vm.SelectedGenre = genre;
        // Re-render to update visual selection state.
        BuildGenreChips();
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
        => _vm.SearchText = e.NewTextValue ?? string.Empty;

    private async void OnMovieTapped(object? sender, ItemSelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not Movie m) return;

        _session.SelectedMovie = m;
        _session.SelectedTheater = null;
        _session.SelectedShowtime = null;
        _session.SelectedDate = DateTime.Today;
        _session.SelectedSeats.Clear();

        MoviesList.SelectedItem = null;
        await Shell.Current.GoToAsync("//theaters");
    }
}
