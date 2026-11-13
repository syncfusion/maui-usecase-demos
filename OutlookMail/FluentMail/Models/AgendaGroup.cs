namespace FluentMail.Models;

public class AgendaGroup
{
    public string Title { get; set; } = string.Empty;
    public string CountText { get; set; } = string.Empty;
    public List<CalendarEntry> Items { get; set; } = [];
}
