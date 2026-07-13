using Microsoft.Extensions.DependencyInjection;
using EmployeeDirectory.Services;

namespace EmployeeDirectory
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            InitializeTheme();
        }

        private void InitializeTheme()
        {
            // Initialize theme colors in app resources
            var themeService = ThemeService.Instance;
            
            // Add theme color keys to app resources
            Resources.Add("ThemePrimaryColor", Color.FromArgb("#6750A4"));
            Resources.Add("ThemeSecondaryColor", Color.FromArgb("#03DAC6"));
            Resources.Add("ThemeTertiaryColor", Color.FromArgb("#FFB703"));
            Resources.Add("ThemeErrorColor", Color.FromArgb("#CF6679"));
            Resources.Add("ThemeBackgroundColor", Color.FromArgb("#FFFBFE"));
            Resources.Add("ThemeSurfaceColor", Color.FromArgb("#FFFBFE"));
            Resources.Add("ThemeSurfaceVariantColor", Color.FromArgb("#E8DEF8"));
            Resources.Add("ThemeOnPrimaryColor", Color.FromArgb("#FFFFFF"));
            Resources.Add("ThemeOnSurfaceColor", Color.FromArgb("#1F1F1F"));
            Resources.Add("ThemeOnSurfaceVariantColor", Color.FromArgb("#49454F"));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}