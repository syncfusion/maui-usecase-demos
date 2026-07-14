using E_Commerce_Orders.Models;

namespace E_Commerce_Orders
{
    public partial class App : Application
    {
        public static Order? SelectedOrder { get; set; }

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}