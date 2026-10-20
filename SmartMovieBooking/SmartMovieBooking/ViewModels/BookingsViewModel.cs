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

        // CRITICAL: subscribe to the repository's collection so newly created
        // bookings show up here without recreating the page. Without this the
        // Bookings tab stays empty after a successful purchase because the
        // constructor snapshot was taken before any booking existed.
        _repo.All.CollectionChanged += OnRepoChanged;
    }

    public ObservableCollection<Booking> Bookings { get; }

    [ObservableProperty] private string _filter = "Upcoming";

    public bool HasBookings => Bookings.Count > 0;
    public int UpcomingCount => Bookings.Count;
    public int HistoryCount => Bookings.Count;

    private void OnRepoChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Marshal to the UI thread since IBookingRepository.Add may be
        // called from an async continuation.
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Bookings.Clear();
            foreach (var b in _repo.All)
                Bookings.Insert(Math.Min(Bookings.Count, 0), b);

            OnPropertyChanged(nameof(HasBookings));
            OnPropertyChanged(nameof(UpcomingCount));
            OnPropertyChanged(nameof(HistoryCount));
        });
    }

    public void Dispose()
    {
        if (_repo.All is INotifyCollectionChanged n)
            n.CollectionChanged -= OnRepoChanged;
        GC.SuppressFinalize(this);
    }
}