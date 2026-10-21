# Harness Document Index — CANONICALITY

**Canonical (authoritative) harness documents — implement from these only:**

| Document | Role |
|---|---|
| `spec.md` | Product specification: scope, user journeys, FR-1…FR-12, NFRs, menu/toolbar spec |
| `architecture.md` | Architecture: layering, DI, SfDiagram integration pattern, command routing, navigation |
| `ui-spec.md` | UI specification derived from `design-images/` mockups: layout, menus, toolbar, panels, theme |
| `tasks.md` | Task breakdown (Tasks 1–10) with objectives, requirements, impacted files, acceptance criteria, validation steps |
| `validation.md` | Validation checklist to run per task and fully after Task 10 |

**Earlier working drafts (00–08 numbered files):** superseded by the canonical documents
above. They are retained only for provenance. Where any numbered draft disagrees with a
canonical document, the canonical document wins. Known superseded statements include:
- `01-architecture.md` refers to a `WindowViewModel` for panel toggles — superseded by
  `architecture.md` (`DiagramDesignerViewModel.ShowStencil/ShowProperties/ShowToolbar`).
- `05-design-guidelines.md` / `02-project-structure.md` mention a third `DiagramTheme.xaml`
  dictionary — superseded by `tasks.md` Task 1 (merge `Colors.xaml` + `Styles.xaml` only).
- `08-implementation-tasks.md` checkbox states do not reflect actual repo state (only
  `Constants/` exists; Models/Services/ViewModels/Views/Controls are unimplemented).

**Authoritative external sources (do not invent beyond these):**
1. `diagram-control-specs.md` — SfDiagram/SymbolPalette architecture, APIs, events, constraints.
2. `design-images/*.png` — visual ground truth for all screens.
3. `.agents/skills/` + `.claude/skills/` — available Syncfusion MAUI control skills.
4. Syncfusion Diagram help: https://help.syncfusion.com/diagram-sdk/maui/overview
