# Plan: Sprint Board UI Revamp

Incremental, each step leaves the solution building.

1. **Foundation** – add colour tokens, `AppStyles.xaml`, merge in `App.xaml`; update default `Page` background. (Build)
2. **Theme engine** – `App.ApplySyncfusionTheme()` for Light/Dark + Syncfusion overrides. (Build)
3. **Page rewrite** – merge Desktop+Mobile trees into one `MainPage.xaml` using `OnIdiom` for nav placement, paddings, card grid, filter bar, popup sizing.
4. **Code-behind simplification** – delete all `Mobile_*` handlers and `FindByName` boilerplate; use generated fields; single view switch + nav highlight.
5. **Data views polish** – Kanban card template, DataGrid date/priority columns, horizontal Kanban host with min width.
6. **Small support code** – `PriorityToColorConverter`, `HasRemainingAvatars` VM property.
7. **Verify** – `scripts/verify.ps1` (build), launch smoke test, manual theme/idiom matrix.
8. **Follow-ups** – on-device screenshots on Android/iOS/Mac (needs those toolchains).

## Risks
| Risk | Mitigation |
|------|-----------|
| `OnIdiom` on Grid.Row/Column not evaluated at runtime on some platforms | Standard MAUI markup extension; verify on Phone emulator (T9) |
| Syncfusion colours stay light in dark mode | Theme dictionary swap + overrides, verify visually |
| Kanban inside horizontal ScrollView sizing | `SizeChanged` sets min width = 4 × column width |
| Windows reports Idiom=Desktop; narrow windows | Kanban/DataGrid scroll horizontally; summary cards are `*` columns |
