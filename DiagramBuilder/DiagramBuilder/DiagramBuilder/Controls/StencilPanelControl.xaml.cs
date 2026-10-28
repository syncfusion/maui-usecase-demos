using System.ComponentModel;
using DiagramBuilder.Models;
using DiagramBuilder.ViewModels;
using Microsoft.Maui.Layouts;
using Syncfusion.Maui.Expander;

namespace DiagramBuilder.Controls;

/// <summary>
/// Stencil panel: panel header + search entry + accordion of categories
/// (ui-spec.md §4). The BindingContext must be a <see cref="DiagramBuilder.ViewModels.StencilViewModel"/>.
/// Search text is two-way bound to <see cref="DiagramBuilder.ViewModels.StencilViewModel.FilterText"/>
/// via the SearchEntry's Text two-way binding. Tile previews are rendered from
/// code-behind so each shape (Process, Decision, Rectangle, …) and each connector
/// (Straight, Orthogonal, Bezier) shows a real glyph instead of a placeholder.
/// </summary>
public partial class StencilPanelControl : ContentView
{
    private StencilViewModel? _vm;

    /// <summary>Public accessor so the host page can wire the chevron toggle command.</summary>
    public PanelHeaderControl Header => _header;

    public StencilPanelControl()
    {
        InitializeComponent();
        SearchEntry.SetBinding(Entry.TextProperty, new Binding(nameof(StencilViewModel.FilterText), BindingMode.TwoWay));
        PropertyChanged += OnSelfPropertyChanged;
    }

