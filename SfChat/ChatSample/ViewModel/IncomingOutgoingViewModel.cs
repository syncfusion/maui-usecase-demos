using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Syncfusion.Maui.Chat;
using System.Threading.Tasks;
using System;

namespace ChatSample
{
    public class IncomingOutgoingViewModel : BindableObject
    {
        public ObservableCollection<object> Messages { get; } = new ObservableCollection<object>();

        public Author CurrentUser { get; set; }

        public ChatTypingIndicator TypingIndicator { get; set; }

        private bool showTypingIndicator;
        public bool ShowTypingIndicator
        {
            get => showTypingIndicator;
            set
            {
                showTypingIndicator = value;
                OnPropertyChanged(nameof(ShowTypingIndicator));
            }
        }

        public ICommand SendMessageCommand { get; }

        private readonly Author otherAuthor;
        private int replyIndex = 0;

        public IncomingOutgoingViewModel()
        {
            CurrentUser = new Author { Avatar = "me", Name = "You" };
            otherAuthor = new Author { Name = "Alex" };

            TypingIndicator = new ChatTypingIndicator 
            { 
                Authors = new System.Collections.Generic.List<Author>(),
                AvatarViewType = AvatarViewType.Text,
                Text = "typing..."
            };

            GenerateMessages();

            SendMessageCommand = new Command<object>(ExecuteSendMessageCommand);

             _ = StartTypingSimulation();
        }

        private async void ExecuteSendMessageCommand(object sender)
        {
            if (sender is SendMessageEventArgs eventArgs && eventArgs.Message is TextMessage message)
            {
                message.DeliveryState = Syncfusion.Maui.Chat.DeliveryStates.Sent;
                await UpdateDeliveryStatesIfCurrentUser(message);
            }
        }

        private async void GenerateMessages()
        {
            // Sample messages - demonstrate delivery states for outgoing messages
            var userMessage1 = new TextMessage
            {
                Author = CurrentUser,
                Text = "Haso ypu here?",
                DeliveryState = Syncfusion.Maui.Chat.DeliveryStates.Sent,
            };

            Messages.Add(userMessage1);
            await UpdateDeliveryStatesIfCurrentUser(userMessage1);

            Messages.Add(new TextMessage
            {
                Author = otherAuthor,
                Text = "Sure we ettinla arare.",
                DeliveryState = DeliveryStates.None
            });

            var userMessage2 = new TextMessage
            {
                Author = CurrentUser,
                Text = "2 PM oTTgl",
                DeliveryState = Syncfusion.Maui.Chat.DeliveryStates.Sent,
            };
            Messages.Add(userMessage2);
            await UpdateDeliveryStatesIfCurrentUser(userMessage2);

        }

        private readonly ObservableCollection<string> autoReplies = new ObservableCollection<string>
        {
            "Yes, I'm here 🙂",
            "On my way!",
            "Got it 👍",
            "Let's meet at 2 PM",
            "Okay, see you soon!",
            "Sure, sounds good!"
        };

        private async Task StartTypingSimulation()
        {
            try
            {
                while (true)
                {
                    await Task.Delay(4000).ConfigureAwait(true);
                    
                    // Show typing indicator
                    TypingIndicator.Authors.Clear();

                    TypingIndicator.Authors.Add(new Author() { Name = "Alex", Avatar = "alex.png" });                
                    TypingIndicator.AvatarViewType = AvatarViewType.Image;
                    TypingIndicator.Authors.Add(otherAuthor);
                    TypingIndicator.Text = "Alex is typing...";
                    ShowTypingIndicator = true;

                    await Task.Delay(2500).ConfigureAwait(true);
                    
                    // Hide typing indicator
                    ShowTypingIndicator = false;

                    // ✅ Pick message from collection
                    string replyText = autoReplies[replyIndex];

                    // ✅ Move to next message
                    replyIndex++;
                    if (replyIndex >= autoReplies.Count)
                    {
                        //replyIndex = 0; // loop again
                        return;
                    }

                    // ✅ Add message
                    var incoming = new TextMessage
                    {
                        Author = otherAuthor,
                        Text = replyText,
                        DateTime = DateTime.Now
                    };

                    Messages.Add(incoming);

                    await Task.Delay(10000).ConfigureAwait(true);
                }
            }
            catch
            {
                // ignore errors in simulation
            }
        }

        private async Task UpdateDeliveryStatesIfCurrentUser(MessageBase messageObj)
        {
            try
            {
                if (messageObj?.Author == CurrentUser)
                {
                    await Task.Delay(500).ConfigureAwait(true);
                    messageObj.DeliveryState = DeliveryStates.Sent;
                    await Task.Delay(500).ConfigureAwait(true);
                    messageObj.DeliveryState = DeliveryStates.Delivered;
                    await Task.Delay(500).ConfigureAwait(true);
                    messageObj.DeliveryState = DeliveryStates.Read;
                }
            }
            catch
            {
                // ignore errors in demo progression
            }
        }
    }
}
