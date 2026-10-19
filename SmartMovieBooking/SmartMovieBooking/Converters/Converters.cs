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

/// <summary>
/// Returns a friendly label for the date strip (Today / Tomorrow / Weekend / Weekday + day + weekday + month).
/// parameter = "labelOnly" returns just "Today"/"Tomorrow"/etc.
/// parameter = "dateOnly" returns "24".
/// parameter = "weekdayOnly" returns "Thu".
/// otherwise returns the full line.
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
            >= 2 and <= 6 => DateTime.Today.AddDays(days).DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? "Weekend" : "Weekday",
            _ => string.Empty
        };
        var day = dt.Day.ToString();
        var weekday = dt.ToString("ddd", culture);
        var month = dt.ToString("MMM", culture);

        var mode = (parameter as string)?.ToLowerInvariant();
        return mode switch
        {
            "labelonly" => rel,
            "dateonly"  => day,
            "weekdayonly" => weekday,
            "monthonly" => month,
            "full" => $"{rel} {day} {weekday} • {month}",
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
        var key = value?.ToString() switch
        {
            "AlmostFull"  => "ThemeTertiaryContainer",
            "SellingFast" => "ThemeError",
            "SoldOut"     => "ThemeOutline",
            _             => "ThemeOutline"
        };
        return (Color)Application.Current!.Resources[key];
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class ShowtimeStatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "AlmostFull"  => "Almost Full",
            "SellingFast" => "Selling Fast",
            "SoldOut"     => "Sold Out",
            _             => "Available"
        };
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class SeatStateToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value?.ToString() switch
        {
            "Selected" => "ThemePrimary",
            "Occupied" => "ThemeSurfaceContainerHigh",
            "Aisle"    => "ThemeBackground",
            _          => "ThemeSurface"
        };
        return (Color)Application.Current!.Resources[key];
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class SeatTierToBorderColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Premium"   => (Color)Application.Current!.Resources["ThemeTertiary"],
            "Executive" => (Color)Application.Current!.Resources["ThemeSecondary"],
            _           => Colors.Transparent
        };
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class StringEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var p = parameter as string;
        return string.Equals(value?.ToString(), p, StringComparison.OrdinalIgnoreCase);
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class TitleCaseConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return string.Empty;
        var s = value.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpper(s[0], culture) + s[1..];
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class FormatToTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && !string.IsNullOrWhiteSpace(s))
            return s;
        return string.Empty;
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
    {
        var n = value switch
        {
            int i  => i,
            long l => (int)l,
            _      => 0
        };
        // parameter "invert" => show when count == 0
        if ((parameter as string)?.Equals("invert", StringComparison.OrdinalIgnoreCase) == true)
            return n == 0;
        return n > 0;
    }
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
    {
        if (value is AiRole role && role == AiRole.User)
            return LayoutOptions.End;
        return LayoutOptions.Start;
    }
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
        if (value is AiRole.User)
            return (Color)Application.Current.Resources["ThemePrimary"];
        return (Color)Application.Current.Resources["ThemeSurfaceContainerLowest"];
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}