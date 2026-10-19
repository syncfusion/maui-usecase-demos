using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class TheatersView : ContentView
{
    private readonly TheatersViewModel _vm;
    private bool _subscribed;
    private MainPage _lastPage = MainPage.MovieHome;

    public TheatersView(TheatersViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;

        if (!_subscribed)
        {
            EventBus.ShowtimeClicked += OnShowtimeClicked;
            _subscribed = true;
        }
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
    }

    public async Task ReloadAsync() => await _vm.InitializeAsync();

    private void OnShowtimeClicked(object? sender, Showtime e)
        => _vm.OnShowtimeSelectedCommand.Execute(e);
}