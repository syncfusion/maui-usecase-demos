# specs.md — Digital Queue & Counter Management (Specification)

> Harness-planning document 2 of 4. Companion documents: [`indent.md`](indent.md), [`plan.md`](plan.md), [`task.md`](task.md).
>
> Defines functional and non-functional requirements, data model, workflows, screens, control mapping, and measurable acceptance criteria. Every acceptance criterion (AC-*) is traced to a source and is represented by one or more tasks in [`task.md`](task.md).

## Conventions

- **REQ-F-*** functional requirement · **REQ-N-*** non-functional requirement · **AC-*** acceptance criterion · **OQ-*** open question (mirrors [`indent.md`](indent.md) §9).
- **[FACT]** = stated in a source document · **[ASSUMPTION]** = proposed by this spec, needs confirmation · **[OPEN]** = unresolved decision.
- Source paths are relative to the `DigitalQueueManagement/` project root (e.g. `docs/Design Flow/overview.md`).

---

## 1. Domain data model

### 1.1 Token (REQ-F-01) [FACT]

Source: [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §8; §4–§5.

| Field | Type | Notes | Source |
|---|---|---|---|
| Token Number | String | e.g. `A-104`, `B-202`, `C-301`; prefix groups tokens by category series in the mocks | overview §8; design mocks |
| Service Type | Enum | General Enquiry · Payments · Customer Support | overview §4, §8 |
| Priority | Enum | Normal · Priority | overview §4, §8 |
| Assigned Counter | String | Counter 1–3, or none while unassigned | overview §8 |
| Status | Enum | Waiting · Serving · Skipped · Completed (exactly one) | overview §5, §8 |
| Estimated Wait | Integer (minutes) | Derived display value (see §4) | overview §8 |

### 1.2 Counter (REQ-F-02) [FACT]

Source: [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §8; §4.

| Field | Type | Notes | Source |
|---|---|---|---|
| Counter ID | Integer | 1, 2, 3 | overview §8 |
| Counter Name | String | e.g. "Counter 1"; mocks add cosmetic sub-labels ("Express Desk") | overview §8; design mocks |
| Current Token | String | Token currently Serving at this counter, or none | overview §8 |
| Active Status | Boolean | Whether the counter is online/available | overview §8 |

- **[ASSUMPTION]** Cosmetic desk sub-labels ("Express Desk", "General Inquiry", "Customer Support") and an "operator name" appearing in [`docs/Desktop Designs/interaction_navigation_mappings.md`](docs/Desktop%20Designs/interaction_navigation_mappings.md) are display-only and are **not** required model fields. (OQ-4)

### 1.3 Service categories & priorities (REQ-F-03) [FACT]

- Categories: **General Enquiry, Payments, Customer Support** (overview §4). Canonical labels per overview §4; mock variants ("General Inquiry", "Payments & Billing") are cosmetic. (OQ-4)
- Priorities: **Normal, Priority** (overview §4).

**AC-01** — A loaded token exposes all six fields in §1.1 with valid enum values. *(task: T-DOM-01)*
**AC-02** — A loaded counter exposes all four fields in §1.2; three counters (IDs 1–3) exist. *(task: T-DOM-02)*

---

## 2. Sample data & reset (REQ-F-04) [FACT]

Source: [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §4, §12; [`docs/prompt.md`](docs/prompt.md); reset designs in [`docs/Desktop Designs/Bottom Sheet 4 - Reset Queue`](docs/Desktop%20Designs/Bottom%20Sheet%204%20-%20Reset%20Queue) and [`docs/Mobile Designs/Bottom Sheet 4 - Reset Queue`](docs/Mobile%20Designs/Bottom%20Sheet%204%20-%20Reset%20Queue).

- **REQ-F-04.1** `DemoDataService` creates **20–30 predefined tokens** across the three categories and both priorities, plus three counters, entirely in memory.
- **REQ-F-04.2** The dataset is internally consistent (status counts, counter assignments, and upcoming order agree). **[ASSUMPTION]** one canonical dataset supersedes the differing illustrative numbers across individual mocks. (OQ-2)
- **REQ-F-04.3** Reset restores the original demonstration data, discarding all transient changes.
- **REQ-F-04.4** Reset is triggered from the App Bar overflow (`⋮`) menu and requires confirmation via the Reset popup/bottom sheet. Confirmation copy: "This will restore the original sample queue and remove all current changes." Source: interaction mappings §4; reset sheet mocks.

**AC-03** — On launch the app holds 20–30 tokens and 3 counters from local data; no network/database/API call occurs. *(tasks: T-DOM-03, T-DOM-05)*
**AC-04** — Invoking Reset and confirming restores the exact original dataset (counts and assignments identical to launch). *(tasks: T-DOM-06, T-SHEET-04)*
**AC-05** — Cancelling the Reset confirmation leaves current state unchanged. *(task: T-SHEET-04)*

---

## 3. Status rules & queue workflows (REQ-F-05) [FACT]

Source: [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §5–§6; [`docs/prompt.md`](docs/prompt.md); counter-operations design mocks.

A token holds exactly one status at a time (overview §5). Operations owned by `QueueService` (overview §12):

| Op | Precondition | Effect | Source |
|---|---|---|---|
| **Call Next** | A Waiting token exists; target counter has no Serving token | Select the **first available Waiting** token, assign it to the selected counter, set status → **Serving** | overview §6; prompt.md |
| **Skip** | Token is **Serving** | Status → **Skipped**; its counter becomes available (Current Token cleared) | overview §6; prompt.md |
| **Recall** | Token is **Skipped** | Status → **Waiting** (returns to queue) | overview §6; prompt.md |
| **Transfer** | Token is **Serving** | Reassign the token to another counter (continues serving there) | overview §6; prompt.md; transfer mocks |
| **Complete** | Token is **Serving** | Status → **Completed**; free the counter | overview §6; prompt.md |
| **Reset** | — | Restore original sample data (see §2) | overview §6; prompt.md |

- **REQ-F-05.1 "first available Waiting"** ordering: **[ASSUMPTION]** queue order follows the documented Upcoming/Waiting sequence shown in the mocks (priority tokens surface ahead of normal within the displayed order). Exact tie-break (strict FIFO vs. priority-weighted) is **[OPEN]**. (OQ-7)
- **REQ-F-05.2** "first available Waiting" is interpreted against the current filter/counter context per the Call action in the counter-operations mocks.

**AC-06** — Call Next on a counter with no active token moves the first available Waiting token to Serving and assigns it to that counter. *(tasks: T-OPS-01, T-COUNTER-02)*
**AC-07** — Skip on a Serving token sets it Skipped and frees its counter. *(task: T-OPS-02)*
**AC-08** — Recall on a Skipped token returns it to Waiting. *(task: T-OPS-03)*
**AC-09** — Transfer on a Serving token reassigns it to the chosen destination counter and keeps it Serving. *(tasks: T-OPS-04, T-SHEET-03)*
**AC-10** — Complete on a Serving token sets it Completed and frees its counter. *(task: T-OPS-05)*
**AC-11** — Each operation is rejected (no-op) when its precondition is not met. *(task: T-OPS-06)*

---

## 4. Wait-time calculation (REQ-F-06) [FACT]

Source: [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §7.

- **REQ-F-06.1** `Estimated Wait = (count of Waiting tokens) × 4 minutes`. Average service duration is fixed at 4 minutes. It is an estimate only; no intelligent prediction.
- **REQ-F-06.2** The value is recomputed whenever the Waiting count changes.
- **[ASSUMPTION]** Per-token "~4 / ~8 / ~12 min" labels in the mocks reflect queue position × 4 min (position-based display), consistent with REQ-F-06.1 applied per position. Confirm. (OQ-2)

**AC-12** — With N Waiting tokens, the displayed queue wait estimate equals N × 4 minutes and updates after each operation that changes N. *(tasks: T-OPS-07, T-OVER-02)*

---

## 5. Analytics / dashboard update expectations (REQ-F-07) [FACT]

Source: [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §15 ("Dashboard updates after every action", "Charts refresh automatically"); Insights design mocks.

Analytics computed by `QueueService` from current state:

- **REQ-F-07.1** KPI summary: Waiting count, Completed count, Active Counters count, Average Wait. (Insights/overview mocks.)
- **REQ-F-07.2** Tokens by Service Type — distribution across Enquiry / Payment / Support (column chart). (Insights mocks.)
- **REQ-F-07.3** Average Wait Time by service category (bar chart). (Insights mocks.)
- **REQ-F-07.4** Counter Performance — completed tokens per Counter 1–3 (column chart). (Insights mocks.)
- **REQ-F-07.5** Completed vs. Pending — fulfillment ratio (doughnut/circular chart). (Insights mocks; mobile doc "Doughnut Chart".)

**AC-13** — After any queue operation, all dashboard metrics (overview cards, gauge, counter status) reflect the new state without manual refresh. *(tasks: T-OVER-02, T-OVER-03, T-OVER-04)*
**AC-14** — After any queue operation, the Insights charts recompute from current state automatically. *(tasks: T-INS-01…T-INS-05)*

---

## 6. Navigation, screens & interactions

> **OQ-1 (navigation model) applies to all of §6.** This spec is written against the **recommended two-screen model** (Overview + Manage-with-segmented-tabs), corroborated by [`docs/Design Flow/Mobile-desktop-screens.md`](docs/Design%20Flow/Mobile-desktop-screens.md), both desktop and mobile `code.html`, the interaction mappings, and overview §15. The alternative (Left Navigation Rail + 4 bottom-nav items from overview §11) is recorded in §8 and **must be confirmed before navigation implementation** (task T-AMB-01). If the alternative is chosen, §6.1 navigation changes but the screen *content* requirements below still hold.

### 6.1 Navigation structure (REQ-F-08)

- **Desktop [FACT: code.html]:** Top header with **Overview / Manage** nav; the Manage area presents a **Segmented Control (Tokens / Counters / Insights)**. App Bar shows title, date, system-live indicator, and `⋮` overflow (hosts Reset). *(Conflicts with overview §11 "Left Navigation Rail" — OQ-1.)*
- **Mobile [FACT: Mobile-desktop-screens.md + code.html]:** **Bottom navigation with two items: Overview and Manage.** The Manage screen has a top **Segmented Control (Tokens / Counters / Insights)**. *(Conflicts with overview §11 "Bottom Tab Navigation: Overview / Tokens / Counters / Insights" — OQ-1.)*

**AC-15** — Desktop and mobile both expose Overview and a Manage area; Manage switches between Tokens, Counters, and Insights via a segmented control. *(tasks: T-NAV-01, T-NAV-02, T-AMB-01)*

### 6.2 Queue Overview screen (REQ-F-09)

Source: desktop [`docs/Desktop Designs/1 First Screen - Queue overview`](docs/Desktop%20Designs/1%20First%20Screen%20-%20Queue%20overview); mobile [`docs/Mobile Designs/1 First Screen - Queue overview`](docs/Mobile%20Designs/1%20First%20Screen%20-%20Queue%20overview); [`docs/Design Flow/Mobile-desktop-screens.md`](docs/Design%20Flow/Mobile-desktop-screens.md) (Mobile Screen 1).

Content:
- **Metric cards:** Now Serving, Waiting Tokens, Completed Tokens, Average Wait. (Mobile: 4 compact cards, 2-column grid.)
- **Queue Load card:** radial gauge with utilization %, waiting count, active counters.
- **Counter Status:** 3 vertically stacked counter cards (name, current token, category, status badge).
- **Upcoming Tokens:** list (mobile: 5 items) with a **segmented control (All / Normal / Priority)** filter; each item shows token number, service type, priority badge, estimated wait.

Interactions (interaction mappings §1–§2):
- Tapping an **Upcoming token** → Token Details popup/sheet.
- Tapping a **Counter Status card** → Counter Operations popup/sheet.

**AC-16** — Overview shows the four metric cards bound to live state. *(task: T-OVER-02)*
**AC-17** — Queue Load uses a Syncfusion Radial Gauge bound to utilization. *(task: T-OVER-03)*
**AC-18** — Counter Status shows three counter cards reflecting current assignments/status. *(task: T-OVER-04)*
**AC-19** — Upcoming Tokens supports All/Normal/Priority segmented filtering; tapping an item opens Token Details. *(tasks: T-OVER-05, T-SHEET-02)*
**AC-20** — Tapping a counter card opens Counter Operations with that counter's context. *(tasks: T-OVER-04, T-SHEET-01)*

### 6.3 Manage → Tokens tab / Token Directory (REQ-F-10)

Source: desktop [`docs/Desktop Designs/2.1 Queue Management- Tokens`](docs/Desktop%20Designs/2.1%20Queue%20Management-%20Tokens); mobile [`docs/Mobile Designs/2.1 Queue Management- Tokens`](docs/Mobile%20Designs/2.1%20Queue%20Management-%20Tokens); Mobile-desktop-screens.md (Mobile Screen 2).

- **Desktop [FACT]:** Token Directory with summary KPIs (Total, Waiting, Serving, Skipped), status filter chips (All/Waiting/Serving/Skipped/Completed), and a **DataGrid / data list** of tokens (token, priority, status, service, counter, wait). Per prompt.md the **DataGrid** backs the token directory.
- **Mobile [FACT]:** **Search bar** ("Search token number"), **status filter chips** (All/Waiting/Serving/Skipped/Completed), and **vertically stacked token cards** (token number, service type, priority badge, assigned counter, status badge, estimated wait). **No desktop-style grid on mobile.**
- Tapping a token → Token Details popup/sheet (interaction mappings §1).

**AC-21** — Desktop Token Directory renders tokens in a Syncfusion DataGrid. *(task: T-TOK-02)*
**AC-22** — Mobile Tokens tab renders stacked token cards (ListView), never a grid. *(task: T-TOK-03)*
**AC-23** — Status filter (chips) filters the visible tokens on both platforms. *(task: T-TOK-04)*
**AC-24** — Mobile search filters tokens by token number (and matching fields). *(task: T-TOK-05)*
**AC-25** — Tapping/selecting a token opens Token Details with that token's context. *(tasks: T-TOK-06, T-SHEET-02)*

### 6.4 Manage → Counters tab / Counter Operations (REQ-F-11)

Source: desktop [`docs/Desktop Designs/2.2 Queue Management- Counters`](docs/Desktop%20Designs/2.2%20Queue%20Management-%20Counters); mobile [`docs/Mobile Designs/2.2 Queue Management- Counters`](docs/Mobile%20Designs/2.2%20Queue%20Management-%20Counters); Mobile-desktop-screens.md (Counters Tab).

- **Counter selector:** segmented control (Counter 1 / 2 / 3). (prompt.md: Segmented Control → counter selection.)
- **Current Service card:** large current token number, service type, priority badge, current counter, status badge.
- **Counter actions:** Primary — **Call Next**, **Complete Service**; Secondary — **Skip Token**, **Recall Token**, **Transfer Token**. Visually grouped (Primary / Secondary).
- **Queue summary:** Waiting / Completed / Skipped counts.
- **Waiting Queue Preview:** next 3 waiting tokens (preview only, not a full list).

**AC-26** — Counters tab lets the user select Counter 1–3 via a segmented control and shows that counter's current service. *(task: T-COUNTER-01)*
**AC-27** — Primary (Call Next, Complete) and secondary (Skip, Recall, Transfer) actions are present and invoke the §3 operations for the selected counter. *(tasks: T-COUNTER-02, T-OPS-01…T-OPS-05)*
**AC-28** — Queue summary shows live Waiting/Completed/Skipped counts. *(task: T-COUNTER-03)*
**AC-29** — Waiting Queue Preview shows the next 3 waiting tokens only. *(task: T-COUNTER-04)*

### 6.5 Manage → Insights tab (REQ-F-12)

Source: desktop [`docs/Desktop Designs/2.3 Queue Management- Insights`](docs/Desktop%20Designs/2.3%20Queue%20Management-%20Insights); mobile [`docs/Mobile Designs/2.3 Queue Management- Insights`](docs/Mobile%20Designs/2.3%20Queue%20Management-%20Insights); Mobile-desktop-screens.md (Insights Tab).

Charts per §5 (Analytics Summary KPIs; Tokens by Service Type column; Average Wait bar; Counter Performance column; Completed vs. Pending doughnut). Mobile: compact, vertically stacked cards.

**AC-30** — Insights renders the four documented charts plus the KPI summary using Syncfusion Charts. *(tasks: T-INS-01…T-INS-05)*
**AC-31** — Mobile Insights stacks compact chart cards vertically (no oversized visualizations). *(task: T-INS-06)*

### 6.6 Popups / bottom sheets (REQ-F-13)

Source: [`docs/Desktop Designs/interaction_navigation_mappings.md`](docs/Desktop%20Designs/interaction_navigation_mappings.md) and [`docs/Mobile Designs/interaction_navigation_mappings.md`](docs/Mobile%20Designs/interaction_navigation_mappings.md) (identical content); bottom-sheet design folders 1–4 on both platforms. Desktop uses **Popup**; mobile uses **bottom sheet** (both via Syncfusion Popup per prompt.md mapping).

| Surface | Trigger(s) | Context passed | Contents / actions | Source |
|---|---|---|---|---|
| **Token Details** | Tap token card (Tokens tab) or Upcoming token (Overview) | Token ID, service category, priority, wait | Token number, service type, priority, assigned counter, queue status (badge), estimated wait; actions: Close (primary), **Recall / Transfer shown conditionally** | mappings §1; token-details mocks |
| **Counter Operations** | Tap counter card (Overview) or counter card/switcher (Counters tab) | Counter ID, status, currently serving token | Counter name, current token, category, priority badge; Primary: Call Next, Complete; Secondary: Skip, Recall, Transfer; Close | mappings §2; counter-ops mocks |
| **Transfer Token** | Transfer action in Counter Operations, or Transfer in Token Details | Origin counter/token, available destinations | Current counter, destination selector (Counter 1/2/3), info note, Confirm Transfer / Cancel | mappings §3; transfer mocks |
| **Reset Confirmation** | App Bar `⋮` overflow → Reset Queue | — | Warning icon + message; Reset Queue / Cancel | mappings §4; reset mocks |

- **REQ-F-13.1 [ASSUMPTION]** Token Details conditional actions: **Recall** visible when status = Skipped; **Transfer** visible when status = Serving. Confirm. (OQ-3)

**AC-32** — Token Details opens from both documented triggers with correct token context and shows conditional Recall/Transfer per status. *(tasks: T-SHEET-02, T-AMB-02)*
**AC-33** — Counter Operations opens from both documented triggers and exposes all five actions for that counter. *(task: T-SHEET-01)*
**AC-34** — Transfer opens from both documented triggers, lists destination counters, and on Confirm performs the §3 Transfer. *(task: T-SHEET-03)*
**AC-35** — Reset confirmation opens only from the `⋮` overflow and gates the §2 reset behind Confirm/Cancel. *(task: T-SHEET-04)*
**AC-36** — Desktop renders these as Popups; mobile renders them as bottom sheets. *(tasks: T-SHEET-01…T-SHEET-04)*

### 6.7 Responsive behavior (REQ-F-14)

Source: Desktop/Mobile `DESIGN.md` (breakpoints: <768 mobile 4-col; 768–1199 tablet 8-col; ≥1200 desktop 12-col); prompt.md ("Do not substitute a desktop-style grid for mobile layouts").

**AC-37** — The app selects the desktop layout on wide/desktop targets and the mobile stacked-card layout on narrow/mobile targets; mobile never shows the DataGrid. *(tasks: T-NAV-01, T-NAV-02, T-RESP-01)*

---

## 7. Syncfusion control mapping & implementation considerations (REQ-F-15)

Source: [`docs/Design Flow/overview.md`](docs/Design%20Flow/overview.md) §10; [`docs/prompt.md`](docs/prompt.md); bundled skills under [`docs/skills/.agents/skills/`](docs/skills/.agents/skills).

| Documented use | Syncfusion control | Package | Skill reference | Implementation considerations (from skill) |
|---|---|---|---|---|
| Token directory (desktop) | **SfDataGrid** | `Syncfusion.Maui.DataGrid` | `syncfusion-maui-datagrid` | Bind `ObservableCollection`; manual columns for token/status/service/counter/wait; built-in sorting/filtering/searching; register handler in `MauiProgram`. |
| Counter cards & token lists (mobile) | **SfListView** | `Syncfusion.Maui.ListView` | `syncfusion-maui-listview` | `ItemTemplate` stacked cards; `ItemsSource` bound; selection → open sheet; MVVM command patterns. |
| Queue load | **SfRadialGauge** | `Syncfusion.Maui.Gauges` | `syncfusion-maui-radial-gauge` | Single axis 0–100; pointer/range bound to utilization %; annotation for % label. |
| Analytics (column/bar) | **SfCartesianChart** | `Syncfusion.Maui.Charts` | `syncfusion-maui-cartesian-charts` | Column/Bar series bound to category aggregates; legend + data labels; auto-refresh on collection change. |
| Completed vs Pending | **SfCircularChart (Doughnut)** | `Syncfusion.Maui.Charts` | `syncfusion-maui-circular-charts` | Doughnut series for ratio; legend. |
| Status indicators | **SfBadgeView / badge styling** | `Syncfusion.Maui.Core` / Toolkit | `syncfusion-maui-badge-view` | Pill badges per status color; positioning/predefined symbols. |
| Filtering & counter selection | **SfSegmentedControl** | `Syncfusion.Maui.Buttons` | `syncfusion-maui-segmented-control` | 2–7 segments; single-selection; selection-changed events drive filtering/tab/counter switch. |
| Token details & confirmations | **SfPopup** | `Syncfusion.Maui.Popup` | `syncfusion-maui-popup` | Modal popups (desktop) / bottom-sheet positioning (mobile); header/body/footer templates; events for confirm/cancel. |
| Setup / licensing | — | `Syncfusion.Maui.Core`, `Syncfusion.Maui.Toolkit` | `syncfusion-maui-getting-started`, `syncfusion-maui-licensing` | `ConfigureSyncfusionCore()` + `ConfigureSyncfusionToolkit()` in `MauiProgram`; register license; add fonts. |

**[FACT]** Package set and host configuration mirror the reference sample `HotelBookingManagement/HotelManagerSample/HotelManagerSample.csproj` and `MauiProgram.cs`.
**[ASSUMPTION]** Badge View maps to `Syncfusion.Maui.Core`/Toolkit `SfBadgeView`; if status badges are simpler as styled labels/chips, that remains design-faithful. Confirm against the badge skill. (noted in task T-OVER-04)

Other skills present in the inventory (not applied; unrelated to this app): e.g. calendar, scheduler, inputs beyond the above, AI smart components. Per prompt.md these are intentionally excluded.

**AC-38** — Each mapped control in the table is used in its documented location; no mapped control is omitted. *(tasks: T-SETUP-02, T-OVER-03, T-TOK-02, T-TOK-03, T-COUNTER-01, T-INS-01…T-INS-05, T-SHEET-01…T-SHEET-04)*

---

## 8. Non-functional requirements

| ID | Requirement | Source |
|---|---|---|
| REQ-N-01 | **Local data only** — no auth, backend, database, web API, or real-time sync. | overview §3; prompt.md |
| REQ-N-02 | **MVVM** with Models / Services / ViewModels / Views / Resources layers. | overview §12 |
| REQ-N-03 | **Cross-platform** single codebase: Windows, macOS, Android, iOS (net10.0 target set as in reference sample). | reference csproj; prompt.md |
| REQ-N-04 | **Visual fidelity**: blue `#00288e` primary, teal `#006a61` secondary, status colors (Waiting blue, Serving amber, Completed green, Skipped red), Inter typography, tabular figures, 4/8px spacing. | Desktop/Mobile `DESIGN.md`; overview §14 |
| REQ-N-05 | **No glassmorphism, no excessive animation, no futuristic styling**; clarity/readability first. | overview §14; prompt.md |
| REQ-N-06 | **Mobile uses stacked cards + bottom sheets**, not desktop grids. | prompt.md; Mobile-desktop-screens.md |
| REQ-N-07 | **Accessibility**: WCAG-contrast status badges, readable large token numbers, touch-friendly targets. | Desktop `DESIGN.md`; mobile design notes |
| REQ-N-08 | **Effort/scope**: completable in 1–2 development days; no unrequested features. | overview §15; prompt.md |
| REQ-N-09 | **Project organization** follows prior samples (Models / Services / ViewModels / Views with DesktopUI & MobileUI subfolders; `.slnx` + `README.md`). | `HotelBookingManagement/HotelManagerSample` |

**AC-39** — No network, database, or API dependency is referenced anywhere in the build. *(task: T-SETUP-01)*
**AC-40** — Solution layout matches the reference-sample organization (project folder + `.slnx` + `README.md`; MVVM layers; DesktopUI/MobileUI views). *(tasks: T-SETUP-01, T-SETUP-03)*
**AC-41** — Theme tokens (colors, typography, status colors) match the `DESIGN.md` specifications. *(task: T-SETUP-04)*

---

## 9. Documented facts vs. assumptions vs. open questions

### 9.1 Facts (stated in sources)
Business scope (§1–§4 here), status rules & operations (§3), wait formula (§4), control mapping (§7), screen contents (§6.2–§6.6), popup triggers/context (§6.6), non-goals and visual rules (§7–§8) — all cited inline above.

### 9.2 Assumptions (proposed; need confirmation)
- A-1 (OQ-1): two-screen navigation model recommended over Left Nav Rail. 
- A-2 (OQ-2): one canonical sample dataset supersedes differing mock numbers; per-position wait labels = position × 4 min.
- A-3 (OQ-3): Token Details conditional actions — Recall for Skipped, Transfer for Serving.
- A-4 (OQ-4): canonical category labels per overview §4; mock sub-labels/operator name are cosmetic (not model fields).

### 9.3 Open questions (unresolved — must be decided)
- **OQ-1** Navigation model (two-screen vs. four-area rail/bottom-nav). *Blocks navigation implementation.* → task T-AMB-01.
- **OQ-2** Canonical dataset figures. → task T-AMB-03.
- **OQ-3** Token Details conditional-action rules. → task T-AMB-02.
- **OQ-4** Category/counter label canonicalization. → task T-AMB-03.
- **OQ-7** Call-Next ordering tie-break (FIFO vs. priority-weighted). → task T-AMB-04.

---

## 10. Acceptance-criteria → task traceability (summary)

Every AC above names its task(s) inline. [`task.md`](task.md) §"Coverage matrix" restates AC-01 … AC-41 against task IDs so coverage can be verified mechanically. Any AC not mapped to at least one task is a defect in the plan.
