using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.ViewModel.Settings;

/// <summary>
/// ViewModel responsible for managing chart settings.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private bool _isTooltipEnabled;
    private bool _isLogarithmicAxis;
    private bool _isAxisInverted;
    private bool _isAxisOpposed = true;

    /// <summary>
    /// Gets or sets a value indicating whether tooltips are enabled.
    /// </summary>
    public bool IsTooltipEnabled
    {
        get => _isTooltipEnabled;
        set
        {
            if (_isTooltipEnabled != value)
            {
                _isTooltipEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether logarithmic axis is enabled.
    /// </summary>
    public bool IsLogarithmicAxis
    {
        get => _isLogarithmicAxis;
        set
        {
            if (_isLogarithmicAxis != value)
            {
                _isLogarithmicAxis = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the axis is inverted.
    /// </summary>
    public bool IsAxisInverted
    {
        get => _isAxisInverted;
        set
        {
            if (_isAxisInverted != value)
            {
                _isAxisInverted = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the axis is opposed.
    /// </summary>
    public bool IsAxisOpposed
    {
        get => _isAxisOpposed;
        set
        {
            if (_isAxisOpposed != value)
            {
                _isAxisOpposed = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Creates a clone of the current settings.
    /// </summary>
    /// <returns>A new SettingsViewModel with the same values.</returns>
    public SettingsViewModel Clone()
    {
        return new SettingsViewModel
        {
            IsTooltipEnabled = this.IsTooltipEnabled,
            IsLogarithmicAxis = this.IsLogarithmicAxis,
            IsAxisInverted = this.IsAxisInverted,
            IsAxisOpposed = this.IsAxisOpposed
        };
    }

    /// <summary>
    /// Applies settings from another SettingsViewModel to this instance.
    /// </summary>
    /// <param name="settings">The settings to apply.</param>
    public void Apply(SettingsViewModel settings)
    {
        IsTooltipEnabled = settings.IsTooltipEnabled;
        IsLogarithmicAxis = settings.IsLogarithmicAxis;
        IsAxisInverted = settings.IsAxisInverted;
        IsAxisOpposed = settings.IsAxisOpposed;
    }
}