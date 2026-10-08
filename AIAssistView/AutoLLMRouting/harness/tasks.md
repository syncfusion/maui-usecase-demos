# Tasks — Auto LLM Routing

**Status:** Mandatory. These are the **task details** an implementer MUST execute, in order,
honoring dependencies. Every task lists its linked requirements (`spec.md`) and a concrete
**Done when** criterion. A task is complete only when its criterion holds **and** its linked
requirement is satisfied. Keep the status column current while implementing.

Status legend: `TODO` · `DOING` · `DONE` · `BLOCKED`

| Field | Meaning |
|-------|---------|
| ID | Stable task id (`T-xx`). |
| Deps | Task ids that must be DONE first. |
| Spec | Requirement ids this task satisfies. |

---

## Phase 0 — Scaffolding

### T-01 · Create MAUI project & solution — `TODO`
- **Deps:** none · **Spec:** NFR-1, NFR-6
- Create `AIAssistView/AutoLLMRouting/AutoLLMRouting/` MAUI app (net10.0 TFMs matching repo
  samples) and `AutoLLMRouting.sln`. Enable `ImplicitUsings`, `Nullable`. Set
  `RootNamespace`/`ApplicationTitle` = `AutoLLMRouting`.
- **Done when:** solution restores and an empty app builds for an available TFM.

### T-02 · Add Syncfusion + HTTP dependencies & registration — `TODO`
- **Deps:** T-01 · **Spec:** UI-1, NFR-3
- Add package refs per `plan.md` §2 (AIAssistView, Core, Cards, Buttons, Charts;
  `Microsoft.Extensions.Http`). Register Syncfusion license + `ConfigureSyncfusionCore()`
  and `AddHttpClient()` in `MauiProgram.cs`.
- **Done when:** project builds with Syncfusion handlers registered; no unused packages beyond plan.

---

## Phase 1 — Domain models

### T-03 · Enums & RouteDecision model — `TODO`
- **Deps:** T-01 · **Spec:** FR-1, FR-2, §7 data contract
- Add `ComplexityLevel`, `ModelRoute`, `DecisionSource`, and `RouteDecision`
  (Complexity, Route, Model, Reason, Source).
- **Done when:** types compile and match the §7 contract exactly.

### T-04 · ChatMessage & RoutingOptions — `TODO`
- **Deps:** T-03 · **Spec:** FR-3, NFR-7
- `ChatMessage` (role, text, optional `RouteDecision`). `RoutingOptions` holding base URL +
  the three model names (`gpt-4o-mini`, `gpt-4.1-mini`, `gpt-4.1`) with safe defaults.
- **Done when:** options are overridable via config (no recompile) and default model names match the constitution.

---

## Phase 2 — Routing logic (pure, testable)

### T-05 · RoutingEngine — `TODO`
- **Deps:** T-03, T-04 · **Spec:** FR-2, FR-3, FR-6
- Map complexity/route → concrete model via `RoutingOptions`; unknown/low-confidence → **Fast**.
- **Done when:** pure, no I/O; returns correct model for each route and defaults to Fast.

### T-06 · HeuristicPromptClassifier — `TODO`
- **Deps:** T-05 · **Spec:** FR-5, FR-6, FR-8, FR-9, FR-10, FR-11, NFR-5
- Implement keyword + length rules from `plan.md` §3.1. MUST yield S1→Cheap, S2→Fast, S3→Best.
- **Done when:** deterministic for the documented examples/scenarios; defaults to Fast otherwise.

### T-07 · Unit tests for routing + heuristic — `TODO`
- **Deps:** T-05, T-06 · **Spec:** FR-11, NFR-5
- Add a lightweight test project (or test harness) asserting S1/S2/S3 routes and default-Fast.
- **Done when:** tests pass locally (`dotnet test`) without network.

---

## Phase 3 — LiteLLM integration

### T-08 · AppConfig (runtime key/endpoint resolution) — `TODO`
- **Deps:** T-02, T-04 · **Spec:** NFR-2, NFR-7, UI-5
- Resolve settings in order: in-app field → env vars (`LITELLM_BASE_URL`, `LITELLM_API_KEY`,
  optional model overrides) → safe defaults (no key default). Expose `IsHeuristicMode` when no key.
- **Done when:** NO secret is hard-coded/committed; missing key ⇒ heuristic mode flag true.

### T-09 · LiteLLMClient (OpenAI-compatible) — `TODO`
- **Deps:** T-08 · **Spec:** FR-7, NFR-2, NFR-4
- `POST {baseUrl}/chat/completions` with model + messages; Bearer auth only when key present;
  timeout + try/catch → typed result; never log the key.
