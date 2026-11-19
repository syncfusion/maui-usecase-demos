# Visitor Pass Management: .NET MAUI Use Case Sample

A cross-platform facility-operations sample for registering visitors and managing their visit lifecycle. The app combines a dashboard, visitor directory, digital pass, and guided registration flow using .NET MAUI and Syncfusion controls.

## Overview

The sample is designed around the front-desk workflow:

- **Dashboard**: expected, checked-in, checked-out, and overdue KPIs; visitor-status and visit-purpose charts; today's visitor list.
- **Directory**: visitor search and status filtering, responsive list/table presentation, and an active visitor preview.
- **Digital Visitor Pass**: visitor and host details, pass barcode, visit-progress timeline, and Check In, Check Out, or Cancel actions.
- **Registration**: a three-step wizard for visitor identity, visit scheduling, and pass generation.
- **Theme**: a light/dark appearance toggle.

## Key Features

- Search visitors and filter by visit status.
- Open a visitor record from the Dashboard or Directory.
- Generate barcode-based passes with a visit timeline.
- Check in, check out, or cancel a visit; shared-store changes update related screens.
- Validate visitor and schedule details before generating a new pass.
- Adapt the registration surface to device idiom: `SfPopup` on desktop/tablet and modal page on phones.

## Data Flow

1. At startup, the DI-registered `VisitorStore` creates fictional seed visitors and assigns pass IDs.
2. The Dashboard derives KPIs and chart collections from the store; the Directory applies its search and status filters to the same visitor set.
3. Selecting a visitor opens the Digital Pass with identity, host, schedule, barcode, and journey status.
4. Check In, Check Out, and Cancel update the visitor in the shared store. Store events trigger dashboard and pass view models to refresh.
5. Registration validates identity and visit schedule, adds a new Expected visitor, and displays the generated pass.

Visitor data and theme state are held in the running app only. There is no backend, identity provider, or persistence layer; restarting restores the seed and discards changes.

## Controls Used

- `SfDataGrid`: desktop visitor tables on the Dashboard and Directory.
- `SfListView`: mobile visitor lists and visit-journey details.
- `SfCircularChart` / `SfCartesianChart`: visitor-status and visit-purpose summaries.
- `SfBarcodeGenerator`: digital pass barcode.
- `SfStepProgressBar`: visit journey progress on the Digital Visitor Pass.
- `SfPopup`, `SfChipGroup`, `SfTextInputLayout`, `SfDatePicker`, and `SfTimePicker`: registration and filtering.
- `SfBadgeView`, `SfAvatarView`, and `SfButton`: status presentation, visitor identity, and actions.

## Architecture

The app uses CommunityToolkit.Mvvm and dependency injection:

- **Application root** (`App.xaml.cs`): creates a `NavigationPage`; page navigation uses `Navigation.PushAsync`, not Shell routing.
- **Views** (`VisitorPass/Views/`): Dashboard, Directory, Visitor Details, and registration surfaces.
- **View models** (`VisitorPass/ViewModels/`): dashboard aggregates, directory filters, visitor details, and registration validation.
- **Models** (`VisitorPass/Models/`): visitor, host, status, purpose, timeline, and chart data types.
- **Services** (`VisitorPass/Services/`): seed data, singleton `VisitorStore`, pass-code generation, and theme state.
- **Controls and converters** (`VisitorPass/Controls/`, `VisitorPass/Converters/`): navigation, avatar/status presentation, and binding helpers.

## Prerequisites

- .NET 10 SDK and the .NET MAUI workload.
- A platform workload/device for Android, iOS, Mac Catalyst, or Windows.
- Visual Studio with MAUI support or VS Code with the .NET MAUI extension.
- Syncfusion MAUI packages restored from NuGet. A valid Syncfusion license may be needed to remove evaluation watermarks.

## Configuration

No application configuration is required. Visitor records and pass codes are generated locally.

### Default Behavior

- The visitor store seeds fictional sample records at startup.
- Pass IDs are generated locally; no barcode/pass service is called.
- Register and lifecycle actions update only the in-memory store.

### Environment Variables / Secrets

None required. The sample does not call an external service or require credentials.

### Notes

- Visitor data and theme changes are not persisted after restart.
- The registration flow creates a sample facility pass; it is not an access-control integration.
- Register a Syncfusion license through the documented licensing setup if required; do not commit the key. See [Syncfusion MAUI licensing](https://help.syncfusion.com/common/licensing/how-to-register-maui-license-key).

## Run

From the repository root, build for a platform workload installed on your machine:

```bash
dotnet build VisitorPassManagement/VisitorPass/VisitorPass.csproj -f net10.0-android
```

Other supported target frameworks are `net10.0-ios`, `net10.0-maccatalyst`, and `net10.0-windows10.0.19041.0`. You can also open [`VisitorPass.slnx`](VisitorPass.slnx) in Visual Studio or VS Code, choose a target, and deploy to a device or emulator.

## Output

This sample folder does not include a separate demo GIF. Run the app to view the Dashboard, Directory, Digital Pass, and registration workflow.

## Project Structure

- `VisitorPass.slnx`: solution file.
- `VisitorPass/`: MAUI app project.
- `VisitorPass/Controls/`: reusable navigation, visitor identity, and status controls.
- `VisitorPass/Converters/`: XAML value converters.
- `VisitorPass/Models/`: visitor, host, status, purpose, timeline, and chart models.
- `VisitorPass/Services/`: visitor seed, store, pass-code generator, and theme service.
- `VisitorPass/ViewModels/`: Dashboard, Directory, Visitor Details, and registration view models.
- `VisitorPass/Views/`: Dashboard, Directory, Visitor Details, and registration pages/views.

## Troubleshooting

**A registered visitor disappears after restarting**

This is expected; visitor records are stored in memory only.

**Check In, Check Out, or Cancel is unavailable**

The available actions depend on the visitor's current status. For example, Check Out is available only after Check In.

**Registration does not advance**

Complete the required identity and schedule fields; validation messages are shown in the form.

**Registration appears as a popup or modal page**

This is intentional. The app chooses the registration host based on phone versus desktop/tablet idiom.

**Syncfusion evaluation watermark appears**

Register a valid Syncfusion license; see Configuration → Notes.

## Security and Privacy Notes

- Visitor names, contact details, host names, and visits are fictional sample data.
- The app has no backend, identity provider, or persistence.
- Do not use the sample for real facility access or enter real visitor information.
- Keep any Syncfusion license key out of source control.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)
