using FluentMail.ViewModels;
using Syncfusion.Maui.Calendar;

namespace FluentMail.Views.Controls;

public partial class CalendarFlyout : ContentView
{
    public CalendarFlyout()
    {
        InitializeComponent();
        BindingContextChanged += (_, _) => Attach();
    }

    CalendarViewModel? viewModel;

    void Attach()
    {
        if (viewModel is not null) viewModel.EntriesChanged -= OnEntriesChanged;
        viewModel = BindingContext as CalendarViewModel;
        if (viewModel is null) return;
        viewModel.EntriesChanged += OnEntriesChanged;
        Configure();
    }

    void OnEntriesChanged(object? sender, EventArgs e) => Configure();

    void Configure() => FlyoutCalendar.MonthView = new CalendarMonthView
    {
        TodayBackground = Color.FromArgb("#0F5DB5"),
        TodayTextStyle = new CalendarTextStyle { TextColor = Colors.White, FontAttributes = FontAttributes.Bold },
        SpecialDayPredicate = date => viewModel!.GetMarker(date)!
    };

    void OnCloseClicked(object? sender, EventArgs e)
    {
        if (Parent?.BindingContext is MailWorkspaceViewModel mail) mail.IsCalendarOpen = false;
    }
}
