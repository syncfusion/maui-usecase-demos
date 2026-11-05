# Patient Care — .NET MAUI Use Case Sample

A healthcare patient-management app built with **Syncfusion .NET MAUI controls**, demonstrating a realistic clinical workflow — dashboard metrics, patient management, appointment scheduling, and care plan tracking — using self-contained local data.

## Overview

Clinicians and care staff need a single place to monitor patient status, manage the day's schedule, and track care plan progress. This sample demonstrates how to build that experience with .NET MAUI and Syncfusion controls:

- **What the sample does** — Shows a clinical dashboard with health metrics and care alerts, a searchable patient list with patient detail views, a day-view appointment scheduler with clinician assignment, and a care plan tracker with step-by-step task progress.
- **Who it is for** — Developers building healthcare, scheduling, or line-of-business management apps with .NET MAUI.
- **What it demonstrates** — Practical use of Syncfusion MAUI charts, scheduler, list view, progress indicators, chips, segmented control, badge view, avatar view, and popup in one cohesive MVVM app.
- **What outcome the user gets** — A complete, cross-platform clinical workflow app that runs locally with no backend, network calls, or credentials required.

## Key Features

- **Dashboard** — Health metric cards, appointment counts by status (circular chart), weekly appointments trend (column chart), care alerts, today's schedule, and care plan progress.
- **Patient management** — Filterable patient list (status chips + autocomplete search), patient detail view with vitals, timeline activity, and progress indicators.
- **Appointment scheduling** — Day-view scheduler with appointment blocks, clinician assignment, status styling, and badge counts.
- **Care plan tracking** — Care plan list with step progress bars showing task completion stages.
- **Notifications** — Care alert feed with severity levels (High / Medium / Low).
- **Adaptive navigation** — Sidebar navigation on desktop and bottom navigation bar on mobile, plus a splash page at startup.

## Data Flow

1. User launches the app and passes through the splash page to the dashboard.
2. The app loads bundled clinical data from in-memory data services (patients, appointments, clinicians, care plans, alerts).
3. ViewModels shape the data into observable collections for binding.
4. The user switches between Dashboard, Patients, Schedule, and Care Plans using the sidebar/bottom navigation, and interacts (search, filter, select a day, tap an alert).
5. The services update their state and the views re-render the results.
6. All data stays local — no network calls, retries, or fallbacks are needed.

## Controls Used

- `SfCartesianChart` (Charts) — Weekly appointments column chart on the dashboard.
- `SfCircularChart` (Charts) — Appointment status breakdown doughnut chart on the dashboard.
- `SfScheduler` (Scheduler) — Day-view appointment calendar with appointment mapping and header view on the Schedule page.
- `SfListView` (ListView) — Patient list, dashboard lists, and alert feeds with grid layout support.
- `SfCircularProgressBar` / `SfLinearProgressBar` (ProgressBar) — Care plan completion and patient vitals progress indicators.
- `SfStepProgressBar` (ProgressBar) — Care plan task step progress on the Care Plans page.
- `SfSegmentedControl` (Buttons) — View switching segments on the Schedule page.
- `SfButton` (Buttons) — Actions and toolbar buttons.
- `SfBadgeView` (Core) — Notification and status badges in headers and lists.
- `SfAvatarView` (Core) — Patient and clinician photos/initials.
- `SfChipGroup` / `SfChip` (Core) — Patient status filters and selection chips.
- `SfAutocomplete` (Inputs) — Patient name search on the Patients page.
- `SfPopup` (Popup) — Patient detail and alert popups.

## Architecture

The sample follows the MVVM pattern with constructor-injected services:

