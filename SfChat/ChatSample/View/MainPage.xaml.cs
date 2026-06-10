namespace ChatSample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            
            // Set the custom message template selector after initialization
           // Chat.MessageTemplate = new MessageDataTemplateSelector(Chat);
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}
