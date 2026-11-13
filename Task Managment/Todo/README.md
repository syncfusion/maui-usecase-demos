# Todo

A .NET MAUI task management sample that demonstrates note creation, filtering, editing, and organization with Syncfusion controls.

## Overview

This sample shows a local, self-contained task manager for users who need to capture notes, mark important items, track reminders, organize custom lists, and manage deleted notes. It uses in-memory data and does not depend on a backend service.

The sample is for developers and customers who want to see how to build a practical note and task workflow in .NET MAUI with Syncfusion UI controls.

It uses Syncfusion list, input, picker, popup, expander, and button components to support task browsing, editing, reminders, and sidebar interactions.

The outcome is a clear task-management experience with built-in categories, custom lists, search, and note editing in a single sample.

## Key Features

Feature 1: Create and edit notes with title, due date, reminder date, repeat frequency, and list assignment.

Feature 2: Filter notes into My Notes, Important, Reminder, and Bin views.

Feature 3: Organize notes with custom lists and search across the task set.

Feature 4: Restore or permanently delete notes from the Bin.

## AI Flow

This sample does not include AI functionality.

1. The user selects a view or edits a note.
2. The app loads the local note and list data.
3. The UI updates through the shared ViewModel.
4. The edited note is saved back into the in-memory store.
5. The filtered results are refreshed in the list UI.
6. No AI fallback or retry behavior is used.

## Controls Used

SfListView: Displays notes and filtered collections.

SfTextInputLayout: Provides structured input fields in the note editor.

SfDatePicker: Lets the user pick due dates and reminder dates.

SfPopup: Hosts dialog-style note editing and related interactions.

SfExpander: Groups collapsible sections in the task UI.

SfButton: Supports actions such as add, edit, delete, restore, and navigation.

## Architecture

The sample is organized around a shared ViewModel that drives multiple views.

View / Page: Displays the note lists, filters, dialogs, and sidebar content.

ViewModel: Holds the task state, filtering logic, and command handling.

Models: Represent notes, custom lists, and related task data.

Services: Provide in-memory note storage, list storage, navigation, and search behavior.

Supporting UI components: Converters, dialogs, and layout helpers adapt the data for display.

## Prerequisites

.NET SDK: .NET 10 SDK.

MAUI workload: .NET MAUI workload for the target platform.

Visual Studio: Visual Studio 2022 with .NET MAUI support, if using the IDE.

Syncfusion packages: Syncfusion MAUI packages version 35.1.43.

License: A valid Syncfusion license may be required for full use of the controls.

AI provider: Not required.

## AI Configuration

This sample does not use AI.

## Default Behavior

Mock AI: Not used.

Local/offline logic: Used for all note, filter, and search behavior.

Real cloud AI service: Not used.

## Environment Variables / Secrets

Variable | Required | Description
EXAMPLE_ENDPOINT | no | Not used in this sample.
EXAMPLE_KEY | no | Not used in this sample.
EXAMPLE_MODEL | no | Not used in this sample.
EXAMPLE_TIMEOUT_SECONDS | no | Not used in this sample.

Notes

Do not commit keys to source control.
Use environment variables, user secrets, or secure storage when secrets are needed in other samples.
If configuration is missing, the sample continues to use local in-memory data.

## Run

```powershell
dotnet build Todo/Todo.csproj -f net10.0-android
```

If you target another platform, use the corresponding framework supported by the project.

## Output
<img width="800" height="425" alt="Todo_windows" src="https://github.com/user-attachments/assets/4342e2b9-d618-4b0d-9a3a-8938449c01aa" />


## Project Structure

Views/: UI pages and layouts.

ViewModels/: presentation logic.

Models/: data structures.

Services/: app services for notes, lists, navigation, and search.

Resources/: styles, images, and assets.

## Troubleshooting

Issue 1: Missing MAUI workload.
Cause: The required workload is not installed.
Fix: Install the .NET MAUI workload for your target platform.

Issue 2: Build errors after upgrading packages.
Cause: Package versions or SDK versions may be out of sync.
Fix: Verify the .NET 10 SDK and Syncfusion package versions.

Issue 3: Unexpected note or filter behavior.
Cause: The sample uses shared in-memory state across views.
Fix: Return to the relevant category and confirm the active filter.

## Security and Privacy Notes

Do not hard-code API keys.

Avoid sending unnecessary sensitive data to external services.

Minimize prompt content to only what is needed.

Prefer secure storage for credentials.

Review any external API calls for data exposure.

## Supported Platforms

Android.

iOS.

Windows.

macOS.
