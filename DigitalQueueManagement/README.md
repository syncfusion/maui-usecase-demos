# Digital Queue Management

A .NET MAUI sample for issuing and managing customer-service queue tokens across service counters.

## Overview

Digital Queue Management demonstrates how a service desk can organize customer waitlists and coordinate multiple counters in one app. It is intended for teams exploring digital queue workflows and .NET MAUI user interfaces.

- Customers are represented by queue tokens that staff can call, complete, skip, recall, or transfer.
- Staff can manage service counters and see current queue activity.
- Queue state is stored locally in memory for the sample.
- The app uses deterministic local logic; it does not integrate an AI provider.
- Users get a single view of queue status, counter activity, and basic queue analytics.

## Key Features

- View queue and counter summaries, including queue progress and analytics.
- Call the next token and complete, skip, or recall service.
- Transfer tokens between available counters.
- Add, edit, and remove service counters.
- Adapt dialogs and bottom sheets to desktop and mobile layouts.

## Controls Used

- `SfCardView` — presents queue and counter summary cards.
- `SfChip` — displays compact status and count badges in analytics cards.
- `SfListView` — displays queue tokens and service counters.
- `SfSegmentedControl` — switches between app sections and filters.
- `SfLinearProgressBar` — visualizes queue progress.
- `SfCircularChart` — summarizes queue activity.
- `SfPopup` — displays desktop dialogs and mobile bottom sheets.

## Architecture

The app separates queue state and operations from the UI:

- `Views/` — MAUI XAML pages and their code-behind.
- `ViewModel/` — presentation state and queue/counter operations.
- `Model/` — queue models and the in-memory `QueueStore`.
- `Controls/` — custom controls, including a borderless entry.
- `Resources/` — styles, fonts, app icon, and splash screen.
- `MauiProgram.cs` — configures MAUI, Syncfusion controls, fonts, and logging.

The views bind to view models, which use `QueueStore` to read and update the in-memory queue. The resulting state is shown in the queue, counter, and overview screens.

## Prerequisites

- .NET 10 SDK.
- .NET MAUI workload.
- A supported device or emulator for the target platform.
- A valid Syncfusion license for the licensed Syncfusion controls.
- Visual Studio with .NET MAUI support is recommended for IDE-based development.

No AI provider account or credentials are required.

## AI Configuration

This sample does not use AI. Its default behavior is local, deterministic queue management with in-memory sample data. There is no AI service configuration or AI fallback behavior.

## Run

Run these commands from the `DigitalQueueManagement` directory. Install the MAUI workload if it is not already available:

```powershell
dotnet workload install maui
dotnet restore .\DigitalQueueManagement\DigitalQueueManagement.csproj
dotnet build .\DigitalQueueManagement\DigitalQueueManagement.csproj -f net10.0-windows10.0.19041.0
dotnet run --project .\DigitalQueueManagement\DigitalQueueManagement.csproj -f net10.0-windows10.0.19041.0
```

The project also targets Android, iOS, and Mac Catalyst. Build or run with `net10.0-android`, `net10.0-ios`, or `net10.0-maccatalyst` on a host that supports the target platform, using a device or emulator as appropriate.

## Output

The app opens to a queue-management interface with overview cards, token and counter lists, service actions, and queue analytics. The sample does not currently include an application screenshot.

## Project Structure

- `DigitalQueueManagement/Views/` — queue, overview, counter, management, and dialog UI.
- `DigitalQueueManagement/ViewModel/` — presentation logic and user actions.
- `DigitalQueueManagement/Model/` — queue data models and in-memory storage.
- `DigitalQueueManagement/Controls/` — reusable custom controls.
- `DigitalQueueManagement/Resources/` — styles, fonts, images, and app assets.
- `DigitalQueueManagement/Platforms/` — platform-specific application setup.

## Troubleshooting

### MAUI workload or target-framework errors

Cause: The .NET MAUI workload or platform-specific tooling is missing.

Fix: Install the .NET 10 SDK and MAUI workload, then verify that the host supports the selected target framework.

### Syncfusion license warnings

Cause: A valid Syncfusion license has not been registered for the app.

Fix: Register your Syncfusion license using the recommended approach for your development environment. Do not commit license keys to source control.

### Queue changes do not persist after restarting

Cause: Queue data is stored in memory and is reset when the app restarts.

Fix: This is expected in the sample. Add persistent storage if the application requires queue data to survive restarts.

## Security and Privacy Notes

- The sample uses local in-memory queue data and does not send queue information to an AI service.
- Avoid entering sensitive personal information in sample data.
- Do not hard-code or commit Syncfusion license keys.
- Review data handling and external service integrations before adapting the sample for production.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)
