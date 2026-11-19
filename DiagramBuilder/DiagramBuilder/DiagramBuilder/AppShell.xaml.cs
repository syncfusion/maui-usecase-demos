using DiagramBuilder.Constants;
using DiagramBuilder.Views;

namespace DiagramBuilder
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(AppConstants.RouteCreateDiagram, typeof(CreateDiagramPage));
        }
    }
}