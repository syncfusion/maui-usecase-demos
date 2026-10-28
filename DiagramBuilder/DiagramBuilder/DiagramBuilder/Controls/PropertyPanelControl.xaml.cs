using System.ComponentModel;
using DiagramBuilder.Constants;
using DiagramBuilder.Models;
using DiagramBuilder.ViewModels;

namespace DiagramBuilder.Controls;

/// <summary>
/// Property inspector panel (ui-spec.md §6). BindingContext is a
/// <see cref="PropertyPanelViewModel"/>; section headers come from the VM's
/// <c>SectionTitles</c> (AppStrings — no literal labels in XAML). Per-row editors
/// are constructed in code-behind because the panel needs Syncfusion SfNumericEntry,
/// color pickers, segmented controls and text inputs which aren't easy to template
/// inline. The empty-state view shows when <c>IsEmptyState</c> is true; the
/// sections view when false.
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
            var header = new Label
            {
                Style = (Style)Application.Current!.Resources["PropertySectionHeaderStyle"],
                Text = vm.SectionTitles[i],
                AutomationId = "property_section_" + section.ToString().ToLowerInvariant(),
                Margin = new Thickness(12, 12, 12, 4),
            };
            SectionsStack.Add(header);

            var rows = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(12, 0, 12, 0) };
            if (vm.Sections.TryGetValue(section, out var models))
            {
                foreach (var model in models)
                {
                    rows.Add(BuildRow(model, vm, section));
                }
            }

            if (rows.Children.Count == 0)
            {
                // Some sections (e.g. INSERT) are command-only and always visible.
                rows = BuildStaticSection(section, vm);
            }

            SectionsStack.Add(rows);
        }

        _built = true;
    }

    /// <summary>Sections whose rows aren't backed by PropertyModel (mock commands).</summary>
    private VerticalStackLayout BuildStaticSection(PropertyGroup section, PropertyPanelViewModel vm)
    {
        var rows = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(12, 0, 12, 0) };
        if (section == PropertyGroup.Insert)
        {
            rows.Add(BuildActionRow(AppStrings.PropertyInsertLink, "property_insert_link"));
            rows.Add(BuildActionRow(AppStrings.PropertyInsertImage, "property_insert_image"));
        }

        return rows;
    }

    private View BuildActionRow(string label, string automationId)
    {
        return new Border
        {
            Stroke = (Color)Application.Current!.Resources["PanelBorderColor"],
            StrokeThickness = 1,
            Padding = new Thickness(8, 6),
            BackgroundColor = (Color)Application.Current!.Resources["CanvasBackground"],
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
            Content = new Label
            {
                Text = label,
                FontSize = 13,
                TextColor = (Color)Application.Current!.Resources["TextPrimary"],
                VerticalTextAlignment = TextAlignment.Center,
                AutomationId = automationId,
            },
        };
    }

    private View BuildRow(PropertyModel model, PropertyPanelViewModel vm, PropertyGroup section)
    {
        return section switch
        {
            PropertyGroup.Dimensions => BuildDimensionRow(model, vm),
            PropertyGroup.Insert => BuildActionRow(model.LabelKey, "property_" + model.Key),
            PropertyGroup.Background => BuildBackgroundRow(model, vm),
            PropertyGroup.Fill => BuildFillRow(model, vm),
            PropertyGroup.Border => BuildBorderRow(model, vm),
            PropertyGroup.Text => BuildTextRow(model, vm),
            _ => BuildFallbackRow(model, vm),
        };
    }

    private View BuildDimensionRow(PropertyModel model, PropertyPanelViewModel vm)
    {
        var grid = new Grid
        {
            Padding = new Thickness(0, 4),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(110) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            }),
            ColumnSpacing = 8,
        };
        grid.Add(new Label
        {
            Text = model.LabelKey,
            Style = (Style)Application.Current!.Resources["PropertyLabelStyle"],
            VerticalOptions = LayoutOptions.Center,
        });
        var entry = new Syncfusion.Maui.Inputs.SfNumericEntry
        {
            Minimum = model.Min ?? 0,
            Maximum = model.Max ?? 100000,
            CustomFormat = "{0:0.##}",
            AutomationId = "property_" + model.Key,
        };
        if (model.Value is IConvertible)
        {
            entry.Value = System.Convert.ToDouble(model.Value, System.Globalization.CultureInfo.InvariantCulture);
        }
        entry.ValueChanged += (_, args) =>
        {
            model.Value = args.NewValue;
            if (vm.ApplyCommand.CanExecute(model))
            {
                vm.ApplyCommand.Execute(model);
            }
        };
        Grid.SetColumn(entry, 1);
        grid.Add(entry);
        return grid;
    }

    private View BuildBackgroundRow(PropertyModel model, PropertyPanelViewModel vm)
    {
        var grid = new Grid
        {
            Padding = new Thickness(0, 4),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(110) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            }),
            ColumnSpacing = 8,
        };
        grid.Add(new Label
        {
            Text = model.LabelKey,
            Style = (Style)Application.Current!.Resources["PropertyLabelStyle"],
            VerticalOptions = LayoutOptions.Center,
        });
        var picker = new ColorPickerButton
        {
            ColorValue = model.Value as string ?? string.Empty,
            AutomationId = "property_" + model.Key,
        };
        picker.ColorChanged += (_, key) =>
        {
            // Picker has updated its own ColorValue; mirror into the model so Apply sees it.
            model.Value = key;
            if (vm.ApplyCommand.CanExecute(model))
            {
                vm.ApplyCommand.Execute(model);
            }
        };
        Grid.SetColumn(picker, 1);
        grid.Add(picker);
        return grid;
    }

    private View BuildFillRow(PropertyModel model, PropertyPanelViewModel vm)
    {
        var grid = new Grid
        {
            Padding = new Thickness(0, 4),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(110) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            }),
            ColumnSpacing = 8,
        };
        grid.Add(new Label
        {
            Text = model.LabelKey,
            Style = (Style)Application.Current!.Resources["PropertyLabelStyle"],
            VerticalOptions = LayoutOptions.Center,
        });

        View editor = model.Key switch
        {
            "Fill" or "FillColorKey" => new ColorPickerButton
            {
                ColorValue = model.Value as string ?? string.Empty,
                AutomationId = "property_" + model.Key,
            },
            "FillOpacity" => new Slider
            {
                Minimum = 0,
                Maximum = 1,
                Value = model.Value is IConvertible cv ? System.Convert.ToSingle(cv) : 1f,
                AutomationId = "property_" + model.Key,
            },
            _ => new Entry { Text = model.Value?.ToString() ?? string.Empty, Style = (Style)Application.Current!.Resources["PropertyEntryStyle"] },
        };

        if (editor is ColorPickerButton picker)
        {
            picker.ColorChanged += (_, key) =>
            {
                // Picker already updates its own ColorValue; mirror into the model so Apply sees it.
                model.Value = key;
                if (vm.ApplyCommand.CanExecute(model))
                {
                    vm.ApplyCommand.Execute(model);
                }
            };
        }
        else if (editor is Slider slider)
        {
            slider.ValueChanged += (_, args) =>
            {
                model.Value = args.NewValue;
                if (vm.ApplyCommand.CanExecute(model))
                {
                    vm.ApplyCommand.Execute(model);
                }
            };
        }

        Grid.SetColumn(editor, 1);
        grid.Add(editor);
        return grid;
    }

    private View BuildBorderRow(PropertyModel model, PropertyPanelViewModel vm)
    {
        var grid = new Grid
        {
            Padding = new Thickness(0, 4),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(110) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            }),
            ColumnSpacing = 8,
        };
        grid.Add(new Label
        {
            Text = model.LabelKey,
            Style = (Style)Application.Current!.Resources["PropertyLabelStyle"],
            VerticalOptions = LayoutOptions.Center,
        });

        View editor = model.Key switch
        {
            "BorderStyle" => BuildSegmented(
                new[] { AppStrings.PropertyBorderSolid, AppStrings.PropertyBorderDashed, AppStrings.PropertyBorderDotted },
                model.Value as string ?? AppStrings.PropertyBorderSolid,
                "property_border_style"),
            "Border" => new ColorPickerButton { ColorValue = model.Value as string ?? string.Empty, AutomationId = "property_border" },
            "BorderThickness" => new Syncfusion.Maui.Inputs.SfNumericEntry
            {
                Minimum = 0,
                Maximum = 64,
                CustomFormat = "{0:0.##}",
                AutomationId = "property_border_thickness",
            },
            "BorderOpacity" => new Slider
            {
                Minimum = 0,
                Maximum = 1,
                Value = model.Value is IConvertible cv ? System.Convert.ToSingle(cv) : 1f,
                AutomationId = "property_border_opacity",
            },
            _ => new Entry { Text = model.Value?.ToString() ?? string.Empty, Style = (Style)Application.Current!.Resources["PropertyEntryStyle"] },
        };

        if (editor is Slider s)
        {
            s.ValueChanged += (_, args) =>
            {
                model.Value = args.NewValue;
                if (vm.ApplyCommand.CanExecute(model))
                {
                    vm.ApplyCommand.Execute(model);
                }
            };
        }
        else if (editor is Syncfusion.Maui.Inputs.SfNumericEntry ne)
        {
            if (model.Value is IConvertible)
            {
                ne.Value = System.Convert.ToDouble(model.Value, System.Globalization.CultureInfo.InvariantCulture);
            }
            ne.ValueChanged += (_, args) =>
            {
                model.Value = args.NewValue;
                if (vm.ApplyCommand.CanExecute(model))
                {
                    vm.ApplyCommand.Execute(model);
                }
            };
        }

        Grid.SetColumn(editor, 1);
        grid.Add(editor);
        return grid;
    }

    private View BuildTextRow(PropertyModel model, PropertyPanelViewModel vm)
    {
        var grid = new Grid
        {
            Padding = new Thickness(0, 4),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(110) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            }),
            ColumnSpacing = 8,
        };
        grid.Add(new Label
        {
            Text = model.LabelKey,
            Style = (Style)Application.Current!.Resources["PropertyLabelStyle"],
            VerticalOptions = LayoutOptions.Center,
        });

        View editor = model.Key switch
        {
            "Text" => new Entry
            {
                Text = model.Value?.ToString() ?? string.Empty,
                Style = (Style)Application.Current!.Resources["PropertyEntryStyle"],
                AutomationId = "property_text_content",
            },
            "FontSize" => new Syncfusion.Maui.Inputs.SfNumericEntry
            {
                Minimum = 6,
                Maximum = 96,
                CustomFormat = "{0:0}",
                AutomationId = "property_font_size",
            },
            "TextColor" => new ColorPickerButton { ColorValue = model.Value as string ?? string.Empty, AutomationId = "property_text_color" },
            "TextBold" or "TextItalic" or "TextUnderline" => BuildToggle(model.Value as bool? ?? false, "property_text_" + model.Key.ToLowerInvariant()),
            "HorizontalAlignment" => BuildSegmented(
                new[] { AppStrings.PropertyAlignLeft, AppStrings.PropertyAlignCenter, AppStrings.PropertyAlignRight },
                model.Value as string ?? AppStrings.PropertyAlignLeft,
                "property_text_halign"),
            "VerticalAlignment" => BuildSegmented(
                new[] { AppStrings.PropertyAlignTop, AppStrings.PropertyAlignMiddle, AppStrings.PropertyAlignBottom },
                model.Value as string ?? AppStrings.PropertyAlignTop,
                "property_text_valign"),
            _ => new Entry { Text = model.Value?.ToString() ?? string.Empty, Style = (Style)Application.Current!.Resources["PropertyEntryStyle"] },
        };

        if (editor is Entry entry)
        {
            entry.TextChanged += (_, args) =>
            {
                model.Value = args.NewTextValue;
                if (vm.ApplyCommand.CanExecute(model))
                {
                    vm.ApplyCommand.Execute(model);
                }
            };
        }
        else if (editor is Syncfusion.Maui.Inputs.SfNumericEntry ne)
        {
            if (model.Value is IConvertible)
            {
                ne.Value = System.Convert.ToDouble(model.Value, System.Globalization.CultureInfo.InvariantCulture);
            }
            ne.ValueChanged += (_, args) =>
            {
                model.Value = args.NewValue;
                if (vm.ApplyCommand.CanExecute(model))
                {
                    vm.ApplyCommand.Execute(model);
                }
            };
        }

        Grid.SetColumn(editor, 1);
        grid.Add(editor);
        return grid;
    }

    private View BuildFallbackRow(PropertyModel model, PropertyPanelViewModel vm)
    {
        var grid = new Grid
        {
            Padding = new Thickness(0, 4),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(110) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            }),
            ColumnSpacing = 8,
        };
        grid.Add(new Label
        {
            Text = model.LabelKey,
            Style = (Style)Application.Current!.Resources["PropertyLabelStyle"],
            VerticalOptions = LayoutOptions.Center,
        });
        var entry = new Entry
        {
            Text = model.Value?.ToString() ?? string.Empty,
            Style = (Style)Application.Current!.Resources["PropertyEntryStyle"],
            AutomationId = "property_" + model.Key,
        };
        entry.TextChanged += (_, args) =>
        {
            model.Value = args.NewTextValue;
            if (vm.ApplyCommand.CanExecute(model))
            {
                vm.ApplyCommand.Execute(model);
            }
        };
        Grid.SetColumn(entry, 1);
        grid.Add(entry);
        return grid;
    }

    private static View BuildSegmented(string[] options, string selected, string automationId)
    {
        // Wrap each segment in a Border so we can apply Stroke / StrokeThickness
        // (Button doesn't expose those — only BorderColor / BorderWidth).
        var stack = new HorizontalStackLayout { Spacing = 0 };
        foreach (var opt in options)
        {
            var btn = new Button
            {
                Text = opt,
                FontSize = 11,
                Padding = new Thickness(8, 4),
                HeightRequest = 28,
                CornerRadius = 0,
                BackgroundColor = opt == selected
                    ? (Color)Application.Current!.Resources["PrimaryColor"]
                    : (Color)Application.Current!.Resources["CanvasBackground"],
                TextColor = opt == selected
                    ? Colors.White
                    : (Color)Application.Current!.Resources["TextPrimary"],
                BorderColor = (Color)Application.Current!.Resources["PanelBorderColor"],
                BorderWidth = 1,
                AutomationId = automationId + "_" + opt,
            };
            stack.Children.Add(btn);
        }
        return stack;
    }

    private static View BuildToggle(bool isChecked, string automationId)
    {
        return new Switch
        {
            IsToggled = isChecked,
            OnColor = (Color)Application.Current!.Resources["PrimaryColor"],
            AutomationId = automationId,
        };
    }
}