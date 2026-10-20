using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

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
    // Syncfusion's SfChip API has incompatible property names between minor
    // versions, so a plain Border + Label + TapGestureRecognizer is used.
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
        BuildGenreChips();
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
        => _vm.SearchText = e.NewTextValue ?? string.Empty;

    // Called from TapGestureRecognizer on each movie card (whole-card tap).
    private void OnMovieCardTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not Movie movie) return;
        NavigateToTheatersFor(movie);
    }

    // Called from the "Book Now" SfButton inside each card. The SfButton's
    // BindingContext is the Movie (inherited from the SfCardView parent).
    private void OnBookNowClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is Movie movie)
            NavigateToTheatersFor(movie);
    }

    // Called from SfListView's SelectionChanged when the user clicks a row.
    private void OnMovieTapped(object? sender, Syncfusion.Maui.ListView.ItemSelectionChangedEventArgs e)
    {
        if (e.AddedItems.FirstOrDefault() is not Movie movie) return;
        MoviesList.SelectedItem = null;
        NavigateToTheatersFor(movie);
    }

    private void NavigateToTheatersFor(Movie movie)
    {
        _session.SelectedMovie = movie;
        _session.SelectedTheater = null;
        _session.SelectedShowtime = null;
        _session.SelectedDate = DateTime.Today;
        _session.SelectedSeats.Clear();

        // Shell.Current.GoToAsync with "//" prefix navigates to an
        // absolute tab - in this case the Theaters tab.
        MainThread.BeginInvokeOnMainThread(() => _ = Shell.Current.GoToAsync("//theaters"));
    }
}