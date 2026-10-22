using System.Globalization;

namespace SmartMovieBooking;

public class TabIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || parameter is null)
            return null!;

        var activeTab = value.ToString();
        var tab = parameter.ToString();
        var isSelected = activeTab == tab;

        return tab switch
        {
            "Movies" => isSelected ? "moviehomelight.png" : "moviehome.png",
            "Theaters" => isSelected ? "theaterslight.png" : "theaters.png",
            "Bookings" => isSelected ? "bookinglight.png" : "booking.png",
            _ => null!
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}