using System.ComponentModel;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.ViewModels;
using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Expander;
using Syncfusion.Maui.Inputs;

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
                // Idempotent: BindingContext can be re-applied (e.g. when the host
                // page re-appears) and a duplicate subscription would rebuild the
                // panel twice per refresh.
                vm.PropertyChanged -= OnVmPropertyChanged;
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
        for (var i = 0; i < vm.SectionGroups.Count; i++)
        {
            var section = vm.SectionGroups[i];

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

            var title = vm.SectionTitles[i];
            SectionsStack.Add(string.IsNullOrEmpty(title)
                ? rows
                : BuildSection(section, title, rows));
        }

        _built = true;
    }

    /// <summary>
    /// Wraps a section's rows in an <see cref="SfExpander"/> so every group can be
    /// collapsed independently — the design shows a chevron beside each header.
    ///
    /// The content is attached on <c>Loaded</c> rather than in the initializer:
    /// <c>SfExpander.OnContentChanged</c> returns early while the control is not
    /// yet loaded, so content assigned before the view joins the visual tree is
    /// silently dropped and the section renders empty.
    /// </summary>
    private static SfExpander BuildSection(PropertyGroup section, string title, View content)
    {
        var header = new Label
        {
            Style = (Style)Application.Current!.Resources["PropertySectionHeaderStyle"],
            Text = title,
            AutomationId = "property_section_" + section.ToString().ToLowerInvariant(),
        };

        var expander = new SfExpander
        {
            Header = header,
            IsExpanded = true,
            HeaderIconPosition = ExpanderIconPosition.End,
            HeaderBackground = Colors.Transparent,
            HeaderIconColor = Services.ResourceLookup.ResourceColor("TextSecondary"),
            AnimationDuration = 120,
        };

        AttachExpanderContent(expander, content);
        return expander;
    }

    /// <summary>
    /// Assigns <paramref name="content"/> to <paramref name="expander"/> once the
    /// control is loaded, so Syncfusion's <c>OnContentChanged</c> actually hosts it.
    ///
    /// <c>SfExpander.OnContentChanged</c> starts with an <c>IsViewLoaded</c> guard and
    /// returns early when the control has not joined the visual tree yet — content
    /// assigned beforehand is silently dropped and the section renders empty. The
    /// assignment is therefore deferred to <c>Loaded</c>, and repeated defensively
    /// because <c>Loaded</c> can fire again after the view is re-parented.
    /// </summary>
    private static void AttachExpanderContent(SfExpander expander, View content)
    {
        void Attach()
        {
            if (!ReferenceEquals(expander.Content, content))
            {
                expander.Content = content;
            }
        }

        if (expander.IsLoaded)
        {
            Attach();
            return;
        }

        expander.Loaded += (_, _) => Attach();
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
        else if (model.Editor == PropertyEditorKind.Color)
        {
            stack.Add(new Label
            {
                Text = model.LabelKey,
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
            ColumnSpacing = 10,
            ColumnDefinitions = new ColumnDefinitionCollection(
                Enumerable.Range(0, group.Count)
                    .Select(i => new ColumnDefinition { Width = new GridLength(ColumnWeight(group, i), GridUnitType.Star) })
                    .ToArray()),
        };

        for (var i = 0; i < group.Count; i++)
        {
            var cell = new VerticalStackLayout { Spacing = 3 };

            // The explicit caption (Border / Color / Thickness) wins. Rows whose
            // editor already carries an inline prefix (X, Y, W, H) keep that
            // prefix instead of repeating the full label as a caption.
            var hasInlinePrefix = !string.IsNullOrEmpty(group[i].PrefixKey);
            var caption = group[i].CaptionKey
                ?? (hasInlinePrefix ? null : group[i].LabelKey);
            if (!string.IsNullOrEmpty(caption))
            {
                cell.Add(new Label
                {
                    Text = caption,
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

    /// <summary>
    /// Relative width of a column in a multi-editor row. Text-bearing editors
    /// (dropdowns) need more room than numeric ones, so "Solid" is not clipped
    /// in the 320 px inspector.
    /// </summary>
    private static double ColumnWeight(IReadOnlyList<PropertyModel> group, int index)
    {
        var model = group[index];
        return model.Editor switch
        {
            PropertyEditorKind.Combo => 1.4,
            PropertyEditorKind.Color => 0.9,
            _ => 1.0,
        };
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
            PropertyEditorKind.Radio => BuildRadioEditor(model, vm),
            PropertyEditorKind.Toggle => BuildToggleEditor(model, vm),
            _ => BuildTextEditor(model, vm),
        };

        if (string.IsNullOrEmpty(editor.AutomationId))
        {
            editor.AutomationId = "property_" + model.Key.ToLowerInvariant();
        }

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
            Minimum = model.Min ?? double.MinValue,
            Maximum = model.Max ?? double.MaxValue,
            // SfNumericEntry.CustomFormat expects a .NET *custom numeric* format
            // string ("#,0.00##"), not a composite format ("{0:0.##}"). Passing
            // braces makes them render literally, e.g. "{35:0}".
            CustomFormat = model.IsInteger ? "#,0" : "#,0.##",
            IsEditable = true,
            ValueChangeMode = ValueChangeMode.OnKeyFocus,
            ShowBorder = true,
            ShowClearButton = false,
            UpDownPlacementMode = NumericEntryUpDownPlacementMode.Inline,
            UpDownButtonAlignment = UpDownButtonAlignment.Right,
            SmallChange = model.IsInteger ? 1 : 0.1,
            LargeChange = model.IsInteger ? 10 : 1,
            UpDownButtonColor = Services.ResourceLookup.ResourceColor("TextSecondary"),
            FontSize = 13,
            HeightRequest = 38,
            HorizontalTextAlignment = TextAlignment.Start,
            VerticalTextAlignment = TextAlignment.Center,
            AutomationId = "property_value_" + model.Key.ToLowerInvariant(),
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
            ColumnSpacing = 6,
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(12) },
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
            HeightRequest = 38,
            HorizontalTextAlignment = TextAlignment.Start,
            VerticalTextAlignment = TextAlignment.Center,
            // The combo renders a clear (✕) button by default, which crowds the
            // narrow panel and suggests the value can be emptied — these pickers
            // always have a valid selection, so the button is hidden.
            IsClearButtonVisible = false,
            IsEditable = true,
            IsFilteringEnabled = true,
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
            // Property pickers start at white; the shape color changes only after a selection is committed.
            ColorValue = "#FFFFFFFF",
            HeightRequest = 42,
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
            NumberFormat = model.IsInteger ? "#,0" : "#,0.##",
            // No Tooltip: the default tooltip is a large themed (purple) bubble drawn above
            // the thumb. Inside the expander row it was clipped to a solid block that hid the
            // track entirely. The header readout already shows the live value.
            ShowLabels = false,
            ShowTicks = false,
            ShowDividers = false,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center,
            // Explicit minimum so the track/thumb always get measured space.
            MinimumHeightRequest = 32,
        };

        slider.ValueChanged += (_, args) => Commit(vm, model, args.NewValue * model.Scale);

        // Header row (label left, live readout right) above the full-width track,
        // so the value no longer floats beside/above an off-centre track.
        var valueLabel = new Label
        {
            Text = FormatSliderValue(model, slider.Value),
            FontSize = 12,
            TextColor = Services.ResourceLookup.ResourceColor("TextSecondary"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.End,
        };
        slider.ValueChanged += (_, args) => valueLabel.Text = FormatSliderValue(model, args.NewValue);

        slider.Margin = new Thickness(0);
        slider.TrackStyle = new Syncfusion.Maui.Sliders.SliderTrackStyle
        {
            ActiveSize = 4,
            InactiveSize = 4,
            ActiveFill = Services.ResourceLookup.ResourceColor("PrimaryColor"),
            InactiveFill = Services.ResourceLookup.ResourceColor("PanelBorderColor"),
        };
        slider.ThumbStyle = new Syncfusion.Maui.Sliders.SliderThumbStyle
        {
            Radius = 8,
            Fill = Services.ResourceLookup.ResourceColor("PrimaryColor"),
        };

        var grid = new Grid
        {
            RowSpacing = 2,
            RowDefinitions = new RowDefinitionCollection(new[]
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(36) },
            }),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            }),
        };

        grid.Add(new Label
        {
            Text = model.LabelKey,
            Style = (Style)Application.Current!.Resources["PropertyCaptionStyle"],
            VerticalOptions = LayoutOptions.Center,
        });
        Grid.SetColumn(valueLabel, 1);
        grid.Add(valueLabel);
        Grid.SetRow(slider, 1);
        Grid.SetColumnSpan(slider, 2);
        grid.Add(slider);
        return grid;
    }

    private static string FormatSliderValue(PropertyModel model, double value)
    {
        // The slider track is 0-100 (Scale converts to the 0-1 model value), so the
        // readout shows the track value itself, e.g. "100%", not the scaled 1.
        return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0:0}%", value);
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

    /// <summary>
    /// Radio-style option row (used by the page orientation picker). Rendered
    /// with native radio buttons so the selected option is unmistakable.
    /// </summary>
    private View BuildRadioEditor(PropertyModel model, PropertyPanelViewModel vm)
    {
        var stack = new VerticalStackLayout { Spacing = 4 };
        var options = model.Options.ToList();
        var selected = model.Value as string;

        foreach (var option in options)
        {
            var radio = new RadioButton
            {
                Content = option,
                GroupName = model.Key,
                FontSize = 13,
                TextColor = Services.ResourceLookup.ResourceColor("TextPrimary"),
                VerticalOptions = LayoutOptions.Center,
                AutomationId = "property_" + model.Key.ToLowerInvariant() + "_" + option.ToLowerInvariant(),
            };

            radio.CheckedChanged += (_, args) =>
            {
                if (args.Value)
                {
                    Commit(vm, model, option);
                }
            };

            // Set the checked state after the handler is attached so the initial
            // value does not raise a spurious change.
            radio.IsChecked = string.Equals(option, selected, StringComparison.Ordinal);
            stack.Add(radio);
        }

        return stack;
    }

    private View BuildToggleEditor(PropertyModel model, PropertyPanelViewModel vm)
    {
        var isOn = model.Value is bool value && value;
        var toggle = new SfSwitch
        {
            IsOn = isOn,
            HeightRequest = 30,
            MinimumHeightRequest = 30,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
        };

        var onSettings = CreateToggleSettings(
            Services.ResourceLookup.ResourceColor("PrimaryColor"),
            Colors.White,
            Services.ResourceLookup.ResourceColor("PrimaryColor"),
            Colors.White);
        var offColor = Services.ResourceLookup.ResourceColor("TextSecondary");
        var offSettings = CreateToggleSettings(
            Services.ResourceLookup.ResourceColor("CanvasBackground"),
            offColor,
            offColor,
            offColor);

        var commonStates = new VisualStateGroup { Name = "CommonStates" };
        var onState = new VisualState { Name = "On" };
        onState.Setters.Add(new Setter
        {
            Property = SfSwitch.SwitchSettingsProperty,
            Value = onSettings,
        });
        var onHoveredState = new VisualState { Name = "OnHovered" };
        onHoveredState.Setters.Add(new Setter
        {
            Property = SfSwitch.SwitchSettingsProperty,
            Value = onSettings,
        });
        var offState = new VisualState { Name = "Off" };
        offState.Setters.Add(new Setter
        {
            Property = SfSwitch.SwitchSettingsProperty,
            Value = offSettings,
        });
        var offHoveredState = new VisualState { Name = "OffHovered" };
        offHoveredState.Setters.Add(new Setter
        {
            Property = SfSwitch.SwitchSettingsProperty,
            Value = offSettings,
        });
        commonStates.States.Add(onState);
        commonStates.States.Add(onHoveredState);
        commonStates.States.Add(offState);
        commonStates.States.Add(offHoveredState);

        var stateGroups = new VisualStateGroupList();
        stateGroups.Add(commonStates);
        VisualStateManager.SetVisualStateGroups(toggle, stateGroups);
        VisualStateManager.GoToState(toggle, isOn ? "On" : "Off");

        toggle.StateChanged += (_, args) => Commit(vm, model, args.NewValue);
        SemanticProperties.SetDescription(toggle, model.LabelKey);
        return toggle;
    }

    private static SwitchSettings CreateToggleSettings(Color trackBackground, Color trackStroke, Color thumbBackground, Color thumbStroke) =>
        new()
        {
            TrackBackground = trackBackground,
            TrackStroke = trackStroke,
            TrackStrokeThickness = 1.5,
            TrackWidthRequest = 40,
            TrackHeightRequest = 20,
            TrackCornerRadius = new CornerRadius(10),
            ThumbBackground = thumbBackground,
            ThumbStroke = thumbStroke,
            ThumbStrokeThickness = 1,
            ThumbWidthRequest = 14,
            ThumbHeightRequest = 14,
            ThumbCornerRadius = new CornerRadius(7),
        };

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
