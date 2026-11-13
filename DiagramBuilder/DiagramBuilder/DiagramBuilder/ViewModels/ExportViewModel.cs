using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.Services;

namespace DiagramBuilder.ViewModels;

public partial class ExportViewModel : ObservableObject
{
    private readonly IDiagramFileService _fileService;
    private readonly IDialogService _dialogService;
    private readonly Func<string> _documentTitle;

    public ExportViewModel(IDiagramFileService fileService, IDialogService dialogService, Func<string> documentTitle)
    {
        _fileService = fileService;
        _dialogService = dialogService;
        _documentTitle = documentTitle;
        FileName = documentTitle();
    }

    [ObservableProperty]
    private ExportFormat selectedFormat = ExportFormat.Png;

    [ObservableProperty]
    private string region = AppStrings.ExportRegionContent;

    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    private bool isExporting;

    public IReadOnlyList<string> FormatLabels { get; } =
    [
        AppStrings.ExportFormatPng,
        AppStrings.ExportFormatJpeg,
        AppStrings.ExportFormatPdf,
        AppStrings.ExportFormatSvg,
    ];

    public string SelectedFormatLabel
    {
        get => FormatLabelFor(SelectedFormat);
        set => SelectedFormat = ParseFormat(value);
    }

    public IReadOnlyList<string> RegionLabels { get; } =
    [
        AppStrings.ExportRegionContent,
        AppStrings.ExportRegionPageSettings,
    ];

    public Func<ExportFormat, string, string, Task<ExportArtifact>>? ExportDiagramAsync { get; set; }

    public event EventHandler? ExportCompleted;

    public event EventHandler? CancelRequested;

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (IsExporting)
        {
            return;
        }

        IsExporting = true;
        try
        {
            if (ExportDiagramAsync is null)
            {
                await _dialogService.ShowAlertAsync(AppStrings.ExportFailedTitle, AppStrings.ExportOutputUnavailableMessage);
                return;
            }

            if (!RegionLabels.Contains(Region))
            {
                throw new ArgumentException(AppStrings.ExportRegionLabel);
            }

            var artifact = await ExportDiagramAsync(SelectedFormat, Region, FileName);
            var path = await _fileService.SaveExportAsync(FileName, artifact.Extension, artifact.Content);
            await _dialogService.ShowAlertAsync(AppStrings.ExportDialogTitle, string.Format(AppStrings.ExportSavedPathFormat, path));
            ExportCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or FormatException)
        {
            await _dialogService.ShowAlertAsync(AppStrings.ExportFailedTitle, exception.Message);
        }
        finally
        {
            IsExporting = false;
        }
    }

    [RelayCommand]
    private void Cancel() => CancelRequested?.Invoke(this, EventArgs.Empty);

    public void Reset()
    {
        SelectedFormat = ExportFormat.Png;
        Region = AppStrings.ExportRegionContent;
        FileName = _documentTitle();
    }

    private static ExportFormat ParseFormat(string label) => label switch
    {
        AppStrings.ExportFormatPng => ExportFormat.Png,
        AppStrings.ExportFormatJpeg => ExportFormat.Jpeg,
        AppStrings.ExportFormatPdf => ExportFormat.Pdf,
        AppStrings.ExportFormatSvg => ExportFormat.Svg,
        _ => throw new ArgumentException(AppStrings.ExportFormatInvalidMessage, nameof(label)),
    };

    private static string FormatLabelFor(ExportFormat format) => format switch
    {
        ExportFormat.Png => AppStrings.ExportFormatPng,
        ExportFormat.Jpeg => AppStrings.ExportFormatJpeg,
        ExportFormat.Pdf => AppStrings.ExportFormatPdf,
        ExportFormat.Svg => AppStrings.ExportFormatSvg,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, AppStrings.ExportFormatInvalidMessage),
    };

    partial void OnSelectedFormatChanged(ExportFormat value) =>
        OnPropertyChanged(nameof(SelectedFormatLabel));
}
