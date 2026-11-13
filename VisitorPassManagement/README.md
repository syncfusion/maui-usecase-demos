# Visitor Pass Management — .NET MAUI Sample

A Syncfusion-flavoured .NET MAUI sample that shows how to build a complete
visitor pass management application for a facility: a real-time dashboard
with KPIs and charts, a searchable visitor directory, a digital visitor
pass with barcode, journey timeline, and telemetry, plus a multi-step
registration surface.

All data is in-memory / mocked via `VisitorData.cs`; no backend service is
required.

## Use Case

A facility operations team needs a single app to:

1. See today's activity on a dashboard (Expected / Checked-In / Closed /
   Overdue counts; a status donut chart and a visits-by-purpose column
   chart; today's visitors list).
2. Search and filter every visitor in a directory (text search +
   status chips; an "active dossier" preview).
3. Open a **Digital Visitor Pass** for any visitor with a barcode,
   identity card, host block, escort policy, telemetry tiles and a
   journey timeline. Operators can **Check In**, **Check Out**, or
   **Cancel** a visit.
4. Register a new visitor through a three-step wizard
   (Identity → Schedule → Pass Generated).

## Screens

| Screen | Route | File |
|---|---|---|
| Dashboard / Command Center | `MainPage` (Shell) | `Views/DashboardPage.xaml` |
| Visitor Directory | (Navigation.PushAsync from Dashboard) | `Views/DirectoryPage.xaml` |
| Digital Visitor Pass | (Navigation.PushAsync from Dashboard/Directory) | `Views/VisitorDetailsPage.xaml` |
| Register Visitor | (SfPopup on Desktop/Tablet; Navigation.PushAsync on Phone) | `Views/RegisterVisitorView.xaml` / `Views/RegisterVisitorPage.xaml` |

## Syncfusion Controls Demonstrated

The sample is built around Syncfusion's MAUI suite. Each control is wired
to a real view-model so the showcase is meaningful, not decorative.

| Control | Where it appears |
|---|---|
| `SfDataGrid` | Dashboard "Today's Visitors" grid, Directory visitor grid |
| `SfCircularChart` (DoughnutSeries) | Dashboard "Visitor Status Roster" |
| `SfCartesianChart` (ColumnSeries) | Dashboard "Visits Classified By Purpose" |
| `SfBarcodeGenerator` (Code128) | Digital Visitor Pass barcode |
| `SfStepProgressBar` | Digital Pass "Visit Journey Progression" |
| `SfPopup` | Register Visitor dialog |
| `SfChipGroup` | Directory status filter chips |
| `SfBadgeView` | Status badges inside `VisitorStatusBadge` |
| `SfAvatarView` | Visitor avatar tiles |
| `SfListView` | Mobile "Today's Visitors" list, Active Dossier card, Journey checkpoints |
| `SfButton` | Register Visitor header trigger, Check In / Check Out / Cancel |
| `SfTextInputLayout` | Register form fields |
| `SfDatePicker`, `SfTimePicker` | Register scheduling step |
| `SfEntry` | Search Entry in TopNavBar / Directory |

## Syncfusion Packages

`Syncfusion.Maui.Core`, `Syncfusion.Maui.Toolkit`, `Syncfusion.Maui.Charts`,
`Syncfusion.Maui.DataGrid`, `Syncfusion.Maui.Popup`, `Syncfusion.Maui.Barcode`,
`Syncfusion.Maui.ProgressBar` — all on version `33.2.15`.

The `SyncfusionThemeResourceDictionary` (MaterialLight) is loaded in `App.xaml`
so all controls get themed brushes automatically; `VisitorPassTheme.xaml`
loads afterwards to override the Syncfusion palette keys with the visitor-pass
brand palette.

## Architecture

Strict MVVM with `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`,
`[RelayCommand]`) over a `BaseViewModel : ObservableObject`.

```
VisitorPass/
├── App.xaml(.cs)              # Syncfusion theme + palette + converters
├── AppShell.xaml(.cs)        # Shell root hosting DashboardPage
├── MauiProgram.cs             # DI registrations + Syncfusion hosting
├── Controls/                  # Reusable ContentView controls
│   ├── TopNavBar.xaml(.cs)    # Title / date / search / theme toggle / avatar
│   ├── SideNavigation.xaml(.cs)
│   ├── SfEntry.cs             # Native-chrome-free Entry subclass
│   ├── VisitorAvatar.xaml(.cs)
│   └── VisitorStatusBadge.xaml(.cs)
├── Converters/                # IValueConverter helpers
├── Models/                    # Visitor, VisitorStatus, VisitPurpose, …
├── Services/
│   ├── IVisitorStore.cs       # Single source of truth (singleton)
│   ├── VisitorStore.cs        # In-memory implementation
│   ├── VisitorData.cs         # Seeded visitors
│   ├── PassCodeGenerator.cs   # Per-day visitor pass codes
│   └── ThemeService.cs        # Light/Dark toggle (singleton)
├── ViewModels/                # Dashboard, Directory, VisitorDetails, Register
└── Views/                     # ContentPages + shared RegisterVisitorView
```

Single source of truth: every page and view-model resolves `IVisitorStore`
from DI. Mutations raise `VisitorsChanged`; every page re-derives its
filtered / aggregate collections from the same store.

