using VisitorPass.Models;

namespace VisitorPass.Services;

/// <summary>
/// Static seed of visitor records for the in-memory <see cref="VisitorStore"/>.
///
/// Per HARNESS §9 contract the seed must contain between 18 and 22 visitors
/// with every status represented (at least one Expected / CheckedIn /
/// CheckedOut / Cancelled / Overdue) and every visit purpose represented
/// (at least one Business Meeting / Interview / Package Delivery /
/// Maintenance Visit). Dates cluster around today; the test clock is
/// injectable so the seed stays deterministic across runs.
///
/// The <see cref="PassCodeGenerator"/> argument is used so each seeded
/// record gets a distinct, format-conformant <c>VP-yyyyMMdd-####</c> code.
/// </summary>
public static class VisitorData
{
    /// <summary>
    /// Avatar swatches used for <c>SfAvatarView.BackgroundColor</c>. They
    /// form a small palette so the Directory grid + Digital Pass identity
    /// card show colour variety without lighting up the screen with neon.
    /// Picked from the brand-aligned teal/violet/amber family.
    /// </summary>
    private static readonly string[] AvatarSwatches =
    {
        "#5146E5", // Primary
        "#7C77FF", // PrimaryLight
        "#10B981", // CheckedIn
        "#3B82F6", // Expected
        "#F59E0B", // Overdue
        "#DC2626", // Cancelled
        "#6B7280", // CheckedOut
        "#3F38B8", // PrimaryDark
    };

