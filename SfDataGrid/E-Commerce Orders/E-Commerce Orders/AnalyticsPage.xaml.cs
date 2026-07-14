using E_Commerce_Orders.ViewModels;

namespace E_Commerce_Orders
{
    public partial class AnalyticsPage : ContentPage
    {
        private readonly AnalyticsViewModel _vm = new();

        public AnalyticsPage()
        {
            InitializeComponent();
            BindingContext = _vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = _vm.LoadAsync();
        }
    }
}