    private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BindingContext))
        {
            if (_vm is not null)
            {
                _vm.PropertyChanged -= OnVmPropertyChanged;
            }

            _vm = BindingContext as StencilViewModel;
            if (_vm is not null)
            {
                _vm.PropertyChanged += OnVmPropertyChanged;
                RebuildGroups();
            }
        }
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(StencilViewModel.VisibleGroups) or nameof(StencilViewModel.HasResults))
        {
            RebuildGroups();
        }
    }

    private void RebuildGroups()
    {
        GroupsHost.Children.Clear();
        if (_vm is null)
        {
            return;
        }

        foreach (var group in _vm.VisibleGroups)
        {
            GroupsHost.Children.Add(BuildExpander(group));
        }
    }

    private SfExpander BuildExpander(StencilGroup group)
    {
        var expander = new SfExpander
        {
            IsExpanded = group.IsExpanded,
            AutomationId = "stencil_group_" + group.Id,
        };

        var headerGrid = new Grid
        {
            Padding = new Thickness(12, 10),
            ColumnDefinitions = new ColumnDefinitionCollection(new[]
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            }),
        };
        headerGrid.Add(new Label
        {
            Text = group.Title,
            Style = (Style)Resources["StencilGroupHeaderStyle"],
            VerticalOptions = LayoutOptions.Center,
        });
        headerGrid.Add(new Label
        {
            Text = group.ItemCountText,
            Style = (Style)Resources["StencilGroupHeaderStyle"],
            VerticalOptions = LayoutOptions.Center,
        });
        Grid.SetColumn((BindableObject)headerGrid.Children[1], 1);
        expander.Header = headerGrid;

        var tiles = new FlexLayout
        {
            Wrap = FlexWrap.Wrap,
            Direction = FlexDirection.Row,
            JustifyContent = FlexJustify.Start,
            Padding = new Thickness(8, 4),
        };
        foreach (var item in group.Items)
        {
            tiles.Children.Add(BuildTile(item, group.IsConnectorGroup));
        }
        expander.Content = tiles;
        return expander;
    }

    private View BuildTile(StencilItem item, bool isConnectorGroup)
    {
        var tile = new Border
        {
            Stroke = (Color)Application.Current!.Resources["PanelBorderBrush"],
            StrokeThickness = 1,
            Padding = new Thickness(6, 6),
            Margin = new Thickness(4),
            WidthRequest = 80,
            HeightRequest = 70,
            BackgroundColor = (Color)Application.Current!.Resources["CanvasBackground"],
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
            AutomationId = "stencil_tile_" + item.Id,
        };
        var stack = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        stack.Add(BuildPreview(item, isConnectorGroup));
        stack.Add(new Label
        {
            Text = item.DisplayName,
            FontSize = 11,
            HorizontalTextAlignment = TextAlignment.Center,
            TextColor = (Color)Application.Current!.Resources["TextPrimary"],
        });
        tile.Content = stack;

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            // Connectors are a click-to-set tool; shapes are drag-to-canvas (handled by SymbolPalette).
            if (isConnectorGroup && _vm is not null)
            {
                _vm.ActiveConnectorToolId = item.Id;
            }
        };
        tile.GestureRecognizers.Add(tap);
        return tile;
    }

    private View BuildPreview(StencilItem item, bool isConnectorGroup)
    {
        if (isConnectorGroup)
        {
            // ConnectorKind is nullable on the model; a connector tile always has one.
            return item.ConnectorKind is { } kind
                ? BuildConnectorPreview(kind)
                : BuildFallbackPreview();
        }

        // ShapeType is nullable on the model; a shape tile always has one.
        return item.ShapeType is { } shapeType
            ? BuildShapePreview(shapeType)
            : BuildFallbackPreview();
    }

    private static View BuildFallbackPreview() => new Microsoft.Maui.Controls.Shapes.Rectangle
    {
        WidthRequest = 36,
        HeightRequest = 20,
        Stroke = new SolidColorBrush(Colors.Gray),
        StrokeThickness = 1.5,
    };

    private View BuildShapePreview(ShapeType type)
    {
        var strokeColor = (Color)Application.Current!.Resources["TextPrimary"];
        var fillColor = (Color)Application.Current!.Resources["CanvasBackground"];
        View shape = type switch
        {
            ShapeType.Rectangle => new Microsoft.Maui.Controls.Shapes.Rectangle
            {
                WidthRequest = 36,
                HeightRequest = 20,
                Fill = new SolidColorBrush(fillColor),
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 1.5,
            },
            ShapeType.Circle => new Microsoft.Maui.Controls.Shapes.Ellipse
            {
                WidthRequest = 28,
                HeightRequest = 28,
                Fill = new SolidColorBrush(fillColor),
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 1.5,
            },
            ShapeType.Triangle => BuildPolygonShape("48,2 2,30 46,30", strokeColor, fillColor),
            ShapeType.Hexagon => BuildPolygonShape("12,2 36,2 48,16 36,30 12,30 0,16", strokeColor, fillColor),
            ShapeType.Diamond => BuildPolygonShape("24,2 46,16 24,30 2,16", strokeColor, fillColor),
            ShapeType.Pentagon => BuildPolygonShape("24,2 46,14 38,30 10,30 2,14", strokeColor, fillColor),
            ShapeType.Star => BuildPolygonShape("24,2 30,16 46,16 33,24 38,38 24,30 10,38 15,24 2,16 18,16", strokeColor, fillColor),
            ShapeType.Process => new Microsoft.Maui.Controls.Shapes.Rectangle
            {
                WidthRequest = 36,
                HeightRequest = 16,
                Fill = new SolidColorBrush(fillColor),
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 1.5,
            },
            ShapeType.Decision => BuildPolygonShape("24,2 46,16 24,30 2,16", strokeColor, fillColor),
            ShapeType.Terminator => new Microsoft.Maui.Controls.Shapes.Ellipse
            {
                WidthRequest = 36,
                HeightRequest = 16,
                Fill = new SolidColorBrush(fillColor),
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 1.5,
            },
            ShapeType.Document => new Microsoft.Maui.Controls.Shapes.Path
            {
                Data = ParsePathData("M2,2 L34,2 L46,14 L46,30 L2,30 Z M34,2 L34,14 L46,14"),
                Fill = new SolidColorBrush(fillColor),
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 1.5,
            },
            ShapeType.DataPreparation => new Microsoft.Maui.Controls.Shapes.Path
            {
                Data = ParsePathData("M2,2 L46,2 L40,30 L8,30 Z"),
                Fill = new SolidColorBrush(fillColor),
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 1.5,
            },
            _ => new Microsoft.Maui.Controls.Shapes.Rectangle
            {
                WidthRequest = 36,
                HeightRequest = 20,
                Fill = new SolidColorBrush(fillColor),
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 1.5,
            },
        };

        return new ContentView
        {
            WidthRequest = 48,
            HeightRequest = 36,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = shape,
        };
    }

    private View BuildConnectorPreview(ConnectorKind kind)
    {
        var strokeColor = (Color)Application.Current!.Resources["TextPrimary"];
        return kind switch
        {
            ConnectorKind.Straight => new Microsoft.Maui.Controls.Shapes.Line
            {
                X1 = 2, Y1 = 18, X2 = 46, Y2 = 18,
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 2,
                HeightRequest = 36,
                WidthRequest = 48,
            },
            ConnectorKind.Orthogonal => new Microsoft.Maui.Controls.Shapes.Path
            {
                Data = ParsePathData("M2,4 L24,4 L24,32 L46,32"),
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 2,
                HeightRequest = 36,
                WidthRequest = 48,
            },
            ConnectorKind.Bezier => new Microsoft.Maui.Controls.Shapes.Path
            {
                Data = ParsePathData("M2,30 C12,4 36,4 46,30"),
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 2,
                HeightRequest = 36,
                WidthRequest = 48,
            },
            _ => new Microsoft.Maui.Controls.Shapes.Line
            {
                X1 = 2, Y1 = 18, X2 = 46, Y2 = 18,
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = 2,
                HeightRequest = 36,
                WidthRequest = 48,
            },
        };
    }

    private static View BuildPolygonShape(string points, Color strokeColor, Color fillColor)
    {
        var polygon = new Microsoft.Maui.Controls.Shapes.Polygon
        {
            Fill = new SolidColorBrush(fillColor),
            Stroke = new SolidColorBrush(strokeColor),
            StrokeThickness = 1.5,
            HeightRequest = 36,
            WidthRequest = 48,
        };
        foreach (var pt in points.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var coords = pt.Split(',');
            polygon.Points.Add(new Point(double.Parse(coords[0], System.Globalization.CultureInfo.InvariantCulture),
                                          double.Parse(coords[1], System.Globalization.CultureInfo.InvariantCulture)));
        }
        return polygon;
    }

    /// <summary>
    /// Parses an SVG-style path data string (e.g. "M2,2 L34,2 Z") into a
    /// <see cref="Microsoft.Maui.Controls.Shapes.Geometry"/> suitable for
    /// <see cref="Microsoft.Maui.Controls.Shapes.Path.Data"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Microsoft.Maui.Controls.Shapes.PathGeometryConverter"/> exposes no static
    /// <c>FromString</c> helper — the string must go through
    /// <see cref="Microsoft.Maui.Controls.Shapes.PathFigureCollectionConverter.ParseStringToPathFigureCollection"/>
    /// (or an instance <c>ConvertFrom</c> call). See maui-shapes-api-reference.md §8.
    /// </remarks>
    private static Microsoft.Maui.Controls.Shapes.Geometry ParsePathData(string pathData)
    {
        var figures = new Microsoft.Maui.Controls.Shapes.PathFigureCollection();
        Microsoft.Maui.Controls.Shapes.PathFigureCollectionConverter
            .ParseStringToPathFigureCollection(figures, pathData);
        return new Microsoft.Maui.Controls.Shapes.PathGeometry { Figures = figures };
    }
}
