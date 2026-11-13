# Intent: Fluent Mail

## Purpose

Fluent Mail is a showcase sample for Syncfusion .NET MAUI. It shows eight Syncfusion controls working together in one realistic email workflow, so a customer can learn how to use them in their own app.

## Who it is for

A .NET MAUI developer who is evaluating Syncfusion controls. They open the sample, run it, and within a few minutes understand which control does what and how to copy the pattern.

## What success looks like

- The customer can name each control on screen and find its XAML and ViewModel quickly.
- Every visible button and filter does something real.
- The look matches Microsoft Outlook: Fluent 2 styling, Outlook blue, light and dark themes.
- The code is small enough to read in one sitting.

## Principles

1. **Showcase, not product.** The sample teaches. It does not try to be a mail client.
2. **Syncfusion controls first.** Use a Syncfusion control wherever one exists. Use plain MAUI layouts (Grid, VerticalStackLayout, Border, Label) only to arrange them.
3. **Local and self-contained.** All data is mock data in memory. There are no network calls.
4. **Clarity over abstraction.** Each page has one ViewModel. No dependency injection container, no app-wide state, no service layers.
5. **No fake UI.** If a control is visible, its action works. If a feature cannot work, leave it out.
6. **Measurable completion.** The acceptance criteria in spec.md section 10 are the review checklist. The sample is done only when every one passes.

## Non-goals

Authentication, user accounts, cloud sync, backend APIs, remote databases, AI features, splash screens, onboarding, dependency injection, app-wide state management.

## Controls demonstrated

| Control | Role in the sample |
|---|---|
| SfTreeView | Mailbox folders and categories |
| SfListView | Messages, drafts, archive cards, attachments |
| SfRichTextEditor | Draft and compose editor |
| SfAutocomplete | Recipient lookup and search suggestions |
| SfChipGroup | Recipient chips, tags, quick filters, status |
| SfAvatarView | Sender avatars and the user profile |
| SfBadgeView | Unread, draft and folder counts |
| SfPopup | Context menus, filter menu, confirmation, snackbar |

## Related documents

- [spec.md](spec.md): what each screen must do.
- [architecture.md](architecture.md): how the code is organized.
- [plan.md](plan.md): the order of work in milestones.
- [tasks.md](tasks.md): the checklist for each milestone.
