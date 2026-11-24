namespace FluentMail.Services;

public interface IAlertService
{
    Task ShowAsync(string title, string message, string cancel);
}

public sealed class AlertService : IAlertService
{
    public async Task ShowAsync(string title, string message, string cancel)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page is not null)
            await page.DisplayAlertAsync(title, message, cancel);
    }
}
