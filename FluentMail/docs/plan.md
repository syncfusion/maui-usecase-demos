# Plan: Fluent Mail

Work is split into milestones. Each milestone leaves the app building and running with no warnings. Each ends with a review before the next begins.

## Milestone 0: Foundation

- Create the solution and project with the repository's target frameworks and package versions.
- Add Fluent color tokens, light and dark, and base styles.
- Add models, `MockData`, and `Mailbox` with the required counts.
- Add `StateView` and a Shell that switches between rail and tabs.

Done when: the app launches on each target and shows empty pages in both themes.

## Milestone 1: Mail workspace (desktop screen 1)

- `SfTreeView` with folder badges.
- `SfListView` message list with avatars and unread style.
- Reading pane with chips and attachments.
- Context menu popup, filter popup, and search autocomplete.
- Folder counts that follow the collections.
- Narrow layout with pane navigation.

Done when: folder, message, and menu actions all work, and unread counts update.

## Milestone 2: Mobile mail flows

- Archive page with quick filter chips, cards, storage card, and FAB.
- Deleted items page with Restore, Empty trash, and retention card.
- Verify that Restore adds the message to Inbox.

Done when: every filter changes the list and Restore is visible in the Inbox.

## Milestone 3: Compose and drafts

- Compose page: account selector, recipient autocomplete and chips, subject, rich text editor, attachments, Send validation, snackbar.
- Draft workspace: draft folders, draft list with Resume and save status (Draft Saved, Saving..., Unsaved Changes), editor reusing the compose controls.

Done when: Send validation messages and success snackbar work, and editing a draft updates the ViewModel and save status.

## Milestone 4: Polish and release

- Check light and dark themes, wide and narrow layouts, tap target sizes, and text clipping.
- Check loading, empty, and error states on every list.
- Remove unused code, files, and resources.
- Write the README.
- Run the final build with warnings treated as errors.

- Review every criterion in spec.md section 10 and record the evidence.

Done when: every acceptance criterion and every final customer validation answer passes.

## Risks

| Risk | Response |
|---|---|
| A Syncfusion package name or API differs from what is assumed | Check the Syncfusion docs for the installed version before using a control |
| SfRichTextEditor behaves differently per platform | Test on one desktop and one mobile target early, in Milestone 3 |
| Windows or Apple targets cannot build on the current machine (Windows and Android are the required targets) | State which targets were built and which were not |
| No snackbar control exists | Build it from SfPopup, as the spec says |
| Hyperlink or list commands differ in the SfRichTextEditor toolbar | Check the toolbar items in the Syncfusion docs and confirm each of the six commands works |
| Contrast fails in dark theme | Check token pairs when defining `Colors.xaml` |
