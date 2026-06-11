using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;

namespace NotificationsCenter
{
    public class NotificationsViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Notification> notifications;
        private bool isRefreshing;

        public ObservableCollection<Notification> Notifications
        {
            get => notifications;
            set
            {
                notifications = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notifications)));
            }
        }

        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                isRefreshing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRefreshing)));
            }
        }

        public ICommand PullingCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ToggleExpandCommand { get; }

        public NotificationsViewModel()
        {
            PullingCommand = new Command(OnPulling);
            DeleteItemCommand = new Command<Notification>(OnDeleteItem);
            ToggleExpandCommand = new Command<Notification>(OnToggleExpand);
            Notifications = new ObservableCollection<Notification>();
        }

        public void PopulateNotifications()
        {
            // Populate notifications
            Notifications.Clear();
            var notificationsList = new List<Notification>
        {
            new Notification
            {
                Icon = "\ue716",
                Title = "System Update",
                Preview = "App updated successfully",
                FullMessage = "Your application has been updated.",
                Category = "System",
                Timestamp = DateTime.Now.AddHours(-2)
            },
            new Notification
            {
                Icon = "\uE71b",
                Title = "Friend Request",
                Preview = "John sent you a request",
                FullMessage = "Accept or ignore the request.",
                Category = "Social",
                Timestamp = DateTime.Now.AddHours(-8)
            },
            new Notification
            {
                Icon = "\ue71f",
                Title = "Promotion",
                Preview = "Flat 50% discount",
                FullMessage = "Enjoy seasonal offers.",
                Category = "Promos",
                Timestamp = DateTime.Now.AddHours(-3)
            },
            new Notification
            {
                Icon = "\ue716",
                Title = "Security Alert",
                Preview = "New login detected",
                FullMessage = "A login was detected from a new device.",
                Category = "System",
                Timestamp = DateTime.Now.AddDays(-1)
            },
            new Notification
            {
                Icon = "\uE77B",
                Title = "New Message",
                Preview = "You have a new chat message",
                FullMessage = "A new message has arrived in your inbox.",
                Category = "Social",
                Timestamp = DateTime.Now.AddMinutes(-30)
            },
            new Notification
            {
                Icon = "\uE719",
                Title = "Flash Sale",
                Preview = "Limited time offer",
                FullMessage = "Hurry up! Flash sale ends tonight.",
                Category = "Promos",
                Timestamp = DateTime.Now.AddHours(-6)
            },
            new Notification
            {
                Icon = "\uE7BA",
                Title = "System Backup",
                Preview = "Backup completed",
                FullMessage = "Your system backup was completed successfully.",
                Category = "System",
                Timestamp = DateTime.Now.AddHours(-10)
            },
            new Notification
            {
                Icon = "\ue774",
                Title = "Event Reminder",
                Preview = "Meeting starts soon",
                FullMessage = "Reminder: Your scheduled meeting starts in 15 minutes.",
                Category = "Social",
                Timestamp = DateTime.Now.AddMinutes(-10)
            },
            new Notification
            {
                Icon = "\uE7BF",
                Title = "Special Offer",
                Preview = "Exclusive deal just for you",
                FullMessage = "Unlock exclusive discounts available only today.",
                Category = "Promos",
                Timestamp = DateTime.Now.AddDays(-2)
            }
        };

            // Sort notifications by timestamp (newest first)
            var sortedNotifications = notificationsList.OrderByDescending(n => n.Timestamp).ToList();
            
            foreach (var notification in sortedNotifications)
            {
                Notifications.Add(notification);
            }

            IsRefreshing = false;
        }

        private void OnDeleteItem(Notification notification)
        {
            if (notification != null && Notifications?.Contains(notification) == true)
            {
                Notifications.Remove(notification);
            }
        }

        private void OnToggleExpand(Notification notification)
        {
            if (notification != null)
            {
                notification.IsExpanded = !notification.IsExpanded;
            }
        }

        private void OnPulling()
        {
            PopulateNotifications();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
