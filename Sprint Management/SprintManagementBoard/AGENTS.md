# Sprint Management Board – agent harness

Read first: `docs/revamp/INTENT.md`, `SPEC.md`, `DESIGN.md`, `PLAN.md`, `TASKS.md`.

## Rules
- UI changes = restyle only; do not add screens/features.
- One `MainPage.xaml`. Platform differences ONLY via `OnIdiom` (or `OnPlatform` for tiny OS quirks). Never fork pages or visual trees.
- No hard-coded colours in pages; use tokens in `Resources/Styles/Colors.xaml` with `AppThemeBinding` and styles from `AppStyles.xaml`.
- Every Syncfusion control must look right in Light and Dark (`App.ApplySyncfusionTheme`).
- Use the Syncfusion skills (kanban, datagrid, scheduler, theming) for API questions.

## Verify (run before finishing any change)
`powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`
Then tick items in `docs/revamp/TASKS.md`.
