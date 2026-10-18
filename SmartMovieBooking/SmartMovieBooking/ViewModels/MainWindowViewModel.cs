using CommunityToolkit.Mvvm.ComponentModel;
using SmartMovieBooking.Models;

namespace SmartMovieBooking.ViewModels;

public enum MainPage
{
    MovieHome,
    Theaters,
    Bookings
}

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private MainPage _currentPage = MainPage.MovieHome;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _cityLabel = "New York, USA";
    [ObservableProperty] private string _userName = "Alex Carter";
    [ObservableProperty] private string _userAvatarInitials = "AC";
    [ObservableProperty] private bool _isAiCopilotOpen;
    [ObservableProperty] private Movie? _selectedMovie;
}