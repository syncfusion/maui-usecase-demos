# Patient Companion (MediFlow): .NET MAUI Use Case Sample

A cross-platform patient-companion sample built with .NET MAUI and Syncfusion controls. MediFlow demonstrates a patient dashboard, health and medication summaries, visit management, medical records, and a guided appointment-booking flow.

## Overview

The app is organized into five Shell tabs:

- **Home**: patient greeting, upcoming appointments, care-plan progress, quick actions, and vitals.
- **Visits**: upcoming and past appointments, booking, cancellation, and rescheduling.
- **Health**: blood pressure, heart rate, and SpO2 trends with selectable ranges.
- **Meds**: medication groups, dose status, filters, and next-dose progress.
- **Help**: medical records and patient profile information.

## Key Features

- Home dashboard using local patient and appointment data.
- Appointment booking by specialty, doctor, future date, time slot, and visit type (Clinic or Video).
- Appointment review with doctor, clinic, and bundled map details.
- Existing appointment rescheduling and cancellation.
- Health trend chart with 7-day, 1-month, and 3-month ranges.
- Medication list and dose progress, visit history, and medical record summaries.
- Appointment persistence in MAUI `Preferences` on the device.

## Data Flow

1. The Shell opens the Home tab; page view models request patient, doctor, appointment, medication, record, and health sample data from injected services.
2. Home and Visits observe the shared `AppointmentService` and refresh when its appointment-changed event is raised.
3. Booking proceeds through Specialty → Doctor → Date & Time → Review. Date/time options exclude dates and times in the past.
4. Confirmation validates that the appointment is in the future, then saves the booking through `AppointmentService`.
5. The service serializes appointment fields to MAUI `Preferences`; Visits reloads and displays the saved booking. Rescheduling reuses the date/time and review portion of the flow.
6. The review map uses the bundled `world-map.json` asset; it does not require a map API.

Patient details, medical records, medication examples, doctor information, and health readings are fictional sample data. Health readings are generated locally; this is not a connected medical record system.

## Controls Used

- `SfCardView`: appointment, doctor, medication, metric, quick-action, visit, and vital cards.
- `SfCartesianChart` / `SfSegmentedControl`: health trends and range selection.
- `SfCarousel`: Home vitals strip.
- `SfLinearProgressBar` / `SfCircularProgressBar`: care-plan and dose progress.
- `SfChipGroup`: appointment time slots and medication filters.
- `SfMaps`: doctor location preview using the packaged map asset.
- `SfAvatarView`, `SfButton`, and `SfListView`: patient/doctor visuals, actions, and records list.

## Architecture

The app uses CommunityToolkit.Mvvm, dependency-injected services/view models, and Shell navigation:

- **Views** (`PatientCompanion/Views/`): five tab pages and specialty, doctor, date/time, and review views.
- **View models** (`PatientCompanion/ViewModels/`): page and appointment-step state.
- **Models** (`PatientCompanion/Models/`): patient, appointment, doctor, medication, health, and record types.
- **Services** (`PatientCompanion/Services/`): `MockDataService`, `AppointmentService`, and `WorldMapAssetService`.
- **Controls** (`PatientCompanion/Controls/`): reusable cards, page layout, and bottom navigation.
- **Resources**: styles, fonts, images, and `Resources/Raw/world-map.json`.

`AppointmentService` is a singleton so appointment changes are shared among pages. The service stores appointments locally in MAUI Preferences; the other sample datasets are constructed in code.

## Prerequisites

- .NET 10 SDK and the .NET MAUI workload.
- A platform workload/device for Android, iOS, Mac Catalyst, or Windows.
- Visual Studio with MAUI support or VS Code with the .NET MAUI extension.
- Syncfusion MAUI packages restored from NuGet. A valid Syncfusion license may be needed to remove evaluation watermarks.

## Configuration

No API keys, environment variables, or external endpoints are required. The bundled map and sample data are available locally.

### Default Behavior

- Patient, clinician, medication, health, and medical-record examples are generated/loaded locally.
- Appointment additions, cancellations, and reschedules are serialized to MAUI Preferences on the device.
- The map is loaded from the app package and requires no map service credentials.

### Environment Variables / Secrets

None required. Do not enter real health information; the appointment data saved by this demo is local to the app's Preferences.

### Notes

- Clearing the app's data or uninstalling it removes its locally stored appointments.
- The appointment store is a demo repository, not a medical scheduling backend.
- Register a Syncfusion license through the documented licensing setup if required; do not commit the key. See [Syncfusion MAUI licensing](https://help.syncfusion.com/common/licensing/how-to-register-maui-license-key).
- This app is for demonstration and is not medical advice.

## Run

From the repository root, build for a platform workload installed on your machine:

```bash
dotnet build PatientCompanion/PatientCompanion/PatientCompanion.csproj -f net10.0-android
```

Other supported target frameworks are `net10.0-ios`, `net10.0-maccatalyst`, and `net10.0-windows10.0.19041.0`. Alternatively, open [`PatientCompanion.slnx`](PatientCompanion.slnx) in Visual Studio or VS Code, choose a target, and deploy.

## Output

![Patient Companion](PatientCompanion.gif)

## Project Structure

- `PatientCompanion.slnx`: solution file.
- `PatientCompanion/Views/`: tab pages and appointment flow views.
- `PatientCompanion/ViewModels/`: page and appointment-flow logic.
- `PatientCompanion/Models/`: patient, provider, visit, health, medication, and record models.
- `PatientCompanion/Services/`: mock data, local appointment persistence, and packaged map loading.
- `PatientCompanion/Controls/`: reusable cards, navigation, and layout controls.
- `PatientCompanion/Resources/`: styles, fonts, images, and the bundled map asset.

## Troubleshooting

**Appointments remain after restarting the app**

This is expected. `AppointmentService` stores appointments in MAUI Preferences. Clear the app's data to return to the initial appointment seed.

**The review map is blank**

Confirm `Resources/Raw/world-map.json` is present and included as a MAUI asset in the project.

**No appointment time is available**

The flow hides past time slots for the current day. Select a later date or a future time.

**Syncfusion evaluation watermark appears**

Register a valid Syncfusion license; see Configuration → Notes.

## Security and Privacy Notes

- Patient, appointment, medication, and health content is fictional.
- Appointments are saved locally in Preferences; the sample does not connect to a backend.
- Do not enter real patient information or use this app for clinical care.
- Keep any Syncfusion license key out of source control.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)
