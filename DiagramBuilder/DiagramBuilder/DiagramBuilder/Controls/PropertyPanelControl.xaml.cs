using System.ComponentModel;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.ViewModels;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Expander;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Popup;

namespace DiagramBuilder.Controls;

/// <summary>
/// Property inspector panel (ui-spec.md §6). BindingContext is a
/// <see cref="PropertyPanelViewModel"/>; section headers come from the VM's
/// <c>SectionTitles</c> (AppStrings — no literal labels in XAML). Per-row editors
/// are constructed in code-behind because the panel needs Syncfusion SfComboBox,
/// SfNumericEntry, SfSlider, SfSegmentedControl and SfColorPicker which aren't
/// easy to template inline. The empty-state view shows when <c>IsEmptyState</c>
/// is true; the sections view when false.
///
/// Row layout follows the design reference: a caption sits above each editor
/// (Border / Color / Thickness), and every editor spans the panel width so the
/// dense control strips fit the 320 px panel.
/// </summary>
public partial class PropertyPanelControl : ContentView
{
    private bool _built;

    /// <summary>Public accessor so the host page can wire the chevron toggle command.</summary>
    public PanelHeaderControl Header => _header;

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
        if ((e.PropertyName == nameof(PropertyPanelViewModel.Sections)
             || e.PropertyName == nameof(PropertyPanelViewModel.IsEmptyState)) && _built)
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

            var rows = new VerticalStackLayout { Spacing = 6 };
            if (vm.Sections.TryGetValue(section, out var models))
            {
                foreach (var group in GroupRows(models))
                {
                    rows.Add(group.Count == 1
                        ? BuildRow(group[0], vm)
                        : BuildPairedRow(group, vm));
                }
            }

            if (rows.Children.Count == 0)
            {
                // Some sections (e.g. INSERT) are command-only and always visible.
                rows = BuildStaticSection(section, vm);
            }

