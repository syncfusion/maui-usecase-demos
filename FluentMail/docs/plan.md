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
- Narrow layout with pane navigation.

Done when: folder, message, and menu actions all work, and unread counts update.

## Milestone 2: Mobile mail flows

- Archive page with quick filter chips, cards, storage card, and FAB.
- Deleted items page with Restore, Empty trash, and retention card.
- Verify that Restore adds the message to Inbox.

Done when: every filter changes the list and Restore is visible in the Inbox.

## Milestone 3: Compose and drafts

- Compose page: account selector, recipient autocomplete and chips, subject, rich text editor, attachments, Send validation, snackbar.
- Draft workspace: draft folders, draft list with Resume and save status, editor reusing the compose controls.

Done when: Send validation messages and success snackbar work, and editing a draft updates the ViewModel and save status.

## Milestone 4: AI assistant

- `AiSettings`, `AiClient`, `AiAssistantViewModel`, `AiMenuPopup`.
- Setup empty state, loading, error with retry, result preview with Replace and Discard.

Done when: without a key the setup state shows; with a valid key all three actions return text and Replace works.

## Milestone 5: Polish and release

- Check light and dark themes, wide and narrow layouts, tap target sizes, and text clipping.
- Check loading, empty, and error states on every list.
- Remove unused code, files, and resources.
- Write the README.
- Run the final build with warnings treated as errors.

Done when: every item in the acceptance criteria in [spec.md](spec.md) passes.

## Risks

| Risk | Response |
|---|---|
| A Syncfusion package name or API differs from what is assumed | Check the Syncfusion docs for the installed version before using a control |
| SfRichTextEditor behaves differently per platform | Test on one desktop and one mobile target early, in Milestone 3 |
| Windows or Apple targets cannot build on the current machine | State which targets were built and which were not |
| No snackbar control exists | Build it from SfPopup, as the spec says |
| Real AI key unavailable for testing | Test the error and setup states, and test the success path with a stubbed `HttpMessageHandler` if tests are added |
