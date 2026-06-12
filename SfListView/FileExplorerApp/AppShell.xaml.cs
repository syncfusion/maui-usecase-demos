namespace FileExplorerApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ExplorerPage), typeof(ExplorerPage));
        }
    }
}
