# Patient Companion (MediFlow) - .NET MAUI Use Case Sample

MediFlow Patient Companion is a cross-platform patient companion application built with .NET MAUI that demonstrates a realistic healthcare workflow: at-a-glance health status, medication tracking, medical records, visit history, and a guided 4-step appointment booking flow.

## Key Highlights

* A complete, realistic patient companion scenario with mock data — no backend, network, or credentials required
* Guided 4-step appointment booking (Specialty → Doctor → Date & Time → Review & Confirm) with a reschedule flow
* Multiple Syncfusion MAUI controls used in context on a single screen flow
* Clean MVVM architecture (CommunityToolkit.Mvvm) with DI-registered ViewModels
* Cross-platform: Android, iOS, Mac Catalyst, and Windows

## Features

* **Home** – Welcome header (bound to mock patient), next-appointments collection, care-plan progress bound to the patient's `HealthScore`, quick actions, and a vitals strip
* **Visits** – Upcoming/past visit lists with appointment cards, plus "Book an appointment" and reschedule support
* **Health** – Health metric cards and trend charts (Heart Rate / Blood Pressure / SpO2) with range switching (7D / 1M / 3M)
* **Medications** – Next dose card with circular progress, medication filter chips, grouped medication list
* **Records (Help)** – Medical records list and patient profile
* **Appointment booking** – 4-step wizard with progress indicator, back navigation, doctor selection, date/time picking, visit type selection, and a review screen with a location map and confirm/edit actions

## Controls Demonstrated

| Syncfusion Control | Used For |
|---|---|
| `SfCardView` (Cards) | Cards for appointments, doctors, medications, health metrics, review sections |
| `SfCartesianChart` (Charts) | Health trend charts (line charts with legends and axes) |
| `SfSegmentedControl` (Buttons) | Range switcher on the health chart card |
| `SfButton` (Buttons) | Primary actions (Continue, Confirm, Select Doctor, quick actions) |
| `SfChipGroup` (Core) | Time slot selection on the date/time step |
| `SfCarousel` (Carousel) | Vitals strip on Home |
| `SfAvatarView` (Core) | Doctor and patient avatars |
| `SfListView` (ListView) | Medical records list |
| `SfLinearProgressBar` (ProgressBar) | Care plan progress on Home (bound to `VitalSummary.HealthScore`) |
| `SfCircularProgressBar` (ProgressBar) | Next dose progress on Medications |
| `SfMaps` (Maps) | Hospital location map on the appointment review screen (world-map data loaded from a bundled Raw asset — no network needed) |

> Note: visit-type and specialty selection in the wizard are implemented with `Border` + `DataTrigger` (matching the existing component language of the sample); the time-slot picker is the only `SfChipGroup`.

## Technologies Used

* .NET MAUI (net10.0) – Cross-platform framework for Android, iOS, macOS, and Windows
* Syncfusion MAUI Controls (v34.2.6)
* CommunityToolkit.Mvvm / CommunityToolkit.Maui
* Mock data only – bundled `world-map.json` asset, no API calls, no API keys, no accounts

## Prerequisites

* .NET 10 SDK
* Visual Studio 2022 (17.14+) or VS Code with the .NET MAUI workload installed (Android SDK for Android builds)
* A Visual Studio device/emulator or a connected device for your target platform

Note: Syncfusion components may require a license key. Register it in `MauiProgram.cs` via `Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY")` if your usage requires it.

## How to Run

1. Clone the repository:

   ```bash
   git clone https://github.com/syncfusion/maui-usecase-demos
   ```

2. Open the solution:

   ```
   PatientCompanion/PatientCompanion.slnx
   ```

3. Select your target platform (Android, iOS, Mac Catalyst, or Windows) and start debugging. No configuration is required — all data is mocked locally and the map data is bundled with the app.

> Sample data only — not medical advice.

## Project Structure

```
PatientCompanion/
├── Controls/          # Reusable UI controls (cards, layout, bottom navigation)
├── Converters/        # XAML value converters
├── Models/            # Domain models (Doctor, Appointment, Medication, ...)
├── Resources/         # Colors, styles, fonts, images, bundled Raw assets (world-map.json)
├── Services/          # MockDataService, AppointmentService (local persistence)
├── ViewModels/        # CommunityToolkit.Mvvm ViewModels
└── Views/             # Shell tab pages + appointment wizard step views
```
