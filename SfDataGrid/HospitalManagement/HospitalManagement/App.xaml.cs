using Microsoft.Extensions.DependencyInjection;
using Syncfusion.Maui.Themes;

namespace HospitalManagement
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // Set Light Theme as default
            SetAppTheme(AppTheme.Light);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        public void SetAppTheme(AppTheme appTheme)
        {
            try
            {
                UserAppTheme = appTheme;

                // Update Syncfusion theme if available
                var mergedDictionaries = Current?.Resources?.MergedDictionaries;
                var sfTheme = mergedDictionaries?.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();

                if (sfTheme != null)
                {
                    if (appTheme == AppTheme.Dark)
                    {
                        sfTheme.VisualTheme = SfVisuals.MaterialDark;
                    }
                    else
                    {
                        sfTheme.VisualTheme = SfVisuals.MaterialLight;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting app theme: {ex.Message}");
            }
        }
    }
}