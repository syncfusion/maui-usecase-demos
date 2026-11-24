using FluentMail.Models;
using FluentMail.ViewModels;
using Syncfusion.Maui.ListView;

namespace FluentMail.Views;

public partial class MailWorkspacePage : ContentPage
{
    readonly MailWorkspaceViewModel viewModel;

    public MailWorkspacePage(MailWorkspaceViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;

        // The phone layout has its own plain Editor; only the desktop/tablet layout uses the
        // rich text editor. Binding it in code keeps it from being wired up on phones.
        if (DeviceInfo.Idiom != DeviceIdiom.Phone)
            DesktopEditor.SetBinding(Syncfusion.Maui.RichTextEditor.SfRichTextEditor.ValueProperty,
                new Binding("Composer.Body", BindingMode.TwoWay));

        Loaded += (_, _) => viewModel.PropertyChanged += OnViewModelPropertyChanged;
        Unloaded += (_, _) => viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MailWorkspaceViewModel.IsComposing) or nameof(MailWorkspaceViewModel.IsMessageOpen))
        {
            CloseDropDowns();
            // Workaround: SfAutocomplete re-opens its suggestion drop-down (and regains focus) a
            // moment after its Text/SelectedItems are reset by Composer.Reset() or the layout
            // switches, so a single synchronous close is not enough. Close it again after the
            // reset has been processed.
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(150), CloseDropDowns);
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), CloseDropDowns);
        }
    }

    void CloseDropDowns()
    {
        foreach (var box in new[] { DesktopTo, DesktopCc, PhoneTo, PhoneCc })
        {
            box.IsDropDownOpen = false;
            box.Unfocus();
        }
    }

    // SfListView only exposes swipe/tap as events, so these thin handlers forward to view-model commands.
    void OnMobileSwipeEnded(object? sender, Syncfusion.Maui.ListView.SwipeEndedEventArgs e)
    {
        if (e.Offset < MobileMailList.SwipeOffset || e.DataItem is not MailMessage message) return;

        var command = e.Direction == SwipeDirection.Right ? viewModel.DeleteMessageCommand : viewModel.ArchiveMessageCommand;
        command.Execute(message);
        MobileMailList.ResetSwipeItem();
    }

    void OnMobileItemTapped(object? sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        if (e.DataItem is MailMessage message)
            viewModel.OpenMessageCommand.Execute(message);
    }
}
