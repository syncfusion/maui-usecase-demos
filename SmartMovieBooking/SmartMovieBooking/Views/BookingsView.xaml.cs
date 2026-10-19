using SmartMovieBooking.Models;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class BookingsView : ContentPage
{
    private readonly BookingsViewModel _vm;

    public BookingsView(BookingsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
        _vm.Bookings.CollectionChanged += (_, _) => Rebuild();
        Rebuild();
    }

    private void Rebuild()
    {
        BookingsList.Children.Clear();
        if (_vm.Bookings.Count == 0)
        {
            EmptyState.IsVisible = true;
            return;
        }
        EmptyState.IsVisible = false;
        foreach (var b in _vm.Bookings)
            BookingsList.Children.Add(BuildCard(b));
    }

    private Border BuildCard(Booking b)
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

        var stack = new VerticalStackLayout { Spacing = 6 };
        stack.Add(new Label { Text = b.Movie?.Title ?? "Movie", FontFamily = "OpenSansSemibold", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#1A1A2E") });
        stack.Add(new Label { Text = $"🎬 {b.Theater?.Name} · {b.Showtime?.StartTime}", FontSize = 12, TextColor = Color.FromArgb("#49454F") });
        var seatLabel = new Label
        {
            Text = string.Join(", ", b.Seats.Select(s => s.Id)),
            FontSize = 12,
            TextColor = Color.FromArgb("#3525CD"),
            FontFamily = "OpenSansSemibold"
        };
        stack.Add(seatLabel);
        stack.Add(new Label { Text = $"Booking ID: {b.BookingId} · Total: ${b.Total:F2}", FontSize = 11, TextColor = Color.FromArgb("#7A7B8C") });
        card.Content = stack;
        return card;
    }
}
