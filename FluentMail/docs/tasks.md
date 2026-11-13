# Tasks: Fluent Mail

Mark each task when it is done and verified. Milestones match [plan.md](plan.md). Acceptance criteria IDs refer to section 10 of [spec.md](spec.md).

## M0 Foundation

- [ ] Create `FluentMail.slnx` and `FluentMail/FluentMail.csproj` (net10.0 targets, packages from architecture.md)
- [ ] Register Syncfusion in `MauiProgram.cs`
- [ ] Add `Colors.xaml` tokens for light and dark (check contrast pairs, AC-THEME-003)
- [ ] Add `Styles.xaml` (typography, buttons, cards, 44 dp tap targets)
- [ ] Add models: `MailMessage` (with CC and attachments), `Contact`, `AttachmentInfo` (name, type, size), `FolderNode`, `MailAccount`
- [ ] Add `MockData` with 50 threads, 20 drafts, 30 archived, 15 deleted, 25 contacts, 10 attachments, and an empty category folder
- [ ] Add `Mailbox` with `Restore`, `Archive`, `Delete`, `Send`, `Save` operations
- [ ] Add `StateView` (loading, empty, error)
- [ ] Add `AppShell` with desktop rail and mobile tabs

## M1 Mail workspace

- [ ] `MailWorkspaceViewModel`: folders, visible messages, selection, filters, search, `SelectFolderCommand`
- [ ] SfTreeView folder tree with highlight and SfBadgeView counts (AC-MAIL-001, AC-MAIL-002)
- [ ] SfListView message list with SfAvatarView and selected-card highlight (AC-MAIL-003)
- [ ] Reading pane: avatar, name, address, recipients, CC, tags, subject, attachments, body (AC-MAIL-004)
- [ ] Folder empty state: "No messages available" and "Try another folder" (AC-MAIL-005)
- [ ] Context menu SfPopup (Archive, Delete, Flag, Mark unread)
- [ ] Filter SfPopup (All, Unread, Flagged, Has files)
- [ ] SfAutocomplete search
- [ ] Wide and Narrow visual states with back navigation
- [ ] Loading and error states

## M2 Mobile mail flows

- [ ] `ArchiveViewModel` with filter logic and `FilterCommand` (AC-ARCHIVE-001)
- [ ] Archive page: chips, cards with avatar, sender, subject, preview, archived date, file tags (AC-ARCHIVE-002)
- [ ] Storage card from sample data: used, remaining, sync status (AC-ARCHIVE-003)
- [ ] Empty filter result: "No archived items found" (AC-ARCHIVE-004)
- [ ] New Mail FAB opens Compose
- [ ] `DeletedItemsViewModel` with `RestoreCommand` and `EmptyTrashCommand`
- [ ] Deleted page: rows with sender, subject, deleted time, preview (AC-DELETED-001)
- [ ] Restore moves the item to Inbox, counts update, snackbar shows (AC-DELETED-002)
- [ ] Empty trash with confirm popup, count zero, empty state (AC-DELETED-003)
- [ ] Retention card always visible with the exact policy text (AC-DELETED-004)

## M3 Compose and drafts

- [ ] `ComposeViewModel`: account, recipients, subject, body, attachments, `SendCommand`
- [ ] Compose page: account popup, SfAutocomplete, recipient chips, SfRichTextEditor, attachment list (AC-COMPOSE-001, 002, 006)
- [ ] Clear the search box after a contact is selected
- [ ] Send validation messages for recipient and subject (AC-COMPOSE-003, 004)
- [ ] Send success: Sent Items updated, draft removed, counts updated, snackbar for three seconds (AC-COMPOSE-005)
- [ ] `DraftWorkspaceViewModel`: draft list, `SelectDraftCommand`, Resume, `SaveStatus`
- [ ] Draft page: folders tree, list with date and attachment indicator, editor (AC-DRAFT-001, 002)
- [ ] Editor bound two-way to the ViewModel with no code-behind sync (AC-DRAFT-003)
- [ ] Toolbar shows Bold, Italic, Underline, Bullet List, Numbered List, Hyperlink (AC-DRAFT-004)
- [ ] Save status: Draft Saved, Saving..., Unsaved Changes (AC-DRAFT-005)

## M4 Polish and release

- [ ] Light and dark pass on every page (AC-THEME-001, 002)
- [ ] Wide and narrow pass on every page, portrait and landscape (AC-RESP-001, 002, 003)
- [ ] Tap target, clipping, and overlap pass
- [ ] Scrolling check with the 50-item mail list and 30-item archive (AC-PERF-001, 002, 003)
- [ ] Code-behind review: only `InitializeComponent` (AC-MVVM-001, 002, 003)
- [ ] Loading, empty, and error states reachable and checked
- [ ] Remove unused code, files, resources, and comments (AC-SAMPLE-002, 003)
- [ ] Confirm every visible action works (AC-SAMPLE-001)
- [ ] Confirm no secrets in the repository
- [ ] Write `README.md`: purpose, controls demonstrated, features, architecture overview, setup, how to run, responsive behavior, known limitations (AC-SAMPLE-006)
- [ ] Build with warnings as errors on Windows and Android (AC-SAMPLE-004, 005)
- [ ] Record which targets were not built
- [ ] Answer the final customer validation questions

## Review checklist

Copy the criteria from spec.md section 10 into the review. Each criterion needs a pass mark and its evidence. The sample is complete only when all pass.

| Epic | Criteria | Passed | Evidence |
|---|---|---|---|
| Mail Workspace | AC-MAIL-001 to 005 | [ ] | |
| Draft Workspace | AC-DRAFT-001 to 005 | [ ] | |
| Compose Mail | AC-COMPOSE-001 to 006 | [ ] | |
| Archive | AC-ARCHIVE-001 to 004 | [ ] | |
| Deleted Items | AC-DELETED-001 to 004 | [ ] | |
| Responsive Layout | AC-RESP-001 to 003 | [ ] | |
| Theme Support | AC-THEME-001 to 003 | [ ] | |
| Performance | AC-PERF-001 to 003 | [ ] | |
| MVVM Compliance | AC-MVVM-001 to 003 | [ ] | |
| Sample Quality | AC-SAMPLE-001 to 006 | [ ] | |
| Final customer validation | 7 questions, all Yes | [ ] | |
