using DigitalQueueManagement.Views;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalQueueManagement;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new(_services.GetRequiredService<MainPage>()) { Title = "Digital Queue Management" };
}
