# Spec: Fluent Mail

Terms: a **thread** is one email conversation shown as one row in the message list. A **draft** is an unsent message. A **quick filter** is a chip that narrows a list. A **pane** is one column of a screen.

The sample has no AI assistant and makes no network calls.

## 1. Global requirements

- Platforms: Windows and macOS (desktop layout), Android and iOS (mobile layout). Windows and Android are the minimum build targets.
- Themes: light and dark, following the system theme, with a theme resource for every color.
- Style: Fluent 2, Outlook palette (primary blue `#0F6CBD`, neutral gray surfaces, 8 px spacing grid, Segoe UI Variable or the platform default font).
- Tap targets are at least 44 x 44 dp on mobile. Text wraps or truncates with an ellipsis. Controls never overlap.
- Every list has three non-happy states: **loading** (activity indicator while mock data loads), **empty** (icon, message, and a next step), and **error** (message and a Retry command).
- No business logic in code-behind.

## 2. Mock data

Generated in code at startup, deterministic (fixed random seed), realistic enterprise content.

| Data | Count |
|---|---|
| Email threads (Inbox and Sent Items) | 50 |
| Drafts | 20 |
| Archived mails | 30 |
| Deleted mails | 15 |
| Contacts | 25 |
| Attachment records | 10 |

Each message has: sender, recipients, CC list (some messages), subject, preview, body, date, read state, flagged state, tags, and zero or more attachments. Each attachment has a file name, file type, and size.

Sent Items starts with a few messages so that it is not empty. At least one folder category (for example a child of Inbox) is empty so that the empty folder state can be reached.

## 3. Desktop screen 1: Mail workspace

Three columns: folders pane, message list pane, reading pane.

| Area | Control | Behavior |
|---|---|---|
| Folders | SfTreeView | Folders: Inbox, Sent Items, Drafts, Archive, Deleted. Inbox has category children (for example Clients, Projects). Each node shows an SfBadgeView count. |
| Message list | SfListView | Shows sender avatar (SfAvatarView), sender, subject, preview, status, attachment icon. Unread rows are bold. |
| Reading pane | Labels, SfListView, SfChipGroup | Sender avatar, name and address, recipients, CC, tags, subject, attachment list, message body. |

Rules:

- Selecting a folder replaces the message list and clears the previous selection.
- A folder badge shows the number of items in that folder's collection. Badges update after restore, delete, send, and archive.
- Selecting a message fills the reading pane and marks it read.
- Right-click or long-press on a message opens an SfPopup context menu: Archive, Delete, Flag, Mark unread. Each action moves or updates the message.
- A search box using SfAutocomplete suggests senders and subjects and filters the list.
- The toolbar has a filter button that opens an SfPopup with All, Unread, Flagged, Has files.
- Empty folder shows "No messages available" and "Try another folder". No selection shows "Select a message to read".

## 4. Desktop screen 2: Draft workspace

Three columns: draft folders, draft list, editor.

- Draft folders: a short SfTreeView (All drafts, Replies, New messages).
- Draft list: SfListView rows with subject, preview, last modified date, attachment indicator, save status, and a **Resume** button.
- Editor: recipients (SfChipGroup chips plus SfAutocomplete), subject entry, SfRichTextEditor, attachment SfListView.
- Selecting a draft or pressing Resume loads recipients, subject, body, and attachments into the editor.
- Editing the subject, recipients, or body updates ViewModel properties through bindings and sets the save status to "Unsaved Changes". A short pause after the last edit starts a save, shows "Saving...", and then "Draft Saved". The status always matches the ViewModel state.
- The SfRichTextEditor toolbar applies bold, italic, underline, bullet list, numbered list, and hyperlink.
- The draft count is shown with an SfBadgeView on the Drafts folder.

## 5. Mobile screen 1: Archive

- Quick filter chips (SfChipGroup, single choice): **All**, **Has Files**, **This Year**, **Flagged**. Selecting a chip filters the list at once.
- Archive cards in SfListView: SfAvatarView, sender, subject, preview, archived date, file tags (SfChipGroup, display only).
- Storage card: used space, remaining space, and sync status. Used space is the sum of archive attachment sizes. Remaining space is a fixed mock quota minus used space. Sync status is derived from the mock data (for example "Up to date").
- New Mail floating action button navigates to Compose.
- Empty filter result shows "No archived items found".

## 6. Mobile screen 2: Deleted items

