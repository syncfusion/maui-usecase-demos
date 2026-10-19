using CommunityToolkit.Mvvm.ComponentModel;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    public ProfileViewModel()
    {
        Name = "Alex Carter";
        Email = "alex.carter@email.com";
        MemberSince = "Aug 2023";
        City = "New York, NY";
    }

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _memberSince = string.Empty;
    [ObservableProperty] private string _city = string.Empty;
    public string Initials
    {
        get
        {
            var parts = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? "?" : (parts[0][0] + (parts.Length > 1 ? parts[^1][0].ToString() : "")).ToUpper();
        }
    }
}