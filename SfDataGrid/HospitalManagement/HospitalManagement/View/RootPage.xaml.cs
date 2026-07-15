namespace HospitalManagement.View
{
    public partial class RootPage : ContentPage
    {
        private Button _currentSelectedButton;
        private AppTheme _currentTheme = AppTheme.Light;

        public RootPage()
        {
            InitializeComponent();
            _currentSelectedButton = BtnDashboard;
            _currentTheme = Application.Current?.UserAppTheme ?? AppTheme.Light;
            ShowDashboard();
            
            // Ensure navigation and theme buttons are styled after page loads
            this.Loaded += (s, e) =>
            {
                UpdateNavigationButtonStyles();
                UpdateThemeButtonStyles();
            };
        }

        private void UpdateButtonSelection(Button selectedButton)
        {
            _currentSelectedButton = selectedButton;
            UpdateNavigationButtonStyles();
        }

        private void UpdateNavigationButtonStyles()
        {
            var selectedBackground = Color.FromArgb("#6750A4");
            var selectedTextColor = Colors.White;
            var unselectedTextColor = _currentTheme == AppTheme.Dark ? Colors.White : Colors.Black;

            if (BtnDashboard != null)
            {
                BtnDashboard.BackgroundColor = _currentSelectedButton == BtnDashboard ? selectedBackground : Colors.Transparent;
                BtnDashboard.TextColor = _currentSelectedButton == BtnDashboard ? selectedTextColor : unselectedTextColor;
            }

            if (BtnPatients != null)
            {
                BtnPatients.BackgroundColor = _currentSelectedButton == BtnPatients ? selectedBackground : Colors.Transparent;
                BtnPatients.TextColor = _currentSelectedButton == BtnPatients ? selectedTextColor : unselectedTextColor;
            }
        }

        private void ShowDashboard()
        {
            var page = new global::HospitalManagement.View.DashboardPage();
            ContentHost.Content = page.Content;
            ContentHost.BindingContext = page.BindingContext;
        }

        private void ShowPatients()
        {
            var page = new global::HospitalManagement.View.DataGridPage();
            ContentHost.Content = page.Content;
            ContentHost.BindingContext = page.BindingContext;
        }

        private void OnDashboardClicked(object sender, EventArgs e)
        {
            UpdateButtonSelection(BtnDashboard);
            ShowDashboard();
            HideThemePopup();
        }

        private void OnPatientsClicked(object sender, EventArgs e)
        {
            UpdateButtonSelection(BtnPatients);
            ShowPatients();
            HideThemePopup();
        }

        private void OnThemeClicked(object sender, EventArgs e)
        {
            ShowThemePopup();
            UpdateNavigationButtonStyles();
        }

        private void OnThemeOverlayTapped(object sender, TappedEventArgs e)
        {
            HideThemePopup();
        }

        private void OnCloseThemeClicked(object sender, EventArgs e)
        {
            HideThemePopup();
        }

        private void OnLightThemeClicked(object sender, EventArgs e)
        {
            ApplyTheme(AppTheme.Light);
        }

        private void OnDarkThemeClicked(object sender, EventArgs e)
        {
            ApplyTheme(AppTheme.Dark);
        }

        private void ShowThemePopup()
        {
            UpdateThemeButtonStyles();
            ThemeOverlay.IsVisible = true;
        }

        private void HideThemePopup()
        {
            ThemeOverlay.IsVisible = false;
        }

        private void ApplyTheme(AppTheme theme)
        {
            _currentTheme = theme;
            (Application.Current as App)?.SetAppTheme(theme);
            UpdateNavigationButtonStyles();
            UpdateThemeButtonStyles();
            HideThemePopup();
        }

        private void UpdateThemeButtonStyles()
        {
            var unselectedTextColor = _currentTheme == AppTheme.Dark ? Colors.White : Colors.Black;
            var unselectedBorderColor = _currentTheme == AppTheme.Dark ? Color.FromArgb("#606060") : Color.FromArgb("#CCCCCC");

            // Light Theme Button
            if (BtnLightTheme != null)
            {
                if (_currentTheme == AppTheme.Light)
                {
                    BtnLightTheme.BackgroundColor = Color.FromArgb("#6750A4");
                    BtnLightTheme.TextColor = Colors.White;
                    BtnLightTheme.BorderWidth = 0;
                    BtnLightTheme.BorderColor = Colors.Transparent;
                }
                else
                {
                    BtnLightTheme.BackgroundColor = Colors.Transparent;
                    BtnLightTheme.TextColor = unselectedTextColor;
                    BtnLightTheme.BorderWidth = 1;
                    BtnLightTheme.BorderColor = unselectedBorderColor;
                }
            }

            // Dark Theme Button
            if (BtnDarkTheme != null)
            {
                if (_currentTheme == AppTheme.Dark)
                {
                    BtnDarkTheme.BackgroundColor = Color.FromArgb("#6750A4");
                    BtnDarkTheme.TextColor = Colors.White;
                    BtnDarkTheme.BorderWidth = 0;
                    BtnDarkTheme.BorderColor = Colors.Transparent;
                }
                else
                {
                    BtnDarkTheme.BackgroundColor = Colors.Transparent;
                    BtnDarkTheme.TextColor = unselectedTextColor;
                    BtnDarkTheme.BorderWidth = 1;
                    BtnDarkTheme.BorderColor = unselectedBorderColor;
                }
            }
        }
    }
}
