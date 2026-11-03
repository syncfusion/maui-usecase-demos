# Spec: Fluent Mail

Terms: a **thread** is one email conversation shown as one row in the message list. A **draft** is an unsent message. A **quick filter** is a chip that narrows a list.

## 1. Global requirements

- Platforms: Windows and macOS (desktop layout), Android and iOS (mobile layout).
- Themes: light and dark, following the system theme, with theme resources for every color.
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

Each message has: sender, recipients, subject, preview, body, date, read state, flagged state, tags, and zero or more attachments.

## 3. Desktop screen 1: Mail workspace

Three columns: folders pane, message list pane, reading pane.

| Area | Control | Behavior |
|---|---|---|
| Folders | SfTreeView | Folders: Inbox, Sent Items, Drafts, Archive, Deleted. Inbox has category children (for example Clients, Projects). Each node shows a SfBadgeView count. |
| Message list | SfListView | Shows sender avatar (SfAvatarView), sender, subject, preview, status, attachment icon. Unread rows are bold. |
| Reading pane | Labels, SfListView, SfChipGroup | Sender details, recipients, tags, attachment list, message body. |

Rules:

- Selecting a folder replaces the message list.
- Selecting a message fills the reading pane and marks it read. The badge counts update.
- Right-click or long-press on a message opens an SfPopup context menu: Archive, Delete, Flag, Mark unread. Each action moves or updates the message.
- A search box using SfAutocomplete suggests senders and subjects and filters the list.
- The toolbar has a filter button that opens an SfPopup with All, Unread, Flagged, Has files.
- Empty folder shows an empty state. No selection shows "Select a message to read".

## 4. Desktop screen 2: Draft workspace

Three columns: draft folders, draft list, editor.

- Draft folders: a short SfTreeView (All drafts, Replies, New messages).
- Draft list: SfListView rows with subject, preview, save status ("Saved", "Saving...", "Not saved"), and a **Resume** button.
- Editor: recipients (SfChipGroup chips plus SfAutocomplete), subject entry, SfRichTextEditor, attachment SfListView.
- Selecting a draft or pressing Resume loads it into the editor.
- Editing the subject, recipients, or body updates the ViewModel and sets the save status to "Not saved". A Save button sets it to "Saved" and refreshes the preview.
- Formatting works through the SfRichTextEditor toolbar: bold, italic, underline, lists, alignment, links.
- Draft count shown with SfBadgeView on the Drafts folder.

## 5. Mobile screen 1: Archive

- Quick filter chips (SfChipGroup, single choice): **All**, **Has Files**, **This Year**, **Flagged**. Selecting a chip filters the list.
- Archive cards in SfListView: SfAvatarView, sender, subject, preview, file tags (SfChipGroup, display only).
- Storage information card: used and total size computed from the archive attachment sizes.
- New Mail floating action button navigates to Compose.
- Empty state when a filter matches nothing.

## 6. Mobile screen 2: Deleted items

- SfListView of deleted mail.
- **Restore**: moves the item to the Inbox collection. The Inbox unread badge updates. The item appears in the Inbox list.
- **Empty trash**: asks for confirmation in an SfPopup, then clears the deleted collection and shows the empty state.
- Retention policy card: text stating that items are kept for 30 days, with each item showing days left.

## 7. Mobile screen 3: Compose mail

- From account selector: SfPopup list of two mock accounts.
- Recipients: SfAutocomplete over the 25 contacts. Choosing a contact adds a removable chip (SfChipGroup).
- Subject entry.
- SfRichTextEditor with formatting toolbar.
- Attachment list (SfListView) with add (picks from the 10 attachment records) and remove.
- **Send** validates in this order:
  1. At least one recipient. Otherwise show "Add at least one recipient."
  2. Subject not empty. Otherwise show "Enter a subject."
- On success the message is added to Sent Items, the form resets, and a snackbar built from SfPopup appears for three seconds: "Message sent".

## 8. AI assistant

An SfPopup menu on the compose and draft editors with three actions: **Refine Tone**, **Summarize**, **Professional Rewrite**.

- The editor body is sent to the OpenAI chat completions endpoint with a prompt for the chosen action.
- The result is shown in an SfPopup preview with **Replace** and **Discard** buttons. Replace writes it into the editor.
- While waiting, a loading indicator shows and the actions are disabled.
- Failure (network, 401, 429, timeout) shows an error state with Retry.
- If the key equals the placeholder `YOUR_API_KEY_HERE` or is empty, the popup shows an empty state: what is missing, which file to edit, and a link to the README section.
- Source line in `AiSettings.cs`:

  ```csharp
  // TODO: Add your OpenAI API key before running.
  public const string ApiKey = "YOUR_API_KEY_HERE";
  ```

## 9. Responsive behavior

- Width 900 dp or more, or desktop idiom: multi-column layout. Narrower: single column with navigation between folders, list, and reading pane.
- Implemented with VisualStateManager states (`Wide`, `Narrow`), adaptive Grid definitions, and `OnIdiom` for sizes and templates.
- Desktop hosts screens 1 and 2 in a left navigation rail (Mail, Drafts). Mobile hosts screens in a Shell tab bar (Archive, Deleted, Compose).
- Each screen also works on the other form factor in its adaptive layout, so the sample runs on any device.

## 10. Acceptance criteria

- Builds with zero errors and zero warnings on the supported targets.
- Every action listed above works.
- No hardcoded secrets. No dead files, unused code, or commented-out code.
- Light and dark themes checked on both layouts.
- Loading, empty, and error states reachable and checked.
- README contains: overview, controls demonstrated, features, setup, AI setup, how to run, responsive behavior, known limitations.
