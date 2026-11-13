using Syncfusion.Maui.Buttons;

namespace DigitalQueueManagement.Controls;

public partial class PriorityFilter : ContentView
{
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex),
        typeof(int),
        typeof(PriorityFilter),
        0,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedIndexChanged);

    public PriorityFilter()
    {
        InitializeComponent();
        FilterControl.ItemsSource = new[] { "All", "Normal", "Priority" };
        FilterControl.SelectionChanged += OnSelectionChanged;
        FilterControl.SelectedIndex = SelectedIndex;
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var filter = (PriorityFilter)bindable;
        filter.FilterControl.SelectedIndex = (int)newValue;
    }

    private void OnSelectionChanged(object? sender, Syncfusion.Maui.Buttons.SelectionChangedEventArgs e)
    {
        if (e.NewIndex is int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }
}
