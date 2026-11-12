using Syncfusion.Licensing;

namespace FluentMail;

public partial class App : Application
{
    public App()
    {
        SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JBaF5cXmRCf1FpQHxbf1x2ZFRGalhYTnNWUiweQnxTdENiWH5ccXZVRmFZWEd3XEleZg==");
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}
