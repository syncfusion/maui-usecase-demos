namespace HospitalManagement
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            // Register routes
            Routing.RegisterRoute("addpatient", typeof(HospitalManagement.View.AddPatientPage));

            // Platform-specific navigation
            if (Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.WinUI ||
                Microsoft.Maui.Devices.DeviceInfo.Platform == Microsoft.Maui.Devices.DevicePlatform.MacCatalyst)
            {
                // Desktop: Use custom RootPage
                this.Items.Clear();
                var rootPage = new ShellContent
                {
                    ContentTemplate = new DataTemplate(typeof(HospitalManagement.View.RootPage)),
                    Route = "RootPage"
                };
                this.Items.Add(rootPage);
                this.CurrentItem = rootPage;
            }
            else
            {
                // Mobile: Use TabBar navigation
                this.Items.Clear();

                var dashboardTab = new ShellContent
                {
                    Title = "Dashboard",
                    ContentTemplate = new DataTemplate(typeof(HospitalManagement.View.DashboardPage)),
                    Route = "DashboardPage"
                };
                var patientsTab = new ShellContent
                {
                    Title = "Patients",
                    ContentTemplate = new DataTemplate(typeof(HospitalManagement.View.DataGridPage)),
                    Route = "DataGridPage"
                };

                var tabBar = new TabBar();
                tabBar.Items.Add(dashboardTab);
                tabBar.Items.Add(patientsTab);

                this.Items.Add(tabBar);
                this.CurrentItem = tabBar;
            }
        }
        // Removed ConfigurePlatformUI, handled above
    }
}
