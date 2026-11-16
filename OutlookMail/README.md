# Fluent Mail

A Syncfusion .NET MAUI mail workspace sample that demonstrates an Outlook-style inbox, search, compose, calendar, and folder management experience with local mock data.

## Overview

Fluent Mail shows how to build a polished mail-style application in .NET MAUI using Syncfusion controls and MVVM.

- **What the sample does:** presents a mailbox with folders, message lists, search, calendar, compose, archive, and deleted mail flows.
- **Who it is for:** MAUI developers who want to see how Syncfusion controls can be combined into a real-world productivity UI.
- **What AI capability it uses:** none. This sample uses local, deterministic app logic rather than an AI service.
- **What outcome the user gets:** a fully self-contained mail showcase that demonstrates common mail app interactions without any backend or account setup.

## Key Features

- Folder navigation with favorites, hover actions, and message counts
- Search with top contacts, recent searches, and tappable message results
- Compose mail with recipient autocomplete and rich text editing
- Mobile mail list with swipe actions, folder popup, and message reader

## AI Flow

This sample does not use AI.

1. The user interacts with the mail workspace, search page, calendar, or compose UI.
2. The app reads local mock mailbox data from in-memory collections.
3. No AI service is called.
4. The app applies local filtering, selection, and navigation logic.
5. The result is rendered directly in the UI.
6. No fallback or retry behavior is required because the sample is offline and self-contained.

## Controls Used

- **SfListView** — displays folders, mail lists, mobile swipe lists, and agenda items.
- **SfCalendar** — shows the calendar experience and date selection UI.
- **SfPopup** — provides the mobile folder picker popup.
- **SfAutocomplete** — powers recipient lookup in the compose experience.
- **SfRichTextEditor** — supports composing and editing formatted message content.
- **SfComboBox** — provides folder and sort selection in the workspace UI.
- **SfButton** — used for primary actions such as Send and Discard.
- **SfEntry** — lightweight search input used in the search page.

## Architecture

- **View / Page** — `MailWorkspacePage`, `SearchPage`, and `CalendarPage` host the sample UI.
- **ViewModel** — `MailWorkspaceViewModel`, `SearchViewModel`, and `CalendarViewModel` manage state and commands.
- **Models** — mail, folder, contact, calendar, and search result models represent the sample data.
- **Supporting UI components** — `MobileHeader`, `MobileTabBar`, `CalendarFlyout`, `StateView`, and `SfEntry` provide reusable layout pieces.
- **Data layer** — `MockData` and `Mailbox` keep the sample local and self-contained.

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload
- Visual Studio 2022 with MAUI support
- Syncfusion MAUI packages referenced in the project file
- A Syncfusion license key if your environment requires one

## AI Configuration

No AI configuration is required.

### Default Behavior

- Mock AI: not used
- Local/offline logic: used throughout the sample
- Real cloud AI service: not used

### Environment Variables / Secrets

| Variable | Required | Description |
| --- | --- | --- |
| None | No | This sample does not use AI credentials or service configuration. |

**Notes**

- Do not commit keys to source control.
- Use environment variables, user secrets, or secure storage for real AI-backed apps.
- If configuration is missing, there is no AI fallback path because this sample does not call an AI service.

## Run

```bash
dotnet build FluentMail.sln -c Debug
```

If you want to target a specific platform, use the matching MAUI target framework from the project file.

## Output

Visual summary: an Outlook-style mail workspace with a folder column, message list, search page, mobile folder popup, swipe actions on mobile, calendar view, and a compose experience.

## Project Structure

- `Views/` — UI pages and layouts
- `ViewModels/` — presentation logic and commands
- `Models/` — data structures
- `Data/` — local mock mailbox and calendar data
- `Converters/` — UI value converters
- `Resources/` — styles, images, fonts, and other assets

## Troubleshooting

**Issue 1: Missing AI configuration**  
Cause: Required environment variables or secrets are not set.  
Fix: Not applicable to this sample because it does not use AI configuration.

**Issue 2: Build or workload errors**  
Cause: Missing SDK, workload, or incompatible package version.  
Fix: Verify the required .NET version and install the needed MAUI workload.

**Issue 3: UI content does not appear as expected**  
Cause: Local mock data, bindings, or view-model state may be out of sync.  
Fix: Check the relevant ViewModel and XAML binding paths.

## Security and Privacy Notes

- Do not hard-code API keys.
- Avoid sending unnecessary sensitive data to any external service.
- Minimize prompt content to only what is needed when AI is added later.
- Prefer secure storage for credentials.
- Review any external API calls for data exposure.

## Supported Platforms

- Android
- iOS
- Windows
- macOS

## License

This repository is intended for demonstration and learning purposes. Follow the repository license and any applicable Syncfusion licensing terms before distributing or reusing the sample.

- The sample uses local mock data only.
- There is no authentication or cloud sync.
- There is no backend API.
