using FluentMail.Models;
using MailContact = FluentMail.Models.Contact;

namespace FluentMail.Data;

public static class MockData
{
    public static Mailbox Mailbox { get; } = CreateMailbox();

    static Mailbox CreateMailbox()
    {
        var mailbox = new Mailbox();
        mailbox.Folders.Add(new FolderNode { Name = "Inbox", Count = 0, Children = [new FolderNode { Name = "Projects" }, new FolderNode { Name = "Clients" }] });
        mailbox.Folders.Add(new FolderNode { Name = "Sent Items", Count = 0 });
        mailbox.Folders.Add(new FolderNode { Name = "Drafts", Count = 0 });
        mailbox.Folders.Add(new FolderNode { Name = "Archive", Count = 0 });
        mailbox.Folders.Add(new FolderNode { Name = "Deleted", Count = 0 });

        var inbox = new[]
        {
            ("Anastasia Callahan", "anastasia.callahan@contoso.com", "Training announcement", "Please review the attached agenda before the leadership session.", true, true),
            ("Avram Kent", "avram.kent@contoso.com", "Quarterly forecast review", "Sharing the updated forecast workbook for Friday's review.", false, false),
            ("Otto Ashley", "otto.ashley@contoso.com", "Annual conference logistics", "The room block is confirmed and travel guidance is attached.", true, false),
            ("Prescott Mccormick", "prescott.mccormick@contoso.com", "Today's meeting schedule", "Here is the latest version of the operations standup schedule.", false, true),
            ("Stewart Gibbs", "stewart.gibbs@contoso.com", "Application for job title", "Can you confirm the final wording for the posting and interview plan?", false, false),
            ("Renee Walters", "renee.walters@contoso.com", "Marketing campaign assets", "The creative brief and channel calendar are ready for review.", true, false),
            ("Jordan Patel", "jordan.patel@contoso.com", "Client onboarding checklist", "We need the updated onboarding checklist for the finance team.", false, false),
            ("Monica Reed", "monica.reed@contoso.com", "Security awareness training", "Please complete the awareness module by end of week.", true, false),
            ("Ethan Brooks", "ethan.brooks@contoso.com", "Budget approval requested", "The revised budget is now routed for approval.", false, true),
            ("Camila Torres", "camila.torres@contoso.com", "Release notes draft", "I've drafted the release notes for the product update.", false, false),
        };

        for (var i = 0; i < 50; i++)
        {
            var item = inbox[i % inbox.Length];
            var round = i / inbox.Length;
            mailbox.Inbox.Add(new MailMessage
            {
                Id = $"inbox-{i + 1}", Folder = "Inbox", Sender = item.Item1, SenderEmail = item.Item2,
                Subject = round switch
                {
                    0 => item.Item3,
                    1 => item.Item3 + " follow-up",
                    2 => item.Item3 + " updated",
                    3 => item.Item3 + " action items",
                    _ => item.Item3 + " latest"
                },
                Preview = item.Item4,
                Body = item.Item4 + "\n\nRegards,\n" + item.Item1,
                Recipients = "kerry.best@contoso.com", Cc = i % 3 == 0 ? "project.office@contoso.com" : string.Empty,
                Status = item.Item5 ? "Attachments" : "Unread", HasAttachments = item.Item5, IsRead = item.Item6 || i % 4 == 0, IsFlagged = i % 7 == 0,
                ReceivedOn = DateTime.Today.AddDays(-(i % 14)).AddHours(8 + (i % 9))
            });
        }

        var sent = new[]
        {
            ("Me", "me@contoso.com", "Executive summary shared", "I shared the executive summary with the leadership team.", "ceo@contoso.com, finance@contoso.com", true),
            ("Me", "me@contoso.com", "Workshop invitation", "The workshop invite was sent to the operations group.", "operations@contoso.com", false),
            ("Me", "me@contoso.com", "Project status update", "Weekly status update sent to the delivery stakeholders.", "pm@contoso.com, engineering@contoso.com", false),
            ("Me", "me@contoso.com", "Invoice follow-up", "I sent a follow-up on the outstanding invoices.", "accounts.payable@contoso.com", false),
            ("Me", "me@contoso.com", "Hiring panel schedule", "The interview panel calendar has been circulated.", "talent@contoso.com", false),
            ("Me", "me@contoso.com", "Budget revision sent", "The revised budget was shared with the finance leads.", "finance@contoso.com", true),
            ("Me", "me@contoso.com", "Client call recap", "I sent the recap and next steps after the client call.", "clientsuccess@contoso.com", false),
            ("Me", "me@contoso.com", "Security training reminder", "The reminder was sent to the entire department.", "allstaff@contoso.com", false),
        };

        for (var i = 0; i < sent.Length; i++)
        {
            var item = sent[i];
            mailbox.SentItems.Add(new MailMessage
            {
                Id = $"sent-{i + 1}", Folder = "Sent Items", Sender = item.Item1, SenderEmail = item.Item2,
                Subject = item.Item3, Preview = item.Item4, Body = item.Item4 + "\n\nBest,\nFinance Operations",
                Recipients = item.Item5, Status = "Sent", IsRead = true, HasAttachments = item.Item6,
                ReceivedOn = DateTime.Today.AddDays(-i).AddHours(8 + i)
            });
        }

        var drafts = new[]
        {
            ("Draft: Team offsite agenda", "agenda@contoso.com", "Need to add the breakout sessions and room assignments."),
            ("Draft: Q4 planning review", "planning@contoso.com", "Follow up with revised goals and action items."),
            ("Draft: Vendor comparison", "procurement@contoso.com", "Include the pricing table and risk summary."),
            ("Draft: Staff announcement", "allstaff@contoso.com", "Polish the wording before HR review."),
            ("Draft: Product launch note", "product@contoso.com", "Add the release date and ownership details."),
            ("Draft: Budget approval recap", "finance@contoso.com", "Confirm the numbers before sending."),
        };

        for (var i = 0; i < drafts.Length; i++)
            mailbox.Drafts.Add(new MailMessage { Id = $"draft-{i + 1}", Folder = "Drafts", Sender = "Me", SenderEmail = "me@contoso.com", Subject = drafts[i].Item1, Preview = drafts[i].Item3, Body = drafts[i].Item3, Recipients = drafts[i].Item2, Status = i % 2 == 0 ? "Saved" : "Unsaved Changes", HasAttachments = i % 2 == 0, ReceivedOn = DateTime.Today.AddDays(-i) });

        var archive = new[]
        {
            ("Nadia Stone", "nadia.stone@contoso.com", "Archive: FY planning deck", "The deck has been moved to the archive after leadership sign-off.", true),
            ("Mason Clark", "mason.clark@contoso.com", "Archive: Procurement records", "Procurement documents were archived with the final approval set.", false),
            ("Priya Nair", "priya.nair@contoso.com", "Archive: Marketing campaign wrap-up", "Campaign results and creative assets are now archived.", true),
            ("Leo Martinez", "leo.martinez@contoso.com", "Archive: Client kickoff notes", "Kickoff notes and next steps were preserved for reference.", false),
            ("Ivy Chen", "ivy.chen@contoso.com", "Archive: Compliance review", "The compliance review package is now archived.", false),
            ("Grace Morgan", "grace.morgan@contoso.com", "Archive: HR policy update", "HR policy updates were archived after publishing.", false),
            ("Daniel Moore", "daniel.moore@contoso.com", "Archive: Office relocation plan", "The relocation plan is complete and archived.", true),
        };

        for (var i = 0; i < archive.Length; i++)
            mailbox.Archive.Add(new MailMessage { Id = $"archive-{i + 1}", Folder = "Archive", Sender = archive[i].Item1, SenderEmail = archive[i].Item2, Subject = archive[i].Item3, Preview = archive[i].Item4, Body = archive[i].Item4, Recipients = "team@contoso.com", Status = "Archived", HasAttachments = archive[i].Item5, IsFlagged = i % 2 == 0, ArchivedOn = DateTime.Today.AddDays(-(i + 14)) });

        var deleted = new[]
        {
            ("Olivia Parker", "olivia.parker@contoso.com", "Deleted: duplicate invite", "This duplicate invite was removed after the calendar merge.", true),
            ("Noah Bennett", "noah.bennett@contoso.com", "Deleted: outdated file request", "The request was superseded by the latest upload.", false),
            ("Emma Wilson", "emma.wilson@contoso.com", "Deleted: stale approvals", "This thread was cleared after approvals were finalized.", false),
            ("Lucas Reed", "lucas.reed@contoso.com", "Deleted: canceled meeting", "The canceled meeting notice has been deleted.", false),
            ("Tara Hughes", "tara.hughes@contoso.com", "Deleted: obsolete proposal", "The older proposal file was removed from the inbox thread.", true),
            ("Evan Price", "evan.price@contoso.com", "Deleted: expired agenda", "The event agenda was deleted after the meeting ended.", false),
        };

        for (var i = 0; i < deleted.Length; i++)
            mailbox.Deleted.Add(new MailMessage { Id = $"deleted-{i + 1}", Folder = "Deleted", Sender = deleted[i].Item1, SenderEmail = deleted[i].Item2, Subject = deleted[i].Item3, Preview = deleted[i].Item4, Body = deleted[i].Item4, Recipients = "archive@contoso.com", Status = "Deleted", HasAttachments = deleted[i].Item5, DeletedOn = DateTime.Today.AddDays(-(i + 2)) });

        mailbox.Contacts.Add(new MailContact { Name = "Ava Johnson", Email = "ava.johnson@contoso.com" });
        mailbox.Contacts.Add(new MailContact { Name = "Noah Smith", Email = "noah.smith@contoso.com" });
        mailbox.Contacts.Add(new MailContact { Name = "Mia Chen", Email = "mia.chen@contoso.com" });
        mailbox.Contacts.Add(new MailContact { Name = "Liam Patel", Email = "liam.patel@contoso.com" });
        mailbox.Contacts.Add(new MailContact { Name = "Sophia Garcia", Email = "sophia.garcia@contoso.com" });
        mailbox.Attachments.Add(new AttachmentInfo { FileName = "Proposal.pdf", FileType = "PDF", FileSize = 218000 });
        mailbox.RefreshCounts();
        mailbox.Folders[0].UnreadCount = mailbox.Inbox.Count(x => !x.IsRead);
        mailbox.Folders[1].UnreadCount = 0;
        mailbox.Folders[2].UnreadCount = mailbox.Drafts.Count(x => x.Status == "Unsaved Changes");
        mailbox.Folders[3].UnreadCount = mailbox.Archive.Count(x => x.IsFlagged);
        mailbox.Folders[4].UnreadCount = mailbox.Deleted.Count;
        return mailbox;
    }

}
