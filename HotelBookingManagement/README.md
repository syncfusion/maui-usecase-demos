# Hotel Booking Management: .NET MAUI Use Case Sample

A cross-platform front-office application for a 15-room hotel. This .NET MAUI sample demonstrates reservation management, room and housekeeping status, operational dashboards, and adaptive mobile/desktop layouts using Syncfusion controls.

## Overview

The application has three main destinations:

- **Dashboard**: arrivals and in-house KPIs, folio totals, room inventory breakdown, revenue chart, and a searchable/paged list of recent reservations.
- **Rooms**: housekeeping board with floor and status filters, room cards, and a table presentation on desktop.
- **Bookings**: searchable reservation list, booking KPIs, pagination, Add Booking, and a selected-booking detail strip.

Phones and iOS/Android devices use the mobile `SfTabView` shell. Other device idioms use the desktop sidebar and `NavigationPage`.

## Key Features

- A 15-room inventory arranged across three floors, with room type, nightly rate, and operational status.
- Dashboard visualizations for room status and revenue with selectable 7-day, 1-month, and 3-month ranges.
- Reservation filtering, paging, row selection, and a booking-detail summary.
- Validated Add Booking flow with guest and stay information, date selection, floor selection, and available-room selection.
- Responsive booking entry: desktop `SfPopup` form and mobile modal page with expandable Guest and Stay sections.
- Shared state: a booking added in one surface updates the store and refreshes related room and dashboard summaries.

## Data Flow

1. On startup, `HotelStore` seeds 15 rooms and 24 demo bookings (9 named records plus 15 generated records).
2. The Dashboard, Rooms, and Bookings view models project data from the same DI-registered `IHotelStore` singleton.
3. The user searches or filters reservations/rooms, inspects dashboard metrics, or opens Add Booking.
4. The form validates the stay and room selection. A successful booking is appended to the shared store and the assigned room becomes Reserved.
5. Store events notify the view models, which refresh lists, chart summaries, pagers, and KPI values.

The seed is deterministic and anchored to **September 10, 2026** (`HotelConfig.ReferenceToday`) so date-dependent content remains stable. Data is in memory only: restarting restores the seed. The sample has no database, login, network service, or payment processing.

## Controls Used

- `SfDataGrid` / `SfDataPager`: desktop dashboard and booking tables, plus the Rooms table view.
- `SfListView`: room cards and mobile lists.
- `SfCartesianChart` / `SfCircularChart`: revenue and room-status summaries.
- `SfChipGroup`: room, floor, and reservation filters.
- `SfPopup`, `SfCalendar`, `SfComboBox`, and `SfTextInputLayout`: desktop Add Booking form.
- `SfTabView` and `SfExpander`: mobile navigation and booking form sections.
- `SfButton` and `SfBusyIndicator`: mobile actions and busy feedback.

## Architecture

The sample uses page-specific view models backed by a shared in-memory store:

- **Models** (`HotelManagerSample/Models/`): room and booking types, hotel configuration, seed data, and `IHotelStore`/`HotelStore`.
- **View models** (`HotelManagerSample/ViewModels/`): Dashboard, Rooms, Bookings, and desktop Add Booking logic.
- **Desktop views** (`HotelManagerSample/Views/DesktopUI/`): Dashboard, Rooms, and Bookings.
- **Mobile views** (`HotelManagerSample/Views/MobileUI/`): three-tab shell, dashboard, rooms, bookings, and new-booking form.
- **Controls** (`HotelManagerSample/Controls/`): sidebar, booking popup content, and supporting entry control.
- **Converters and resources**: presentation converters, platform themes, fonts, and images.

`MauiProgram.cs` registers the store as a singleton and registers desktop/mobile view models and pages. The app chooses its root surface at runtime using platform and device idiom.

## Prerequisites

- .NET 10 SDK and the .NET MAUI workload.
- A platform workload/device for Android, iOS, Mac Catalyst, or Windows.
- Visual Studio with MAUI support or VS Code with the .NET MAUI extension.
- Syncfusion MAUI packages restored from NuGet. A valid Syncfusion license may be needed to remove evaluation watermarks.

## Configuration

No runtime configuration is required. Seed data, hotel settings, and the reference date are defined in the project.

### Default Behavior

- All room and reservation data is generated locally at startup.
- The sample uses one shared in-memory store for both mobile and desktop surfaces.
- The reference date stays fixed at September 10, 2026; it is not the device's current date.

### Environment Variables / Secrets

None required. The sample does not connect to a hotel system, payment provider, or remote API.

### Notes

- Change `HotelConfig.ReferenceToday` in `HotelManagerSample/Models/HotelModels.cs` to move the sample's date-dependent data.
- Changes to rooms and bookings are not persisted; a restart restores the seed.
- Booking totals and payment/status labels are for demonstration only. No card data is collected or payment is charged.
- Register a Syncfusion license through the documented licensing setup if required; do not commit the key. See [Syncfusion MAUI licensing](https://help.syncfusion.com/common/licensing/how-to-register-maui-license-key).

## Run

From the repository root, build for a platform workload installed on your machine:

```bash
dotnet build HotelBookingManagement/HotelManagerSample/HotelManagerSample.csproj -f net10.0-android
```

Other supported target frameworks are `net10.0-ios`, `net10.0-maccatalyst`, and `net10.0-windows10.0.19041.0`. You can also open [`HotelManagerSample.slnx`](HotelManagerSample.slnx) in Visual Studio or VS Code and deploy to a device or emulator.

## Output

This sample folder does not include a separate demo GIF. Run the app to view the dashboard, rooms, and booking workflows on the selected platform.

## Project Structure

- `HotelManagerSample.slnx`: solution file.
- `HotelManagerSample/`: MAUI app project.
- `HotelManagerSample/Models/`: hotel configuration, seeded data, store, rooms, and bookings.
- `HotelManagerSample/ViewModels/`: shared and surface-specific page view models.
- `HotelManagerSample/Views/DesktopUI/`: desktop Dashboard, Rooms, and Bookings pages.
- `HotelManagerSample/Views/MobileUI/`: mobile shell and tab pages, including new booking.
- `HotelManagerSample/Controls/`, `Converters/`, and `Resources/`: reusable UI, binding converters, themes, and assets.

## Troubleshooting

**The app opens with the wrong layout**

The app selects mobile for Android/iOS and phone idioms; other idioms use the desktop surface. Check the selected target and device idiom.

**Dashboard dates do not match today's date**

The demo is intentionally anchored to September 10, 2026. Update `HotelConfig.ReferenceToday` if a different sample date is needed.

**A booking or room change disappears after restart**

The store is in memory only. The seeded room and booking data is restored on launch.

**Changes do not update across pages**

The pages must resolve the shared `IHotelStore` registered in `MauiProgram.cs`; avoid creating separate store instances.

**Syncfusion evaluation watermark appears**

Register a valid Syncfusion license; see Configuration → Notes.

## Security and Privacy Notes

- Guest names, reservation details, and payment labels are fictional demo data.
- The app has no authentication, backend, data persistence, or payment integration.
- Do not enter real guest or payment information.
- Keep any Syncfusion license key out of source control.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)
