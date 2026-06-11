using Syncfusion.Maui.ListView;
using Syncfusion.Maui.PullToRefresh;

namespace NotificationsCenter;

/// <summary>
/// Behavior class to handle PullToRefresh and SwipeEnded events for Notifications ListView.
/// </summary>
public class NotificationsPullToRefreshBehavior : Behavior<ContentPage>
{
    private SfPullToRefresh? pullToRefresh;
    private SfListView? listView;
    private ContentPage? page;

    /// <summary>
    /// Attach behavior to ContentPage and set up event handlers.
    /// </summary>
    protected override void OnAttachedTo(ContentPage bindable)
    {
        page = bindable;
        pullToRefresh = bindable.FindByName<SfPullToRefresh>("pullToRefresh");
        listView = bindable.FindByName<SfListView>("listView");

        if (pullToRefresh != null)
        {
            pullToRefresh.Pulling += PullToRefresh_Pulling;
        }

        if (listView != null)
        {
            listView.SwipeEnded += ListView_SwipeEnded;
            listView.ItemTapped += ListView_ItemTapped;
        }

        base.OnAttachedTo(bindable);
    }

    /// <summary>
    /// Handle Pulling event - add items to the UI as the user pulls.
    /// </summary>
    private void PullToRefresh_Pulling(object? sender, Syncfusion.Maui.PullToRefresh.PullingEventArgs e)
    {
        var viewModel = page?.BindingContext as NotificationsViewModel;
        if (viewModel == null)
            return;

        // Add items to the UI if pulls
        if (viewModel.PullingCommand.CanExecute(null))
        {
            viewModel.PullingCommand.Execute(null);
        }
    }

    /// <summary>
    /// Handle SwipeEnded event - execute delete command from ViewModel.
    /// </summary>
    private void ListView_SwipeEnded(object? sender, Syncfusion.Maui.ListView.SwipeEndedEventArgs e)
    {
        var viewModel = page?.BindingContext as NotificationsViewModel;
        if (viewModel == null)
            return;

        if (e.DataItem is Notification notification)
        {
            viewModel.DeleteItemCommand.Execute(notification);
        }
    }

    /// <summary>
    /// Handle ItemTapped event - toggle expand and refresh view.
    /// </summary>
    private void ListView_ItemTapped(object? sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        var viewModel = page?.BindingContext as NotificationsViewModel;
        if (viewModel == null)
            return;

        if (e.DataItem is Notification notification)
        {
            viewModel.ToggleExpandCommand.Execute(notification);
            listView?.RefreshView();
        }
    }

    /// <summary>
    /// Detach behavior from ContentPage and unsubscribe from events.
    /// </summary>
    protected override void OnDetachingFrom(ContentPage bindable)
    {
        if (pullToRefresh != null)
        {
            pullToRefresh.Pulling -= PullToRefresh_Pulling;
        }

        if (listView != null)
        {
            listView.SwipeEnded -= ListView_SwipeEnded;
            listView.ItemTapped -= ListView_ItemTapped;
        }

        pullToRefresh = null;
        listView = null;
        page = null;

        base.OnDetachingFrom(bindable);
    }
}
