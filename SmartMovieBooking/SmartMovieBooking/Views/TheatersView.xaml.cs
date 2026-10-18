using SmartMovieBooking.Models;
using SmartMovieBooking.Services;
using SmartMovieBooking.ViewModels;

namespace SmartMovieBooking.Views;

public partial class TheatersView : ContentView
{
    private readonly TheatersViewModel _vm;
    private bool _subscribed;

    public TheatersView(TheatersViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (!_subscribed)
        {
            EventBus.ShowtimeClicked += OnShowtimeClicked;
            _subscribed = true;
        }
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        if (Parent is null && _subscribed)
        {
            EventBus.ShowtimeClicked -= OnShowtimeClicked;
            _subscribed = false;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
    }

    private void OnShowtimeClicked(object? sender, Showtime e)
        => _vm.OnShowtimeSelectedCommand.Execute(e);
}