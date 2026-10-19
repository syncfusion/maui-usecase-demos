using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartMovieBooking.Models;
using SmartMovieBooking.Services;

namespace SmartMovieBooking.ViewModels;

public partial class DateChipItem : ObservableObject
{
    [ObservableProperty] private DateTime _date;
    [ObservableProperty] private string _label = string.Empty;
    [ObservableProperty] private string _dayNumber = string.Empty;
    [ObservableProperty] private string _weekday = string.Empty;
    [ObservableProperty] private bool _isSelected;
}

public partial class TheatersViewModel : ObservableObject
{
    private readonly ITheaterService _theaters;
    private readonly IShowtimeService _showtimes;
    private readonly BookingSession _session;
    private readonly MainWindowViewModel _main;

    public TheatersViewModel(ITheaterService theaters,
                             IShowtimeService showtimes,
                             BookingSession session,
                             MainWindowViewModel main)
    {
        _theaters = theaters;
        _showtimes = showtimes;
        _session = session;
        _main = main;
        BuildDateStrip();
    }

    public ObservableCollection<DateChipItem> DateChips { get; } = new();
    public ObservableCollection<Theater> Theaters { get; } = new();

    [ObservableProperty] private Movie? _movie;
    [ObservableProperty] private DateChipItem? _selectedDateChip;
    [ObservableProperty] private bool _isLoading;

    public event EventHandler<Showtime>? ShowtimeSelected;

    public async Task InitializeAsync()
    {
        Movie = _session.SelectedMovie;
        await LoadTheatersAsync();
    }

    private void BuildDateStrip()
    {
        DateChips.Clear();
        var today = DateTime.Today;
        for (int i = 0; i < 7; i++)
        {
            var dt = today.AddDays(i);
            var rel = ComputeLabel(dt);
            var item = new DateChipItem
            {
                Date = dt,
                Label = rel,
                DayNumber = dt.Day.ToString(),
                Weekday = $"{dt.ToString("ddd", CultureInfo.InvariantCulture)} • {dt.ToString("MMM", CultureInfo.InvariantCulture)}",
                IsSelected = i == 0
            };
            DateChips.Add(item);
        }
        SelectedDateChip = DateChips[0];
    }

    partial void OnSelectedDateChipChanged(DateChipItem? value)
    {
        if (value is null) return;
        // mark selection visually
        foreach (var c in DateChips) c.IsSelected = false;
        value.IsSelected = true;
        _session.SelectedDate = value.Date;
        _ = LoadTheatersAsync();
    }

    public async Task LoadTheatersAsync()
    {
        IsLoading = true;
        try
        {
            var movieId = Movie?.Id ?? _session.SelectedMovie?.Id ?? "";
            if (string.IsNullOrEmpty(movieId)) return;

            _session.SelectedDate = SelectedDateChip?.Date ?? DateTime.Today;

            var theaters = await _theaters.GetTheatersForMovieAsync(movieId);
            Theaters.Clear();
            foreach (var t in theaters)
            {
                t.Showtimes = (await _showtimes.GetAsync(movieId, t.Id, _session.SelectedDate)).ToList();
                Theaters.Add(t);
            }
        }
        finally { IsLoading = false; }
    }

    /// <summary>Public so the view can invoke the generated command from the bus.</summary>
    [RelayCommand]
    public void OnTheaterContinue(Theater theater)
    {
        // Use the most recently selected showtime for this theater; if none,
        // pick the first available one.
        var s = _session.SelectedShowtime;
        if (s is null || s.TheaterId != theater.Id)
        {
            s = theater.Showtimes.FirstOrDefault(x => x.Status != ShowtimeStatus.SoldOut);
            if (s is null) return;
            _session.SelectedShowtime = s;
            _session.SelectedTheater = theater;
        }

        EventBus.PublishTheaterContinueClicked(theater);
    }

    /// <summary>Public so the view can invoke the generated command from the bus.</summary>
    [RelayCommand]
    public void OnShowtimeSelected(Showtime showtime)
    {
        // Find the parent theater
        Theater? parent = null;
        foreach (var t in Theaters)
        {
            if (t.Showtimes.Contains(showtime)) { parent = t; break; }
        }
        if (parent is null) return;

        // Toggle selection across all theaters
        foreach (var t in Theaters)
            foreach (var s in t.Showtimes)
                s.IsSelected = false;
        showtime.IsSelected = true;

        _session.SelectedTheater = parent;
        _session.SelectedShowtime = showtime;

        ShowtimeSelected?.Invoke(this, showtime);
    }

    private static string ComputeLabel(DateTime dt)
    {
        var today = DateTime.Today;
        var days = (dt.Date - today.Date).Days;
        return days switch
        {
            0 => "Today",
            1 => "Tomorrow",
            >= 2 and <= 6 when dt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday => "Weekend",
            >= 2 and <= 6 => "Weekday",
            _ => string.Empty
        };
    }
}