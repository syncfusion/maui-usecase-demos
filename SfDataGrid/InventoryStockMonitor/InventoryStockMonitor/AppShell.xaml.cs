using StockMonitor.Services;
using StockMonitor.Views;

namespace StockMonitor
{
    public partial class AppShell : Shell
    {
        private ToolbarItem? _themeToggleButton;

        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("InventoryDetailPage", typeof(InventoryDetailPage));
            
            // Initialize theme to light
            ThemeService.SetTheme(isDark: false);
        }

        private void OnThemeToggleClicked(object sender, EventArgs e)
        {
            // Toggle the theme
            ThemeService.SetTheme(!ThemeService.IsDarkTheme);
            UpdateThemeIcon();
        }

        private void UpdateThemeIcon()
        {
            if (_themeToggleButton == null)
            {
                _themeToggleButton = FindByName("ThemeToggleButton") as ToolbarItem;
            }

            if (_themeToggleButton != null)
            {
                // Update text based on current theme
                _themeToggleButton.Text = ThemeService.IsDarkTheme ? "☀️" : "🌙";
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateThemeIcon();
        }
    }
}
