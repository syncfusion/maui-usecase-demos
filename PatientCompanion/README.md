# Patient Companion (MediFlow)

A cross-platform .NET MAUI patient companion app that demonstrates a realistic healthcare workflow: at-a-glance health status, medication tracking, medical records, visit history, and a guided 4-step appointment booking flow.

## Overview

MediFlow Patient Companion models a real-world mobile companion app for a patient managing their day-to-day care. Instead of juggling separate channels for appointments, medication schedules, and health reports, the patient gets a single place to see the next dose, check health trends, browse medical records, and book or reschedule appointments in minutes.

- **What the sample does** – Presents a mock patient's dashboard (upcoming appointments, care-plan progress, vitals, quick actions) and walks through the complete appointment-booking journey: specialty → doctor → date & time → review & confirm, including rescheduling an existing visit.
- **Who it is for** – Developers who want to see how Syncfusion MAUI controls are combined in a full, realistic healthcare scenario, and product/UX teams looking for a reference patient-companion flow.
- **What it uses** – Syncfusion MAUI controls (cards, charts, carousel, map, progress bars, list, buttons, chips, avatar) in a clean MVVM architecture. No cloud services are involved.
- **What outcome the user gets** – A complete, self-contained demo app that runs with zero configuration: all patient, doctor, and medical data is generated locally, bookings persist on-device, and the hospital map renders from a bundled offline asset.

## Key Features

- **Home dashboard** – Welcome header bound to the mock patient, next-appointments collection, care-plan progress bound to the patient's `HealthScore`, quick actions, and a scrolling vitals strip.
- **Guided appointment booking** – 4-step wizard (Specialty → Doctor → Date & Time → Review & Confirm) with a progress indicator, back navigation, visit-type selection, a location map on the review screen, and confirm/edit actions.
- **Reschedule flow** – An existing upcoming appointment can be rebooked through the same wizard; changes are applied back to the appointment.
- **Medication tracking** – Next-dose card with circular dose progress, filter chips, and a grouped medication list.
- **Health trends** – Metric cards and line charts for Heart Rate / Blood Pressure / SpO2 with a 7D / 1M / 3M range switcher.
- **Visits & records** – Upcoming/past appointment cards, medical-records list, and patient profile.

## Application Flow

The main business workflow is the appointment booking journey:

1. The user taps **Book an appointment** on the Home or Visits screen (or selects *Reschedule* on an existing visit).
2. The app prepares context: specialties are loaded from the mock data service; for reschedule, the existing appointment is stashed as pending.
3. Step 1 – the user picks a **specialty**; step 2 – a filtered **doctor list** (with availability, rating, hospital) is shown and a doctor is selected.
4. Step 3 – the user chooses a **date** and a **time slot** (chip grid) and the **visit type** (Clinic Visit / Video Visit).
5. Step 4 – the **review screen** summarizes the selection and shows the hospital location on an offline map; the user taps **Confirm**.
6. The `AppointmentService` validates the booking, marks it *Confirmed*, persists it locally, and raises a change event — all tab pages refresh and the new visit appears on Home and in the Visits list.

### Tab-by-tab walkthrough

<!-- Add visual assets (screenshots / short GIFs) under `media/` and reference them here, e.g.:
<img src="media/home.png" alt="Home dashboard" width="300"/>
-->

| Screen | What the user sees |
|---|---|
| Home | Greeting with patient avatar, next appointment card, care-plan progress bar, quick-action cards, vitals carousel |
| Visits | Upcoming and past appointment cards with doctor, date/time, and status; book/reschedule actions |
| Health | Metric cards plus Heart Rate / Blood Pressure / SpO2 line charts with a range switcher |
| Medications | Next-dose card with circular progress, filter chips, grouped medication list |
| Records | Patient profile details and a medical-records list |
| Booking wizard | Four step views with progress indicator, ending in a review screen with an offline location map |

## Controls Used

