using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

public partial class PrintSettingsViewModel : ObservableObject
{
    private static readonly IReadOnlyDictionary<string, (double Width, double Height)> PaperDimensions =
        new Dictionary<string, (double, double)>
        {
            [AppStrings.PrintPaperLetter] = (816, 1056),
            [AppStrings.PrintPaperLegal] = (816, 1344),
            [AppStrings.PrintPaperTabloid] = (1056, 1632),
            [AppStrings.PrintPaperA3] = (1123, 1587),
            [AppStrings.PrintPaperA4] = (794, 1123),
            [AppStrings.PrintPaperA5] = (559, 794),
            [AppStrings.PrintPaperA6] = (397, 559),
        };

    private readonly IDialogService _dialogService;

    public PrintSettingsViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    [ObservableProperty]
    private string region = AppStrings.ExportRegionContent;

    [ObservableProperty]
    private string paperSize = AppStrings.PrintPaperLetter;

    [ObservableProperty]
    private string orientation = AppStrings.PrintPortrait;

    [ObservableProperty]
    private bool scaleToFit = true;

    [ObservableProperty]
    private string customWidth = "816";

    [ObservableProperty]
    private string customHeight = "1056";

    [ObservableProperty]
    private bool isPrinting;

    public IReadOnlyList<string> RegionLabels { get; } =
    [
        AppStrings.ExportRegionContent,
        AppStrings.ExportRegionPageSettings,
    ];

    public IReadOnlyList<string> PaperSizeLabels { get; } =
    [
        AppStrings.PrintPaperLetter,
        AppStrings.PrintPaperLegal,
        AppStrings.PrintPaperTabloid,
        AppStrings.PrintPaperA3,
        AppStrings.PrintPaperA4,
        AppStrings.PrintPaperA5,
        AppStrings.PrintPaperA6,
        AppStrings.PrintCustomPaperSize,
    ];

    public IReadOnlyList<string> OrientationLabels { get; } =
    [
        AppStrings.PrintPortrait,
        AppStrings.PrintLandscape,
    ];

    public bool IsCustomPaperSize => PaperSize == AppStrings.PrintCustomPaperSize;

    public Func<PrintOptions, Task>? PrintAsync { get; set; }

    public event EventHandler? PrintCompleted;

    public event EventHandler? CancelRequested;

    partial void OnPaperSizeChanged(string value) =>
        OnPropertyChanged(nameof(IsCustomPaperSize));

    [RelayCommand]
    private async Task PrintDocumentAsync()
    {
        if (IsPrinting)
        {
            return;
        }

        try
        {
            IsPrinting = true;
            if (PrintAsync is null)
            {
                throw new InvalidOperationException(AppStrings.PrintFailedTitle);
            }

            await PrintAsync(CreateOptions());
            PrintCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (ArgumentException exception)
        {
            await _dialogService.ShowAlertAsync(AppStrings.PrintFailedTitle, exception.Message);
        }
        catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException or IOException)
        {
            await _dialogService.ShowAlertAsync(AppStrings.PrintFailedTitle, exception.Message);
        }
        finally
        {
            IsPrinting = false;
        }
    }

    [RelayCommand]
    private void Cancel() => CancelRequested?.Invoke(this, EventArgs.Empty);

    public PrintOptions CreateOptions()
    {
        if (!RegionLabels.Contains(Region) || !OrientationLabels.Contains(Orientation))
        {
            throw new ArgumentException(AppStrings.PrintPaperSizeInvalidMessage);
        }

        double width;
        double height;
        if (PaperSize == AppStrings.PrintCustomPaperSize)
        {
            if (!double.TryParse(CustomWidth, NumberStyles.Float, CultureInfo.InvariantCulture, out width)
                || !double.TryParse(CustomHeight, NumberStyles.Float, CultureInfo.InvariantCulture, out height)
                || width <= 0 || height <= 0)
            {
                throw new ArgumentException(AppStrings.PrintPaperSizeInvalidMessage);
            }
        }
        else if (!PaperDimensions.TryGetValue(PaperSize, out (double Width, double Height) dimensions))
        {
            throw new ArgumentException(AppStrings.PrintPaperSizeInvalidMessage);
        }
        else
        {
            (width, height) = dimensions;
        }

        if (Orientation == AppStrings.PrintLandscape)
        {
            (width, height) = (Math.Max(width, height), Math.Min(width, height));
        }
        else
        {
            (width, height) = (Math.Min(width, height), Math.Max(width, height));
        }

        return new PrintOptions(Region, PaperSize, Orientation, ScaleToFit, width, height);
    }
}
