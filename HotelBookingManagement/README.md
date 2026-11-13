# Hotel Booking Management (HotelManagerSample)

A .NET MAUI front-office application for a **15-room hotel**, showing a
Dashboard / Rooms / Bookings workflow with a dedicated UI surface for
desktop (sidebar + `NavigationPage`) and mobile (`MobileShell` + `SfTabView`).

Everything runs **fully in-memory**: no network calls, no database, no
login, no cloud sync. A single `HotelStore` singleton is seeded with
deterministic demo data at startup, and every page projects from it.

---

## Sample overview

| | |
|---|---|
| **Scenario** | Hotel front-office — reservations, room inventory, housekeeping status, revenue |
| **Inventory** | 15 keys across 3 floors × 5 rooms (`HotelConfig.TotalRooms`) |
| **Seed data** | 9 hand-authored demo bookings + ~15 generated ones, seeded with `Random(20260910)` |
| **Reference date** | `2026-09-10` (`HotelConfig.ReferenceToday`) — fixed so the dashboard renders identically on every cold start |
| **Data store** | `IHotelStore` / `HotelStore` singleton, registered in `MauiProgram` |
| **UI switch** | `App.CreateWindow` picks the surface at runtime — see [Desktop vs. mobile](#desktop-vs-mobile-routing) |

### The three pages

**Dashboard** — greeting header, KPI tiles (today's arrivals, in-house stays,
folio volume), an inventory doughnut chart, a revenue cartesian chart with a
7D / 1M / 3M toggle, and a paginated table of recent reservations with a
segmented "Today's Check-Ins / Pending Payments" filter and a search box.

**Rooms** — a housekeeping board for all 15 keys. Toggle between a card view
(`SfListView` + `GridLayout`) and a table view (`SfDataGrid`). Filter by floor
chip, status chip, or free-text search; KPI strip summarises inventory.

**Bookings** — the full reservation list with KPI tiles, text search,
`SfDataGrid` + `SfDataPager` pagination, an Add-Booking popup, and a
**selection detail strip** that fills in when the user taps a grid row.

### Add-Booking flow (both surfaces)

A shared, validated reservation form:
- **Desktop** — `SfPopup` hosting `AddBookingPopupContent` (`SfTextInputLayout`,
  `SfCalendar` range selection, `SfChipGroup` floor picker, availability dropdown).
- **Mobile** — `MobileNewBookingPage`, a modal page with two `SfExpander`
  sections (Guest / Stay).

Both funnel through their own view-model, validate identically, and commit via
`IHotelStore.AddBooking`, which flips the assigned room's status and raises
`BookingsChanged` + `RoomsChanged` so every visible KPI refreshes at once.

---

## Controls demonstrated

| Control | Assembly | Where |
|---|---|---|
| `SfDataGrid`, `DataGridStyle`, `DataGridTemplateColumn` | `Syncfusion.Maui.DataGrid` | Bookings table, Rooms table view, Dashboard reservations table |
| `SfDataPager`, `DataPagerStyle` | `Syncfusion.Maui.DataGrid` | Bookings + Dashboard pagination |
| `SfListView`, `GridLayout` | `Syncfusion.Maui.ListView` | Rooms card view |
| `SfChipGroup`, `SfChip` | `Syncfusion.Maui.Toolkit` | Floor / status / booking-status filters, Add-Booking floor picker |
| `SfButton` | `Syncfusion.Maui.Buttons` | Mobile New-Booking FAB |
| `SfCartesianChart`, `SplineAreaSeries` | `Syncfusion.Maui.Charts` | Revenue trend chart |
| `SfCircularChart`, `DoughnutSeries` | `Syncfusion.Maui.Charts` | Inventory doughnut |
| `SfExpander` | `Syncfusion.Maui.Expander` | Mobile New-Booking Guest / Stay sections |
| `SfPopup` | `Syncfusion.Maui.Popup` | Desktop Add-Booking modal |
| `SfCalendar` | `Syncfusion.Maui.Calendar` | Add-Booking check-in / check-out range picker |
| `SfTextInputLayout` | `Syncfusion.Maui.Inputs` | Add-Booking field labels |
| `SfTabView` | `Syncfusion.Maui.Core` | `MobileShell` bottom-tab host |
| `SfBusyIndicator` | `Syncfusion.Maui.Core` | Mobile busy overlay |
| `SfEntry` | *(local control)* | Borderless search inputs — `Controls/SfEntry.cs` |

> `SfGauges` is referenced by the project but no gauge control is currently
> instantiated in the XAML.

---

## Architecture

MVVM with a shared in-memory store.

```
Models/       HotelStore (IHotelStore) · Room · Booking · HotelConfig · HotelData
ViewModels/   DashboardViewModel · RoomsViewModel · BookingsViewModel · AddBookingViewModel
Views/        DesktopUI/ (Dashboard, Rooms, Bookings)
              MobileUI/  (MobileShell + Dashboard, Bookings, Rooms, NewBooking)
Controls/     AppSidebar · AddBookingPopupContent · SfEntry · PhoneSanitiser
Converters/   Status→brush · avatars · chips · active-state · mobile CTA
```

**Single source of truth.** `IHotelStore` exposes two events —
`BookingsChanged` and `RoomsChanged`. Every view-model subscribes, so adding
a booking on one page updates the Dashboard KPIs, the Rooms board, and the
Bookings grid in a single cascade. No page talks to another page.

**Desktop vs. mobile routing.** `App.CreateWindow` calls `App.IsMobileRuntime()`:
phones (any platform) plus iOS/MacCatalyst get `MobileShell`; everything else
gets `NavigationPage(DashboardPage)`. Both live in one project.

**State lives on the view-models.** Interactive UI state — the active revenue
range, the active segment pill, the grid selection, the dynamic grid height —
is bound from the view-model rather than mutated in code-behind. `DashboardViewModel`
exposes `IsRevenue7DActive` / `IsRevenue1MActive` / `IsRevenue3MActive` and
`IsCheckInsActive` / `IsPendingActive`, driven by `SetRevenueRangeCommand` /
`SetActiveSegmentCommand`. `RoomsViewModel.GridHeight` and
`BookingsViewModel.BookingsGridHeight` compute the grids' `HeightRequest` from
the visible row count. Code-behind is limited to control-specific concerns:
popup open/close, pager reset, visual-tree lookups.

---

## SfDataGrid interactivity

The **Bookings** grid is a live, interactive grid:

- `SelectionMode="Single"` with a two-way `SelectedItem` binding to
  `BookingsViewModel.SelectedBooking`
- `CellTapped` handler reads `e.RowData` and updates the selection
- `SelectionChanged` handler syncs `AddedItems` / `RemovedItems` back to the VM
- `AllowSorting="True"` — tap any column header to sort
- A detail strip below the pager renders the selected booking
  (reference · guest · room · status · total) and has a dismiss button

`BookingsViewModel.SelectedBookingRow`, `HasSelectedBooking`, and
`SelectedBookingSummary` are the bound projections. The Dashboard and Rooms
grids remain passive renderers (`InputTransparent="True"`) — they sit inside
`ScrollView`s alongside other content and only need to paint.

---

## Requirements

- **.NET SDK 10.0.x** with the **MAUI workload** (`maui-android`, plus
  `maui-ios` / `maui-maccatalyst` / `maui-windows` for those targets)
- **Syncfusion MAUI licence** — required at build and run time. See
  [Syncfusion licensing](https://www.syncfusion.com/sales/license).
- **Visual Studio 2026** (or equivalent) with the MAUI workload, or the
  `maui` workload installed via `dotnet workload install maui`

### Target frameworks

| Target | Condition |
|---|---|
| `net10.0-android` | always |
| `net10.0-ios`, `net10.0-maccatalyst` | non-Linux hosts |
| `net10.0-windows10.0.19041.0` | Windows hosts |

Minimum OS versions: Android 21, iOS 15, MacCatalyst 15, Windows 10.0.17763.

### NuGet dependencies (pinned)

| Package |
|---|
| `Syncfusion.Maui.Toolkit` |
| `Syncfusion.Maui.DataGrid` |
| `Syncfusion.Maui.ListView` |
| `Syncfusion.Maui.Charts` |
| `Syncfusion.Maui.Buttons` |
| `Syncfusion.Maui.Gauges` |
| `Syncfusion.Maui.Inputs` |
| `Syncfusion.Maui.Calendar` |
| `Syncfusion.Maui.Popup` |
| `Syncfusion.Maui.Expander` |
| `Syncfusion.Maui.TabView` |

---

## Running the sample

The project lives inside `SfDataGrid/HotelBookingManagement/` alongside its
solution file `HotelManagerSample.slnx`.

### From the command line

Pick a target framework and run it. On a machine with the Android workload
installed:

```
dotnet build HotelManagerSample.slnx -f net10.0-android
dotnet build HotelManagerSample.slnx -f net10.0-windows10.0.19041.0   # Windows
dotnet build HotelManagerSample.slnx -f net10.0-ios                   # macOS
```

Then deploy to a connected emulator/simulator/device, or run the desktop head
on Windows.

### From Visual Studio

1. Open `SfDataGrid/HotelBookingManagement/HotelManagerSample.slnx`.
2. Set `HotelManagerSample` as the startup project.
3. Choose a target from the build-configuration dropdown (Android emulator,
   iOS simulator, or the Windows machine).
4. Press **F5**.

### Choosing a surface

`App.IsMobileRuntime()` decides automatically — you do not need to configure
anything:

- **Phone idiom** (any platform) → mobile `MobileShell`
- **iOS or MacCatalyst** → mobile `MobileShell`
- **Tablet / desktop / unknown** → desktop sidebar + `NavigationPage`

To exercise both surfaces on a single developer machine, run on a Windows host
to get the desktop layout and on an Android emulator (phone profile) to get the
mobile layout.

---

## Project layout reference

| Path | Contents |
|---|---|
| `MauiProgram.cs` | Host bootstrap, Syncfusion + font registration, DI registrations |
| `App.xaml.cs` | Application root, surface routing, service-provider capture |
| `Models/HotelStore.cs` | `IHotelStore` + `HotelStore` singleton |
| `Models/HotelModels.cs` | `Room`, `Booking`, `SelectableBooking`, `HotelConfig`, `HotelData` seed |
| `ViewModels/` | One view-model per page + the Add-Booking form |
| `Views/DesktopUI/` | `DashboardPage`, `RoomsPage`, `BookingsPage` |
| `Views/MobileUI/` | `MobileShell` + dashboard / rooms / bookings / new-booking pages |
| `Controls/` | `AppSidebar`, `AddBookingPopupContent`, `SfEntry`, `PhoneSanitiser` |
| `Converters/` | Value converters used by the XAML |
| `Resources/Fonts/` | `OpenSans-Regular`, `OpenSans-Semibold`, `MauiMaterialAssets` |
| `Platforms/` | Android / iOS / MacCatalyst / Windows platform heads |

### Registered font aliases

| Alias | File |
|---|---|
| `OpenSansRegular` | `OpenSans-Regular.ttf` |
| `OpenSansSemibold` | `OpenSans-Semibold.ttf` |
| `MaterialAssets` | `MauiMaterialAssets.ttf` (Material icon glyphs) |

---

## Notes

- All data is regenerated in memory on every launch; there is no persistence.
  Reloading the app restores the original seeded state.
- `HotelConfig.ReferenceToday` is pinned to `2026-09-10` so screenshots and
  walkthroughs stay reproducible. Change it in `Models/HotelModels.cs` to shift
  the whole dataset relative to a real "today".
- The Windows title bar is customised by `Platforms/Windows/TitleBarController.cs`,
  which uses reflection to subscribe to `WindowCreated` because that event's
  argument type changed between WinUI SDK versions.