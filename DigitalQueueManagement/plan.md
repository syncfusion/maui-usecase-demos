# plan.md — Digital Queue & Counter Management (Implementation Plan)

> Harness-planning document 3 of 4. Companion documents: [`indent.md`](indent.md), [`specs.md`](specs.md), [`task.md`](task.md).
>
> A phased plan to build the showcase from the reviewed documentation. Each phase lists its objective, dependencies, work, expected outcome, verification, and the [`specs.md`](specs.md) requirements/ACs it advances. Scope is kept to the documented showcase — no backend or production features.

## How to read this plan

- Phases are ordered by dependency, not just convenience. A phase may start only when its listed dependencies are complete.
- **Phase 0 (ambiguity resolution) precedes dependent UI work** per [`docs/prompt.md`](docs/prompt.md) ("Include tasks for resolving or confirming the documented ambiguities before dependent implementation work").
- Detailed, checkable tasks live in [`task.md`](task.md); this document gives the shape and rationale.
- The baseline organization follows the reference sample `HotelBookingManagement/HotelManagerSample` (project folder + `.slnx` + `README.md`; `Models/ Services/ ViewModels/ Views(DesktopUI, MobileUI)/ Resources/`).

---

## Phase 0 — Resolve documented ambiguities (decision gate)

- **Objective:** Convert the open questions in [`specs.md`](specs.md) §9.3 into confirmed decisions so downstream work is not built on guesses.
- **Dependencies:** none (must come first).
- **Work:**
  - OQ-1 Navigation model: confirm two-screen (Overview + Manage/segmented) vs. overview §11 Left Nav Rail. Record the decision.
  - OQ-3 Token Details conditional actions (Recall↔Skipped, Transfer↔Serving).
  - OQ-2 / OQ-4 Canonical sample dataset figures and category/counter labels.
  - OQ-7 Call-Next ordering tie-break (FIFO vs. priority-weighted).
- **Expected outcome:** A short decision record (in `task.md` task notes or `README.md`) fixing each open question; [`specs.md`](specs.md) assumptions promoted to facts or revised.
- **Verification:** Every OQ in `specs.md` §9.3 has a recorded decision; no later phase references an unresolved OQ.
- **Advances:** OQ-1…OQ-7; unblocks Phases 2–7.

---

## Phase 1 — Project setup & Syncfusion integration

- **Objective:** Create the MAUI project skeleton with Syncfusion configured, matching repo conventions.
- **Dependencies:** Phase 0 (navigation decision informs shell/layout scaffolding).
- **Work:**
  - Create `DigitalQueueManagement/<ProjectName>/` project + `.slnx` + `README.md` mirroring the reference sample.
  - Target framework set net10.0 (android/ios/maccatalyst/windows) as in the reference csproj.
  - Add Syncfusion packages: `Syncfusion.Maui.Core`, `Syncfusion.Maui.Toolkit`, `Syncfusion.Maui.DataGrid`, `Syncfusion.Maui.ListView`, `Syncfusion.Maui.Charts`, `Syncfusion.Maui.Gauges`, `Syncfusion.Maui.Buttons`, `Syncfusion.Maui.Popup`.
  - `MauiProgram`: `ConfigureSyncfusionCore()`, `ConfigureSyncfusionToolkit()`, fonts (incl. Inter), license registration, DI container.
  - Establish folder layout: `Models/ Services/ ViewModels/ Views/DesktopUI/ Views/MobileUI/ Resources/`.
  - Define theme resources: colors (primary `#00288e`, secondary `#006a61`, status colors), Inter typography, spacing — per `DESIGN.md`.
- **Expected outcome:** Solution builds and launches an empty shell on desktop and mobile; Syncfusion initialized; no network/database/API references.
- **Verification:** `dotnet build` succeeds; app launches; theme resources resolve; REQ-N-01/02/03/09 satisfied at skeleton level.
- **Advances:** REQ-N-01, REQ-N-02, REQ-N-03, REQ-N-09, AC-39, AC-40, AC-41; control-setup half of AC-38.
- **Source basis:** reference csproj & `MauiProgram.cs`; `syncfusion-maui-getting-started`, `syncfusion-maui-licensing`; `DESIGN.md`.

---

## Phase 2 — Domain layer: models, services, sample data

