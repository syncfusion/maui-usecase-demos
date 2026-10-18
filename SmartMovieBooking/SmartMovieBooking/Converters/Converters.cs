using System.Globalization;
using SmartMovieBooking.Models;

namespace SmartMovieBooking.Converters;

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

public sealed class SelectedToBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return (Color)Application.Current!.Resources["ThemePrimary"];
        return Color.Transparent;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class SelectedToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return (Color)Application.Current!.Resources["ThemeOnPrimary"];
        return (Color)Application.Current!.Resources["ThemeOnSurfaceVariant"];
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class DateToLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime date) return string.Empty;
        var today = DateTime.Today;
        if (date.Date == today) return "Today";
        if (date.Date == today.AddDays(1)) return "Tomorrow";
        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) return "Weekend";
        return "Weekday";
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class ShowtimeStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ShowtimeStatus s) return Colors.Transparent;
        return s switch
        {
            ShowtimeStatus.Available   => (Color)Application.Current!.Resources["ThemeOutline"],
            ShowtimeStatus.AlmostFull  => (Color)Application.Current!.Resources["ThemeAmber"],
            ShowtimeStatus.SellingFast => (Color)Application.Current!.Resources["ThemeError"],
            ShowtimeStatus.SoldOut     => (Color)Application.Current!.Resources["ThemeOutlineVariant"],
            _ => Colors.Transparent
        };
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class ShowtimeStatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is ShowtimeStatus s ? s switch
        {
            ShowtimeStatus.AlmostFull  => "Almost Full",
            ShowtimeStatus.SellingFast => "Selling Fast",
            ShowtimeStatus.SoldOut     => "Sold Out",
            _ => "Available"
        } : string.Empty;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class SeatStateToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SeatState s) return Colors.Transparent;
        return s switch
        {
            SeatState.Available => (Color)Application.Current!.Resources["SeatAvailable"],
            SeatState.Occupied  => (Color)Application.Current!.Resources["SeatOccupied"],
            SeatState.Selected  => (Color)Application.Current!.Resources["SeatSelected"],
            SeatState.Aisle     => Colors.Transparent,
            _ => Colors.Transparent
        };
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class CurrencyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d) return $"${d:F2}";
        if (value is double dbl) return $"${dbl:F2}";
        if (value is int i) return $"${i}.00";
        return string.Empty;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class StringToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            return Color.FromArgb(hex);
        return Colors.Transparent;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return false;
        return string.Equals(value.ToString(), parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}