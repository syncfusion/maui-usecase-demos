using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SmartMovieBooking.Models;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class BookingsViewModel : ObservableObject
{
    private readonly IBookingRepository _repo;

    public BookingsViewModel(IBookingRepository repo)
    {
        _repo = repo;
        Bookings = new ObservableCollection<Booking>(_repo.All);
    }

    public ObservableCollection<Booking> Bookings { get; }

    [ObservableProperty] private string _filter = "Upcoming";

    public bool HasBookings => Bookings.Count > 0;
    public int UpcomingCount => Bookings.Count;
    public int HistoryCount => Bookings.Count;
}
