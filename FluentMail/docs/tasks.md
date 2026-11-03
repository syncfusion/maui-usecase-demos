# Tasks: Fluent Mail

Mark each task when it is done and verified. Milestones match [plan.md](plan.md).

## M0 Foundation

- [ ] Create `FluentMail.slnx` and `FluentMail/FluentMail.csproj` (net10.0 targets, packages from architecture.md)
- [ ] Register Syncfusion in `MauiProgram.cs`
- [ ] Add `Colors.xaml` tokens for light and dark
- [ ] Add `Styles.xaml` (typography, buttons, cards, 44 dp tap targets)
- [ ] Add models: `MailMessage`, `Contact`, `AttachmentInfo`, `FolderNode`, `MailAccount`
- [ ] Add `MockData` with 50 threads, 20 drafts, 30 archived, 15 deleted, 25 contacts, 10 attachments
- [ ] Add `Mailbox` with `Restore`, `Archive`, `Delete`, `Send`, `Save` operations
- [ ] Add `StateView` (loading, empty, error)
- [ ] Add `AppShell` with desktop rail and mobile tabs

## M1 Mail workspace

- [ ] `MailWorkspaceViewModel`: folders, visible messages, selection, filters, search
- [ ] SfTreeView folder tree with SfBadgeView counts
- [ ] SfListView message list with SfAvatarView
- [ ] Reading pane: details, recipient and tag chips, attachment list, body
- [ ] Context menu SfPopup (Archive, Delete, Flag, Mark unread)
- [ ] Filter SfPopup (All, Unread, Flagged, Has files)
- [ ] SfAutocomplete search
- [ ] Wide and Narrow visual states with back navigation
- [ ] Empty, loading, error states

## M2 Mobile mail flows

- [ ] `ArchiveViewModel` with filter logic (All, Has Files, This Year, Flagged)
- [ ] Archive page: chips, cards, file tags, storage card, New Mail FAB
- [ ] `DeletedItemsViewModel` with Restore and Empty trash
- [ ] Deleted page: list, retention card, confirm popup
- [ ] Check that a restored message appears in Inbox and the badge updates

## M3 Compose and drafts

- [ ] `ComposeViewModel`: account, recipients, subject, body, attachments, Send
- [ ] Compose page: account popup, SfAutocomplete, recipient chips, SfRichTextEditor, attachment list
- [ ] Send validation messages (recipient, subject)
- [ ] Snackbar SfPopup after send, message appears in Sent Items
- [ ] `DraftWorkspaceViewModel`: draft list, selection, Resume, save status
- [ ] Draft page: folders tree, draft list, editor
- [ ] Editor changes update the ViewModel and save status

## M4 AI assistant

- [ ] `AiSettings` with placeholder key and TODO comment
- [ ] `AiClient` with three prompts and error mapping
- [ ] `AiAssistantViewModel` with Run, Replace, Discard
- [ ] `AiMenuPopup` with actions, result preview, setup empty state, error and retry
- [ ] Attach the AI menu to compose and draft editors

## M5 Polish and release

- [ ] Light and dark pass on every page
- [ ] Wide and narrow pass on every page
- [ ] Tap target, clipping, and overlap pass
- [ ] Loading, empty, and error states reachable and checked
- [ ] Remove unused code, files, resources, and comments
- [ ] Confirm no secrets in the repository
- [ ] Write `README.md` with all required sections
- [ ] Build with warnings as errors on every target the machine supports
- [ ] Record which targets were not built

## Verification checklist

- [ ] Folder selection changes the message list
- [ ] Message selection fills the reading pane
- [ ] Each context menu action works
- [ ] Each archive filter changes the list
- [ ] Restore puts the message in Inbox
- [ ] Empty trash clears the list and shows the empty state
- [ ] Send rejects no recipient and empty subject
- [ ] Send success shows the snackbar and adds to Sent Items
- [ ] Selecting a draft loads the editor
- [ ] AI shows the setup state without a key
- [ ] AI success, error, and retry work with a key
