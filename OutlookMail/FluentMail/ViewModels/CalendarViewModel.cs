using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentMail.Data;
using FluentMail.Models;
using Syncfusion.Maui.Calendar;

namespace FluentMail.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    readonly ObservableCollection<CalendarEntry> entries = CalendarData.Entries;

    public event EventHandler? EntriesChanged;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthTitle), nameof(DayBadge))]
    private DateTime? selectedDate = DateTime.Today;

    [ObservableProperty]
    private string mode = "Agenda";

    [ObservableProperty]
    private ObservableCollection<AgendaGroup> agendaGroups = [];

    [ObservableProperty]
    private bool isAddOpen;

    [ObservableProperty]
    private string newTitle = string.Empty;

    [ObservableProperty]
    private bool newIsReminder;

    [ObservableProperty]
    private TimeSpan newTime = new(9, 0, 0);

    public string MonthTitle => (SelectedDate ?? DateTime.Today).ToString("MMMM\nyyyy");
    public string DayBadge => (SelectedDate ?? DateTime.Today).ToString("MMM\ndd").ToUpperInvariant();
    public double PhoneCalendarHeight => Mode == "Month" ? 330 : 120;
    public bool IsMonthMode => Mode == "Month";
    public bool IsWeekMode => Mode != "Month";

    public CalendarViewModel() => Refresh();

    partial void OnSelectedDateChanged(DateTime? value) => Refresh();

    partial void OnModeChanged(string value)
    {
        OnPropertyChanged(nameof(IsMonthMode));
        OnPropertyChanged(nameof(PhoneCalendarHeight));
        OnPropertyChanged(nameof(IsWeekMode));
        Refresh();
    }

    [RelayCommand]
    void SetMode(string mode) => Mode = mode;

    [RelayCommand]
    void OpenAdd()
    {
        NewTitle = string.Empty;
        NewIsReminder = false;
        IsAddOpen = true;
    }

    [RelayCommand]
    void CancelAdd() => IsAddOpen = false;

    [RelayCommand]
    void SaveEntry()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        var start = (SelectedDate ?? DateTime.Today).Date.Add(NewTime);
        entries.Add(new CalendarEntry
        {
            Title = NewTitle.Trim(),
            Category = NewIsReminder ? "Reminder" : "Event",
            IsReminder = NewIsReminder,
            Start = start,
            End = NewIsReminder ? start : start.AddHours(1)
        });
        IsAddOpen = false;
        Refresh();
        EntriesChanged?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    void DeleteEntry(CalendarEntry? entry)
    {
        if (entry is null) return;
        entries.Remove(entry);
        Refresh();
        EntriesChanged?.Invoke(this, EventArgs.Empty);
    }

    public CalendarIconDetails? GetMarker(DateTime date)
    {
        var day = entries.Where(x => x.Start.Date == date.Date).ToList();
        if (day.Count == 0) return null;
        var color = day.Any(x => x.IsHighPriority) ? Colors.Red : day.All(x => x.IsReminder) ? Colors.Gray : Color.FromArgb("#0F5DB5");
        return new CalendarIconDetails { Icon = day.Any(x => x.IsReminder) && day.All(x => x.IsReminder) ? CalendarIcon.Bell : CalendarIcon.Dot, Fill = color };
    }

    void Refresh()
    {
        var start = (SelectedDate ?? DateTime.Today).Date;
        var days = Mode == "Agenda" ? new[] { start, start.AddDays(1) } : new[] { start };
        var groups = new ObservableCollection<AgendaGroup>();
        foreach (var day in days)
        {
            var items = entries.Where(x => x.Start.Date == day).OrderBy(x => x.Start).ToList();
            if (items.Count == 0 && day != start) continue;
            groups.Add(new AgendaGroup
            {
                Title = Describe(day),
                CountText = items.Count == 1 ? "1 Event" : $"{items.Count} Events",
                Items = items
            });
        }
        AgendaGroups = groups;
    }

    static string Describe(DateTime day)
    {
        var text = day.ToString("dddd, MMM d");
        if (day == DateTime.Today) return $"Today • {text}";
        if (day == DateTime.Today.AddDays(1)) return $"Tomorrow • {text}";
        if (day == DateTime.Today.AddDays(-1)) return $"Yesterday • {text}";
        return text;
    }
}
