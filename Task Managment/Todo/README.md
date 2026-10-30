# Todo

This .NET MAUI sample demonstrates a task-management interface with note
creation and editing, due dates and reminders, repeat options, custom lists,
search, and important, reminder, and bin views. The data services are
in-memory; notes do not persist after the app closes.
The sample seed data uses a fixed reference date (February 18, 2026) for
consistent screenshots, so its due dates may appear in the past.

## Requirements

- .NET 10 SDK with the MAUI workload for the target platform.
- Syncfusion MAUI controls pinned to version 35.1.40.

## Build

From this directory, build the Android target with:

```powershell
dotnet build Todo/Todo.csproj -f net10.0-android
```

The iOS, Mac Catalyst, and Windows target frameworks are conditionally included
by the project on their supported operating systems.

## Syncfusion controls

The sample uses Syncfusion `SfListView`, `SfTextInputLayout`, and `SfDatePicker`.
The due-date and reminder-date pickers on the edit-note dialog are bound to the
view model. The separate reminder view filters existing notes; it does not
schedule operating-system notifications. The header avatar opens the working
theme picker; account management, sign-out, and notification scheduling are
not included.

## Scope

The sample currently includes My Notes, Important, Reminder, Bin, custom-list,
and search views. It does not include a separate "Review today" screen or
load-more/pagination behavior.