            SectionsStack.Add(BuildSection(section, vm.SectionTitles[i], rows));
        }

        _built = true;
    }

    /// <summary>
    /// Wraps a section's rows in an <see cref="SfExpander"/> so every group can be
    /// collapsed independently — the design shows a chevron beside each header.
    /// </summary>
    private static SfExpander BuildSection(PropertyGroup section, string title, View content)
    {
        var header = new Label
        {
            Style = (Style)Application.Current!.Resources["PropertySectionHeaderStyle"],
            Text = title,
            AutomationId = "property_section_" + section.ToString().ToLowerInvariant(),
        };

        return new SfExpander
        {
            Header = header,
            Content = content,
            IsExpanded = true,
            HeaderIconPosition = ExpanderIconPosition.End,
            HeaderBackground = Colors.Transparent,
            HeaderIconColor = Services.ResourceLookup.ResourceColor("TextSecondary"),
            AnimationDuration = 120,
        };
    }

    /// <summary>Sections whose rows aren't backed by PropertyModel (mock commands).</summary>
    private static VerticalStackLayout BuildStaticSection(PropertyGroup section, PropertyPanelViewModel vm)
    {
        var rows = new VerticalStackLayout { Spacing = 8 };
        if (section == PropertyGroup.Insert)
        {
            rows.Add(BuildActionRow(AppStrings.PropertyInsertLink, "property_insert_link"));
            rows.Add(BuildActionRow(AppStrings.PropertyInsertImage, "property_insert_image"));
        }

        return rows;
    }

    private static View BuildActionRow(string label, string automationId)
    {
        return new Border
        {
            Stroke = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
            StrokeThickness = 1,
            Padding = new Thickness(8, 6),
            BackgroundColor = Services.ResourceLookup.ResourceColor("CanvasBackground"),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
            Content = new Label
            {
                Text = label,
                FontSize = 13,
                TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                VerticalTextAlignment = TextAlignment.Center,
                AutomationId = automationId,
            },
        };
    }

    private View BuildRow(PropertyModel model, PropertyPanelViewModel vm)
    {
        // Caption + control stack (the design places a small caption over each editor).
        var stack = new VerticalStackLayout { Spacing = 2 };
        if (!string.IsNullOrEmpty(model.CaptionKey))
        {
            stack.Add(new Label
            {
                Text = model.CaptionKey,
                Style = (Style)Application.Current!.Resources["PropertyCaptionStyle"],
                AutomationId = "property_caption_" + model.Key.ToLowerInvariant(),
            });
        }

        stack.Add(BuildEditor(model, vm));
        return stack;
    }

    /// <summary>
    /// Splits a section's models into visual rows. Models that declare a shared
    /// <see cref="PropertyModel.PairCount"/> land on the same row (X | Y, W | H);
    /// everything else occupies a row of its own.
    /// </summary>
    private static List<List<PropertyModel>> GroupRows(IReadOnlyList<PropertyModel> models)
    {
        var rows = new List<List<PropertyModel>>();
        var index = 0;
        while (index < models.Count)
        {
            var model = models[index];
            if (model.PairCount <= 1)
            {
                rows.Add([model]);
                index++;
                continue;
            }

            var group = new List<PropertyModel>();
            while (group.Count < model.PairCount && index < models.Count)
            {
                group.Add(models[index]);
                index++;
            }

            rows.Add(group);
        }

        return rows;
    }

    /// <summary>Builds a row that hosts several editors side by side.</summary>
    private View BuildPairedRow(IReadOnlyList<PropertyModel> group, PropertyPanelViewModel vm)
    {
        var grid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = new ColumnDefinitionCollection(
                Enumerable.Range(0, group.Count)
                    .Select(_ => new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) })
                    .ToArray()),
        };

        for (var i = 0; i < group.Count; i++)
        {
            var cell = new VerticalStackLayout { Spacing = 2 };
            if (!string.IsNullOrEmpty(group[i].CaptionKey))
            {
                cell.Add(new Label
                {
                    Text = group[i].CaptionKey,
                    Style = (Style)Application.Current!.Resources["PropertyCaptionStyle"],
                    AutomationId = "property_caption_" + group[i].Key.ToLowerInvariant(),
                });
            }

            cell.Add(BuildEditor(group[i], vm));
            Grid.SetColumn(cell, i);
            grid.Add(cell);
        }

        return grid;
    }

    private View BuildEditor(PropertyModel model, PropertyPanelViewModel vm)
    {
        View editor = model.Editor switch
        {
            PropertyEditorKind.Number => BuildNumberEditor(model, vm),
            PropertyEditorKind.Combo => BuildComboEditor(model, vm),
            PropertyEditorKind.Color => BuildColorEditor(model, vm),
            PropertyEditorKind.Slider => BuildSliderEditor(model, vm),
            PropertyEditorKind.Segmented => BuildSegmentedEditor(model, vm),
            PropertyEditorKind.Toggle => BuildToggleEditor(model, vm),
            _ => BuildTextEditor(model, vm),
        };

        editor.AutomationId = "property_" + model.Key.ToLowerInvariant();
        return editor;
    }

    private View BuildTextEditor(PropertyModel model, PropertyPanelViewModel vm)
    {
        var entry = new Entry
        {
            Text = model.Value?.ToString() ?? string.Empty,
            Style = (Style)Application.Current!.Resources["PropertyEntryStyle"],
        };
        entry.TextChanged += (_, args) => Commit(vm, model, args.NewTextValue);
        return entry;
    }

    private View BuildNumberEditor(PropertyModel model, PropertyPanelViewModel vm)
    {
        var entry = new SfNumericEntry
        {
            Minimum = model.Min ?? 0,
            Maximum = model.Max ?? 100000,
            CustomFormat = "{0:0.##}",
            ShowBorder = true,
            FontSize = 13,
            HorizontalTextAlignment = TextAlignment.Start,
        };

        if (model.Value is IConvertible convertible)
        {
            entry.Value = Convert.ToDouble(convertible, System.Globalization.CultureInfo.InvariantCulture);
        }

        entry.ValueChanged += (_, args) => Commit(vm, model, args.NewValue);

        // The design prefixes each dimension field with its letter (X, Y, W, H).
        if (string.IsNullOrEmpty(model.PrefixKey))
        {
            return entry;
        }

        var grid = new Grid
        {
            ColumnSpacing = 4,
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(14) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            }),
        };

        grid.Add(new Label
        {
            Text = model.PrefixKey,
            FontSize = 13,
            TextColor = Services.ResourceLookup.ResourceColor("TextSecondary"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
        });

        Grid.SetColumn(entry, 1);
        grid.Add(entry);
        return grid;
    }

    private View BuildComboEditor(PropertyModel model, PropertyPanelViewModel vm)
    {
        var combo = new SfComboBox
        {
            ItemsSource = model.Options.ToList(),
            FontSize = 13,
            ShowBorder = true,
            HorizontalTextAlignment = TextAlignment.Start,
        };

        var selected = model.Value as string;
        if (selected is not null)
        {
            var index = model.Options.ToList().IndexOf(selected);
            if (index >= 0)
            {
                combo.SelectedIndex = index;
            }
        }

        combo.ValueChanged += (_, args) =>
        {
            if (args.NewValue is string text)
            {
                Commit(vm, model, text);
            }
        };

        return combo;
    }

    private View BuildColorEditor(PropertyModel model, PropertyPanelViewModel vm)
    {
        var picker = new ColorPickerButton
        {
            ColorValue = model.Value as string ?? string.Empty,
        };
        picker.ColorChanged += (_, key) => Commit(vm, model, key);
        return picker;
    }

    private View BuildSliderEditor(PropertyModel model, PropertyPanelViewModel vm)
    {
        var slider = new Syncfusion.Maui.Sliders.SfSlider
        {
            Minimum = model.Min ?? 0,
            Maximum = model.Max ?? 100,
            Value = model.Value is IConvertible convertible
                ? Convert.ToDouble(convertible, System.Globalization.CultureInfo.InvariantCulture)
                : model.Min ?? 0,
            Tooltip = new Syncfusion.Maui.Sliders.SliderTooltip { ShowAlways = false },
        };

        slider.ValueChanged += (_, args) => Commit(vm, model, args.NewValue * model.Scale);
        return slider;
    }

    private View BuildSegmentedEditor(PropertyModel model, PropertyPanelViewModel vm)
    {
        var options = model.Options.ToList();
        var segmented = new SfSegmentedControl
        {
            ItemsSource = options.Select(option => new SfSegmentItem { Text = option }).ToList(),
            SelectionMode = SegmentSelectionMode.Single,
            StrokeThickness = 1,
        };

        var selected = model.Value as string;
        if (selected is not null)
        {
            var index = options.IndexOf(selected);
            if (index >= 0)
            {
                segmented.SelectedIndex = index;
            }
        }

        segmented.SelectionChanged += (_, args) =>
        {
            if (args.NewValue is SfSegmentItem item)
            {
                Commit(vm, model, item.Text);
            }
            else if (args.NewIndex is int index && index >= 0 && index < options.Count)
            {
                Commit(vm, model, options[index]);
            }
        };

        return segmented;
    }

    private View BuildToggleEditor(PropertyModel model, PropertyPanelViewModel vm)
    {
        var toggle = new SfSwitch
        {
            IsOn = model.Value is bool value && value,
            SwitchSettings = new SwitchSettings
            {
                TrackBackground = Services.ResourceLookup.ResourceColor("PrimaryColor"),
                ThumbBackground = Colors.White,
            },
        };
        toggle.StateChanged += (_, args) => Commit(vm, model, args.NewValue);
        return toggle;
    }

    /// <summary>Pushes an edited value into the model and applies it to the live element.</summary>
    private static void Commit(PropertyPanelViewModel vm, PropertyModel model, object? value)
    {
        model.Value = value;
        if (vm.ApplyCommand.CanExecute(model))
        {
            vm.ApplyCommand.Execute(model);
        }
    }
}
