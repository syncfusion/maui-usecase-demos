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
        History = new ObservableCollection<Booking>(_repo.All);

        // Subscribe via INotifyCollectionChanged interface to avoid the
        // protected CollectionChanged event accessor on some target frameworks.
        ((INotifyCollectionChanged)_repo.All).CollectionChanged += (_, _) => MainThread.BeginInvokeOnMainThread(() =>
        {
            Bookings.Clear();
            History.Clear();
            foreach (var b in _repo.All)
            {
                Bookings.Add(b);
                History.Add(b);
            }
            OnPropertyChanged(nameof(HasBookings));
            OnPropertyChanged(nameof(UpcomingCount));
            OnPropertyChanged(nameof(HistoryCount));
            OnPropertyChanged(nameof(NextIn));
        });

        OnPropertyChanged(nameof(HasBookings));
        OnPropertyChanged(nameof(UpcomingCount));
        OnPropertyChanged(nameof(HistoryCount));
        OnPropertyChanged(nameof(NextIn));
    }

    public ObservableCollection<Booking> Bookings { get; }
    public ObservableCollection<Booking> History { get; }

    [ObservableProperty] private string _filter = "Upcoming";

    public bool HasBookings => Bookings.Count > 0;
    public int UpcomingCount => Bookings.Count;
    public int HistoryCount => Bookings.Count;
    public string NextIn => "2h 45m";
}