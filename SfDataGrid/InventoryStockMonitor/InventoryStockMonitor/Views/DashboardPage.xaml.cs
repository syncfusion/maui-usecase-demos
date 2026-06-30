using StockMonitor.ViewModels;

namespace StockMonitor.Views
{
    public partial class DashboardPage : ContentPage
    {
        private readonly DashboardViewModel _viewModel;
        private const double DesktopBreakpoint = 800; // Width threshold for desktop vs mobile
        private bool _isFirstLoad = true; // Track if this is the first time loading data

        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Only load data on first appearance, not on every navigation back
            if (_isFirstLoad)
            {
                _viewModel.LoadDataCommand.Execute(null);
                _isFirstLoad = false;
            }
            else
            {
                // When returning from detail page, refresh the grid to show updated/deleted values
                // WITHOUT reloading from file (keeps edits in memory)
                _viewModel.RefreshGridDisplay();
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width > 0 && height > 0)
            {
                UpdateLayoutForScreenSize(width);
            }
        }

        private void OnGridSizeChanged(object sender, EventArgs e)
        {
            UpdateLayoutForScreenSize(MainGrid.Width);
        }

        private void UpdateLayoutForScreenSize(double screenWidth)
        {
            if (MainGrid == null) return;

            // Clear existing definitions
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();

            if (screenWidth < DesktopBreakpoint)
            {
                // Mobile layout - stack vertically with MORE space for grid
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });      // Grid gets remaining space
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });     // Controls auto-size

                // Move grid to row 0 - takes most of available space
                Grid.SetColumn(InventoryGrid, 0);
                Grid.SetRow(InventoryGrid, 0);
                InventoryGrid.HeightRequest = -1;  // Let it fill available space

                // Move controls to row 1
                Grid.SetColumn(ControlsPanel, 0);
                Grid.SetRow(ControlsPanel, 1);

                // Set controls panel orientation to vertical for mobile
                ControlsPanel.Orientation = StackOrientation.Vertical;
            }
            else
            {
                // Desktop layout - side by side
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 200 });
                MainGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });

                // Keep grid at column 0
                Grid.SetColumn(InventoryGrid, 0);
                Grid.SetRow(InventoryGrid, 0);
                InventoryGrid.HeightRequest = -1;

                // Keep controls at column 1
                Grid.SetColumn(ControlsPanel, 1);
                Grid.SetRow(ControlsPanel, 0);

                // Set controls panel orientation to vertical for desktop
                ControlsPanel.Orientation = StackOrientation.Vertical;
            }
        }
    }
}
