using FluentMail.Models;
using FluentMail.ViewModels;
using Syncfusion.Maui.ListView;

namespace FluentMail.Views;

public partial class MailWorkspacePage : ContentPage
{
    public MailWorkspacePage()
    {
        InitializeComponent();
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