    /// <summary>
    /// Build the seed set. <paramref name="now"/> is injected so tests can
    /// pin "today" and the seed visitors' timestamps stay deterministic.
    /// </summary>
    /// <param name="now">Reference "now" for the seed.</param>
    /// <param name="generator">Pass code generator that hands out distinct
    /// <c>VP-yyyyMMdd-####</c> codes.</param>
    /// <returns>A list of visitors ready to seed the <see cref="VisitorStore"/>.</returns>
    public static IReadOnlyList<Visitor> BuildVisitors(
        DateTime now,
        PassCodeGenerator generator)
    {
        var today = now.Date;
        var visitors = new List<Visitor>(capacity: 22);
        var swatchIndex = 0;

        void Add(
            string name,
            string host,
            VisitPurpose purpose,
            VisitorStatus status,
            DateTime visitDate,
            TimeSpan visitTime,
            int durationMinutes,
            DateTime? checkInTime = null,
            DateTime? checkOutTime = null,
            string? vehicleNumber = null,
            string contactNumber = "+1-555-0100",
            string? notes = null)
        {
            var v = new Visitor
            {
                VisitorName = name,
                Host = host,
                Purpose = purpose.Display(),
                ContactNumber = contactNumber,
                VehicleNumber = vehicleNumber,
                VisitDate = visitDate,
                VisitTime = visitTime,
                ExpectedDurationMinutes = durationMinutes,
                Status = status,
                CheckInTime = checkInTime,
                CheckOutTime = checkOutTime,
                AvatarColor = AvatarSwatches[swatchIndex % AvatarSwatches.Length],
                Notes = notes,
            };
            v.PassId = generator.Next(visitDate, visitors.Count);
            v.PassCode = v.PassId;
            swatchIndex++;
            visitors.Add(v);
        }

        // ----- Today: Expected (3) -----
        Add("Alice Whitfield",   Hosts.All[0], VisitPurpose.BusinessMeeting,  VisitorStatus.Expected,
            today,                     new TimeSpan(10, 30, 0), 60, contactNumber: "+1-555-0182", notes: "Board prep");
        Add("Ravi Kapoor",        Hosts.All[2], VisitPurpose.Interview,        VisitorStatus.Expected,
            today,                     new TimeSpan(11, 15, 0), 90, contactNumber: "+1-555-0231", notes: "Senior SWE final");
        Add("Maya Robinson",      Hosts.All[4], VisitPurpose.PackageDelivery,  VisitorStatus.Expected,
            today,                     new TimeSpan(13,  0, 0), 30, contactNumber: "+1-555-0410", vehicleNumber: "KA-01-HH-2310");

        // ----- Today: Checked In (4) -----
        Add("Lukas Brennan",      Hosts.All[1], VisitPurpose.MaintenanceVisit, VisitorStatus.CheckedIn,
            today,                     new TimeSpan( 9,  0, 0), 120,
            checkInTime: today.AddHours(9).AddMinutes(2),
            contactNumber: "+1-555-0911", vehicleNumber: "KA-05-JJ-7782",
            notes: "HVAC quarterly");
        Add("Chen Wei",           Hosts.All[3], VisitPurpose.BusinessMeeting,  VisitorStatus.CheckedIn,
            today,                     new TimeSpan(10,  0, 0), 60,
            checkInTime: today.AddHours(10).AddMinutes(5),
            contactNumber: "+1-555-0234", notes: "Vendor demo");
        Add("Priya Nair",         Hosts.All[0], VisitPurpose.Interview,        VisitorStatus.CheckedIn,
            today,                     new TimeSpan(11,  0, 0), 60,
            checkInTime: today.AddHours(11).AddMinutes(-3),
            contactNumber: "+1-555-0671", notes: "PM round 2");
        Add("Diego Fernández",    Hosts.All[2], VisitPurpose.PackageDelivery,  VisitorStatus.CheckedIn,
            today,                     new TimeSpan( 9, 30, 0), 30,
            checkInTime: today.AddHours(9).AddMinutes(28),
            contactNumber: "+1-555-0820", vehicleNumber: "KA-03-AB-4421");

        // ----- Today: Overdue (2) -----
        // Visitors whose expected end time has passed but who never checked in.
        Add("Sanjay Verma",       Hosts.All[4], VisitPurpose.MaintenanceVisit, VisitorStatus.Overdue,
            today.AddDays(-1),          new TimeSpan(15,  0, 0), 60,
            contactNumber: "+1-555-0304", vehicleNumber: "KA-09-CD-1130",
            notes: "Conf room AV");
        Add("Eleanor Park",       Hosts.All[1], VisitPurpose.Interview,        VisitorStatus.Overdue,
            today.AddDays(-1),          new TimeSpan(14,  0, 0), 90,
            contactNumber: "+1-555-0667", notes: "Candidate no-show");

        // ----- Today: Checked Out (4) -----
        Add("Tobias Ng",          Hosts.All[0], VisitPurpose.BusinessMeeting,  VisitorStatus.CheckedOut,
            today,                     new TimeSpan( 8,  0, 0), 60,
            checkInTime: today.AddHours(8).AddMinutes(-2),
            checkOutTime: today.AddHours(9).AddMinutes(12),
            contactNumber: "+1-555-0512", notes: "Weekly sync");
        Add("Isla O'Connor",      Hosts.All[3], VisitPurpose.Interview,        VisitorStatus.CheckedOut,
            today,                     new TimeSpan( 9,  0, 0), 60,
            checkInTime: today.AddHours(9).AddMinutes(4),
            checkOutTime: today.AddHours(10).AddMinutes(35),
            contactNumber: "+1-555-0990", notes: "Hiring loop");
        Add("Mateo Silva",        Hosts.All[2], VisitPurpose.PackageDelivery,  VisitorStatus.CheckedOut,
            today,                     new TimeSpan( 8, 30, 0), 30,
            checkInTime: today.AddHours(8).AddMinutes(28),
            checkOutTime: today.AddHours(8).AddMinutes(54),
            contactNumber: "+1-555-0444", vehicleNumber: "KA-07-EF-9052");
        Add("Yuki Tanaka",        Hosts.All[1], VisitPurpose.MaintenanceVisit, VisitorStatus.CheckedOut,
            today,                     new TimeSpan( 7,  0, 0), 120,
            checkInTime: today.AddHours(7).AddMinutes(-1),
            checkOutTime: today.AddHours(9).AddMinutes(2),
            contactNumber: "+1-555-0156", vehicleNumber: "KA-02-GH-6644",
            notes: "Plumbing follow-up");

        // ----- Today: Cancelled (2) -----
        Add("Helena Müller",      Hosts.All[4], VisitPurpose.BusinessMeeting,  VisitorStatus.Cancelled,
            today,                     new TimeSpan(13, 30, 0), 60,
            contactNumber: "+1-555-0211", notes: "Rescheduled by host");
        Add("Sven Larsen",        Hosts.All[3], VisitPurpose.Interview,        VisitorStatus.Cancelled,
            today,                     new TimeSpan(14,  0, 0), 60,
            contactNumber: "+1-555-0388", notes: "Candidate withdrew");

        // ----- Yesterday (a few for trend / chart context) -----
        var yesterday = today.AddDays(-1);
        Add("Marina Volkov",      Hosts.All[0], VisitPurpose.BusinessMeeting,  VisitorStatus.CheckedOut,
            yesterday,                  new TimeSpan(11,  0, 0), 60,
            checkInTime: yesterday.AddHours(11).AddMinutes(-2),
            checkOutTime: yesterday.AddHours(12).AddMinutes(8),
            contactNumber: "+1-555-0712");
        Add("Owen Becker",        Hosts.All[2], VisitPurpose.MaintenanceVisit, VisitorStatus.CheckedOut,
            yesterday,                  new TimeSpan( 9,  0, 0), 120,
            checkInTime: yesterday.AddHours(9).AddMinutes(1),
            checkOutTime: yesterday.AddHours(11).AddMinutes(45),
            contactNumber: "+1-555-0801", vehicleNumber: "KA-06-IJ-5589",
            notes: "Server room check");
        Add("Anika Sato",         Hosts.All[1], VisitPurpose.PackageDelivery,  VisitorStatus.Cancelled,
            yesterday,                  new TimeSpan(15, 30, 0), 30,
            contactNumber: "+1-555-0933", vehicleNumber: "KA-04-KL-2210");

        // ----- Earlier in the week (chart context) -----
        Add("Hannah Greer",       Hosts.All[3], VisitPurpose.BusinessMeeting,  VisitorStatus.CheckedOut,
            today.AddDays(-3),          new TimeSpan(10,  0, 0), 60,
            checkInTime: today.AddDays(-3).AddHours(10).AddMinutes(-3),
            checkOutTime: today.AddDays(-3).AddHours(11).AddMinutes(20),
            contactNumber: "+1-555-0612");
        Add("Jorge Esteban",      Hosts.All[4], VisitPurpose.MaintenanceVisit, VisitorStatus.CheckedOut,
            today.AddDays(-5),          new TimeSpan( 8,  0, 0), 240,
            checkInTime: today.AddDays(-5).AddHours(8).AddMinutes(-2),
            checkOutTime: today.AddDays(-5).AddHours(12).AddMinutes(10),
            contactNumber: "+1-555-0244", vehicleNumber: "KA-08-MN-3309");
        Add("Zara Hussain",       Hosts.All[0], VisitPurpose.Interview,        VisitorStatus.CheckedOut,
            today.AddDays(-4),          new TimeSpan(14,  0, 0), 90,
            checkInTime: today.AddDays(-4).AddHours(14).AddMinutes(-1),
            checkOutTime: today.AddDays(-4).AddHours(15).AddMinutes(28),
            contactNumber: "+1-555-0107");
        Add("Olusegun Adebayo",   Hosts.All[2], VisitPurpose.BusinessMeeting,  VisitorStatus.CheckedOut,
            today.AddDays(-2),          new TimeSpan(15,  0, 0), 60,
            checkInTime: today.AddDays(-2).AddHours(15).AddMinutes(-4),
            checkOutTime: today.AddDays(-2).AddHours(16).AddMinutes(2),
            contactNumber: "+1-555-0980");

        return visitors;
    }
}
