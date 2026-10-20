# Diagram Builder — UI Requirements

Derived from `design-images/` mockups (image_1..8 = full designer states; *_section.png =
menu flyouts; toolbar_*.png = toolbar flyouts; property_panel.png = right panel).

## U1. Overall frame
- Desktop chrome: light theme, 8 px outer padding, panels separated by 1 px `PanelBorder` lines.
- Title bar row: app name "Diagram Builder" + document name, right-aligned status text.
- Menu bar row directly below; toolbar row below that; content grid fills the rest.
- Column widths: Stencil 280 px, Property 320 px (desktop); both collapsible to 0 with
  animated width, toggled from Window menu or panel header chevrons.

## U2. Menu bar (MenuBarControl)
Horizontal `MenuCommandItem` list: FILE · EDIT · VIEW · ARRANGE · WINDOW.
Clicking opens a flyout (Syncfusion `SfPopup`-styled dropdown or MAUI MenuFlyout bound via
`MenuCommandItem.Children`). Sections mirror the mockups:
- file_section.png: New / Open / Save / Save As / — / Export / Print
- edit_section.png: Undo / Redo / — / Cut / Copy / Paste / Delete / Duplicate / — / Select All
- view_section.png: Zoom In / Zoom Out / Fit To Screen / — / Show Grid ✓ / Snap To Grid ✓
- arrange_section.png: Send To Back / Bring To Front / Send Backward / Bring Forward /
  — / Align Objects ▸ (Left Right Center Top Bottom Middle) / Distribute Objects ▸
  (Horizontally Vertically) / Match Size ▸ (Same Width Same Height Same Width & Height) /
  — / Group / Ungroup
- window_section.png: Show Toolbar ✓ / Show Stencil ✓ / Show Properties ✓

Checkable items show a check glyph; shortcuts right-aligned (e.g. "Ctrl+Z").

## U3. Toolbar (ToolbarControl)
Icon buttons 36×36, 4 px spacing, grouped by 1 px separators exactly as mockup groups 1–8.
Flyout menus for shapes/connectors (`toolbar_shape.png`, `toolbar_sline.png`) render
large 48 px shape glyphs with labels. Fill/border swatches (`toolbar_fill_color_picker.png`)
open `SfColorPicker` in a popup anchored to the button. Toggle buttons show active state with
`SelectionBlue` underline/background.

## U4. Stencil panel
- Header "Shapes" with collapse chevron + search entry (placeholder "Search shapes").
- Categories as accordion items, multiple expanded allowed.
- Shape tiles: 64×64 preview area with glyph + name beneath; hover highlight.

## U5. Property panel (matches property_panel.png)
Section headers uppercase, 11 px letter-spaced; two-column label/field rows; numeric fields
use SfNumericEntry (no spinners on desktop, right-aligned); color rows show swatch + hex text;
segmented buttons for line style and alignment; checkboxes for toggles.
Empty state: centered ghost icon + "Select an object to edit its properties".

## U6. Canvas
- White canvas over #F5F6F8 workspace background; grid dots at 20 px when enabled.
- Status bar (bottom): selection summary, zoom %, snap/grid indicators.

## U7. Accessibility & polish
- AutomationId on every interactive element (`toolbar_undo`, `stencil_search`, …).
- Tooltips on all toolbar/menu items.
- 12 px control corner radius, subtle 1 px borders, no drop shadows except popups.
- Focus visuals on all inputs; minimum hit target 36 px.

## U8. Visual states
- Selected node → `SelectionBlue` outline (control-native).
- Toolbar toggles: active tool (pan/selection/connector) highlighted.
- Menu flyout items: hover background #EFEFEF, disabled 40 % opacity.
