using SmartMovieBooking.Models;
using System.Globalization;

namespace SmartMovieBooking;

public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : (object)false;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : (object)false;
}

public sealed class CountToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int n && n > 0;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class CountToInverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int n && n == 0;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class StringEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && parameter is string p && string.Equals(s, p, StringComparison.OrdinalIgnoreCase);
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class NotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null && !(value is string s && string.IsNullOrWhiteSpace(s));
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class CurrencyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            decimal d => $"${d:F2}",
            double dbl => $"${dbl:F2}",
            int i => $"${i}.00",
            _ => string.Empty
        };
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class SelectedToBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return (Color)Application.Current!.Resources["Primary"];
        return Colors.Transparent;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class SelectedToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return (Color)Application.Current!.Resources["OnPrimary"];
        return (Color)Application.Current!.Resources["OnSurfaceVariant"];
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class EnumToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return false;
        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class DateLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime dt) return string.Empty;
        var today = DateTime.Today;
        var days = (dt.Date - today.Date).Days;
        return days switch
        {
            0 => "Today",
            1 => "Tomorrow",
            >= 2 and <= 6 => dt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? "Weekend" : "Weekday",
            _ => string.Empty
        };
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class ShowtimeStatusLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() switch
        {
            "AlmostFull" => "Almost Full",
            "SellingFast" => "Selling Fast",
            "SoldOut" => "Sold Out",
            _ => "Available"
        };
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class ShowtimeStatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.ToString() switch
        {
            "SellingFast" => Color.FromArgb("#BA1A1A"),
            "AlmostFull" => Color.FromArgb("#B45309"),
            _ => Color.FromArgb("#7A7B8C")
        };
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class SeatColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SeatState state) return Color.FromArgb("#E5E6F1");
        return state switch
        {
            SeatState.Selected  => Color.FromArgb("#1E88E5"),
            SeatState.Occupied  => Color.FromArgb("#9CA3AF"),
            SeatState.Aisle     => Colors.Transparent,
            _                   => Color.FromArgb("#E5E6F1")
        };
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class SeatTextColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is SeatState state && state == SeatState.Selected
            ? Colors.White
            : Color.FromArgb("#1A1A2E");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class SeatEnabledConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SeatState s) return true;
        return s == SeatState.Available;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
