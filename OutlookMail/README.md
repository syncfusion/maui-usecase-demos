# Fluent Mail: .NET MAUI Use Case Sample

An Outlook-style mail workspace built with **Syncfusion .NET MAUI controls**, demonstrating adaptive inbox, search, compose, folder, calendar, and mobile message workflows using local mock data.

## Overview

Fluent Mail provides a self-contained productivity app experience for desktop, tablet, and phone layouts:

- **What the sample does**: Shows a multi-folder mailbox, filters and searches messages, opens and manages mail, composes and sends demo messages, navigates mobile search and calendar pages, and opens a desktop calendar flyout.
- **Who it is for**: Developers building mail clients, productivity dashboards, or adaptive MAUI applications.
- **What it demonstrates**: Desktop and phone-specific layouts, grouped `SfListView` and `SfTreeView`-style folder navigation, `SfCalendar`, `SfPopup`, recipient `SfAutocomplete`, rich-text editing, mobile swipe actions, and MVVM state management.
- **What outcome the user gets**: A runnable local mail showcase with mock folders and messages. It does not connect to an email account or send mail over a network.

## Key Features

- **Adaptive mailbox**: Desktop/tablet layouts use a folder column, message list, and reading pane or composer; phone layouts use a focused-mail/other-mail switcher, folder popup, grouped mail list, and bottom tab bar.
- **Folder navigation**: Mock folders are grouped under Favorites and Kerry Best. Folder counts, favorite folders, archive, delete, and selected-folder state are managed in the in-memory mailbox.
- **Desktop message actions**: Search within the selected folder, filter All/Unread/Flagged, sort and manage messages, and select a message in the reading pane.
- **Compose mail**: Desktop uses recipient autocomplete and `SfRichTextEditor`; phone uses autocomplete and a plain editor. Sending creates a Sent Items entry and shows a demo confirmation.
- **Mobile search**: The Search page supports text search plus unread, flagged, to-me, attachment, and top-contact filters. It can open selected results in a message reader.
- **Calendar**: The desktop calendar is an in-app flyout. On phones, Calendar is a separate Shell page with Agenda, Day, and Month modes and controls to add or remove events.
- **Mobile swipe actions**: Swiping a mail item to the right deletes it; swiping left archives it. Each action updates the in-memory mailbox collections.

## Data Flow

1. `MauiProgram.CreateMauiApp()` registers the static mock `Mailbox`, alert service, composer, and page/ViewModels through dependency injection.
2. `MockData.CreateMailbox()` initializes folder nodes, inbox, sent, draft, archive, deleted, and contact collections in memory.
3. `MailWorkspaceViewModel` derives desktop folder/message collections and phone focused/other folder collections from `Mailbox`.
4. Desktop folder, search, and filter changes update `VisibleMessages`; selecting a message marks it read and refreshes unread folder counts.
5. Mobile folder selection rebuilds `MobileMessages`, while the Search page applies its independent search and filter criteria across inbox, sent, and archive messages.
6. Archive/delete actions move or remove the selected message in the mailbox collections. Sent mail is appended to `SentItems` by `ComposeViewModel`.
7. `CalendarViewModel` reads and modifies `CalendarData.Entries` in memory. The data is shared by the desktop flyout and phone calendar page.

## Controls Used

- `SfListView` (ListView): Desktop folders and messages, mobile grouped mail, and desktop calendar agenda.
- `SfCalendar` (Calendar): Desktop calendar flyout and phone Agenda/Day/Month calendar views.
- `SfPopup` (Popup): Mobile folder-selection menu.
- `SfAutocomplete` (Inputs): Desktop and phone recipient lookup in the compose experience.
- `SfRichTextEditor` (Rich Text): Desktop compose body with a formatting toolbar.
- `SfComboBox` (Inputs): Desktop sort/filter selection and phone sort selection.
- `SfButton` (Buttons): Desktop New Mail and calendar/compose actions.
- `SfTreeView` / Syncfusion DataSource grouping: The folder list uses a `SfListView` plus data-source grouping rather than a standalone `SfTreeView`.
- .NET MAUI `Entry`, `Editor`, `Switch`, `TimePicker`, `Border`, and `ImageButton`: Phone compose fields and supporting controls.
- `BorderlessEntry`: Project-specific `Entry` subclass that registers platform handler mappings to remove native entry chrome.

## Architecture

The sample uses MVVM and a three-route .NET MAUI Shell:

