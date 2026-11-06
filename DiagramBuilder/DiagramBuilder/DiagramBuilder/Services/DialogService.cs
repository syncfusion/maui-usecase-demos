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
        if (page is null)
        {
            return Task.CompletedTask;
        }

#if NET10_0_OR_GREATER
        return page.DisplayAlertAsync(title, message, "OK");
#else
        // Page.DisplayAlertAsync was added in .NET 10; on earlier targets use the
        // synchronous API off the UI thread so the caller can still await it.
        return Task.Run(() => page.DisplayAlert(title, message, "OK"));
#endif
    }

    public Task<bool> ConfirmAsync(string title, string message)
    {
        var page = CurrentPage;
        if (page is null)
        {
            return Task.FromResult(false);
        }

#if NET10_0_OR_GREATER
        return page.DisplayAlertAsync(title, message, "OK", "Cancel");
#else
        return Task.Run(() => page.DisplayAlert(title, message, "OK", "Cancel"));
#endif
    }
}
