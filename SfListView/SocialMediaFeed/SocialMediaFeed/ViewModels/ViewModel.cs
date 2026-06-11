using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace SocialMediaFeed
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;

    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Post> Posts { get; set; }

        private bool isRefreshing;
        private bool isBusy;
        public ICommand LoadMoreCommand { get; }
        public ICommand RefreshCommand { get; }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainViewModel()
        {
            Posts = new ObservableCollection<Post>
        {
            // ✅ 1. Image Post
             new Post
            {
                Name = "Sophia Mehta",
                Caption = "Golden hour meets timeless history 🌅",
                ProfileImage = "profile2.jpg",
                PostImage = "feed2.png"
            },

             new Post
            {
                Name = "Emma Wilson",
                Caption = "Nature at its finest — waterfalls, greenery, and peaceful surroundings 🌿",
                ProfileImage = "profile4.jpg",
                IsLinkPreview = true,
                LinkTitle = "Niagara Falls Travel Guide",
                LinkUrl = "www.travelnature.com/niagara",
                LinkImage = "feed4.jpg"
            },

            new Post
            {
                Name = "Rohit Sharma",
                Caption = "Peaceful evening at the water villas 🌊✨",
                ProfileImage = "profile1.jpg",
                PostImage = "feed1.jpg"
            },

            // ✅ 2. Image Post
           

            // ✅ 3. Mysore Palace
            new Post
            {
                Name = "Ananya Iyer",
                Caption = "Exploring the majestic Mysore Palace — a masterpiece of royal architecture, vibrant domes, and rich heritage. Truly breathtaking! 👑✨",
                ProfileImage = "profile3.jpg",
                PostImage = "feed3.jpg"
            },

            // ✅ 4. Link Preview
           
        };

            RefreshCommand = new Command(OnRefresh);
            LoadMoreCommand = new Command(OnLoadMore);
        }

        private async void OnRefresh()
        {
            if (IsRefreshing)
                return;

            IsRefreshing = true;

            await Task.Delay(1500); // simulate API call

            // ✅ Insert 3 NEW mixed posts at TOP

            // 1️⃣ Image Post
            Posts.Insert(0, new Post
            {
                Name = "Arjun Verma",
                Caption = "Where the mountains meet the sky, and everything feels still 🌄✨",
                ProfileImage = "profile9.jpg",
                PostImage = "feed5.jpg"
            });

            // 2️⃣ Link Preview Post
            Posts.Insert(0, new Post
            {
                Name = "Neha Kapoor",
                Caption = "Stumbled upon this beautiful feature on the Taj Mahal — an iconic symbol of love and one of the most breathtaking monuments in the world 👇",
                ProfileImage = "profile6.jpg",
                IsLinkPreview = true,
                LinkTitle = "Discover the Beauty of the Taj Mahal",
                LinkUrl = "www.travelguide.com/tajmahal",
                LinkImage = "feed10.jpg"
            });

            // 3️⃣ Text-only Post
            Posts.Insert(0, new Post
            {
                Name = "Kiran Das",
                Caption = "Sometimes the best moments are the simplest ones ✨",
                ProfileImage = "profile7.jpg",
                PostImage = "feed7.jpg"
            });

            IsRefreshing = false;
        }

        private async void OnLoadMore()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            await Task.Delay(1500); // simulate API call

            // ✅ Insert 3 NEW mixed posts at TOP

            // 1️⃣ Image Post
            // ✅ 1. Image Post
            Posts.Add(new Post
            {
                Name = "Aditi Sharma",
                Caption = "A breathtaking view of a serene mountain lake surrounded by lush greenery, with storm clouds rolling in and lightning illuminating the sky ⚡🌿",
                ProfileImage = "profile5.jpg",
                PostImage = "feed8.jpg"
            });

            // ✅ 2. Link Preview Post
            Posts.Add(new Post
            {
                Name = "Arjun Menon",
                Caption = "Came across this incredible wildlife capture — a majestic elephant in its natural habitat, surrounded by open grasslands and scattered trees. Truly a glimpse of nature in its purest form 🐘🌿",
                ProfileImage = "profile9.jpg",
                IsLinkPreview = true,
                LinkTitle = "Discover Wildlife and Nature Exploration",
                LinkUrl = "www.travelguide.com/wildlife",
                LinkImage = "feed9.jpg"
            });

            // ✅ 3. Text-only Post
            Posts.Add(new Post
            {
                Name = "Rahul Menon",
                Caption = "A stunning glimpse of ancient temple ruins glowing under a golden sunset — a timeless masterpiece of architecture and history 🏛️✨",
                ProfileImage = "profile10.jpg",
                PostImage = "feed6.jpg"
            });

            IsBusy = false;
        }
    }

}
