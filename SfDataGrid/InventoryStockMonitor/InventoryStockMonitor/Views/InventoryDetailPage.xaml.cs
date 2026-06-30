using StockMonitor.ViewModels;

namespace StockMonitor.Views
{
    public partial class InventoryDetailPage : ContentPage
    {
        private readonly InventoryDetailViewModel _viewModel;

        public InventoryDetailPage(InventoryDetailViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width > 0 && height > 0)
            {
                UpdateLayoutForScreenSize(width);
            }
        }

        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            // Page loaded - binding initialized
        }

        private void OnDetailGridSizeChanged(object sender, EventArgs e)
        {
            UpdateLayoutForScreenSize(DetailGrid.Width);
        }

        private void UpdateLayoutForScreenSize(double screenWidth)
        {
            if (DetailGrid == null || FormStack == null) return;
            
            if (screenWidth <= 0)
            {
                screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            }

            // On mobile (< 600px), use full width labels
            // On desktop (>= 600px), use 120px labels for compact layout
            if (screenWidth < 600)
            {
                // Mobile: stack labels and inputs vertically
                foreach (var child in FormStack.Children)
                {
                    if (child is Grid grid)
                    {
                        grid.ColumnDefinitions.Clear();
                        grid.RowDefinitions.Clear();
                        
                        // Single column, label on row 0, input on row 1
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                        // Position children: label in row 0, input in row 1
                        int row = 0;
                        foreach (var gridChild in grid.Children)
                        {
                            grid.SetColumn(gridChild, 0);
                            grid.SetRow(gridChild, row);
                            row++;
                        }
                    }
                }
            }
            else
            {
                // Desktop: keep horizontal layout with fixed label width
                foreach (var child in FormStack.Children)
                {
                    if (child is Grid grid)
                    {
                        grid.ColumnDefinitions.Clear();
                        grid.RowDefinitions.Clear();
                        
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = 129 });
                        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                        grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                        // Reset grid positioning
                        foreach (var gridChild in grid.Children)
                        {
                            if (grid.Children.IndexOf(gridChild) == 0)
                            {
                                grid.SetColumn(gridChild, 0);
                                grid.SetRow(gridChild, 0);
                            }
                            else
                            {
                                grid.SetColumn(gridChild, 1);
                                grid.SetRow(gridChild, 0);
                            }
                        }
                    }
                }
            }
        }
    }
}
