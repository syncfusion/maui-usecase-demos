using FluentMail.Models;
using FluentMail.ViewModels;
using Syncfusion.Maui.ListView;

namespace FluentMail.Views;

public partial class MailWorkspacePage : ContentPage
{
    public MailWorkspacePage()
    {
        InitializeComponent();
        if (DeviceInfo.Idiom != DeviceIdiom.Phone)
            DesktopEditor.SetBinding(Syncfusion.Maui.RichTextEditor.SfRichTextEditor.ValueProperty,
                new Binding("Composer.Body", BindingMode.TwoWay));
        if (BindingContext is MailWorkspaceViewModel vm)
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(MailWorkspaceViewModel.IsComposing) or nameof(MailWorkspaceViewModel.IsMessageOpen))
                {
                    CloseDropDowns();
                    // The popup can be re-opened asynchronously by the text reset; close it again afterwards.
                    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(150), CloseDropDowns);
                    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), CloseDropDowns);
                }
            };
    }

    void CloseDropDowns()
    {
        foreach (var box in new[] { DesktopTo, DesktopCc, PhoneTo, PhoneCc })
        {
            box.IsDropDownOpen = false;
            box.Unfocus();
        }
    }

    void OnMobileSwipeEnded(object? sender, Syncfusion.Maui.ListView.SwipeEndedEventArgs e)
    {
        if (e.Offset < MobileMailList.SwipeOffset || e.DataItem is not MailMessage message || BindingContext is not MailWorkspaceViewModel viewModel) return;

        var command = e.Direction == SwipeDirection.Right ? viewModel.DeleteMessageCommand : viewModel.ArchiveMessageCommand;
        command.Execute(message);
        MobileMailList.ResetSwipeItem();
    }

    void OnMobileItemTapped(object? sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is MailMessage message && BindingContext is MailWorkspaceViewModel viewModel)
            viewModel.OpenMessageCommand.Execute(message);
    }
}
