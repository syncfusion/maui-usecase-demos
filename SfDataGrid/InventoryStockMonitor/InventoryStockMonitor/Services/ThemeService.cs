using Syncfusion.Maui.Themes;

namespace StockMonitor.Services
{
    public class ThemeService
    {
        public static bool IsDarkTheme { get; private set; } = false;
        

        public static void SetTheme(bool isDark)
        {
            IsDarkTheme = isDark;
            
            // First, apply color resources
            if (isDark)
            {
                ApplyDarkThemeColors();
            }
            else
            {
                ApplyLightThemeColors();
            }
            
            // Then set MAUI theme
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
            }
            
            // Finally, update Syncfusion theme
            try
            {
                ICollection<ResourceDictionary> mergedDictionaries = Application.Current?.Resources.MergedDictionaries;
                var theme = mergedDictionaries?.OfType<SyncfusionThemeResourceDictionary>().FirstOrDefault();
                if (theme != null)
                {
                    theme.VisualTheme = isDark ? SfVisuals.MaterialDark : SfVisuals.MaterialLight;
                }
            }
            catch (Exception ex)
            {
                // Syncfusion theme update might fail in some scenarios, silently continue
            }
        }

        private static void ApplyLightThemeColors()
        {
            var resources = Application.Current!.Resources;

            // Material 3 Light Colors
            resources["Primary"] = Color.FromArgb("#6750A4");
            resources["OnPrimary"] = Color.FromArgb("#FFFFFF");
            resources["PrimaryContainer"] = Color.FromArgb("#EADDFF");
            resources["OnPrimaryContainer"] = Color.FromArgb("#21005D");
            resources["Secondary"] = Color.FromArgb("#625B71");
            resources["OnSecondary"] = Color.FromArgb("#FFFFFF");
            resources["SecondaryContainer"] = Color.FromArgb("#E8DEF8");
            resources["OnSecondaryContainer"] = Color.FromArgb("#1D192B");
            resources["Tertiary"] = Color.FromArgb("#7D5260");
            resources["OnTertiary"] = Color.FromArgb("#FFFFFF");
            resources["TertiaryContainer"] = Color.FromArgb("#FFD8E4");
            resources["OnTertiaryContainer"] = Color.FromArgb("#31111D");
            resources["Error"] = Color.FromArgb("#B3261E");
            resources["OnError"] = Color.FromArgb("#FFFFFF");
            resources["ErrorContainer"] = Color.FromArgb("#F9DEDC");
            resources["OnErrorContainer"] = Color.FromArgb("#410E0B");
            resources["Background"] = Color.FromArgb("#FFFBFE");
            resources["OnBackground"] = Color.FromArgb("#1C1B1F");
            resources["Surface"] = Color.FromArgb("#FFFBFE");
            resources["OnSurface"] = Color.FromArgb("#1C1B1F");
            resources["SurfaceVariant"] = Color.FromArgb("#E7E0EC");
            resources["OnSurfaceVariant"] = Color.FromArgb("#49454E");
            resources["Outline"] = Color.FromArgb("#79747E");
            resources["OutlineVariant"] = Color.FromArgb("#CAC7D0");
        }

        private static void ApplyDarkThemeColors()
        {
            var resources = Application.Current!.Resources;

            // Material 3 Dark Colors
            resources["Primary"] = Color.FromArgb("#D0BCFF");
            resources["OnPrimary"] = Color.FromArgb("#371E55");
            resources["PrimaryContainer"] = Color.FromArgb("#4F378B");
            resources["OnPrimaryContainer"] = Color.FromArgb("#EADDFF");
            resources["Secondary"] = Color.FromArgb("#CCC7DB");
            resources["OnSecondary"] = Color.FromArgb("#332D41");
            resources["SecondaryContainer"] = Color.FromArgb("#4A4458");
            resources["OnSecondaryContainer"] = Color.FromArgb("#E8DEF8");
            resources["Tertiary"] = Color.FromArgb("#E6B9D7");
            resources["OnTertiary"] = Color.FromArgb("#492532");
            resources["TertiaryContainer"] = Color.FromArgb("#633B48");
            resources["OnTertiaryContainer"] = Color.FromArgb("#FFD8E4");
            resources["Error"] = Color.FromArgb("#F2B8B5");
            resources["OnError"] = Color.FromArgb("#8C1D18");
            resources["ErrorContainer"] = Color.FromArgb("#B3261E");
            resources["OnErrorContainer"] = Color.FromArgb("#F9DEDC");
            resources["Background"] = Color.FromArgb("#1C1B1F");
            resources["OnBackground"] = Color.FromArgb("#E6E1E5");
            resources["Surface"] = Color.FromArgb("#1C1B1F");
            resources["OnSurface"] = Color.FromArgb("#E6E1E5");
            resources["SurfaceVariant"] = Color.FromArgb("#49454E");
            resources["OnSurfaceVariant"] = Color.FromArgb("#CAC7D0");
            resources["Outline"] = Color.FromArgb("#998DA0");
            resources["OutlineVariant"] = Color.FromArgb("#49454E");
        }
    }
}
