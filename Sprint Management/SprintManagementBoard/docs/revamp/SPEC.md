# Spec: Sprint Board UI Revamp

## Scope
Restyle and re-lay-out the existing `MainPage`. Functionality is preserved.

## Functional requirements (unchanged behaviour)
| ID | Behaviour |
|----|-----------|
| F1 | Switch between Kanban, Scheduler (timeline), DataGrid via navigation |
| F2 | Project ComboBox filters all three views |
| F3 | Date-range filter popup (Apply / Cancel); *Clear* added as a reset for the same filter |
| F4 | Summary counts bound to VM (To Do, In Progress, Testing, Done) |
| F5 | "+ New Task" popup: title, description, assignee, status, dates, progress; saves via `ITaskService` |
| F6 | Avatar stack of assignees with "+N" overflow |

## Visual requirements
| ID | Requirement |
|----|-------------|
| V1 | Semantic palette tokens for Light and Dark (see DESIGN.md) |
| V2 | Cards, fields, popups use shared styles `CardBorder`, `FieldBorder`, `PrimaryAction`, `SecondaryAction` |
| V3 | Vector nav icons (`Path`) replace PNG icons so they tint for active/idle |
| V4 | Syncfusion theme switches `MaterialLight` / `MaterialDark` with system theme, plus header/column colour overrides |
| V5 | Priority shown as coloured badge in DataGrid; dates formatted `dd MMM yyyy` |
| V6 | Kanban cards: rounded, status accent bar, tag chips, assignee avatar |

## Platform / layout requirements (all via `OnIdiom`)
| Element | Phone | Tablet | Desktop |
|---------|-------|--------|---------|
| Navigation | bottom bar (3 equal tabs) | left rail 84px | left rail 84px |
| Page padding | 16 | 24 | 32 |
| Summary cards | 2 x 2 grid | 1 x 4 | 1 x 4 |
| Filter bar | project on row 1; avatars + date chip row 2 | single row | single row |
| Header subtitle | hidden | shown | shown |
| Board height | 520 | 600 | 640 |
| Popups | full-width with 16 margin | max 460/560 centred | max 460/560 centred |
| Kanban min column | 270 | 230 | 230 (host scrolls horizontally if narrower) |

## Constraints
- Single `MainPage.xaml`; remove `DesktopRoot`/`MobileRoot`, `Mobile_*` handlers/controls.
- Keep Syncfusion packages and licence registration.
- No new NuGet packages.

## Acceptance
Build `net10.0-windows` succeeds with 0 warnings/errors; app launches; manual matrix in TASKS.md (T9) passes.
