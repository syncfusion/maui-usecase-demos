# Diagram Builder — Design Guidelines

## Theme (light only)

| Key | Value | Usage |
|---|---|---|
| `PrimaryColor` | #0D6EFD | accents, active tool, primary buttons |
| `WindowBackground` | #F5F6F8 | page background |
| `CanvasBackground` | #FFFFFF | diagram surface |
| `PanelBackground` | #FAFAFA | left/right panels, toolbar, menu bar |
| `PanelBorderColor` | #DADADA | panel/toolbar separators |
| `SelectionBlue` | #2196F3 | selection highlights, checked menu glyphs |
| `TextPrimary` | #1F1F1F | headings, labels |
| `TextSecondary` | #5F6368 | hints, tooltips |
| `HoverBackground` | #EFEFEF | hover states |
| `AccentFlow` | #0D6EFD | Flow Chart category accent |
| `AccentMindMap` | #7C3AED | Mind Map category accent |
| `AccentOrgChart` | #0F9D58 | Organization Chart accent |
| `ErrorColor` | #D93025 | destructive actions |

Fonts: `Segoe UI` (Windows native; falls back to OpenSans on MacCatalyst).
Type scale: Title 20 Sb / Section 14 Sb / Body 13 / Caption 11.

All colors are declared **only** in `Resources/Styles/Colors.xaml` under `ResourceKeys`
constants; controls reference `DynamicResource` keys — no literal hex in views.

## Shape default style vocabulary
- Flow shapes: fill #E3F2FD, stroke #1565C0, white annotation text on Process;
  Decision = Diamond, Terminator = rounded rectangle, Document, Data Preparation = hexagon.
- Mind map: radial accent per branch, ellipse shapes, thick connectors.
- Org chart: manager node #0D6EFD fill, staff nodes #FFFFFF with #DADADA stroke,
  orthogonal connectors TopToBottom (`DiagramLayoutType.OrganizationalChart`).

## Component conventions
- Every reusable control exposes bindable properties + commands only; no `FontAttributes`
  or color literals in control XAML — styles from `Styles.xaml`/`DiagramTheme.xaml`.
- Buttons: `ToolbarIconButtonStyle` (36×36, transparent, hover #EFEFEF, active underline).
- Entries: `PropertyEntryStyle` (height 32, 1 px border #DADADA, 8 px radius).
- Section header: `PropertySectionHeaderStyle`.
- Popups (color picker, menus): white, 8 px radius, 1 px #DADADA border, shadow.

## Spacing & metrics
- 8 px base grid; panel padding 12; menu item height 32; toolbar height 48; menu bar 40.
- Zoom chips: pill 28 px height; active chip PrimaryColor background, white text.
