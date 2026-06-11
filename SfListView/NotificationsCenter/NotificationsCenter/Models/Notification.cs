using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NotificationsCenter
{

    public class Notification : INotifyPropertyChanged
    {
        public string Icon { get; set; }
        private string title;
        private string preview;
        private string fullMessage;
        private string category;
        private DateTime timestamp;
        private bool isExpanded;

        public string Title
        {
            get => title;
            set
            {
                title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        public string Preview
        {
            get => preview;
            set
            {
                preview = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Preview)));
            }
        }

        public string FullMessage
        {
            get => fullMessage;
            set
            {
                fullMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullMessage)));
            }
        }

        public string Category
        {
            get => category;
            set
            {
                category = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Category)));
            }
        }

        public DateTime Timestamp
        {
            get => timestamp;
            set
            {
                timestamp = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Timestamp)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FormattedTimestamp)));
            }
        }

        public string FormattedTimestamp
        {
            get
            {
                var timeSpan = DateTime.Now - timestamp;
                
                if (timeSpan.TotalSeconds < 60)
                    return "just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes}m ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours}h ago";
                if (timeSpan.TotalDays < 2)
                    return "Yesterday";
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays}d ago";
                
                return timestamp.ToString("MMM d");
            }
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
