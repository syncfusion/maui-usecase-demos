using System.Globalization;
using SmartMovieBooking.Models;
using SmartMovieBooking.ViewModels;

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

/// <summary>
/// Returns a friendly label for the date strip (Today / Tomorrow / Weekend / Weekday).
/// parameter = "labelOnly" returns just "Today"/"Tomorrow"/etc.
/// parameter = "dateOnly" returns the day number.
/// parameter = "weekdayOnly" returns the abbreviated weekday.
/// parameter = "monthOnly" returns the abbreviated month.
/// </summary>
public sealed class DateToLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime dt) return string.Empty;
        var today = DateTime.Today;
        var days = (dt.Date - today.Date).Days;
        var rel = days switch
        {
            0 => "Today",
            1 => "Tomorrow",
            >= 2 and <= 6 => dt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? "Weekend" : "Weekday",
            _ => string.Empty
        };
        var mode = (parameter as string)?.ToLowerInvariant();
        return mode switch
        {
            "labelonly" => rel,
            "dateonly" => dt.Day.ToString(),
            "weekdayonly" => dt.ToString("ddd", culture),
            "monthonly" => dt.ToString("MMM", culture),
            _ => rel
        };
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

public sealed class SeatTierToBorderColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SeatTier t) return Colors.Transparent;
        return t switch
        {
            SeatTier.Premium   => (Color)Application.Current!.Resources["ThemeTertiary"],
            SeatTier.Executive => (Color)Application.Current!.Resources["ThemeSecondary"],
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

public sealed class StringEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null && parameter is not null && value.ToString() == parameter.ToString();
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class TitleCaseConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var s = value?.ToString();
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return char.ToUpper(s[0]) + s.Substring(1);
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class FormatToTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && TimeSpan.TryParse(s, out var ts))
            return DateTime.Today.Add(ts).ToString("h:mm tt", culture);
        return value ?? string.Empty;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class IsNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is null;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class NotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int n && n > 0;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? 1.0 : 0.4;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class RoleToAlignConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is AiRole role && role == AiRole.User ? LayoutOptions.End : LayoutOptions.Start;
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class RoleToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is AiRole.User ? "👤" : "✨";
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class RoleToBubbleColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (Application.Current is null) return Colors.White;
        return value is AiRole.User
            ? (Color)Application.Current.Resources["ThemePrimary"]
            : (Color)Application.Current.Resources["ThemeSurfaceContainerLowest"];
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
