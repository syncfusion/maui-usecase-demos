using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartMovieBooking.Models;

public partial class CastMember : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _character = string.Empty;
    [ObservableProperty] private string _avatarUrl = string.Empty;
}
