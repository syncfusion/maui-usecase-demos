# Patient Companion: .NET MAUI Use Case Sample

A healthcare companion app built with **Syncfusion .NET MAUI controls**, demonstrating a patient dashboard, appointment management, medication tracking, health trends, medical records, and a guided appointment booking workflow using local sample data.

## Overview

MediFlow Patient Companion presents a mock patient-care experience in one cross-platform app:

- **What the sample does**: Shows the patient's next appointments, care-plan progress, recent vitals, and quick actions; lists upcoming and past visits; displays blood pressure, heart-rate, and SpO2 trends; organizes medications; lists medical records; and provides a four-step appointment booking flow.
- **Who it is for**: Developers building healthcare dashboards, patient portals, scheduling workflows, or MVVM applications with .NET MAUI.
- **What it demonstrates**: Shell navigation, custom bottom navigation, dependency injection, `CommunityToolkit.Mvvm`, and Syncfusion cards, charts, carousel, buttons, chips, avatars, progress bars, list view, and maps.
- **What outcome the user gets**: A self-contained demo that runs with bundled data and stores demo appointments on the device.

## Key Features

- **Home dashboard**: Greeting and patient ID, next-appointment cards, care-plan progress, today's medication summary, a recent-vitals carousel, and quick-action cards.
- **Visit management**: Upcoming and past visit lists with doctor, specialization, date, time, status, and visit type. Upcoming confirmed visits support rescheduling and cancellation.
- **Appointment booking**: A four-step flow for selecting a specialty, selecting a doctor, choosing a clinic/video visit and date/time, and reviewing and confirming the appointment.
- **Rescheduling**: Reopens an existing appointment at the date-and-time step with the selected doctor, visit type, date, and time preloaded.
- **Medication tracking**: Next-dose summary, a fixed 92% weekly-adherence indicator, Today/Upcoming/All filters, period grouping, and mark-taken/skip actions.
- **Health trends**: Blood pressure, heart-rate, and SpO2 metric cards with 7D, 30D, and 90D chart ranges. Chart data is generated locally.
- **Medical records**: Mock laboratory, prescription, and immunization records presented in a list.
- **Offline location preview**: The review step shows the selected hospital location on a map built from the bundled `world-map.json` asset.

## Data Flow

1. The app starts from `AppShell`, which defines the Home, Visits, Health, Medications, and Records tabs.
2. `MauiProgram.CreateMauiApp()` registers the mock data, appointment, and map-asset services, plus the main pages and ViewModels.
3. `MockDataService` supplies mock patient and doctor data. Medication and health ViewModels create their demo data locally.
4. `HomeViewModel` and `VisitsViewModel` obtain appointments from `AppointmentService` and reload when `OnAppointmentsChanged` is raised.
5. The booking page keeps the current selection in `AppointmentViewModel` and `AppointmentBooking`; each step updates the shared state.
6. Confirming the booking calls `AppointmentService.AddAppointment()`, persisting the appointment and navigating to the Visits tab.
7. `AppointmentService` loads and saves demo appointments through MAUI `Preferences`. The map review view loads the bundled world-map JSON through `WorldMapAssetService`.

## Controls Used

- `SfCardView` (Cards): Appointment, doctor, medication, health, vital, quick-action, visit, and booking-review cards.
- `SfCartesianChart` (Charts): Blood pressure, heart-rate, and SpO2 line charts.
- `SfSegmentedControl` (Buttons): 7D, 30D, and 90D health chart ranges.
- `SfButton` (Buttons): Booking, doctor selection, medication, confirmation, and cancel actions.
- `SfChipGroup` (Core): Medication filters and appointment time-slot selection.
- `SfAvatarView` (Core): Patient and doctor avatars.
- `SfCarousel` (Carousel): Recent-vitals strip on the Home page.
- `SfLinearProgressBar` / `SfCircularProgressBar` (ProgressBar): Care-plan progress and weekly medication adherence.
- `SfListView` (ListView): Medical-record list.
- `SfMaps` (Maps): Offline world map and selected-doctor location marker.
- .NET MAUI `CollectionView`, `ScrollView`, `Picker`, `Border`, `Label`, and `TapGestureRecognizer`: Lists, scrolling, month selection, cards, text, and interactions.

## Architecture

The sample follows MVVM with a Shell-based tab structure:

