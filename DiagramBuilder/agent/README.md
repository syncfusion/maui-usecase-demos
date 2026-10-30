# Diagram Builder — Agent Documentation Index

This folder contains the complete, canonical documentation set for building the
Diagram Builder sample. Every document below is authoritative — there are no
superseded drafts.

| Document | Responsibility |
|---|---|
| `spec.md` | Product specification: scope, user journeys J1–J5, functional requirements FR-1…FR-12, non-functional requirements, menu/toolbar specification, mock-data strategy, success criteria |
| `architecture.md` | Architecture: layering, DI map, MVVM rules, SfDiagram integration pattern (behavior + adapter), selection/property data flow, command routing, navigation, project structure, verified SfDiagram API notes, known control constraints |
| `ui-spec.md` | UI specification derived from `design-images/` mockups: window frame, menus, toolbar, stencil, canvas, property panel, dialogs, theme palette, accessibility requirements |
| `tasks.md` | Task breakdown (Tasks 1–10) with objective, implementation requirements, impacted files, acceptance criteria, and validation steps per task |
| `validation.md` | Validation checklist (11 sections) to run after each task (relevant subset) and fully after Task 10 |

**Authoritative external sources:**
1. `diagram-control-specs.md` (repo root) — SfDiagram/SymbolPalette architecture, APIs, events, constraints.
2. `design-images/*.png` — visual ground truth for all screens.
3. `.agents/skills/` + `.claude/skills/` — available Syncfusion MAUI control skills (no Diagram skill exists; `diagram-control-specs.md` is the authoritative Diagram reference).
4. Syncfusion Diagram help: https://help.syncfusion.com/diagram-sdk/maui/overview

**How to build the sample:** read `spec.md` first for the product definition, then
`architecture.md` and `ui-spec.md` for the technical and visual contracts, then execute
`tasks.md` Tasks 1–10 sequentially, running the relevant `validation.md` section after
each task and the full checklist after Task 10.

## Document conventions

- "Mock" means an in-memory implementation surfaced through a service interface — no
  backend, file system, or network is ever involved.
- Requirements are uniquely identified (FR-n) and every task in `tasks.md` traces back
  to them; the acceptance criteria of Task 10 are the release gate.
- The SfDiagram API statements in `architecture.md` §7 were verified against
  `Syncfusion.Maui.Diagram` **35.1.41** and take precedence over any conflicting
  recollection when writing control-facing code.
