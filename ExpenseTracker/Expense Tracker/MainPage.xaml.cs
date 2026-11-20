using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmartExpense;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private int _currentPageIndex = 0;
    public int CurrentPageIndex
    {
        get => _currentPageIndex;
        set
        {
            if (_currentPageIndex != value)
            {
                _currentPageIndex = value;
                OnPropertyChanged();
                UpdateContentForWindows(value);
            }
        }
    }

    public ObservableCollection<NavItem> NavItems { get; } = new();

    private NavItem _selectedItem;
    public NavItem SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem != null)
                _selectedItem.IsSelected = false;

            _selectedItem = value;

            if (_selectedItem != null)
            {
                _selectedItem.IsSelected = true;
                UpdateContentForWindows(_selectedItem.PageIndex);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = this;

        WeakReferenceMessenger.Default.Register<NavigateToTabMessage>(this, (_, msg) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // For Windows, use CurrentPageIndex
                CurrentPageIndex = msg.TabIndex;

                // For Mobile, use TabView
                if (MainTabView_Mobile is not null)
                    MainTabView_Mobile.SelectedIndex = msg.TabIndex;
            });
        });


        NavItems.Add(new NavItem
        {
            Title = "Dashboard",
            PageIndex = 0,
            NormalIcon = ExpenseTrackerIcons.DashboardNormal,
            SelectedIcon = ExpenseTrackerIcons.DashboardSelected
        });

        NavItems.Add(new NavItem
        {
            Title = "Transaction",
            PageIndex = 1,
            NormalIcon = ExpenseTrackerIcons.TransactionNormal,
            SelectedIcon = ExpenseTrackerIcons.TransactionSelected
        });

        SelectedItem = NavItems[0];

        //// Initialize Windows content
        //UpdateContentForWindows(0);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (DeviceInfo.Platform == DevicePlatform.WinUI || DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            // Defer dashboard creation by one frame
            Dispatcher.Dispatch(() =>
            {
                UpdateContentForWindows(0);
            });
        }

        if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // ✅ Ensure Dashboard tab content is created on first load

            Dispatcher.Dispatch(() =>
            {
                EnsureTabContent((int)MainTabView_Mobile.SelectedIndex);

                UpdateMobileTabTextColors((int)MainTabView_Mobile.SelectedIndex);

            });


        }
    }

    private void UpdateContentForWindows(int index)
    {
        if (DeviceInfo.Platform != DevicePlatform.WinUI && DeviceInfo.Platform != DevicePlatform.MacCatalyst)
            return;

        // Use FindByName to avoid relying on generated field that may not be present
        var winView = this.FindByName<ContentView>("WindowsContentView");
        if (winView is null) return;

        winView.Content = index switch
        {
            0 => _dashboardPage ??= new DashboardPage(),
            1 => _transactionsPage ??= new TransactionsPage(),
            _ => _dashboardPage ??= new DashboardPage()
        };
    }
    
    private DashboardPage? _dashboardPage;
    private AddTransactionPage? _addTransactionPage;
    private TransactionsPage? _transactionsPage;


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<NavigateToTabMessage>(this);
    }

    private void OnTabSelectionChanged(
    object sender,
    Syncfusion.Maui.TabView.TabSelectionChangedEventArgs e)
    {
        EnsureTabContent((int)e.NewIndex);


        // NEW: update tab text colors
        UpdateMobileTabTextColors((int)e.NewIndex);
    }

    private void EnsureTabContent(int index)
    {
        if (MainTabView_Mobile.Items is null)
            return;

        // Create a lightweight placeholder immediately so the TabView can render
        // quickly, then instantiate heavy pages asynchronously on the UI thread.
        switch (index)
        {
            case 0: // Dashboard
                if (MainTabView_Mobile.Items[0] is Syncfusion.Maui.TabView.SfTabItem d &&
                    d.Content is null)
                {
                    // quick placeholder with a visible ActivityIndicator so mobile users
                    // see a busy indicator instead of a white/blank area
                    d.Content = new Grid
                    {
                        BackgroundColor = Colors.Transparent,
                        Children =
                        {
                            new ActivityIndicator
                            {
                                IsRunning = true,
                                Color = Color.FromArgb("#2F6D86"),
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            }
                        }
                    };

                    // Defer heavy page creation so UI can render the tab bar and the
                    // placeholder indicator first; then instantiate the DashboardPage.
                    Dispatcher.Dispatch(async () =>
                    {
                        // yield to let the UI paint; small delay reduces perceived jank
                        await Task.Delay(50);
                        if (d.Content is not null && d.Content.GetType() != typeof(DashboardPage))
                            d.Content = new DashboardPage();
                    });
                }
                break;

            case 1: // Transactions 
                if (MainTabView_Mobile.Items[1] is Syncfusion.Maui.TabView.SfTabItem t &&
                    t.Content is null)
                {
                    t.Content = new TransactionsPage();
                }
                break;
        }
    }

    private void UpdateMobileTabTextColors(int index)
    {
        if (MainTabView_Mobile?.Items is null)
            return;

        // Reset all tabs to unselected color
        foreach (var item in MainTabView_Mobile.Items)
        {
            if (item is Syncfusion.Maui.TabView.SfTabItem tab)
            {
                tab.TextColor = Colors.Black;
            }
        }

        if (index >= 0 &&
            index < MainTabView_Mobile.Items.Count &&
            MainTabView_Mobile.Items[index] is Syncfusion.Maui.TabView.SfTabItem selectedTab)
        {
            selectedTab.TextColor = Colors.White;
        }
    }
}