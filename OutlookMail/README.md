# Fluent Mail

A Syncfusion .NET MAUI mail workspace sample that demonstrates an Outlook-style inbox, search, compose, calendar, and folder management experience with local mock data.

## Overview

Fluent Mail shows how to build a polished mail-style application in .NET MAUI using Syncfusion controls and MVVM.

- **What the sample does:** presents a mailbox with folders, message lists, search, calendar, compose, archive, and deleted mail flows.
- **Who it is for:** MAUI developers who want to see how Syncfusion controls can be combined into a real-world productivity UI.
- **What outcome the user gets:** a fully self-contained mail showcase that demonstrates common mail app interactions without any backend or account setup.

## Key Features

- Folder navigation with favorites, hover actions, and message counts
- Search with top contacts, recent searches, and tappable message results
- Compose mail with recipient autocomplete and rich text editing
- Mobile mail list with swipe actions, folder popup, and message reader

## Controls Used

- **SfListView** — displays folders, mail lists, mobile swipe lists, and agenda items.
- **SfCalendar** — shows the calendar experience and date selection UI.
- **SfPopup** — provides the mobile folder picker popup.
- **SfAutocomplete** — powers recipient lookup in the compose experience.
- **SfRichTextEditor** — supports composing and editing formatted message content.
- **SfComboBox** — provides folder and sort selection in the workspace UI.
- **SfButton** — used for primary actions such as Send and Discard.
- **BorderlessEntry** - plain `Entry` subclass (not a Syncfusion control) that removes native input chrome.

## Architecture

- **View / Page** — `MailWorkspacePage`, `SearchPage`, and `CalendarPage` host the sample UI.
- **ViewModel** — `MailWorkspaceViewModel`, `SearchViewModel`, and `CalendarViewModel` manage state and commands.
- **Models** — mail, folder, contact, calendar, and search result models represent the sample data.
- **Supporting UI components** — `MobileHeader`, `MobileTabBar`, `CalendarFlyout`, `StateView`, and `BorderlessEntry` provide reusable layout pieces.
- **Data layer** — `MockData` and `Mailbox` keep the sample local and self-contained.

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload
- Visual Studio 2022 17.12 or later with the .NET MAUI workload (the .NET 10 SDK is installed separately; use the latest Visual Studio 2022 or Visual Studio 2026 for full .NET 10 tooling support), or VS Code with the .NET MAUI extension
- Syncfusion MAUI packages referenced in the project file
- A Syncfusion license key if your environment requires one

## Design Notes

This is a demo, so some choices favour readability over production structure:

- Dependencies (`Mailbox`, `IAlertService`, child view models) are registered in `MauiProgram` and injected, but `Mailbox` is still backed by the static `MockData` and the view models are not covered by unit tests.
- `MailWorkspacePage.xaml.cs` keeps a few UI-only handlers (swipe/tap forwarding from `SfListView` and closing `SfAutocomplete` drop-downs).
- `ComposeViewModel` strips HTML from the editor with a regex to build a preview. This is fine for mock data; do not reuse it to sanitise HTML in production.
- The sender identity and folder names live in `CurrentUser` and `FolderNames`.

## Run

```bash
dotnet build FluentMail.sln -c Debug
```

If you want to target a specific platform, use the matching MAUI target framework from the project file.

## Output

Visual summary: an Outlook-style mail workspace with a folder column, message list, calendar view, and a compose experience.

![.NET MAUI Outlook Sample demo.](OutlookSampleDemo.gif)

## Project Structure

- `Views/` — UI pages and layouts
- `ViewModels/` — presentation logic and commands
- `Models/` — data structures
- `Data/` — local mock mailbox and calendar data
- `Converters/` — UI value converters
- `Resources/` — styles, images, fonts, and other assets

## Troubleshooting

### Path too long exception

If you encounter a "Path Too Long" exception while building the project, close Visual Studio, rename the repository folder to a shorter name, and rebuild the project.
