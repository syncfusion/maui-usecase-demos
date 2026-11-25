# Tasks

Legend: [x] done and verified, [ ] open (needs a device/toolchain not available here)

- [x] T1 Colour tokens (Light/Dark) in `Colors.xaml`; brand primary updated
- [x] T2 `AppStyles.xaml` shared styles, merged in `App.xaml`
- [x] T3 `App.ApplySyncfusionTheme()` + `RequestedThemeChanged` hook (Kanban, Scheduler, ComboBox keys)
- [x] T4 Single `MainPage.xaml` (removed DesktopRoot/MobileRoot duplication)
- [x] T5 `OnIdiom` layout: nav rail / bottom bar, paddings, KPI grid 4 / 2x2, filter bar, popup sizing, board height
- [x] T6 Vector nav icons with active/idle state
- [x] T7 Kanban card template; columns fit host width (`KanbanHost_SizeChanged`); DataGrid fixed columns, `dd MMM yy`, priority badge
- [x] T8 Code-behind cleanup, `PriorityToColorConverter`, `HasRemainingAvatars`, fixed scheduler palette
- [x] T9a Windows build 0 warnings / 0 errors; Android build 0 / 0
- [x] T9b Windows app launches (smoke) – `scripts/verify.ps1`
- [x] T9c Windows Light + Dark verified on all views (board, timeline, grid, new-task popup) – `scripts/shots.ps1`
- [x] T9d Android phone (Pixel 7 emulator) Light + Dark – `scripts/android-shots.ps1`
- [ ] T9e iOS / MacCatalyst visual check (needs a Mac)
- [ ] T9f Android tablet + landscape check
- [ ] T10 Optional: per-user avatars/initials instead of `user.png`
- [ ] T11 Optional: Android bottom nav should extend under the gesture bar (safe-area)

## Screenshots
`docs/revamp/shot-win-{light,dark}-{board,scheduler,datagrid,popup}.png`, `shot-android-phone-{light,dark}.png`

## Dev aids
- `SPRINT_START_VIEW=scheduler|datagrid|popup` (Debug + Windows only) opens that view at startup, used by `scripts/shot.ps1`.
