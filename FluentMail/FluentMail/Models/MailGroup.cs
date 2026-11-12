namespace FluentMail.Models;

public class MailGroup
{
    public string Title { get; set; } = string.Empty;
    public string CountText { get; set; } = string.Empty;
    public List<MailMessage> Items { get; set; } = [];
}
