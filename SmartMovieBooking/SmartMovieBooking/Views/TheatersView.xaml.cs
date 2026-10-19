using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class TheatersView : ContentPage
{
    private readonly TheatersViewModel _vm;
    private readonly BookingSession _session;

    public TheatersView(TheatersViewModel vm, BookingSession session)
    {
        InitializeComponent();
        _vm = vm;
        _session = session;
        BindingContext = _vm;
        BuildDateStrip();
        _ = LoadAsync();
    }

    private void BuildDateStrip()
    {
        DateStrip.Children.Clear();
        foreach (var d in _vm.Dates)
        {
            var border = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Padding = new Thickness(10, 6),
                WidthRequest = 64,
                BackgroundColor = d.Date == _vm.SelectedDate.Date ? Color.FromArgb("#3525CD") : Color.FromArgb("#FFFFFF")
            };
            var stack = new VerticalStackLayout();
            stack.Add(new Label { Text = _vm.FormatDateLabel(d), FontFamily = "OpenSansSemibold", FontSize = 9, HorizontalTextAlignment = TextAlignment.Center, TextColor = d.Date == _vm.SelectedDate.Date ? Colors.White : Color.FromArgb("#7A7B8C") });
            stack.Add(new Label { Text = d.ToString("dd"), FontFamily = "OpenSansSemibold", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, TextColor = d.Date == _vm.SelectedDate.Date ? Colors.White : Color.FromArgb("#1A1A2E") });
            stack.Add(new Label { Text = d.ToString("ddd • MMM"), FontFamily = "OpenSansSemibold", FontSize = 9, HorizontalTextAlignment = TextAlignment.Center, TextColor = d.Date == _vm.SelectedDate.Date ? Colors.White : Color.FromArgb("#49454F") });
            border.Content = stack;

            var tap = new TapGestureRecognizer();
            DateTime captured = d;
            tap.Tapped += async (s, e) =>
            {
                _vm.SelectedDate = captured;
                BuildDateStrip();
                await LoadAsync();
            };
            border.GestureRecognizers.Add(tap);
            DateStrip.Children.Add(border);
        }
    }

    private async Task LoadAsync()
    {
        await _vm.LoadAsync();
        LoadingSpinner.IsVisible = false;
        TheaterList.IsVisible = true;
        TheaterList.Children.Clear();

        if (_vm.MovieTitle != null) MovieSubtitle.Text = $"For: {_vm.MovieTitle}";

        foreach (var theater in _vm.Theaters)
        {
            var card = BuildTheaterCard(theater);
            TheaterList.Children.Add(card);
        }
    }

    private Border BuildTheaterCard(Theater theater)
    {
        var card = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Color.FromArgb("#FFFFFF"),
            Padding = new Thickness(16)
        };
        var shadow = new Shadow { Brush = Color.FromArgb("#1A000000"), Offset = new Point(0, 2), Radius = 6, Opacity = 0.08f };
        card.Shadow = shadow;

        var stack = new VerticalStackLayout { Spacing = 12 };

        // Header
        var headerGrid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection(
            new ColumnDefinition { Width = new GridLength(48) },
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            new ColumnDefinition { Width = GridLength.Auto }
        ), ColumnSpacing = 12 };
        var iconBox = new Border { BackgroundColor = Color.FromArgb("#F1F2F8"), WidthRequest = 48, HeightRequest = 48, StrokeThickness = 0, StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 } };
        iconBox.Content = new Label { Text = theater.Icon, FontSize = 22, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
        Grid.SetColumn(iconBox, 0);
        headerGrid.Children.Add(iconBox);

        var nameStack = new VerticalStackLayout { Spacing = 2 };
        nameStack.Add(new Label { Text = theater.Name, FontFamily = "OpenSansSemibold", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1A1A2E") });
        nameStack.Add(new Label { Text = theater.Location, FontSize = 11, TextColor = Color.FromArgb("#49454F") });
        nameStack.Add(new Label { Text = theater.DistanceLabel, FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#3525CD") });
        Grid.SetColumn(nameStack, 1);
        headerGrid.Children.Add(nameStack);

        var priceLabel = new Label { Text = theater.PriceFrom, FontFamily = "OpenSansSemibold", FontSize = 13, TextColor = Color.FromArgb("#3525CD"), VerticalTextAlignment = TextAlignment.Center };
        Grid.SetColumn(priceLabel, 2);
        headerGrid.Children.Add(priceLabel);
        stack.Add(headerGrid);

        // Showtimes
        if (theater.Showtimes?.Count > 0)
        {
            var pillsWrap = new FlexLayout { Wrap = FlexWrap.Wrap, JustifyContent = FlexJustify.Start };
            foreach (var s in theater.Showtimes)
            {
                var pill = BuildShowtimePill(s, theater);
                pillsWrap.Children.Add(pill);
            }
            stack.Add(pillsWrap);
        }

        card.Content = stack;
        return card;
    }

    private Border BuildShowtimePill(Showtime s, Theater theater)
    {
        var pill = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Padding = new Thickness(10, 6),
            Margin = new Thickness(0, 4, 6, 0),
            BackgroundColor = Color.FromArgb("#F1F2F8")
        };
        var pillStack = new VerticalStackLayout { Spacing = 0 };
        pillStack.Add(new Label { Text = s.StartTime, FontFamily = "OpenSansSemibold", FontSize = 12, TextColor = Color.FromArgb("#1A1A2E") });
        pillStack.Add(new Label { Text = s.Format, FontFamily = "OpenSansSemibold", FontSize = 9, TextColor = Color.FromArgb("#3525CD") });
        pillStack.Add(new Label { Text = _vm.FormatShowtimeStatus(s.Status), FontFamily = "OpenSansSemibold", FontSize = 9, TextColor = _vm.StatusColor(s.Status) });
        pill.Content = pillStack;

        var tap = new TapGestureRecognizer();
        Showtime capturedS = s;
        Theater capturedT = theater;
        tap.Tapped += (s2, e2) => OnShowtimeSelected(capturedT, capturedS);
        pill.GestureRecognizers.Add(tap);
        return pill;
    }

    private void OnShowtimeSelected(Theater theater, Showtime showtime)
    {
        _session.SelectedTheater = theater;
        _session.SelectedShowtime = showtime;
        _session.SelectedDate = _vm.SelectedDate;
        Shell.Current.GoToAsync("seat-selection");
    }
}
