# Design: Sprint Board

## Look
Modern SaaS board: soft grey canvas, white elevated cards with 1px outline and 14px radius, indigo accent,
dark navigation rail. Dark mode uses a deep blue-black canvas with slightly lighter surfaces (no pure black / pure white).

## Tokens (`Resources/Styles/Colors.xaml`)
| Token | Light | Dark |
|-------|-------|------|
| PageBg | #F4F6FB | #0F1117 |
| Surface | #FFFFFF | #1A1D27 |
| SurfaceAlt (fields, headers) | #EEF1F8 | #232734 |
| Outline | #E3E7F0 | #2C3142 |
| TextPrimary | #111827 | #F3F4F6 |
| TextSecondary | #6B7280 | #9CA3AF |
| Accent | #4F46E5 | #818CF8 |
| Nav background | #111827 | #0A0C12 |

Status colours (both modes): Todo `#3B82F6`, InProgress `#8B5CF6`, Testing `#F59E0B`, Done `#10B981`.
Priority badges: Critical `#EF4444`, High `#F97316`, Normal `#3B82F6`, Low `#6B7280`.

Consumption: `{AppThemeBinding Light={StaticResource XLight}, Dark={StaticResource XDark}}`.

## Shared styles (`Resources/Styles/AppStyles.xaml`)
`CardBorder`, `FieldBorder`, `PageTitle`, `MutedLabel`, `PrimaryAction`, `SecondaryAction`, `NavItem`, `NavIcon`, `NavLabel`.

## Typography
OpenSans Semibold for titles/numbers, Regular for body. Page title 28 (Phone 22), KPI number 32 (Phone 26), body 14, caption 12.

## Layout (single page)
```
Grid ColumnDefinitions="Auto,*" RowDefinitions="*,Auto"
 ├─ ScrollView (content)      Phone: col0 span2 row0   | else: col1 rowspan2
 ├─ Nav Grid                  Phone: row1 span2 (bottom)| else: col0 rowspan2 (rail, 84w)
 └─ Popups (overlay, span all)
```
Nav items are positioned with `OnIdiom` on `Grid.Row/Column/ColumnSpan`: Phone = 3 columns in one row; Desktop/Tablet = 3 rows stacked.
Summary cards use a 4-column grid; on Phone the column spans (2) and row placement make it 2x2.

## Theming of Syncfusion controls
`App.ApplySyncfusionTheme()` swaps `SfVisuals.MaterialLight/Dark` and overrides Kanban/Scheduler/ComboBox keys; called at startup and on `RequestedThemeChanged`.

## Accessibility
44px minimum touch targets on buttons/fields/nav (nav ≥ 52), `SemanticProperties.Description` on icon-only actions, text/background contrast ≥ 4.5:1 for primary text in both themes.

## Known limitations
- Kanban drag is disabled in the original sample and stays so.
- Syncfusion inner colours not exposed as theme keys follow the Material theme defaults.
