using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        // Keep Bookings in sync with the repository.
        _repo.All.CollectionChanged += (_, _) => MainThread.BeginInvokeOnMainThread(() =>
        {
            Bookings.Clear();
            foreach (var b in _repo.All) Bookings.Add(b);
            OnPropertyChanged(nameof(HasBookings));
        });

        OnPropertyChanged(nameof(HasBookings));
    }

    public ObservableCollection<Booking> Bookings { get; }

    [ObservableProperty] private string _filter = "All";

    public bool HasBookings => Bookings.Count > 0;
}