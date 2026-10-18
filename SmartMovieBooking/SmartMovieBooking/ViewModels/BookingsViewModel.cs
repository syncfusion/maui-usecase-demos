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
        _repo.All.CollectionChanged += (_, e) => MainThread.BeginInvokeOnMainThread(() =>
        {
            Bookings.Clear();
            foreach (var b in _repo.All) Bookings.Add(b);
        });
    }

    public ObservableCollection<Booking> Bookings { get; }

    [ObservableProperty] private string _filter = "All";
    [ObservableProperty] private bool _isEmpty;

    public bool HasBookings => Bookings.Count > 0;
}