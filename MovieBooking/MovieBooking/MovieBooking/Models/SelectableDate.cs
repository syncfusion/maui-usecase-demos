using CommunityToolkit.Mvvm.ComponentModel;

namespace MovieBooking.Models;

public partial class SelectableDate : ObservableObject
{
    public DateTime Date { get; }

    [ObservableProperty]
    private bool _isSelected;

    public SelectableDate(DateTime date) => Date = date;
}