- **Objective:** Implement the data model, queue logic, and local sample data.
- **Dependencies:** Phase 1 (project), Phase 0 (dataset/ordering/label decisions).
- **Work:**
  - Models: `Token` (number, service type, priority, assigned counter, status, estimated wait), `Counter` (id, name, current token, active), enums (`ServiceType`, `Priority`, `TokenStatus`).
  - `DemoDataService`: builds 20–30 predefined tokens + 3 counters; holds the pristine snapshot; `Reset()` restores it.
  - `QueueService`: Call Next, Skip, Recall, Transfer, Complete with precondition enforcement; queue ordering; wait estimate (Waiting × 4); analytics aggregates (by category, by counter, completed vs pending, averages).
  - State exposed via observable collections/properties for binding.
- **Expected outcome:** All queue operations and analytics computable and unit-verifiable without any UI.
- **Verification:** Unit tests (or a scratch harness) assert AC-06…AC-12 and §5 aggregates; Reset restores original (AC-04/05); preconditions reject invalid ops (AC-11).
- **Advances:** REQ-F-01…F-07, AC-01…AC-14, REQ-N-02.
- **Source basis:** overview §4–§8, §12; prompt.md.

---

## Phase 3 — Navigation & app shell (desktop + mobile)

- **Objective:** Implement the confirmed navigation model and responsive layout selection.
- **Dependencies:** Phase 1; Phase 0 (OQ-1 decision).
- **Work:**
  - Desktop shell: header Overview/Manage nav + Manage segmented control (Tokens/Counters/Insights); App Bar with `⋮` overflow.
  - Mobile shell: bottom navigation (Overview, Manage) + Manage segmented control.
  - Responsive selection between DesktopUI and MobileUI per breakpoints in `DESIGN.md`.
  - Wire the `⋮` → Reset confirmation entry point (sheet implemented in Phase 7).
- **Expected outcome:** User can move between Overview and the three Manage tabs on both platforms; correct layout chosen by width/target.
- **Verification:** AC-15, AC-37 — manual navigation on desktop and mobile; mobile never shows the grid.
- **Advances:** REQ-F-08, REQ-F-14, AC-15, AC-37, REQ-N-06.
- **Source basis:** Mobile-desktop-screens.md; desktop/mobile `code.html`; interaction mappings; `DESIGN.md` breakpoints. (OQ-1)

---

## Phase 4 — Queue Overview screen

- **Objective:** Build the Overview dashboard bound to live state.
- **Dependencies:** Phase 2 (data/services), Phase 3 (shell).
- **Work:**
  - Metric cards: Now Serving, Waiting, Completed, Average Wait.
  - Queue Load radial gauge (SfRadialGauge) bound to utilization %.
  - Counter Status: 3 counter cards (ListView/stacked), status badges; tap → Counter Operations.
  - Upcoming Tokens with All/Normal/Priority segmented filter; tap → Token Details.
- **Expected outcome:** Overview reflects current state and updates after operations.
- **Verification:** AC-16…AC-20, AC-13 (live update), AC-12 (wait estimate), AC-17 (gauge).
- **Advances:** REQ-F-06, F-07, F-09, AC-12, AC-13, AC-16…AC-20.
- **Source basis:** overview Queue Overview mocks (desktop+mobile); Mobile-desktop-screens.md Screen 1; interaction mappings §1–§2.

---

## Phase 5 — Token Directory (Manage → Tokens)

- **Objective:** Build the token list with search/filter, platform-correct.
- **Dependencies:** Phase 2, Phase 3.
- **Work:**
  - Desktop: SfDataGrid with token columns + summary KPIs + status filter chips.
  - Mobile: SfListView stacked token cards + search bar + status filter chips.
  - Status filtering + mobile search; tap/select → Token Details.
- **Expected outcome:** Both platforms list tokens faithfully; desktop = grid, mobile = cards.
- **Verification:** AC-21…AC-25.
- **Advances:** REQ-F-10, REQ-F-13 (token details trigger), AC-21…AC-25, AC-32.
- **Source basis:** Tokens mocks (desktop+mocks); Mobile-desktop-screens.md Screen 2; `syncfusion-maui-datagrid`, `syncfusion-maui-listview`.

---

## Phase 6 — Counter Operations (Manage → Counters)

- **Objective:** Build per-counter operations UI wired to `QueueService`.
- **Dependencies:** Phase 2, Phase 3; Phase 4 shares the counter-tap entry point.
- **Work:**
  - Counter selector segmented control (Counter 1/2/3).
  - Current Service card (large token, service, priority badge, counter, status badge).
  - Action buttons: Primary (Call Next, Complete), Secondary (Skip, Recall, Transfer), visually grouped.
  - Queue summary (Waiting/Completed/Skipped) + Waiting Queue Preview (next 3).
  - Wire actions to the Phase 2 operations; Transfer opens the Transfer surface (Phase 7).
