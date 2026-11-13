using FluentMail.ViewModels;
using Syncfusion.Maui.Calendar;

namespace FluentMail.Views;

public partial class CalendarPage : ContentPage
{
    readonly CalendarViewModel viewModel = new();

    public CalendarPage()
    {
        InitializeComponent();
        BindingContext = viewModel;
        ConfigureCalendar(PhoneCalendar);
        viewModel.EntriesChanged += (_, _) => { ConfigureCalendar(PhoneCalendar); };
        viewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(CalendarViewModel.Mode)) ConfigureCalendar(PhoneCalendar); };
    }

    void ConfigureCalendar(SfCalendar calendar)
    {
        calendar.MonthView = new CalendarMonthView
        {
            NumberOfVisibleWeeks = calendar == PhoneCalendar && viewModel.IsWeekMode ? 1 : 6,
            TodayBackground = Color.FromArgb("#D7E8FB"),
            SpecialDayPredicate = date => viewModel.GetMarker(date)!
        };
    }
}
