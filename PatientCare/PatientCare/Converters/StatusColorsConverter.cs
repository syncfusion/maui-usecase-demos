using System.Collections;
using System.Globalization;
using PatientCare.Models;

namespace PatientCare.Converters;

/// <summary>Converts a collection of <see cref="DashboardStatusPoint"/> into the brush palette Syncfusion chart series should use, so the doughnut segments match the legend swatches.</summary>
public sealed class StatusColorsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable points)
        {
            return Array.Empty<Brush>();
        }

        var brushes = new List<Brush>();
        foreach (var point in points)
        {
            if (point is DashboardStatusPoint status && !string.IsNullOrEmpty(status.Color))
            {
                brushes.Add(new SolidColorBrush(Color.FromArgb(status.Color)));
            }
        }

        return brushes;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
