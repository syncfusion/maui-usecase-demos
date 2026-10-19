using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartMovieBooking.Models;

public partial class Seat : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _row = string.Empty;
    [ObservableProperty] private int _column;
    [ObservableProperty] private SeatState _state = SeatState.Available;
    [ObservableProperty] private SeatTier _tier = SeatTier.Normal;
    [ObservableProperty] private decimal _price;
    [ObservableProperty] private bool _isSelected;
}

public enum SeatState { Available, Occupied, Selected, Aisle }
public enum SeatTier { Normal, Premium, Executive }