- SfListView of deleted mail. Each row shows sender, subject, deleted time, and preview.
- **Restore**: removes the item from Deleted and adds it to the Inbox. Counts update and a snackbar confirms.
- **Empty trash**: asks for confirmation in an SfPopup, then clears the Deleted collection, sets the count to zero, and shows the empty state.
- Retention card is always visible, even when the list is empty. It reads "Messages automatically removed after 30 days."

## 7. Mobile screen 3: Compose mail

- From account selector: SfPopup list of two mock accounts.
- Recipients: SfAutocomplete over the 25 contacts. Suggestions filter as the user types and can be selected. Choosing a contact adds a removable chip (SfChipGroup) and clears the search box. Many recipients are allowed.
- Subject entry.
- SfRichTextEditor with formatting toolbar.
- Attachment list (SfListView) showing file name, file type, and size. The list scrolls when it is long. The user can add from the 10 attachment records and remove.
- **Send** validates in this order:
  1. At least one recipient. Otherwise show "Add at least one recipient."
  2. Subject not empty. Otherwise show "Enter a subject."
  A failed validation does not send.
- On success the message is added to Sent Items, removed from Drafts if it came from a draft, the folder counts update, the form resets, and a snackbar built from SfPopup appears for three seconds: "Message sent".

## 8. Responsive behavior

- Width 900 dp or more, or desktop idiom: multi-column layout with every pane visible and no horizontal scrolling. Narrower: single column with navigation between folders, list, and reading pane.
- Implemented with VisualStateManager states (`Wide`, `Narrow`), adaptive Grid definitions, and `OnIdiom` for sizes and templates.
- Desktop hosts screens 1 and 2 in a left navigation rail (Mail, Drafts). Mobile hosts screens in a Shell tab bar (Archive, Deleted, Compose).
- Each screen also works on the other form factor in its adaptive layout, so the sample runs on any device.
- Portrait and landscape both render without breakage.

## 9. Performance

- Lists use SfListView virtualization with a single `DataTemplate` per list. Templates do not create extra objects per scroll.
- Mail list (50+ items) and archive list (30+ items) scroll smoothly.

## 10. Acceptance criteria

This section is the formal review checklist. The sample is complete only when every criterion passes. Reviewers mark each one with the evidence they used (manual check, build output, or code inspection).

### Epic 1: Mail Workspace

**AC-MAIL-001: Folder navigation.** Given the Mail Workspace is loaded, when the user selects a folder from the mailbox hierarchy, then:

- The selected folder is visually highlighted.
- The message list updates immediately.
- Only messages that belong to the selected folder are shown.
- The previous selection state is cleared.
- No restart or page refresh is needed.
- Inbox shows Inbox messages. Sent Items shows Sent Items messages. Archive shows Archive messages. Deleted shows Deleted messages.

**AC-MAIL-002: Folder counts.** Given folders contain messages, when the workspace loads, then:

- Folder counts are displayed.
- Counts match the underlying collections.
- Counts update automatically after restore, delete, send, or archive.
- Inbox count updates after restore. Sent count updates after send. Archive count updates after archive.

**AC-MAIL-003: Message selection.** Given messages are displayed, when the user selects a message, then:

- The selected card is highlighted.
- The reading pane updates.
- The correct sender, recipients, attachments, and body are shown.

**AC-MAIL-004: Reading pane.** Given a message is selected, the reading pane shows sender avatar, sender name, email address, recipient list, CC list (if available), subject, message body, and attachments. No placeholder text appears.

**AC-MAIL-005: Empty folder state.** Given a folder has zero messages, when it is selected, the page shows "No messages available" and "Try another folder". The page is never blank.

### Epic 2: Draft Workspace

**AC-DRAFT-001: Draft list loading.** Given drafts exist, when the Draft Workspace opens, the list shows subject, preview, last modified date, attachment indicator, and a Resume action.

**AC-DRAFT-002: Load draft.** Given a draft is selected, when the selection changes, the editor loads recipients, subject, body, and attachments. The content matches the selected draft.

**AC-DRAFT-003: Draft editing.** Given a draft is loaded, when content changes, ViewModel properties update. No code-behind logic handles editor synchronization.

**AC-DRAFT-004: Rich formatting.** Given the editor is open, when formatting commands run, formatting is applied correctly. Commands: Bold, Italic, Underline, Bullet List, Numbered List, Hyperlink.

**AC-DRAFT-005: Draft save status.** Given modifications are made, the status shows one of "Draft Saved", "Saving...", or "Unsaved Changes". The status reflects the actual ViewModel state.

### Epic 3: Compose Mail

**AC-COMPOSE-001: Recipient lookup.** Given contacts exist, when the user types in the recipient field, suggestions appear, filter correctly, and can be selected.

