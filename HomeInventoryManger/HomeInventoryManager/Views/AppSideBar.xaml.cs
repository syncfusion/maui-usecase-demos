using HomeInventoryManager.Model;

namespace HomeInventoryManager.Views;

public partial class AppSideBar : ContentView
{
	public AppSideBar()
	{
		InitializeComponent();
	}

	public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
   nameof(ActiveTab), typeof(NavigationTab), typeof(MainPage), NavigationTab.Dashboard);

	public NavigationTab ActiveTab
	{
		get => (NavigationTab)GetValue(ActiveTabProperty);
		set => SetValue(ActiveTabProperty, value);
	}

	private static Task NavigateToAsync(string route)
		=> Shell.Current is null ? Task.CompletedTask : Shell.Current.GoToAsync(route);

	private async void OnDashboardTapped(object sender, TappedEventArgs e)
	{
		await NavigateToAsync("//Dashboard");
	}

	private async void OnInventoryTapped(object sender, TappedEventArgs e)
	{
		await NavigateToAsync("//Inventory");
	}

	private async void OnScanTapped(object sender, TappedEventArgs e)
	{
		await NavigateToAsync("//Scan");
	}

	private async void OnWarrantyTapped(object sender, TappedEventArgs e)
	{
		await NavigateToAsync("//Warranty");
	}

	private async Task GoToAsyncSafe(NavigationTab tab)
	{
		Page page = tab switch
		{
			NavigationTab.Dashboard => new DashboardPage(),
			NavigationTab.Inventory => new InventoryPage(),
			NavigationTab.Scan => new ScanPage(),
			NavigationTab.Warranty => new WarrantyPage(),
			_ => new DashboardPage()
		};

		await Application.Current.Windows[0]
			.Page.Navigation.PushAsync(page);
	}
}