
using System.ComponentModel;

namespace SmartExpense;

/// <summary>
/// Primary view model for the application shell.
/// Currently serves as a placeholder for future global UI state
/// such as theme handling or app-level commands.
/// </summary>
public partial class MainViewModel() : BaseViewModel
{
    // Intentionally left empty.
    // Acts as a composition root for future app-wide state.
}

/// <summary>
/// Centralized factory for font-based icons used in bottom navigation.
/// Ensures consistent glyph, font family, and color usage.
/// </summary>
public static class ExpenseTrackerIcons
{
    private const string FontFamily = "ExpenseTrackerIcons";

    /// <summary>
    /// Creates a font image source for the given glyph and color.
    /// </summary>
    private static FontImageSource Icon(int glyph, Color color) =>
        new()
        {
            FontFamily = FontFamily,
            Glyph = char.ConvertFromUtf32(glyph),
            Color = color
        };

    // Dashboard
    public static FontImageSource DashboardNormal =>
        Icon(0xe716, Colors.White);

    public static FontImageSource DashboardSelected =>
        Icon(0xe716, Color.FromArgb("#1C3F50"));

    // Transactions
    public static FontImageSource TransactionNormal =>
        Icon(0xe738, Colors.White);

    public static FontImageSource TransactionSelected =>
        Icon(0xe738, Color.FromArgb("#1C3F50"));

}

/// <summary>
/// Represents a navigation item in the bottom or side navigation UI.
/// Handles selection state and icon switching.
/// </summary>
public class NavItem : INotifyPropertyChanged
{
    /// <summary>
    /// Display title of the navigation item.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Index of the page associated with this navigation item.
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// Icon displayed when the item is not selected.
    /// </summary>
    public FontImageSource NormalIcon { get; set; }

    /// <summary>
    /// Icon displayed when the item is selected.
    /// </summary>
    public FontImageSource SelectedIcon { get; set; }

    private bool _isSelected;

    /// <summary>
    /// Indicates whether this navigation item is currently selected.
    /// Triggers icon updates when changed.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
                return;

            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(CurrentIcon));
        }
    }

    /// <summary>
    /// Returns the appropriate icon based on selection state.
    /// </summary>
    public FontImageSource CurrentIcon =>
        IsSelected ? SelectedIcon : NormalIcon;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}