| Syncfusion Control | Used For |
|---|---|
| `SfCardView` (Cards) | Appointments, doctors, medications, health metrics, review sections |
| `SfCartesianChart` (Charts) | Health trend line charts (Heart Rate / Blood Pressure / SpO2) with axes and legends |
| `SfSegmentedControl` (Buttons) | 7D / 1M / 3M range switcher on the health chart card |
| `SfButton` (Buttons) | Primary actions (Continue, Confirm, Select Doctor, quick actions) |
| `SfChipGroup` (Core) | Time-slot selection on the date/time step of the wizard |
| `SfCarousel` (Carousel) | Vitals strip on the Home screen |
| `SfAvatarView` (Core) | Doctor and patient avatars |
| `SfListView` (ListView) | Medical records list |
| `SfLinearProgressBar` (ProgressBar) | Care-plan progress on Home (bound to the patient's `HealthScore`) |
| `SfCircularProgressBar` (ProgressBar) | Next-dose progress on the Medications screen |
| `SfMaps` (Maps) | Hospital location map on the review screen — world-map data is loaded from a bundled Raw asset, no network needed |

> Note: visit-type and specialty selection in the wizard are implemented with `Border` + `DataTrigger` (matching the sample's component language); the time-slot picker is the only `SfChipGroup`.

## Architecture

Classic MVVM with dependency injection — data flows one way from local services to the UI and user actions flow back through commands:

- **Views** (`Views/`) – Shell tab pages (Home, Visits, Health, Medications, Records) plus the four appointment-wizard step views. Views are thin: layout and bindings only.
- **ViewModels** (`ViewModels/`) – `CommunityToolkit.Mvvm` classes (observers, commands, `INotifyPropertyChanged`) that translate service data into UI state. Registered as transient singletons-per-page in DI.
- **Services** (`Services/`) –
  - `MockDataService`: generates all demo data in-process (patient, doctors with availability, medications, health metrics/charts, medical records, quick actions).
  - `AppointmentService`: the central appointment repository — seeds one upcoming and one past visit, handles add/confirm/reschedule, persists to local storage via `JSON`, and raises `OnAppointmentsChanged` so all pages refresh.
  - `WorldMapAssetService`: loads the bundled `world-map.json` Raw asset for the offline map.
- **Models** (`Models/`) – Plain POCOs (`Patient`, `Doctor`, `AppointmentBooking`, `Medication`, `HealthChartPoint`, `MedicalRecord`, …) shared by services, ViewModels, and views.
- **Supporting UI components** (`Controls/`) – Reusable card controls (`AppointmentCardView`, `DoctorsCardView`, `MedicationCardView`, `HealthMetricCardView`, …), `AppPageLayout`, and `BottomNavigationView`.
- **Converters** (`Converters/`) – XAML value converters (formatting, status colors, visibility).

DI wiring lives in `MauiProgram.cs`: services are singletons; ViewModels and pages are transient.

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload (`dotnet workload install maui`)
- Visual Studio 2022 (17.14+) or VS Code, with the platform toolchain for your target (e.g., Android SDK for Android builds)
- A Visual Studio device/emulator or a connected device for your target platform
- Syncfusion MAUI components — no account or credentials are required; a Syncfusion license key is optional (see note below)

> Syncfusion components run in evaluation mode unless a license key is registered. If your usage requires one, register it in `MauiProgram.cs` via `Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY")`.

## Data & Configuration

### Default Behavior

The sample uses **local/offline logic only**:

- **Mock data** – all patient, doctor, medication, health, and records data is generated locally by `MockDataService`. No backend, no API calls, no accounts.
- **Local persistence** – confirmed appointments are persisted on-device by `AppointmentService` and restored on next launch.
- **Offline map** – the review-screen map renders from a bundled `world-map.json` Raw asset; no tiles are fetched.

### Environment Variables / Secrets

None. The sample requires no configuration, environment variables, or secrets.

- Do not commit a Syncfusion license key to source control — use user secrets or secure storage if you register one.
- There is no fallback behavior to configure: with zero setup the app runs the full demo out of the box.

## Run

The solution targets `net10.0-android` on all OSes, and adds `net10.0-ios` / `net10.0-maccatalyst` on non-Linux machines and `net10.0-windows10.0.19041.0` on Windows.

```bash
# Android
dotnet build PatientCompanion.slnx -f net10.0-android

# iOS / Mac Catalyst (non-Linux hosts)
dotnet build PatientCompanion.slnx -f net10.0-ios
dotnet build PatientCompanion.slnx -f net10.0-maccatalyst

# Windows
dotnet build PatientCompanion.slnx -f net10.0-windows10.0.19041.0
```

Or open `PatientCompanion/PatientCompanion.slnx` in Visual Studio, select your target platform, and start debugging. No configuration is required.

> Sample data only — not medical advice.

## Output

The final result is a five-tab patient companion app:

1. **Home** – patient greeting, next-appointment card, care-plan progress, quick actions, and a vitals carousel.
2. **Booking wizard** – four step views ending in a review screen that shows the doctor, date/time, visit type, and the hospital location on an offline map; confirming adds the visit to every list.
3. **Visits** – the confirmed (and seeded) appointments as upcoming/past cards, with reschedule support.
4. **Medications** – next-dose circular progress plus a grouped, filterable medication list.
5. **Health** – trend line charts for Heart Rate, Blood Pressure, and SpO2 with a range switcher.

<!-- Add visual assets under `media/` (e.g., media/booking-wizard.gif) and reference them here to complete the visual walkthrough. -->

## Project Structure

```
PatientCompanion/
├── Controls/     # Reusable UI controls (cards, page layout, bottom navigation)
├── Converters/   # XAML value converters
├── Models/       # Domain models (Patient, Doctor, Appointment, Medication, ...)
├── Resources/    # Colors, styles, fonts, images, bundled Raw assets (world-map.json)
├── Services/     # MockDataService, AppointmentService, WorldMapAssetService
├── ViewModels/   # CommunityToolkit.Mvvm ViewModels
└── Views/        # Shell tab pages + appointment wizard step views
```

## Troubleshooting

**Issue 1: Syncfusion evaluation/trial warnings or license errors at startup**
Cause: No license key is registered (or the key belongs to a different version/platform).
Fix: Register a valid key in `MauiProgram.cs` with `SyncfusionLicenseProvider.RegisterLicense(...)`, or run without a key in evaluation mode.

**Issue 2: Build errors such as "The referenced component 'maui' is not installed" or missing workload**
Cause: Missing .NET MAUI workload or an incompatible .NET SDK version.
Fix: Verify the .NET 10 SDK is installed and run `dotnet workload install maui`; on Windows also confirm the Windows 10 SDK is available for the Windows target.

**Issue 3: App starts but the Visits/Home lists look different than expected**
Cause: `AppointmentService` persists bookings locally and re-seeds default appointments on first launch, so previously confirmed or rescheduled visits persist across runs.
Fix: Clear the app's local storage (uninstall/reinstall the app) to reset the demo data.

**Issue 4: Target framework errors for `net10.0-ios` / `net10.0-maccatalyst` / Windows on some machines**
Cause: Those target frameworks are only added on non-Linux and Windows hosts respectively (see `PatientCompanion.csproj`).
Fix: Build on a host that supports the target, or build for Android, which is available everywhere.

## Security and Privacy Notes

- No API keys or credentials are used anywhere in the sample.
- All data is mock data generated in-memory; no real patient information is collected, transmitted, or stored.
- The only data that leaves memory is the local on-device persistence of demo bookings, kept in the app's own storage.
- If you adapt this sample with real data: do not hard-code secrets, avoid sending unnecessary sensitive health data to any external service, and prefer secure storage for any credentials you introduce.

## Supported Platforms

- Android (`net10.0-android`)
- iOS (`net10.0-ios`, non-Linux hosts)
- Mac Catalyst (`net10.0-maccatalyst`, non-Linux hosts)
- Windows (`net10.0-windows10.0.19041.0`, Windows hosts)
