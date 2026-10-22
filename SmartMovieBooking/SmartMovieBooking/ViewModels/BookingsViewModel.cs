using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using SmartMovieBooking.Models;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class BookingsViewModel : ObservableObject, IDisposable
{
    private readonly IBookingRepository _repo;

    public BookingsViewModel(IBookingRepository repo)
    {
        _repo = repo;
        Bookings = new ObservableCollection<Booking>(_repo.All);

        // Subscribe via the IBookingRepository.CollectionChanged event (forwarded
        // from the inner store by BookingRepository). We CAN'T subscribe to
        // _repo.All.CollectionChanged directly - that accessor exposes the
        // protected event on ReadOnlyObservableCollection<T> and the compiler
        // refuses with CS0122. Going through the interface is the safe path.
        _repo.CollectionChanged += OnRepoChanged;
    }

    public ObservableCollection<Booking> Bookings { get; }

    [ObservableProperty] private string _filter = "Upcoming";

    public bool HasBookings => Bookings.Count > 0;
    public int UpcomingCount => Bookings.Count;
    public int HistoryCount => Bookings.Count;

    private void OnRepoChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Marshal to the UI thread since Add may be called from an async continuation.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Bookings.Clear();
            foreach (var b in _repo.All)
                Bookings.Add(b);

            OnPropertyChanged(nameof(HasBookings));
            OnPropertyChanged(nameof(UpcomingCount));
            OnPropertyChanged(nameof(HistoryCount));
        });
    }

    public void Dispose()
    {
        _repo.CollectionChanged -= OnRepoChanged;
        GC.SuppressFinalize(this);
    }
}