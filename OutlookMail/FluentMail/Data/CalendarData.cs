using System.Collections.ObjectModel;
using FluentMail.Models;

namespace FluentMail.Data;

public static class CalendarData
{
    public static ObservableCollection<CalendarEntry> Entries { get; } = Create();

    static ObservableCollection<CalendarEntry> Create()
    {
        var d = DateTime.Today;
        CalendarEntry E(int day, int h, int m, int minutes, string title, string category, string location = "", bool reminder = false, bool high = false) => new()
        {
            Title = title, Category = category, Location = location, IsReminder = reminder, IsHighPriority = high,
            Start = d.AddDays(day).AddHours(h).AddMinutes(m), End = d.AddDays(day).AddHours(h).AddMinutes(m + minutes)
        };

        return
        [
            E(0, 9, 30, 60, "Sprint Planning & Syncfusion MAUI Roadmap", "Engineering Sync", "Microsoft Teams"),
            E(0, 11, 30, 30, "1:1 Sync: Elena Rostova & Sarah Jenkins", "Design & Token Review", "Focus: Design Tokens & SfCalendar Specs"),
            E(0, 13, 30, 60, "Cloud Infrastructure Architecture Review", "High Priority", "Executive Boardroom 4B", high: true),
            E(0, 16, 0, 45, "Client Demo: .NET MAUI Enterprise Mail App", "Product Showcase", "SfCalendar & SfListView showcase"),
            E(0, 17, 30, 0, "Send weekly status report", "Reminder", reminder: true),
            E(1, 10, 0, 60, "MAUI Mobile Platform Sync & Release Gate", "Release", "Teams"),
            E(1, 14, 0, 90, "Quarterly Design All-Hands", "All Hands", "Auditorium"),
            E(2, 9, 0, 0, "Submit expense report", "Reminder", reminder: true),
            E(2, 15, 0, 60, "Vendor review: licensing", "Procurement", "Room 12"),
            E(-1, 11, 0, 45, "Roadmap retrospective", "Engineering Sync", "Teams"),
            E(3, 10, 30, 30, "Budget approval check-in", "Finance", "Teams"),
            E(4, 9, 0, 0, "Renew security training", "Reminder", reminder: true)
        ];
    }
}
