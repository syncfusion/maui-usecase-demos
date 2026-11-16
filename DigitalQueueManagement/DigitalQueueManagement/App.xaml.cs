using DigitalQueueManagement.Views;

namespace DigitalQueueManagement;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage())
		{
			Title = "Digital Queue Management",
			Width = 1440,
			Height = 1000
		};
	}
}
