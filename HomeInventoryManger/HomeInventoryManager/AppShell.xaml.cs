namespace HomeInventoryManager
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
			Routing.RegisterRoute(nameof(Views.DashboardPage), typeof(Views.DashboardPage));
			Routing.RegisterRoute("AIAdvisor", typeof(Views.DashboardPage));
			Routing.RegisterRoute("Reports", typeof(Views.DashboardPage));
			Routing.RegisterRoute("Settings", typeof(Views.DashboardPage));
		}
	}
}