**AC-COMPOSE-002: Recipient chips.** Given a contact is selected, the recipient becomes a chip, the search box clears, and multiple recipients are supported.

**AC-COMPOSE-003: Subject validation.** Given Send is invoked with an empty subject, a validation message is shown and the email is not sent.

**AC-COMPOSE-004: Recipient validation.** Given Send is invoked with no recipients, a validation message is shown and the email is not sent.

**AC-COMPOSE-005: Successful send.** Given a recipient and a subject exist, when Send is selected:

- The mail moves to Sent Items.
- The message is removed from Drafts if applicable.
- A success notification is displayed.
- Folder counts update.

**AC-COMPOSE-006: Attachments.** Given attachments are associated, the list shows file name, file type, and file size. The list scrolls correctly.

### Epic 4: Archive

**AC-ARCHIVE-001: Filter chips.** Given archive items exist, when a filter chip is selected, the list updates immediately.

- All shows the entire archive collection.
- Has Files shows only items with attachments.
- This Year shows only current-year items.
- Flagged shows only flagged items.

**AC-ARCHIVE-002: Archive list.** Every card shows avatar, sender, subject, preview, and archived date.

**AC-ARCHIVE-003: Storage card.** The card shows used space, remaining space, and sync status. Values derive from local sample data. No hardcoded placeholders in the UI.

**AC-ARCHIVE-004: Empty archive.** If the filtered result count is zero, the page shows "No archived items found".

### Epic 5: Deleted Items

**AC-DELETED-001: Deleted list.** Each row shows sender, subject, deleted time, and preview.

**AC-DELETED-002: Restore email.** Given a deleted message, when Restore is selected, the message leaves Deleted, returns to Inbox, counts update, and a success snackbar appears.

**AC-DELETED-003: Empty trash.** Given deleted items exist, when Empty Trash is selected, the Deleted collection is cleared, the count becomes zero, and the empty state shows.

**AC-DELETED-004: Retention policy.** The retention card is always visible and shows "Messages automatically removed after 30 days."

### Epic 6: Responsive Layout

**AC-RESP-001: Desktop layout.** Desktop screens use a multi-column arrangement, show all panes at once, and need no horizontal scrolling.

**AC-RESP-002: Mobile layout.** Mobile screens use a single column with no clipped content and no overlapping controls.

**AC-RESP-003: Orientation.** Landscape and portrait both render correctly with no UI breakage.

### Epic 7: Theme Support

**AC-THEME-001: Light theme.** All screens are usable in light mode.

**AC-THEME-002: Dark theme.** All screens are usable in dark mode.

**AC-THEME-003: Contrast.** Interactive elements meet accessibility contrast requirements (4.5:1 for text, 3:1 for control boundaries and icons).

### Epic 8: Performance

**AC-PERF-001: Mail list scrolling.** Scrolling stays smooth with 50+ mails.

**AC-PERF-002: Archive list scrolling.** Scrolling stays smooth with 30+ records.

**AC-PERF-003: Memory usage.** No excessive object creation while scrolling. The lists rely on SfListView virtualization.

### Epic 9: MVVM Compliance

**AC-MVVM-001.** Business logic does not exist in code-behind.

**AC-MVVM-002.** Commands are used for Send, Restore, Filter, Folder Selection, and Draft Selection.

**AC-MVVM-003.** UI state is bound to ViewModel properties.

### Epic 10: Sample Quality Review

**AC-SAMPLE-001.** No placeholder buttons. Every visible action performs a meaningful operation.

**AC-SAMPLE-002.** No dead code.

**AC-SAMPLE-003.** No unused files.

**AC-SAMPLE-004.** No build warnings.

**AC-SAMPLE-005.** The sample builds on Windows and Android (minimum validation targets).

**AC-SAMPLE-006.** The README includes purpose, controls demonstrated, architecture overview, and running instructions.

### Final customer validation

Before closing the sample, reviewers answer these questions. Every answer must be Yes.

| Question | Expected answer |
|---|---|
| Does TreeView clearly demonstrate folder navigation? | Yes |
| Does ListView clearly demonstrate mail virtualization and templates? | Yes |
| Does RichTextEditor clearly demonstrate email composition? | Yes |
| Does Autocomplete clearly demonstrate recipient lookup? | Yes |
| Does ChipGroup clearly demonstrate filters and recipient chips? | Yes |
| Does the sample look production-quality? | Yes |
| Would a customer use this as a reference implementation? | Yes |

## 11. Additional completion requirements

- No hardcoded secrets. No commented-out code.
- Loading, empty, and error states are reachable and checked.
- The README also covers features, responsive behavior, and known limitations.
