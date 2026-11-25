# Intent: Sprint Board UI Revamp

## Why
The Sprint Management sample is a showcase for Syncfusion .NET MAUI controls (Kanban, Scheduler, DataGrid, ComboBox).
The old UI looked like a prototype: pastel flat cards, a green sidebar of raw PNG icons, white hard-coded surfaces
(unusable in dark mode), and two duplicated visual trees (`DesktopRoot` / `MobileRoot`) with ~30 duplicated named controls.

## Goal
When someone opens the sample it should feel like a real product (Linear / Jira-style board) on every platform
and in both Light and Dark mode.

## Non-goals (hard constraints from the request)
- **No new UI / features.** Same screens: header, 4 summary cards, filter bar, Kanban / Scheduler / DataGrid, New-Task popup, date-range popup.
- No change to models, service, or data flow (one tiny additive VM property: `HasRemainingAvatars`).
- No separate Phone/Desktop pages: **one page, `OnIdiom` for layout differences.**

## Success criteria
1. One `MainPage.xaml`, zero duplicated named controls.
2. Colours come only from theme tokens (`AppThemeBinding`); no hard-coded white/grey surfaces.
3. Syncfusion controls follow system Light/Dark at runtime.
4. Layout correct for Phone, Tablet, Desktop (Windows, macCatalyst) via `OnIdiom`.
5. Build is green; app launches.

## Guiding principles
Token-first styling, one source of truth, idiom-driven layout, accessibility (44px targets, SemanticProperties, contrast).
