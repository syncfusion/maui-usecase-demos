namespace SmartExpense;

/// <summary>
/// Converts a <see cref="TransactionType"/> to a <see cref="Color"/> value
/// suitable for use with control BackgroundColor properties.
/// Income → light green (#D1FAE5), Expense → light pink (#FFE4E6).
/// </summary>
public class TypeToColorConverter : IValueConverter
{
    private static readonly Color IncomeColor = Color.FromArgb("#D1FAE5");
    private static readonly Color ExpenseColor = Color.FromArgb("#FFE4E6");

    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is TransactionType type)
        {
            return type == TransactionType.Income ? IncomeColor : ExpenseColor;
        }
        return ExpenseColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
