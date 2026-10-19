using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class SeatSelectionView : ContentPage
{
    private readonly SeatSelectionViewModel _vm;
    private readonly BookingSession _session;

    public SeatSelectionView(SeatSelectionViewModel vm, BookingSession session)
    {
        InitializeComponent();
        _vm = vm;
        _session = session;
        BindingContext = _vm;
        _vm.Confirmed += (_, _) => OnConfirmInternal();
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await _vm.InitializeAsync();
        ScreenTitleLabel.Text = _vm.ScreenTitle;
        if (_session.SelectedTheater is not null && _session.SelectedShowtime is not null)
            TheaterLabel.Text = $"{_session.SelectedTheater.Name} · {_session.SelectedShowtime.StartTime}";

        SeatRows.Children.Clear();
        foreach (var row in _vm.SeatRows)
            SeatRows.Children.Add(BuildRow(row));

        UpdateTotal();
    }

    private Border BuildRow(SeatRow row)
    {
        var rowContainer = new HorizontalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center };
        var labelL = new Label { Text = row.Label, FontFamily = "OpenSansSemibold", FontSize = 11, TextColor = Color.FromArgb("#7A7B8C"), VerticalTextAlignment = TextAlignment.Center, WidthRequest = 14 };
        rowContainer.Children.Add(labelL);
        foreach (var seat in row.Seats)
        {
            var seatBox = BuildSeatBox(seat);
            rowContainer.Children.Add(seatBox);
        }
        var labelR = new Label { Text = row.Label, FontFamily = "OpenSansSemibold", FontSize = 11, TextColor = Color.FromArgb("#7A7B8C"), VerticalTextAlignment = TextAlignment.Center, WidthRequest = 14 };
        rowContainer.Children.Add(labelR);

        var border = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(8, 4),
            BackgroundColor = Color.FromArgb("#F8F9FF")
        };
        border.Content = rowContainer;
        return border;
    }

    private Border BuildSeatBox(Seat seat)
    {
        var color = seat.State switch
        {
            SeatState.Occupied => Color.FromArgb("#9CA3AF"),
            SeatState.Selected => Color.FromArgb("#3525CD"),
            SeatState.Aisle => Colors.Transparent,
            _ => Color.FromArgb("#E5E6F1")
        };
        var border = new Border
        {
            BackgroundColor = color,
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
            WidthRequest = 24,
            HeightRequest = 24,
            Opacity = seat.State == SeatState.Occupied ? 0.5 : 1.0
        };
        var seatLabel = new Label { Text = seat.Id, FontSize = 7, FontFamily = "OpenSansSemibold",
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            TextColor = seat.State == SeatState.Selected ? Colors.White : Color.FromArgb("#49454F") };
        border.Content = seatLabel;
        if (seat.State != SeatState.Occupied && seat.State != SeatState.Aisle)
        {
            var tap = new TapGestureRecognizer();
            Seat captured = seat;
            tap.Tapped += (s, e) => OnSeatTapped(captured);
            border.GestureRecognizers.Add(tap);
        }
        return border;
    }

    private void OnSeatTapped(Seat seat)
    {
        _vm.ToggleSeatCommand.Execute(seat);
        UpdateTotal();
        // Rebuild all rows to reflect new selection state
        SeatRows.Children.Clear();
        foreach (var row in _vm.SeatRows)
            SeatRows.Children.Add(BuildRow(row));
    }

    private void UpdateTotal()
    {
        TotalLabel.Text = $"${_vm.Total:F2}";
    }

    private void OnConfirmClicked(object? sender, EventArgs e)
    {
        _vm.ConfirmCommand.Execute(null);
    }

    private void OnConfirmInternal()
    {
        Shell.Current.GoToAsync("payment-confirmation");
    }
}
