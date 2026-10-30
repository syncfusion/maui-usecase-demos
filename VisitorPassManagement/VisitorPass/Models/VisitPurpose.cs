using System.ComponentModel;
using System.Reflection;

namespace VisitorPass.Models;

/// <summary>
/// Visit purpose. Per SPEC §5 the four predefined purposes are:
/// Business Meeting, Interview, Package Delivery, Maintenance Visit.
/// The enum value is what <see cref="Visitor.Purpose"/> stores as a
/// string; <see cref="VisitPurposeExtensions.Display"/> renders it
/// for UI.
/// </summary>
public enum VisitPurpose
{
    /// <summary>Business meeting with the host.</summary>
    [Description("Business Meeting")]
    BusinessMeeting = 0,

    /// <summary>Candidate interview.</summary>
    [Description("Interview")]
    Interview = 1,

    /// <summary>Package / courier delivery.</summary>
    [Description("Package Delivery")]
    PackageDelivery = 2,

    /// <summary>Maintenance / facilities visit.</summary>
    [Description("Maintenance Visit")]
    MaintenanceVisit = 3,
}

/// <summary>
/// Helper extensions for <see cref="VisitPurpose"/>.
/// </summary>
public static class VisitPurposeExtensions
{
    /// <summary>Display name for the purpose (matches the <c>Description</c> attribute).</summary>
    public static string Display(this VisitPurpose purpose)
        => GetDescription(purpose);

    /// <summary>Short label for tight pills / chip filters.</summary>
    public static string Short(this VisitPurpose purpose) => purpose switch
    {
        VisitPurpose.BusinessMeeting => "Meeting",
        VisitPurpose.Interview => "Interview",
        VisitPurpose.PackageDelivery => "Delivery",
        VisitPurpose.MaintenanceVisit => "Maintenance",
        _ => purpose.ToString()
    };

    /// <summary>All predefined purposes in the order SPEC §5 lists them.</summary>
    public static IReadOnlyList<VisitPurpose> All { get; } = new[]
    {
        VisitPurpose.BusinessMeeting,
        VisitPurpose.Interview,
        VisitPurpose.PackageDelivery,
        VisitPurpose.MaintenanceVisit,
    };

    private static string GetDescription(VisitPurpose purpose)
    {
        var name = purpose.ToString();
        var field = typeof(VisitPurpose).GetField(name, BindingFlags.Public | BindingFlags.Static);
        if (field?.GetCustomAttribute<DescriptionAttribute>() is { } desc)
            return desc.Description;
        return name;
    }
}
