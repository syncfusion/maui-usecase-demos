using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class MovieHomeView : ContentPage
{
    private readonly HomeViewModel _vm;
    private readonly BookingSession _session;
    private bool _suppressTextChanged;

    public MovieHomeView(HomeViewModel vm, BookingSession session)
    {
        InitializeComponent();
        _vm = vm;
        _session = session;
        BindingContext = _vm;
        BuildGenreChips();
        _ = LoadAsync();
    }

    private void BuildGenreChips()
    {
        GenreChips.Children.Clear();
        foreach (var g in _vm.Genres)
        {
            var border = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 999 },
                Padding = new Thickness(14, 6),
                BackgroundColor = Color.FromArgb("#F1F2F8")
            };
            var label = new Label
            {
                Text = g,
                FontFamily = "OpenSansSemibold",
                FontSize = 11,
                TextColor = Color.FromArgb("#49454F")
            };
            border.Content = label;

            var tap = new TapGestureRecognizer();
            string captured = g;
            tap.Tapped += (s, e) => { _suppressTextChanged = true; _vm.SelectedGenre = captured; _suppressTextChanged = false; UpdateChipVisuals(); };
            border.GestureRecognizers.Add(tap);

            GenreChips.Children.Add(border);
        }
        UpdateChipVisuals();
    }

    private void UpdateChipVisuals()
    {
        for (int i = 0; i < GenreChips.Children.Count && i < _vm.Genres.Count; i++)
        {
            var border = (Border)GenreChips.Children[i];
            var label = (Label)border.Content;
            if (_vm.Genres[i] == _vm.SelectedGenre)
            {
                border.BackgroundColor = Color.FromArgb("#3525CD");
                label.TextColor = Colors.White;
            }
            else
            {
                border.BackgroundColor = Color.FromArgb("#F1F2F8");
                label.TextColor = Color.FromArgb("#49454F");
            }
        }
    }

    private async Task LoadAsync()
    {
        await _vm.LoadAsync();
        MoviesList.ItemsSource = _vm.FilteredMovies;
        MoviesList.SelectionChanged += OnMovieSelected;
        LoadingSpinner.IsVisible = false;
        MoviesList.IsVisible = true;
        _vm.FilteredMovies.CollectionChanged += (_, _) => UpdateChipVisuals();
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HomeViewModel.FilteredMovies))
                MoviesList.ItemsSource = _vm.FilteredMovies;
        };
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
    {
        if (_suppressTextChanged) return;
        _vm.SearchText = e.NewTextValue ?? string.Empty;
    }

    private void OnMovieSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Movie m)
        {
            _session.SelectedMovie = m;
            _session.SelectedTheater = null;
            _session.SelectedShowtime = null;
            _session.SelectedDate = DateTime.Today;
            _session.SelectedSeats.Clear();
            MoviesList.SelectedItem = null;
            Shell.Current.GoToAsync("theaters");
        }
    }
}