- **Views** (`Views/`): `MailWorkspacePage`, `SearchPage`, and `CalendarPage`, plus reusable mobile header, tab bar, calendar flyout, state view, and borderless entry.
- **ViewModels** (`ViewModels/`): Mail workspace state, compose state, search/filter state, and calendar agenda/edit state.
- **Models** (`Models/`): Mail, folder, contact, account, calendar, search, agenda, and file-item data structures.
- **Data** (`Data/`): Static mock mailbox and calendar data, plus the runtime `Mailbox` collection container.
- **Services** (`Services/`): `IAlertService` abstraction used to display a demo send confirmation.
- **Converters** (`Converters/`): Initials, Boolean negation, date grouping, and sorting helpers.
- **Application shell** (`AppShell.xaml`, `AppShell.xaml.cs`): Registers Mail, Search, and Calendar routes with the Shell tab bar disabled.

The mail state is local and process-lifetime only. `ComposeViewModel` and `CalendarViewModel` update the static/mock collections, but the sample does not persist mailbox or calendar changes after restart.

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload, plus the platform workload for Android, iOS, Mac Catalyst, or Windows
- Visual Studio 2022 (17.14+) or VS Code with the MAUI extension
- A configured emulator/simulator or connected target device
- Syncfusion .NET MAUI packages: restored automatically from NuGet

## Configuration

No configuration is required. All messages, folders, contacts, files, and calendar entries are mock data.

### Default Behavior

- **Local/offline logic**: The sample uses static in-memory collections and does not call an email provider or account service.
- **Desktop and phone layouts**: Layout selection uses MAUI device idiom. Desktop/tablet behavior is separate from phone behavior.
- **Demo send**: Sending a composed message adds it to `SentItems` and shows an alert; it does not transmit network mail.

### Environment Variables / Secrets

None required. The sample does not use API keys, credentials, environment variables, or external mail services.

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
dotnet build OutlookMail/FluentMail/FluentMail.csproj -t:Run -f net10.0-android
```

Other supported platforms:

```bash
dotnet build OutlookMail/FluentMail/FluentMail.csproj -t:Run -f net10.0-ios
dotnet build OutlookMail/FluentMail/FluentMail.csproj -t:Run -f net10.0-maccatalyst
dotnet build OutlookMail/FluentMail/FluentMail.csproj -t:Run -f net10.0-windows10.0.19041.0
```

Or open `OutlookMail/FluentMail.sln` in Visual Studio or VS Code, restore packages, and deploy to your target platform.

The iOS and Mac Catalyst targets are included on non-Linux hosts, and the Windows target is included on Windows hosts.

## Output

![Outlook-style mail sample](OutlookSampleDemo.gif)

## Project Structure

- `Views/`: Mail workspace, search, calendar, and reusable controls
- `ViewModels/`: Mail, compose, search, and calendar presentation logic
- `Models/`: Message, folder, contact, account, and calendar data structures
- `Data/`: Mock mailbox and calendar data
- `Services/`: Alert-service abstraction
- `Converters/`: XAML date, initials, filtering, and grouping converters
- `Resources/`: Styles, images, fonts, app icon, and splash asset
- `Platforms/`: Android, iOS, Mac Catalyst, and Windows entry points

## Troubleshooting

**Issue 1: A composed message or calendar change disappears after restart**
Cause: Mailbox and calendar collections are in-memory demo state.
Fix: Restarting or reloading the app recreates the mock data. Persistence would require a database or local preference/file implementation.

**Issue 2: Phone folder or desktop folder counts look stale**
Cause: Folder counts are recalculated after mailbox actions and message read-state changes.
Fix: Verify that the operation completed in `Mailbox`; reopening or reselecting a folder triggers the workspace refresh.

**Issue 3: Rich text editing is unavailable on phone**
Cause: The phone compose layout intentionally uses a plain `Editor`, while `SfRichTextEditor` is bound only for non-phone idioms.
Fix: Run the desktop/tablet layout to use the rich-text editor.

**Issue 4: Build or workload errors**
Cause: The .NET 10 SDK or required MAUI/platform workload is missing.
Fix: Verify `dotnet --version` and install the needed MAUI and platform workloads.

**Issue 5: Trial/evaluation banner shown**
Cause: No Syncfusion license key is registered.
Fix: Register a valid license key in `MauiProgram.cs` as shown in the Notes section.

## Security and Privacy Notes

- All mail, sender, recipient, calendar, and contact data is fictional mock data.
- No account credentials or API keys are used, and no email is transmitted.
- Compose and calendar changes remain in memory unless you add explicit persistence.
- If adapting this demo for real mail, use the provider's authenticated APIs and secure credential storage; never hard-code credentials.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)
