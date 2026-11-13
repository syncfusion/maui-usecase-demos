using Syncfusion.Maui.DataSource.Extensions;

namespace FluentMail.Converters;

public class NewestFirstGroupComparer : IComparer<GroupResult>
{
    public int Compare(GroupResult? x, GroupResult? y) => ((DateTime)(y?.Key ?? DateTime.MinValue)).CompareTo((DateTime)(x?.Key ?? DateTime.MinValue));
}
