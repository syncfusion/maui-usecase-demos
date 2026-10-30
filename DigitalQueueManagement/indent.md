# indent.md — Digital Queue & Counter Management (Use-Case Intent)

> Harness-planning document 1 of 4. Companion documents: [`specs.md`](specs.md), [`plan.md`](plan.md), [`task.md`](task.md).
>
> This file is intentionally named `indent.md` (per the source instruction in [`docs/prompt.md`](docs/prompt.md) — not "intent.md"). It records the *intent* of the use case.

---

## 1. Purpose of this document

This document states **what** the "Digital Queue & Counter Manager" use-case sample is and **why** it is being built, before any application code is written. It is the intent layer of an AI-harness planning set: it fixes the problem, users, goals, scope, non-goals, constraints, and design principles so that the downstream specification, plan, and task list stay anchored to the documented sources.

All claims here are traced to the source documents under `docs/`. Where sources disagree or leave a decision open, the conflict is recorded explicitly (see §9) rather than silently resolved.

---

## 2. Use-case intent

Build a **showcase service-center queue manager** for .NET MAUI that demonstrates queue monitoring and token-processing workflows using **local sample data only**. It is explicitly **not** a production queue-management system.

Source: [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §1 ("This is a showcase use-case application, not a production queue-management system"); [`docs/prompt.md`](docs/prompt.md) ("showcase service-center queue manager using local sample data").

The build is also an **AI-harness development exercise**: the application is to be produced through an AI Harness workflow, and these four planning documents are the harness inputs.

Source: [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §13 ("The application must be developed through an AI Harness workflow"); [`docs/prompt.md`](docs/prompt.md) ("preparing … for an AI cloud harness").

---

## 3. Problem statement

Customers visiting a service center often do not know their position in the queue, which token is currently being served, which counter handles which service, or the approximate waiting time. Operators need a simple visual interface to manage queue progression. The application provides a transparent queue view while demonstrating state management, dashboards, analytics, and workflow operations using Syncfusion controls.

Source: [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §2.

---

## 4. Intended users

Derived from the problem statement; roles are illustrative for a showcase (no authentication or role management is in scope — see §7).

- **Service operators / counter agents** — the primary interactive users: call next, complete, skip, recall, transfer tokens at a counter.
- **Service-center supervisors** — observe the overview dashboard and insights; trigger a data reset.
- **Customers (passive / lobby view)** — see now-serving token, waiting count, and estimated wait.
- **Developers / harness evaluators** — the real audience of the sample; they assess Syncfusion control usage and MAUI patterns.

Source: roles inferred from [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §1–§2 and counter-operations designs. *Assumption:* the specific role split above is a reading of the documentation, not an explicit list in the sources (flagged in §9, OQ-5).

---

## 5. Goals

Primary goals (verbatim intent from [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §3):

- Demonstrate a complete queue workflow.
- Showcase multiple Syncfusion controls.
- Provide attractive dashboard visualizations.
- Support both Desktop and Mobile layouts.
- Use only local data.
- Be suitable for AI-assisted development using a harness workflow.

Supporting intent (from the design sources and [`docs/prompt.md`](docs/prompt.md)):

- Match the documented desktop and mobile designs faithfully (do not substitute a desktop grid for mobile stacked cards).
- Implement full CRUD-style queue state operations (Call Next, Skip, Recall, Transfer, Complete, Reset) over local data.

---

## 6. Scope

### 6.1 Business scope (documented)

| Aspect | Value | Source |
|---|---|---|
| Service center | One (single branch) | overview §4 |
| Counters | Counter 1, Counter 2, Counter 3 | overview §4 |
| Service categories | General Enquiry, Payments, Customer Support | overview §4 |
| Priorities | Normal, Priority | overview §4 |
| Token count | 20–30 predefined local tokens | overview §4 |
| Token statuses | Waiting, Serving, Skipped, Completed (one at a time) | overview §5 |
| Operations | Call Next, Skip, Recall, Transfer, Complete, Reset | overview §6; prompt.md |
| Wait estimate | Waiting token count × 4 minutes (estimate only) | overview §7 |

### 6.2 Functional scope

- Queue overview dashboard (metrics, queue-load gauge, counter status, upcoming tokens).
- Token directory (list/grid of all tokens with search and status filtering).
- Counter operations (per-counter current service + queue actions).
- Queue insights (analytics charts).
- Token details, counter operations, transfer, and reset surfaced as popups (desktop) / bottom sheets (mobile).

Source: [`docs/Design Flow/Mobile-desktop-screens.md`](docs/Design%20Flow/Mobile-desktop-screens.md), the Desktop/Mobile `DESIGN.md` + `code.html` references, and [`docs/Desktop Designs/interaction_navigation_mappings.md`](docs/Desktop%20Designs/interaction_navigation_mappings.md).

### 6.3 Platform scope

Desktop (Windows / macOS) and Mobile (Android / iOS) layouts from a single MAUI codebase, following the reference-sample organization (Desktop UI and Mobile UI view folders).

Source: reference sample `HotelBookingManagement/HotelManagerSample` (`Views/DesktopUI`, `Views/MobileUI`); [`docs/prompt.md`](docs/prompt.md) ("Support desktop and mobile layouts").

---

## 7. Non-goals (explicitly out of scope)

The application will **not** include (verbatim from [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §3 Non-Goals and [`docs/prompt.md`](docs/prompt.md)):

- Authentication / user accounts
- Real-time synchronization
- Database storage
- Web APIs / backend services
- Multiple service branches
- Appointment scheduling
- AI waiting-time prediction
- Complex queue routing
- Emergency / no-show queue handling

Additional process non-goals (from [`docs/prompt.md`](docs/prompt.md)): do not implement application code within this planning task, do not modify existing project files or source documentation, and do not create additional planning files beyond the four named documents.

---

## 8. Constraints

- **Local data only.** All tokens and counters come from an in-memory sample-data service; reset restores the original demonstration data. Source: overview §4, §12; prompt.md.
- **Recommended architecture is MVVM** with Models, Services, ViewModels, Views, and Resources layers. `QueueService` owns token allocation, queue ordering, status updates, and analytics; `DemoDataService` creates predefined tokens/counters and restores sample data. Source: overview §12; prompt.md.
- **Documented Syncfusion control mapping must be honored:** DataGrid → token directory; ListView → counter cards & token lists; Radial Gauge → queue load; Charts → analytics; Badge View → status indicators; Segmented Control → filtering & counter selection; Popup → token details & confirmations. Source: overview §10; prompt.md.
- **Component feasibility is bounded by the bundled Syncfusion skills** under [`docs/skills/.agents/skills/`](docs/skills/.agents/skills) (DataGrid, ListView, Radial Gauge, Cartesian Charts, Circular Charts, Badge View, Segmented Control, Popup, Getting Started, Licensing, and others). Only controls relevant to this mapping are to be used.
- **Visual direction:** clean, professional service-center dashboard; blue primary (`#00288e`) + teal secondary (`#006a61`) with clear status colors; large readable token numbers; touch-friendly mobile layouts; restrained animation. **Avoid glassmorphism and futuristic styling.** Source: overview §14; desktop/mobile `DESIGN.md`; prompt.md.
- **Effort envelope:** scope intended to be completable within 1–2 development days. Source: overview §15.
- **Toolchain:** follow the existing repo conventions — .NET MAUI (net10.0 target set) with Syncfusion packages (`Syncfusion.Maui.DataGrid`, `Syncfusion.Maui.ListView`, `Syncfusion.Maui.Charts`, `Syncfusion.Maui.Gauges`, `Syncfusion.Maui.Buttons`, `Syncfusion.Maui.Popup`, `Syncfusion.Maui.Toolkit`, `Syncfusion.Maui.Core`), as used by the reference sample. Source: `HotelBookingManagement/HotelManagerSample/HotelManagerSample.csproj`.

---

## 9. Important assumptions & unresolved decisions

These are tracked as **open questions (OQ)** and carried consistently into [`specs.md`](specs.md) §8 and [`task.md`](task.md) (resolution tasks run before dependent implementation).

- **OQ-1 — Screen count / navigation model (DOCUMENTED CONFLICT).**
  - [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §11 describes **Desktop** navigation as a **Left Navigation Rail** with four areas (Queue Overview, Token Directory, Counter Operations, Queue Insights), and **Mobile** as **Bottom Tab Navigation** with items Overview / Tokens / Counters / Insights.
  - [`docs/Design Flow/Mobile-desktop-screens.md`](docs/Design%20Flow/Mobile-desktop-screens.md) and the `code.html` references instead describe **two screens** — an **Overview** screen and a **Manage** screen — where Manage hosts a **Segmented Control** (Tokens / Counters / Insights). Mobile bottom navigation has only **two items: Overview and Manage**.
  - The desktop `code.html` uses a **top header nav (Overview / Manage) plus a segmented control (Tokens / Counters / Insights)**, i.e. it matches the two-screen model, **not** the Left Navigation Rail in overview §11.
  - The acceptance criteria in overview §15 say **"Two screens are implemented along with taps and popup and bottom sheet"**, which also matches the two-screen model.
  - **Recommended assumption (to confirm):** implement the **two-screen model** (Overview + Manage-with-segmented-tabs) because it is corroborated by the detailed screen doc, both desktop and mobile `code.html` references, the interaction mappings, and the §15 acceptance statement. Treat overview §11's "Left Navigation Rail / four bottom-nav items" as superseded by the more detailed and more numerous design sources. **Requires sign-off before navigation implementation.**

- **OQ-2 — Analytics sample numbers differ between sources.** Overview cards cite "Completed Tokens 14" (desktop/mobile overview) while the Insights screens cite "Completed 42". Token totals also vary (header "Tokens 24" vs. Insights "50 issued"). These are **illustrative mock values in static design mocks**, not a single canonical dataset. *Assumption:* the harness will define one internally consistent sample dataset; exact mock figures in individual HTML files are not binding. Flagged for confirmation.

- **OQ-3 — Token-details secondary actions are conditional.** The mobile Token Details sheet shows Recall / Transfer "shown conditionally" ([`docs/Design Flow/Mobile-desktop-screens.md`](docs/Design%20Flow/Mobile-desktop-screens.md)). The exact condition-to-status mapping (e.g., Recall only for Skipped, Transfer only for Serving) is **not stated explicitly**. *Assumption:* derive conditions from the status rules in [`specs.md`](specs.md) §3; confirm.

- **OQ-4 — Desktop vs. mobile wording drift.** Desktop mocks sometimes read "General Inquiry" while overview/mobile read "General Enquiry"; "Payments" vs. "Payments & Billing"; counter "operator name" appears in interaction mappings but not in the data model. *Assumption:* canonical category labels follow overview §4 (General Enquiry, Payments, Customer Support); display sub-labels from mocks are cosmetic. Confirm.

- **OQ-5 — User roles are inferred.** The sources do not enumerate explicit user roles (§4 here is a reading). No role-based behavior is in scope regardless (auth is a non-goal), so this does not block implementation.

- **OQ-6 — Document placement.** [`docs/prompt.md`](docs/prompt.md) says place the four docs in "the DigitalQueueManagement project root". They are created at `DigitalQueueManagement/` root accordingly. (The project code folder does not yet exist; only `docs/` is present.) Noted in case the harness expects a different location.

No ambiguity above is resolved silently; each is carried forward as a tracked decision.

---

## 10. Design principles

1. **Documented-source fidelity.** The implementation matches the reviewed desktop and mobile designs; mobile uses stacked cards and bottom sheets, never a desktop-style grid. (prompt.md; Mobile-desktop-screens.md.)
2. **Local, deterministic data.** One in-memory sample dataset; every action mutates state predictably; Reset restores the original. (overview §7, §12.)
3. **MVVM separation.** Views are declarative; `QueueService`/`DemoDataService` hold logic; ViewModels expose bindable state and commands. (overview §12.)
4. **Meaningful Syncfusion usage.** Each mapped control is used where the design calls for it, not decoratively. (overview §10.)
5. **Clarity over density.** Large token numbers, clear status badges, consistent cards, generous spacing on mobile, no glassmorphism, no excessive animation. (overview §14.)
6. **Showcase-appropriate scope.** No backend/production features; buildable in 1–2 days. (overview §3, §15.)
7. **Accessibility & readability.** Tabular figures for token IDs/timers, WCAG-contrast status colors as specified in the `DESIGN.md` files.

---

## 11. Traceability summary

| Intent element | Primary source(s) |
|---|---|
| Showcase / harness purpose | `docs/prompt.md`; `docs/Design Flow/overview.md` §1, §13 |
| Problem & users | `docs/Design Flow/overview.md` §1–§2 |
| Goals / non-goals | `docs/Design Flow/overview.md` §3 |
| Business scope | `docs/Design Flow/overview.md` §4–§7 |
| Data model | `docs/Design Flow/overview.md` §8 |
| Control mapping | `docs/Design Flow/overview.md` §10; `docs/prompt.md` |
| Navigation (conflict) | `docs/Design Flow/overview.md` §11 vs. `docs/Design Flow/Mobile-desktop-screens.md` + `code.html` + `interaction_navigation_mappings.md` |
| Architecture | `docs/Design Flow/overview.md` §12 |
| Visual direction | `docs/Design Flow/overview.md` §14; Desktop/Mobile `DESIGN.md` |
| Acceptance / effort | `docs/Design Flow/overview.md` §15 |
| Control feasibility | `docs/skills/.agents/skills/` (Syncfusion MAUI skills) |
| Project organization | `HotelBookingManagement/HotelManagerSample/` (reference sample) |

Detailed, measurable requirements and acceptance criteria are defined in [`specs.md`](specs.md); the phased build is in [`plan.md`](plan.md); the ordered checklist is in [`task.md`](task.md).
