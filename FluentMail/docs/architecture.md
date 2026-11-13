# Architecture: Fluent Mail

## Stack

- .NET 10 MAUI, same target frameworks as the other samples in this repository.
- CommunityToolkit.Mvvm 8.4.0 for `ObservableObject`, `[ObservableProperty]`, and `[RelayCommand]`.
- Syncfusion packages, version 33.2.15 to match the repository: Core (SfAvatarView, SfBadgeView), TreeView, ListView, RichTextEditor, Inputs (SfAutocomplete), Buttons (SfChipGroup), Popup. Confirm each package name in the NuGet gallery before adding it.
- No other libraries. The sample makes no network calls.

## Principles

- One ViewModel per page. A ViewModel owns its own collections and commands.
- Pages bind to their ViewModel in XAML (`x:DataType` plus `BindingContext`). Code-behind holds only `InitializeComponent`.
- No dependency injection container, no messaging, no app-wide state.
- Mock data comes from one static class, `MockData`. Pages that must show the same mailbox (Mail, Archive, Deleted, Compose) share one in-memory `Mailbox` object held by `MockData`. This is the only shared state, and it exists so Restore and Send visibly change other screens.

## Folder layout

```
FluentMail/
  FluentMail.slnx
  README.md
  docs/                      intent, spec, plan, tasks, architecture
  FluentMail/
    FluentMail.csproj
    MauiProgram.cs           Syncfusion setup, fonts
    App.xaml(.cs)
    AppShell.xaml(.cs)       desktop rail and mobile tabs
    Models/
      MailMessage.cs  Contact.cs  AttachmentInfo.cs  FolderNode.cs  MailAccount.cs
    Data/
      MockData.cs            builds all collections, fixed seed
      Mailbox.cs             shared ObservableCollections and move/send operations
    ViewModels/
      MailWorkspaceViewModel.cs
      DraftWorkspaceViewModel.cs
      ArchiveViewModel.cs
      DeletedItemsViewModel.cs
      ComposeViewModel.cs
    Views/
      MailWorkspacePage.xaml
      DraftWorkspacePage.xaml
      ArchivePage.xaml
      DeletedItemsPage.xaml
      ComposePage.xaml
      Controls/              ContentViews reused across pages
        StateView.xaml       loading, empty, error presentation
        SnackbarPopup.xaml
    Resources/
      Styles/Colors.xaml  Styles.xaml     Fluent tokens, light and dark
      Fonts/  Images/
    Platforms/
```

Keep the number of files at or below this list. Do not add folders for layers that hold one class.

## Data flow

```
MockData --> Mailbox (ObservableCollections) --> ViewModel (filtering, commands) --> XAML bindings --> Syncfusion control
```

Commands change `Mailbox` collections. Collections raise change notifications. Views update without extra code.

## Control to ViewModel map

| Control | Bound to |
|---|---|
| SfTreeView | `Folders` (`ObservableCollection<FolderNode>`), `ChildPropertyName="Children"`, `SelectFolderCommand` |
| SfListView (messages) | `VisibleMessages`, `SelectedMessage` |
| SfBadgeView | `FolderNode.Count` computed from `Mailbox` |
| SfAvatarView | `Contact.Initials`, `Contact.Color` |
| SfChipGroup | `Recipients`, `QuickFilters`, `Tags` |
| SfAutocomplete | `ContactSuggestions`, `SearchText`, `SelectedContact` |
| SfRichTextEditor | `BodyHtml` (two-way via `HtmlText`) |
| SfPopup | `IsMenuOpen`, `IsFilterOpen`, `IsSnackbarOpen` booleans |

## Responsive design

- `VisualStateManager` on each page root with `Wide` and `Narrow` states. A `StateTrigger` driven by `AdaptiveTrigger` (MinWindowWidth 900) switches column definitions and pane visibility.
- `OnIdiom` sets font sizes, paddings, and item heights.
- Narrow mode shows one pane at a time. A `Pane` enum property on the ViewModel (`Folders`, `List`, `Reading`) controls which one is visible, with a back command.
- The desktop shell uses a `FlyoutBehavior` rail. The mobile shell uses a `TabBar`. `Shell` picks one by idiom at startup.

## Theming

- `Colors.xaml` defines named tokens: `Primary`, `Surface`, `SurfaceAlt`, `TextPrimary`, `TextSecondary`, `Divider`, `Danger`, each with light and dark values.
- Pages use `AppThemeBinding` against those tokens. Syncfusion controls get colors through the same tokens, plus Syncfusion theme dictionary (`SyncfusionThemeResourceDictionary`) switched with the app theme.

## State handling

Each ViewModel exposes `ViewState` (`Loading`, `Content`, `Empty`, `Error`). `StateView` shows the matching visual. Loading simulates a short delay (400 ms) so the loading state is visible and testable.

## Draft save status

`DraftWorkspaceViewModel` holds a `SaveStatus` enum (`Saved`, `Saving`, `Unsaved`). Editing the subject, recipients, or body sets `Unsaved` and restarts a short debounce timer. When it fires, the status becomes `Saving`, the draft is saved to `Mailbox`, and the status becomes `Saved`. The editor text is bound two-way to `BodyHtml`, so no code-behind syncs it.

## Commands

Folder selection, message selection, draft selection and Resume, filter, Send, Restore, and Empty trash are all `[RelayCommand]` methods. Pages do not use event handlers for these.

## Performance

Each SfListView uses one `DataTemplate` with no per-item converters that allocate. Collections are built once in `MockData`. Filtering replaces the visible collection contents without recreating items.

## Error handling

Validate only at the boundary: Send inputs. Inside the app, trust the data.

## Testing approach

- ViewModel unit tests in a small test project are optional. If added, cover filter logic, Restore, Send validation, and folder counts, because they have no UI dependency.
- Manual checks use the checklist in [tasks.md](tasks.md).
