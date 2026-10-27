using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

/// <summary>
/// Export dialog state: format/region/file name selection, mock export execution
/// via <see cref="IExportService"/>, result state for the dialog.
/// </summary>
public partial class ExportViewModel : ObservableObject
{
    private readonly IExportService _exportService;
    private readonly Func<string> _documentTitle;

    public ExportViewModel(IExportService exportService, Func<string> documentTitle)
    {
        _exportService = exportService;
        _documentTitle = documentTitle;
        FileName = AppConstants.DefaultExportFileName;
    }

    [ObservableProperty]
    private ExportFormat selectedFormat = ExportFormat.Png;

    [ObservableProperty]
    private string region = AppStrings.ExportRegionContent;

    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    private bool isExporting;

    [ObservableProperty]
    private bool hasResult;

    [ObservableProperty]
    private string resultText = string.Empty;

    /// <summary>Format options (from constants).</summary>
    public IReadOnlyList<string> FormatLabels { get; } =
    [
        AppStrings.ExportFormatPng,
        AppStrings.ExportFormatJpeg,
        AppStrings.ExportFormatPdf,
        AppStrings.ExportFormatSvg,
    ];

    /// <summary>Region options (from constants).</summary>
    public IReadOnlyList<string> RegionLabels { get; } =
    [
        AppStrings.ExportRegionContent,
        AppStrings.ExportRegionPageSettings,
    ];

    public ExportFormat[] Formats { get; } = Enum.GetValues<ExportFormat>();

    [RelayCommand]
    private async Task ExportAsync()
    {
        IsExporting = true;
        HasResult = false;
        try
        {
            var result = await _exportService.ExportAsync(SelectedFormat, Region, FileName);
            ResultText = result.Success
                ? string.Format(AppStrings.ExportSuccessFormat, result.FileName)
                : AppStrings.ExportMockNotice;
            HasResult = true;
        }
        finally
        {
            IsExporting = false;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        SelectedFormat = ExportFormat.Png;
        Region = AppStrings.ExportRegionContent;
        FileName = AppConstants.DefaultExportFileName;
        HasResult = false;
        ResultText = string.Empty;
    }
}