- **Views** (`Views/`): Home, Visits, Health, Medications, and Records pages, plus the appointment booking page and its specialty, doctor, date/time, and review views.
- **ViewModels** (`ViewModels/`): Presentation state and commands for the dashboard, visits, health, medications, records, and booking workflow.
- **Models** (`Models/`): Patient, doctor, appointment, booking, specialty, health, medication, vital, record, and quick-action data structures.
- **Services** (`Services/`): Mock patient/doctor data, appointment state with local persistence, and bundled map-asset loading.
- **Controls** (`Controls/`): Reusable appointment, doctor, medication, health, vital, quick-action, visit, page-layout, and bottom-navigation controls.
- **Converters** (`Converters/`): XAML value conversion helpers.
- **Application shell** (`AppShell.xaml`, `AppShell.xaml.cs`): Tab routes and the hidden Shell tab bar used with the custom bottom navigation.
- **Platform and resources** (`Platforms/`, `Resources/`): Platform entry points, styles, images, fonts, app icon, splash asset, and the raw world-map JSON.

`MauiProgram.cs` registers `MockDataService`, `AppointmentService`, and `WorldMapAssetService` as singleton services. Main pages and selected ViewModels are registered transiently.

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload, plus the platform workload for Android, iOS, Mac Catalyst, or Windows
- Visual Studio 2022 (17.14+) or VS Code with the MAUI extension
- A configured emulator/simulator or connected target device
- Syncfusion .NET MAUI packages: restored automatically from NuGet

## Configuration

No configuration is required. The app uses mock data and bundled assets.

### Default Behavior

- **Local/offline logic**: The sample uses local mock data and the bundled `world-map.json`; it does not call a healthcare or map API.
- **On-device appointments**: Confirmed and rescheduled appointments are stored through MAUI `Preferences` under `mediflow.appointments`.
- **Medication and health data**: Medication status and health chart values are demo values generated in the app.

### Environment Variables / Secrets

None required. The sample does not use API keys, accounts, environment variables, or external credentials.

### Notes

- Syncfusion controls run in evaluation mode unless a valid license key is registered. To register one:
  1. Obtain a license key from <https://www.syncfusion.com/account/license>.
  2. Add the registration call at the top of `MauiProgram.cs` in `CreateMauiApp()`:

     ```csharp
     Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
     ```

  See [Syncfusion licensing](https://help.syncfusion.com/common/licensing/how-to-register-maui-license-key) for details.

## Run

```bash
dotnet build PatientCompanion/PatientCompanion/PatientCompanion.csproj -t:Run -f net10.0-android
```

Other supported platforms:

```bash
dotnet build PatientCompanion/PatientCompanion/PatientCompanion.csproj -t:Run -f net10.0-ios
dotnet build PatientCompanion/PatientCompanion/PatientCompanion.csproj -t:Run -f net10.0-maccatalyst
dotnet build PatientCompanion/PatientCompanion/PatientCompanion.csproj -t:Run -f net10.0-windows10.0.19041.0
```

Or open `PatientCompanion/PatientCompanion.slnx` in Visual Studio or VS Code, restore packages, and deploy to your target platform.

The iOS and Mac Catalyst targets are included on non-Linux hosts, and the Windows target is included on Windows hosts.

## Output

![Patient Companion demo](PatientCompanion.gif)

## Project Structure

- `Views/`: Main tab pages and the appointment booking wizard views
- `ViewModels/`: Presentation state and commands for the app screens
- `Models/`: Patient, doctor, appointment, medication, health, record, and UI data models
- `Services/`: Mock data, appointment storage, and bundled map-asset services
- `Controls/`: Reusable card and navigation controls
- `Converters/`: XAML value converters
- `Resources/`: Images, styles, fonts, app icon, splash asset, and raw map data
- `Platforms/`: Android, iOS, Mac Catalyst, and Windows entry points

## Troubleshooting

**Issue 1: No doctors appear after selecting a specialty**
Cause: Doctor choices are filtered by exact specialization name.
Fix: Select a mock specialty with doctors: Cardiology, Dermatology, Pediatrics, Neurology, or Ophthalmology.

**Issue 2: The map location preview is empty**
Cause: `world-map.json` could not be loaded from the app package.
Fix: Confirm `Resources/Raw/world-map.json` is included as a MAUI asset and wait for the review view's asynchronous load.

**Issue 3: A stored booking does not appear**
Cause: Stored records are rebuilt only for doctor IDs that exist in `MockDataService`.
Fix: Clear the app data or uninstall and reinstall to reset the demo appointments.

**Issue 4: Build or workload errors**
Cause: The .NET 10 SDK or required MAUI/platform workload is missing.
Fix: Verify `dotnet --version` and install the needed MAUI workloads for the selected target.

**Issue 5: Trial/evaluation banner shown**
Cause: No Syncfusion license key is registered.
Fix: Register a valid license key in `MauiProgram.cs` as shown in the Notes section.

## Security and Privacy Notes

- All patient, appointment, doctor, medication, and medical-record data is fictional sample data.
- No API keys or credentials are needed, and no external healthcare or map service is called.
- Demo appointments are stored locally through MAUI `Preferences`.
- This app is for demonstration and is not medical advice. Real patient data would require secure storage and appropriate privacy controls.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)