- **Views** (`Views/`) — XAML pages for Dashboard, Patients, Schedule (Appointments), and Care Plans, plus a Splash page.
- **ViewModels** (`ViewModels/`) — Presentation logic and change notification (`INotifyPropertyChanged`) for each page.
- **Models** (`Models/`) — Data structures for patients, appointments/clinicians, care plans, and dashboard data.
- **Services** (`Services/`) — In-memory data services (`PatientDataService`, `ScheduleDataService`, `CarePlanDataService`, `DashboardDataService`, `NotificationService`) registered in `MauiProgram.cs` via DI.
- **Supporting UI components** (`Controls/`) — Reusable `AppSidebarView` (desktop) and `AppBottomNavView` (mobile) navigation controls; `Converters/` hold XAML value converters.

Data flows from the services → ViewModel observable collections → bound XAML views. All data is mock data generated in code.

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload (plus platform workloads for Android, iOS, Mac Catalyst, or Windows)
- Visual Studio 2022 (17.14+) or VS Code with the MAUI extension
- Syncfusion .NET MAUI packages (v34.2.6) — restored automatically from NuGet

## Configuration

No configuration is required. All clinical data is mock data created in code and held in memory. There are no API keys, environment variables, or secrets.

### Default Behavior

- **Local/offline logic** — the sample is fully self-contained and runs without any network access.

### Environment Variables / Secrets

None required. The sample does not call any external service.

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
dotnet build PatientCare/PatientCare/PatientCare.csproj -t:Run -f net10.0-android
```

Other supported platforms:

```bash
dotnet build PatientCare/PatientCare/PatientCare.csproj -t:Run -f net10.0-ios          # iOS
dotnet build PatientCare/PatientCare/PatientCare.csproj -t:Run -f net10.0-maccatalyst  # macOS
dotnet build PatientCare/PatientCare/PatientCare.csproj -t:Run -f net10.0-windows10.0.19041.0  # Windows
```

Or open `PatientCare/PatientCare.slnx` in Visual Studio / VS Code, restore packages, and deploy to your target platform.

## Output

![Patient care demo](patient-care-demo.mp4)

## Project Structure

- `Views/` — UI pages (Dashboard, Patients, Appointments, Care Plans, Splash)
- `ViewModels/` — presentation logic per page
- `Models/` — patient, schedule, care plan, and dashboard data structures
- `Services/` — in-memory data and notification services
- `Controls/` — custom navigation controls (sidebar, bottom nav)
- `Converters/` — XAML value converters
- `Resources/` — styles, fonts, images, and app assets

## Troubleshooting

**Issue 1: Blank pages or missing data**
Cause: DI services were not registered (modified `MauiProgram.cs`).
Fix: Ensure all five data services are registered in `MauiProgram.CreateMauiApp()`.

**Issue 2: Build or workload errors**
Cause: Missing .NET 10 SDK, MAUI workload, or platform workload.
Fix: Verify the installed .NET version (`dotnet --version`) and install the needed MAUI workload (`dotnet workload install maui`).

**Issue 3: Trial/evaluation banner shown**
Cause: No Syncfusion license key registered.
Fix: Register a valid license key in `MauiProgram.cs` (see the Notes section above).

## Security and Privacy Notes

- All patient data in this sample is fictional mock data — it contains no real personal health information.
- No credentials or API keys are needed, so there is nothing to secure or leak.
- If you extend the sample to use real patient data, treat it as sensitive: keep it encrypted at rest, never log it, and follow applicable healthcare privacy regulations.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)

## License

This sample is provided by Syncfusion under the repository's license. Syncfusion .NET MAUI components require a valid Syncfusion license for production use; see <https://www.syncfusion.com/products/maui> for details. Sample data is fictional and not medical advice.

Syncfusion has no liability for any damage or consequence that may arise by the use or viewing of the Patient Care example. The Patient Care example is for demonstrative purposes and if you choose to use or access the Patient Care example you agree to not hold Syncfusion liable, in any form, for any damage that is related to use, accessing or otherwise viewing the Patient Care example. By accessing, viewing, or otherwise seeing the Patient Care example you acknowledge and agree Syncfusion's examples will not allow you to seek injunctive relief in any form for any claim related to the Patient Care sample. If you do not agree to this, do not view, access, utilize or otherwise do anything with Syncfusion's examples.
