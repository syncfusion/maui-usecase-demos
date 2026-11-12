namespace FluentMail.Models;

public class CalendarEntry
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool IsReminder { get; set; }
    public bool IsHighPriority { get; set; }
    public bool HasLocation => !string.IsNullOrEmpty(Location);
    public string Accent => IsHighPriority ? "#C50F1F" : IsReminder ? "#8A8886" : "#0F62B8";
    public string TimeText => IsReminder ? $"{Start:hh:mm tt}" : $"{Start:hh:mm tt} – {End:hh:mm tt}";
    public string KindText => IsReminder ? "Reminder" : "Event";
}
