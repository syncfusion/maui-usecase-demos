using System.ComponentModel;
using DiagramBuilder.Models;
using DiagramBuilder.ViewModels;

namespace DiagramBuilder.Controls;

/// <summary>
/// Property inspector panel (ui-spec.md §6). BindingContext is a
/// <see cref="PropertyPanelViewModel"/>; section headers come from the VM's
/// <c>SectionTitles</c> (AppStrings — no literal labels in XAML). Per-row editors
/// come from <c>PropertyEditorTemplates.xaml</c>. The empty-state view shows when
/// <c>IsEmptyState</c> is true; the sections view when false.
/// </summary>
public partial class PropertyPanelControl : ContentView
{
    private bool _built;

    public PropertyPanelControl()
    {
        InitializeComponent();
        PropertyChanged += OnSelfPropertyChanged;
    }

    private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BindingContext))
        {
            if (BindingContext is PropertyPanelViewModel vm)
            {
                vm.PropertyChanged += OnVmPropertyChanged;
                BuildSections();
            }
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PropertyPanelViewModel.Sections) && _built)
        {
            BuildSections();
        }
    }

    private void BuildSections()
    {
        if (BindingContext is not PropertyPanelViewModel vm)
        {
            return;
        }

        SectionsStack.Clear();
        for (var i = 0; i < vm.SectionTitles.Count; i++)
        {
            var section = (PropertyGroup)i;
            var header = new Label
            {
                Style = (Style)Resources["PropertySectionHeaderStyle"],
                Text = vm.SectionTitles[i],
                AutomationId = "property_section_" + section.ToString().ToLowerInvariant(),
            };
            SectionsStack.Add(header);

            // Editor rows for this section are supplied by the VM's Sections
            // dictionary and rendered with PropertyEditorTemplates' selectors.
            var rows = new VerticalStackLayout { Spacing = 2 };
            if (vm.Sections.TryGetValue(section, out var models))
            {
                foreach (var model in models)
                {
                    rows.Add(BuildRow(model, vm));
                }
            }

            SectionsStack.Add(rows);
        }

        _built = true;
    }

    private View BuildRow(PropertyModel model, PropertyPanelViewModel vm)
    {
        // Row: label + numeric/color/text editor bound to the model's Value.
        var grid = new Grid { Padding = new Thickness(0, 4), ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)] };
        var label = new Label
        {
            Text = model.LabelKey,
            Style = (Style)Resources["PropertyRowLabelStyle"],
            VerticalOptions = LayoutOptions.Center,
        };
        grid.Add(label);

        switch (model.Group)
        {
            case PropertyGroup.Dimensions:
                var entry = new Syncfusion.Maui.Inputs.SfNumericEntry
                {
                    Value = System.Convert.ChangeType(model.Value ?? 0d, typeof(double)) as double?,
                    Minimum = model.Min ?? 0,
                    Maximum = model.Max ?? 100000,
                };
                entry.ValueChanged += (_, args) =>
                {
                    model.Value = args.NewValue;
                    if (vm.ApplyCommand.CanExecute(model))
                    {
                        vm.ApplyCommand.Execute(model);
                    }
                };
                grid.Add(entry);
                break;
            default:
                var valueLabel = new Label { Text = model.Value?.ToString() ?? string.Empty, VerticalOptions = LayoutOptions.Center };
                grid.Add(valueLabel);
                break;
        }

        return grid;
    }
}