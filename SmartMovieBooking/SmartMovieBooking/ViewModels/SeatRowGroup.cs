using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SmartMovieBooking.Models;

namespace SmartMovieBooking.ViewModels;

/// <summary>
/// One row of seats (A1..A8, plus optional aisle). Mobile layout
/// uses a single row label + 8 cells.
/// </summary>
public partial class SeatRowGroup : ObservableObject
{
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private ObservableCollection<Seat> _seats = new();
    public bool IsPrime { get; set; }
}