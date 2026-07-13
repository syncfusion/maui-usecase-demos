using EmployeeDirectory.Services;

namespace EmployeeDirectory
{
    public partial class AppShell : Shell
    {
        private ToolbarItem? _themeToggleButton;
        public AppShell()
        {
            InitializeComponent();
        }

        private void OnThemeToggleClicked(object sender, EventArgs e)
        {
            var themeService = ThemeService.Instance;
            themeService.IsDarkTheme = !themeService.IsDarkTheme;

            if (_themeToggleButton == null)
            {
                _themeToggleButton = FindByName("ThemeToggleButton") as ToolbarItem;
            }
            // Update toggle button icon
            if (_themeToggleButton != null)
            {
                // Update text based on current theme
                _themeToggleButton.Text = themeService.IsDarkTheme ? "☀️" : "🌙";
            }
        }
    }
}
