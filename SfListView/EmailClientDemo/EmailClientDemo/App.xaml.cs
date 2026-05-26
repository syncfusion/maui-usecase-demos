using Microsoft.Extensions.DependencyInjection;

namespace EmailClientDemo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
#if ANDROID || IOS
            return new Window(new MainPage());

#elif WINDOWS || MACCATALYST
            return new Window(new MainPage1());

#else
            // Fallback (optional but recommended)
            return new Window(new MainPage());
#endif
        }

    }
}