- **Expected outcome:** Operators can run the full workflow per counter; state updates propagate to Overview/Insights.
- **Verification:** AC-26…AC-29, AC-06…AC-11 via the UI, AC-13/14 propagation.
- **Advances:** REQ-F-05, F-11, AC-26…AC-29, AC-06…AC-11.
- **Source basis:** Counters mocks (desktop+mobile); Mobile-desktop-screens.md Counters Tab; counter-ops sheet mocks.

---

## Phase 7 — Popups / bottom sheets (Token Details, Counter Operations, Transfer, Reset)

- **Objective:** Implement the four interactive surfaces with correct triggers and context.
- **Dependencies:** Phases 2–6 (hosts & triggers exist).
- **Work:**
  - SfPopup instances; desktop = modal popup, mobile = bottom sheet.
  - Token Details (with conditional Recall/Transfer per OQ-3 decision), Counter Operations, Transfer (destination selector + confirm), Reset confirmation (from `⋮`).
  - Pass documented context (token/counter IDs) per interaction mappings.
- **Expected outcome:** All four surfaces open from each documented trigger, carry context, and perform their actions.
- **Verification:** AC-32…AC-36, AC-04/05 (reset), AC-09 (transfer).
- **Advances:** REQ-F-13, AC-32…AC-36.
- **Source basis:** interaction mappings §1–§4; bottom-sheet mocks 1–4 (desktop+mobile); `syncfusion-maui-popup`.

---

## Phase 8 — Queue Insights (Manage → Insights)

- **Objective:** Build analytics charts from live aggregates.
- **Dependencies:** Phase 2 (aggregates), Phase 3 (tab).
- **Work:**
  - KPI summary (Waiting/Completed/Active Counters).
  - Tokens by Service Type (column), Average Wait by category (bar), Counter Performance (column), Completed vs Pending (doughnut).
  - Auto-refresh on state change; mobile = compact stacked cards.
- **Expected outcome:** Charts reflect current state and refresh automatically after operations.
- **Verification:** AC-14, AC-30, AC-31.
- **Advances:** REQ-F-07, F-12, AC-14, AC-30, AC-31.
- **Source basis:** Insights mocks (desktop+mobile); Mobile-desktop-screens.md Insights Tab; `syncfusion-maui-cartesian-charts`, `syncfusion-maui-circular-charts`.

---

## Phase 9 — Desktop/mobile validation & visual fidelity

- **Objective:** Verify responsive behavior, control coverage, and design fidelity end-to-end.
- **Dependencies:** Phases 3–8.
- **Work:**
  - Run on a desktop target and a mobile target; walk every screen, tab, and sheet.
  - Confirm mobile uses stacked cards/sheets (no grid); desktop uses the grid.
  - Check theme tokens, status badge colors, tabular figures, spacing; confirm no glassmorphism/excess animation.
  - Confirm every mapped Syncfusion control is present (AC-38) and every AC is demonstrable.
- **Expected outcome:** A working showcase matching the documented designs on both platforms.
- **Verification:** AC-37, AC-38, AC-41, and a full re-check of AC-01…AC-36; record evidence.
- **Advances:** REQ-F-14, REQ-N-04…N-08, AC-37, AC-38, AC-41.
- **Source basis:** Desktop/Mobile `DESIGN.md`; overview §14–§15; prompt.md.

---

## Dependency overview

```
Phase 0 (decisions)
   │
Phase 1 (setup) ──► Phase 2 (domain) ──► Phase 4 (overview) ─┐
   │                   │                  Phase 5 (tokens) ───┤
   └──► Phase 3 (nav) ─┼────────────────► Phase 6 (counters) ─┼─► Phase 7 (sheets) ─► Phase 9 (validation)
                       └────────────────► Phase 8 (insights) ─┘
```

- Phases 4, 5, 6, 8 can proceed in parallel once Phases 1–3 are done (they share the domain layer and shell but not each other).
- Phase 7 depends on 4–6 for trigger hosts.
- Phase 9 is the final gate.

## Scope guardrails (restated)

- No authentication, backend, database, web API, real-time sync, multi-branch, scheduling, AI prediction, complex routing, or emergency handling (overview §3; prompt.md).
- Local in-memory sample data only; Reset restores it.
- Do not substitute a desktop grid for mobile; mobile uses stacked cards + bottom sheets.
- Keep effort within the documented 1–2 day envelope.
