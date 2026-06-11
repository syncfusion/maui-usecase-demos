using System.ComponentModel;

namespace SmartNewsFeed
{
    public class NewsItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        private string _summary = string.Empty;
        public string Summary
        {
            get => _summary;
            set { _summary = value; OnPropertyChanged(nameof(Summary)); }
        }

        public string ImageUrl { get; set; } = string.Empty;

        private DateTime _publishedAt;
        public DateTime PublishedAt
        {
            get => _publishedAt;
            set { _publishedAt = value; OnPropertyChanged(nameof(PublishedAt)); OnPropertyChanged(nameof(TimeAgo)); }
        }

        private bool _isNew;
        public bool IsNew
        {
            get => _isNew;
            set { _isNew = value; OnPropertyChanged(nameof(IsNew)); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        public string TimeAgo
        {
            get
            {
                var diff = DateTime.UtcNow - PublishedAt;

                if (diff.TotalSeconds < 60)
                    return $"{(int)diff.TotalSeconds} sec ago";

                if (diff.TotalMinutes < 60)
                    return $"{(int)diff.TotalMinutes} min ago";

                if (diff.TotalHours < 24)
                    return $"{(int)diff.TotalHours} hr ago";

                return $"{(int)diff.TotalDays} day ago";
            }
        }
    }
}