- **Done when:** returns a typed success/failure; errors handled without crashing; key never logged.

### T-10 · ModelPromptClassifier (gpt-4o-mini) — `TODO`
- **Deps:** T-09, T-06 · **Spec:** FR-4, FR-6
- Call Cheap model with the §6 classifier system prompt; parse strict JSON into `RouteDecision`
  (Source=Model). On any failure, fall back to `HeuristicPromptClassifier` (Source=Heuristic).
- **Done when:** valid JSON → model decision; invalid/error → heuristic fallback, never throws to UI.

### T-11 · ChatOrchestrator — `TODO`
- **Deps:** T-09, T-10, T-05 · **Spec:** FR-7, FR-13, NFR-4
- One-turn pipeline: classify → resolve model → call LiteLLM (or labeled stub in offline mode)
  → assemble `ChatMessage` + `RouteDecision`.
- **Done when:** returns assistant text + a real routing decision in both online and offline modes.

---

## Phase 4 — UI (MVVM)

### T-12 · ChatViewModel — `TODO`
- **Deps:** T-11 · **Spec:** FR-12, FR-13, FR-14, NFR-3, NFR-6
- `SendPrompt` command; observables for `Messages`, `CurrentDecision`, `IsBusy`,
  `IsHeuristicMode`, and analytics counters. Update analytics each turn.
- **Done when:** sending a prompt updates conversation, decision, and counters; UI never blocks.

### T-13 · ChatPage with AI AssistView — `TODO`
- **Deps:** T-12, T-02 · **Spec:** UI-1, FR-12, FR-13, FR-13a
- Build `ChatPage.xaml` with `SfAIAssistView` wired to the VM; render each assistant turn with
  the FR-13a transparency block.
- **Done when:** conversation displays prompts/responses and the analysis block per turn.

### T-14 · Routing Card (UI-2) — `TODO`
- **Deps:** T-13 · **Spec:** UI-2, FR-13
- `SfCard` bound to `CurrentDecision` showing Complexity / Route / Model.
- **Done when:** card updates after every prompt with correct values.

### T-15 · Route chips (UI-3) — `TODO`
- **Deps:** T-13 · **Spec:** UI-3
- Three read-only chips `[Cheap][Fast][Best]`; highlight the selected route via
  `RouteToColorConverter`. No manual override.
- **Done when:** correct chip highlights for the current turn; chips are not interactive pickers.

### T-16 · Heuristic-mode banner + config affordance (UI-5) — `TODO`
- **Deps:** T-13, T-08 · **Spec:** UI-5, FR-5
- Visible indicator when running in heuristic/offline mode; non-blocking way to enter key/endpoint.
- **Done when:** absence of key shows banner and routing still works end to end.

### T-17 · Analytics panel (OPTIONAL / stretch) — `TODO`
- **Deps:** T-12 · **Spec:** UI-4, FR-14
- Show Requests Processed + Cheap/Fast/Best counts (optionally an `SfCircularChart`), live-updating.
- **Done when:** counts match the number/route of prompts sent. _May be skipped if time-boxed._

---

## Phase 5 — Polish & verification

### T-18 · Error/empty/loading states — `TODO`
- **Deps:** T-13 · **Spec:** NFR-3, NFR-4
- Busy indicator during calls; friendly error on failures; sensible empty state.
- **Done when:** forced failures show a friendly message and no crash.

### T-19 · Sample README for the app — `TODO`
- **Deps:** T-13 · **Spec:** NFR-6
- Add `AIAssistView/AutoLLMRouting/README.md`: what it does, how routing works, how to supply a
  key/endpoint (env var or in-app), and the offline heuristic mode. **No secrets.**
- **Done when:** a new developer can configure and run it from the README (no secrets included).

### T-20 · Acceptance pass — `TODO`
- **Deps:** all above (T-17 optional) · **Spec:** all
- Walk `acceptance.md`; confirm each check. Record results.
- **Done when:** all mandatory acceptance checks pass (optional items noted if skipped).

---

## Dependency summary

```
T-01 ─┬─ T-02 ─┬─ T-08 ─ T-09 ─┬─ T-10 ─┐
      │        │               │        ├─ T-11 ─ T-12 ─ T-13 ─┬─ T-14
      ├─ T-03 ─┴─ T-04 ─ T-05 ─┴─ T-06 ─┘                      ├─ T-15
      │                          └─ T-07                       ├─ T-16
      │                                                        └─ T-17*(opt)
      └───────────────────────────────────── T-18 ─ T-19 ─ T-20
```
`*` optional
