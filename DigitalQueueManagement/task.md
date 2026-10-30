# task.md — Digital Queue & Counter Management (Task Checklist)

> Harness-planning document 4 of 4. Companion documents: [`indent.md`](indent.md), [`specs.md`](specs.md), [`plan.md`](plan.md).
>
> The [`plan.md`](plan.md) phases turned into ordered, actionable tasks with stable IDs. **Dependencies are explicit** (the `Depends on` field) and are not implied by ordering alone. Each task has a concrete outcome and a verification/completion check. The coverage matrix at the end maps every acceptance criterion in [`specs.md`](specs.md) to task(s).

## Legend

- Status: `[ ]` todo · `[~]` in progress · `[x]` done.
- `Depends on: —` means no prerequisite.
- Task IDs are stable; do not renumber. New tasks append with the next free number in their group.

---

## Group AMB — Resolve documented ambiguities (Phase 0, decision gate)

> Per [`docs/prompt.md`](docs/prompt.md), these must be resolved/confirmed **before** the dependent implementation tasks noted in each item.

- [ ] **T-AMB-01 — Decide navigation model (OQ-1).**
  - Depends on: —
  - Do: Compare [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §11 (Left Nav Rail / 4 bottom-nav items) vs. [`docs/Design Flow/Mobile-desktop-screens.md`](docs/Design%20Flow/Mobile-desktop-screens.md) + desktop/mobile `code.html` + interaction mappings + overview §15 (two-screen Overview+Manage/segmented). Record the chosen model and rationale.
  - Outcome: One confirmed navigation model documented.
  - Verify: Decision recorded; [`specs.md`](specs.md) §6.1 assumption A-1 marked confirmed/revised.
  - Blocks: T-NAV-01, T-NAV-02.

- [ ] **T-AMB-02 — Confirm Token Details conditional actions (OQ-3).**
  - Depends on: —
  - Do: Decide visibility rules (proposed: Recall when Skipped, Transfer when Serving). Record.
  - Outcome: Rule table for Token Details secondary actions.
  - Verify: Decision recorded; drives T-SHEET-02.
  - Blocks: T-SHEET-02.

- [ ] **T-AMB-03 — Fix canonical sample dataset & labels (OQ-2, OQ-4).**
  - Depends on: —
  - Do: Define one internally consistent dataset (20–30 tokens, status counts, counter assignments, upcoming order) and canonical category/counter labels (per overview §4); note mock sub-labels are cosmetic.
  - Outcome: Canonical dataset spec + label list.
  - Verify: Dataset self-consistent; used by T-DOM-03.
  - Blocks: T-DOM-03.

- [ ] **T-AMB-04 — Decide Call-Next ordering tie-break (OQ-7).**
  - Depends on: —
  - Do: Choose strict FIFO vs. priority-weighted "first available Waiting". Record.
  - Outcome: Ordering rule.
  - Verify: Decision recorded; drives T-OPS-01.
  - Blocks: T-OPS-01.

---

## Group SETUP — Project setup & Syncfusion integration (Phase 1)

- [ ] **T-SETUP-01 — Scaffold MAUI project + solution.**
  - Depends on: T-AMB-01
  - Do: Create `DigitalQueueManagement/<ProjectName>/` project, `.slnx`, `README.md`; net10.0 target set; `Models/ Services/ ViewModels/ Views/DesktopUI/ Views/MobileUI/ Resources/` folders, mirroring `HotelBookingManagement/HotelManagerSample`.
  - Outcome: Buildable empty MAUI app with no network/db/API references.
  - Verify: `dotnet build` succeeds; layout matches reference. (AC-39, AC-40)

- [ ] **T-SETUP-02 — Add & configure Syncfusion.**
  - Depends on: T-SETUP-01
  - Do: Add packages (Core, Toolkit, DataGrid, ListView, Charts, Gauges, Buttons, Popup); `ConfigureSyncfusionCore()`/`ConfigureSyncfusionToolkit()`; register license; add fonts (Inter).
  - Outcome: Syncfusion initialized; a sample control renders.
  - Verify: App launches with a Syncfusion control visible. (partial AC-38)

- [ ] **T-SETUP-03 — DI container & app shell stubs.**
  - Depends on: T-SETUP-02
  - Do: Register `DemoDataService`, `QueueService`, and view-models in DI; create empty DesktopUI/MobileUI page stubs.
  - Outcome: Services resolvable; stub pages navigable.
  - Verify: App resolves services at startup. (AC-40)

- [ ] **T-SETUP-04 — Theme resources.**
  - Depends on: T-SETUP-02
  - Do: Define color tokens (primary `#00288e`, secondary `#006a61`, status colors Waiting/Serving/Completed/Skipped), Inter typography, spacing — per Desktop/Mobile `DESIGN.md`.
  - Outcome: Shared resource dictionaries.
  - Verify: Tokens resolve in a test page; match `DESIGN.md`. (AC-41)

---

## Group DOM — Domain layer (Phase 2)

- [ ] **T-DOM-01 — Token model + enums.**
  - Depends on: T-SETUP-01
  - Do: `Token` (Number, ServiceType, Priority, AssignedCounter, Status, EstimatedWait) + `ServiceType`, `Priority`, `TokenStatus` enums.
  - Outcome: Token type with all §1.1 fields.
  - Verify: Compiles; fields match specs §1.1. (AC-01)

- [ ] **T-DOM-02 — Counter model.**
  - Depends on: T-SETUP-01
  - Do: `Counter` (Id, Name, CurrentToken, ActiveStatus).
  - Outcome: Counter type with all §1.2 fields.
  - Verify: Compiles; three counters representable. (AC-02)

- [ ] **T-DOM-03 — DemoDataService (sample data).**
  - Depends on: T-DOM-01, T-DOM-02, T-AMB-03
  - Do: Build 20–30 predefined tokens + 3 counters from the canonical dataset; hold a pristine snapshot.
  - Outcome: Deterministic in-memory dataset.
  - Verify: 20–30 tokens, 3 counters on load; no external calls. (AC-03)

- [ ] **T-DOM-04 — QueueService skeleton + observable state.**
  - Depends on: T-DOM-03
  - Do: Expose observable token/counter collections and derived properties for binding.
  - Outcome: Bindable state surface.
  - Verify: Collections observable; change notifications fire.

- [ ] **T-DOM-05 — Local-data guarantee check.**
  - Depends on: T-DOM-03
  - Do: Ensure no networking/persistence packages or calls exist.
  - Outcome: Confirmed local-only.
  - Verify: Dependency scan clean. (AC-03, AC-39)

- [ ] **T-DOM-06 — Reset behavior.**
  - Depends on: T-DOM-03
  - Do: `Reset()` restores the pristine snapshot.
  - Outcome: Reset returns to original state.
  - Verify: After mutations + Reset, state equals launch state. (AC-04)

---

## Group OPS — Queue operations & calculations (Phase 2)

- [ ] **T-OPS-01 — Call Next.**
  - Depends on: T-DOM-04, T-AMB-04
  - Do: Select first available Waiting token (per ordering decision), assign to selected counter, set Serving.
  - Verify: AC-06; rejects when no Waiting / counter busy.

- [ ] **T-OPS-02 — Skip.**
  - Depends on: T-DOM-04
  - Do: Serving → Skipped; free counter.
  - Verify: AC-07.

- [ ] **T-OPS-03 — Recall.**
  - Depends on: T-DOM-04
  - Do: Skipped → Waiting.
  - Verify: AC-08.

- [ ] **T-OPS-04 — Transfer.**
  - Depends on: T-DOM-04
  - Do: Reassign Serving token to another counter, keep Serving.
  - Verify: AC-09.

- [ ] **T-OPS-05 — Complete.**
  - Depends on: T-DOM-04
  - Do: Serving → Completed; free counter.
  - Verify: AC-10.

- [ ] **T-OPS-06 — Precondition enforcement.**
  - Depends on: T-OPS-01…T-OPS-05
  - Do: Each op is a no-op when its precondition fails.
  - Verify: AC-11.

- [ ] **T-OPS-07 — Wait estimate + analytics aggregates.**
  - Depends on: T-DOM-04
  - Do: `Wait = Waiting × 4`; aggregates by category, by counter, completed-vs-pending, averages; recompute on change.
  - Verify: AC-12; §5 aggregates correct.

---

## Group NAV — Navigation & shell (Phase 3)

- [ ] **T-NAV-01 — Desktop shell.**
  - Depends on: T-SETUP-03, T-AMB-01
  - Do: Header Overview/Manage nav + Manage segmented control (Tokens/Counters/Insights); App Bar `⋮` overflow.
  - Verify: Desktop navigation works. (AC-15, AC-37)

- [ ] **T-NAV-02 — Mobile shell.**
  - Depends on: T-SETUP-03, T-AMB-01
  - Do: Bottom nav (Overview, Manage) + Manage segmented control.
  - Verify: Mobile navigation works; mobile never shows a grid. (AC-15, AC-37)

---

## Group OVER — Queue Overview screen (Phase 4)

- [ ] **T-OVER-01 — Overview page scaffolding (desktop + mobile).**
  - Depends on: T-NAV-01, T-NAV-02, T-DOM-04
  - Do: Create Overview pages/view-model in DesktopUI and MobileUI.
  - Verify: Pages render within shell.

- [ ] **T-OVER-02 — Metric cards.**
  - Depends on: T-OVER-01, T-OPS-07
  - Do: Now Serving, Waiting, Completed, Average Wait bound to live state.
  - Verify: AC-16; values update after ops. (AC-12, AC-13)

- [ ] **T-OVER-03 — Queue Load radial gauge.**
  - Depends on: T-OVER-01, T-OPS-07
  - Do: SfRadialGauge bound to utilization %.
  - Verify: AC-17. (AC-38)

- [ ] **T-OVER-04 — Counter Status cards + badges.**
  - Depends on: T-OVER-01
  - Do: 3 counter cards (ListView/stacked), status badges; tap → Counter Operations sheet.
  - Verify: AC-18, AC-20. (AC-38)

- [ ] **T-OVER-05 — Upcoming Tokens + segmented filter.**
  - Depends on: T-OVER-01
  - Do: Upcoming list with All/Normal/Priority segmented control; tap item → Token Details.
  - Verify: AC-19.

---

## Group TOK — Token Directory (Phase 5)

- [ ] **T-TOK-01 — Tokens tab scaffolding.**
  - Depends on: T-NAV-01, T-NAV-02, T-DOM-04
  - Do: Tokens tab pages/view-model (desktop + mobile).
  - Verify: Tab renders.

- [ ] **T-TOK-02 — Desktop SfDataGrid.**
  - Depends on: T-TOK-01
  - Do: DataGrid columns (token, priority, status, service, counter, wait) + summary KPIs.
  - Verify: AC-21. (AC-38)

- [ ] **T-TOK-03 — Mobile stacked token cards (SfListView).**
  - Depends on: T-TOK-01
  - Do: Stacked cards (token, service, priority badge, counter, status badge, wait); no grid.
  - Verify: AC-22. (AC-38)

- [ ] **T-TOK-04 — Status filter chips.**
  - Depends on: T-TOK-02, T-TOK-03
  - Do: All/Waiting/Serving/Skipped/Completed filter on both platforms.
  - Verify: AC-23.

- [ ] **T-TOK-05 — Mobile search.**
  - Depends on: T-TOK-03
  - Do: Search by token number (and matching fields).
  - Verify: AC-24.

- [ ] **T-TOK-06 — Token tap → Token Details.**
  - Depends on: T-TOK-02, T-TOK-03
  - Do: Selection opens Token Details with context.
  - Verify: AC-25 (with T-SHEET-02).

---

## Group COUNTER — Counter Operations tab (Phase 6)

- [ ] **T-COUNTER-01 — Counters tab + selector.**
  - Depends on: T-NAV-01, T-NAV-02, T-DOM-04
  - Do: Counter selector segmented control (1/2/3) + Current Service card (large token, service, priority badge, counter, status badge).
  - Verify: AC-26. (AC-38)

- [ ] **T-COUNTER-02 — Action buttons wired.**
  - Depends on: T-COUNTER-01, T-OPS-01…T-OPS-05
  - Do: Primary (Call Next, Complete) + Secondary (Skip, Recall, Transfer), grouped; invoke ops for selected counter; Transfer opens Transfer sheet.
  - Verify: AC-27.

- [ ] **T-COUNTER-03 — Queue summary.**
  - Depends on: T-COUNTER-01, T-OPS-07
  - Do: Live Waiting/Completed/Skipped counts.
  - Verify: AC-28.

- [ ] **T-COUNTER-04 — Waiting Queue Preview (next 3).**
  - Depends on: T-COUNTER-01
  - Do: Show next 3 waiting tokens only.
  - Verify: AC-29.

---

## Group SHEET — Popups / bottom sheets (Phase 7)

- [ ] **T-SHEET-01 — Counter Operations popup/sheet.**
  - Depends on: T-OVER-04, T-COUNTER-02, T-SETUP-02
  - Do: SfPopup (desktop modal / mobile sheet); counter name, current token, category, priority badge; 5 actions + Close; opens from Overview counter card and Counters tab.
  - Verify: AC-33, AC-36. (AC-38)

- [ ] **T-SHEET-02 — Token Details popup/sheet.**
  - Depends on: T-OVER-05, T-TOK-06, T-AMB-02, T-SETUP-02
  - Do: Token number, service type, priority, counter, status badge, estimated wait; Close + conditional Recall/Transfer; opens from Tokens tab and Upcoming tokens.
  - Verify: AC-32, AC-36. (AC-38)

- [ ] **T-SHEET-03 — Transfer popup/sheet.**
  - Depends on: T-SHEET-01, T-SHEET-02, T-OPS-04
  - Do: Current counter, destination selector (Counter 1/2/3), info note, Confirm Transfer/Cancel; opens from Counter Operations and Token Details; Confirm runs Transfer.
  - Verify: AC-34, AC-09, AC-36. (AC-38)

- [ ] **T-SHEET-04 — Reset confirmation popup/sheet.**
  - Depends on: T-NAV-01, T-NAV-02, T-DOM-06, T-SETUP-02
  - Do: Warning + message; Reset Queue/Cancel; opens only from `⋮` overflow; Confirm runs Reset, Cancel leaves state unchanged.
  - Verify: AC-35, AC-04, AC-05, AC-36. (AC-38)

---

## Group INS — Queue Insights (Phase 8)

- [ ] **T-INS-01 — Insights tab + KPI summary.**
  - Depends on: T-NAV-01, T-NAV-02, T-OPS-07
  - Do: Waiting/Completed/Active Counters KPI card.
  - Verify: part of AC-30.

- [ ] **T-INS-02 — Tokens by Service Type (column chart).**
  - Depends on: T-INS-01
  - Do: SfCartesianChart column series over Enquiry/Payment/Support.
  - Verify: AC-30. (AC-38)

- [ ] **T-INS-03 — Average Wait by category (bar chart).**
  - Depends on: T-INS-01
  - Do: SfCartesianChart bar series.
  - Verify: AC-30. (AC-38)

- [ ] **T-INS-04 — Counter Performance (column chart).**
  - Depends on: T-INS-01
  - Do: Completed per Counter 1–3.
  - Verify: AC-30. (AC-38)

- [ ] **T-INS-05 — Completed vs Pending (doughnut).**
  - Depends on: T-INS-01
  - Do: SfCircularChart doughnut series.
  - Verify: AC-30. (AC-38)

- [ ] **T-INS-06 — Auto-refresh + mobile compact layout.**
  - Depends on: T-INS-02…T-INS-05
  - Do: Recompute charts on state change; mobile stacks compact cards.
  - Verify: AC-14, AC-31.

---

## Group RESP — Validation (Phase 9)

- [ ] **T-RESP-01 — Responsive layout validation.**
  - Depends on: all OVER/TOK/COUNTER/INS/SHEET tasks
  - Do: Run desktop + mobile; confirm layout selection; mobile = cards/sheets, desktop = grid.
  - Verify: AC-37.

- [ ] **T-RESP-02 — Control coverage audit.**
  - Depends on: T-RESP-01
  - Do: Confirm every mapped Syncfusion control (specs §7) is present in its location.
  - Verify: AC-38.

- [ ] **T-RESP-03 — Visual fidelity audit.**
  - Depends on: T-RESP-01
  - Do: Check colors, status badges, typography (tabular figures), spacing; no glassmorphism/excess animation.
  - Verify: AC-41, REQ-N-04…N-07.

- [ ] **T-RESP-04 — Full acceptance walkthrough.**
  - Depends on: T-RESP-01, T-RESP-02, T-RESP-03
  - Do: Re-demonstrate AC-01…AC-41 end-to-end; record evidence.
  - Verify: All ACs demonstrable; effort within 1–2 day envelope (REQ-N-08).

---

## Coverage matrix (specs AC → tasks)

| AC | Task(s) |
|---|---|
| AC-01 | T-DOM-01 |
| AC-02 | T-DOM-02 |
| AC-03 | T-DOM-03, T-DOM-05 |
| AC-04 | T-DOM-06, T-SHEET-04 |
| AC-05 | T-SHEET-04 |
| AC-06 | T-OPS-01, T-COUNTER-02 |
| AC-07 | T-OPS-02 |
| AC-08 | T-OPS-03 |
| AC-09 | T-OPS-04, T-SHEET-03 |
| AC-10 | T-OPS-05 |
| AC-11 | T-OPS-06 |
| AC-12 | T-OPS-07, T-OVER-02 |
| AC-13 | T-OVER-02, T-OVER-03, T-OVER-04 |
| AC-14 | T-OPS-07, T-INS-06 |
| AC-15 | T-NAV-01, T-NAV-02, T-AMB-01 |
| AC-16 | T-OVER-02 |
| AC-17 | T-OVER-03 |
| AC-18 | T-OVER-04 |
| AC-19 | T-OVER-05, T-SHEET-02 |
| AC-20 | T-OVER-04, T-SHEET-01 |
| AC-21 | T-TOK-02 |
| AC-22 | T-TOK-03 |
| AC-23 | T-TOK-04 |
| AC-24 | T-TOK-05 |
| AC-25 | T-TOK-06, T-SHEET-02 |
| AC-26 | T-COUNTER-01 |
| AC-27 | T-COUNTER-02, T-OPS-01…T-OPS-05 |
| AC-28 | T-COUNTER-03 |
| AC-29 | T-COUNTER-04 |
| AC-30 | T-INS-01, T-INS-02, T-INS-03, T-INS-04, T-INS-05 |
| AC-31 | T-INS-06 |
| AC-32 | T-SHEET-02, T-AMB-02 |
| AC-33 | T-SHEET-01 |
| AC-34 | T-SHEET-03 |
| AC-35 | T-SHEET-04 |
| AC-36 | T-SHEET-01, T-SHEET-02, T-SHEET-03, T-SHEET-04 |
| AC-37 | T-NAV-01, T-NAV-02, T-RESP-01 |
| AC-38 | T-SETUP-02, T-OVER-03, T-OVER-04, T-TOK-02, T-TOK-03, T-COUNTER-01, T-SHEET-01…T-SHEET-04, T-INS-02…T-INS-05, T-RESP-02 |
| AC-39 | T-SETUP-01, T-DOM-05 |
| AC-40 | T-SETUP-01, T-SETUP-03 |
| AC-41 | T-SETUP-04, T-RESP-03 |

Every AC-01 … AC-41 maps to at least one task. Open questions OQ-1…OQ-7 are owned by T-AMB-01…T-AMB-04 and gate their dependent tasks.
