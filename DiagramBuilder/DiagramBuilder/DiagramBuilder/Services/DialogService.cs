namespace DiagramBuilder.Services;

/// <summary>
/// <see cref="IDialogService"/> implementation using the current application
/// main window for alerts/confirms. ViewModels stay view-agnostic.
/// </summary>
public class DialogService : IDialogService
{
    private static Page? CurrentPage =>
        Application.Current is { } app && app.Windows.Count > 0
            ? app.Windows[0].Page
            : null;

    public Task ShowAlertAsync(string title, string message)
    {
        var page = CurrentPage;
        return page is null ? Task.CompletedTask : page.DisplayAlertAsync(title, message, "OK");
    }

    public Task<bool> ConfirmAsync(string title, string message)
    {
        var page = CurrentPage;
        return page is null ? Task.FromResult(false) : page.DisplayAlertAsync(title, message, "OK", "Cancel");
    }
}
