# Specification — Auto LLM Routing

**Status:** Mandatory. This document defines **what** must be built and the exact
behavior. Each requirement has a stable ID (`FR-*` functional, `NFR-*` non-functional,
`UI-*` interface). Tasks in `tasks.md` and checks in `acceptance.md` reference these IDs.

---

## 1. Purpose

Demonstrate how an AI application can **automatically route user prompts to different GPT
models based on task complexity**, balancing cost, speed, and quality **without requiring
manual model selection**, surfaced through the Syncfusion **AI AssistView**.

---

## 2. Core concept

Instead of making the user choose a model, the system analyzes the prompt and decides:

- **Use Cheap Model** → `gpt-4o-mini`
- **Use Fast Model** → `gpt-4.1-mini`
- **Use Best Model** → `gpt-4.1`

The decision and its rationale are shown to the user before/with each answer.

---

## 3. Application flow (normative)

```
User Prompt
    ↓
Prompt Analyzer  (Cheap Model AI Agent — gpt-4o-mini, or heuristic fallback)
    ↓
Complexity Assessment  (Low | Medium | High)
    ↓
Routing Engine
    ↓
Model Selection  →  Cheap (gpt-4o-mini) | Fast (gpt-4.1-mini) | Best (gpt-4.1)
    ↓
LiteLLM  (OpenAI-compatible proxy, single base URL)
    ↓
AI AssistView Response  (+ visible Routing Card)
```

---

## 4. Functional requirements

### 4.1 Prompt analysis & routing

- **FR-1** The app MUST analyze every user prompt and assign a **complexity**: `Low`,
  `Medium`, or `High`.
- **FR-2** The app MUST map complexity to exactly one **route**: `Cheap`, `Fast`, or `Best`.
- **FR-3** The app MUST map each route to its fixed model: `Cheap→gpt-4o-mini`,
  `Fast→gpt-4.1-mini`, `Best→gpt-4.1`.
- **FR-4** Primary analysis MUST use the **Cheap model (`gpt-4o-mini`) as an AI classifier**,
  prompted to return a structured route decision (route + complexity + short reason).
- **FR-5** When no API key/endpoint is configured, or the classifier call fails, the app
  MUST fall back to a **deterministic heuristic classifier** (keyword + length signals) so
  the routing experience remains fully functional offline.
- **FR-6** Low-confidence or unparseable classifier output MUST default to the **Fast** route.
- **FR-7** The chosen model MUST then be invoked **via LiteLLM** to produce the assistant
  response. In degraded/offline mode the response MAY be a clearly-labeled stub, but the
  routing decision MUST still be real.

### 4.2 Routing strategy (detection rules)

- **FR-8 (Cheap)** Route to **Cheap** when the prompt is short and matches simple intents:
  simple Q&A, **summarization**, grammar correction, basic rewrite.
  _Examples:_ "Summarize this note", "Correct this sentence", "Rewrite professionally".
- **FR-9 (Fast)** Route to **Fast** for normal productivity tasks: email drafting, content
  generation, general assistance, medium complexity.
  _Examples:_ "Draft a status update email", "Create a release note", "Generate a project summary".
- **FR-10 (Best)** Route to **Best** for technical reasoning, architecture discussion, code
  generation, complex analysis, or multi-step requests.
  _Examples:_ "Design an authentication architecture", "Compare Azure and AWS",
  "Generate a MAUI implementation plan".

### 4.3 Canonical scenarios (MUST pass)

| Scenario | User input | Task type | Complexity | Route | Model | Reason |
|----------|-----------|-----------|-----------|-------|-------|--------|
| **S1** | "Summarize this paragraph." | Summarization | Low | Cheap | `gpt-4o-mini` | Simple summarization task; cost-efficient model selected. |
| **S2** | "Draft a professional email requesting project approval." | Content Writing | Medium | Fast | `gpt-4.1-mini` | Balanced writing task; fast, quality response. |
| **S3** | "Compare microservices and monolithic architectures and provide implementation recommendations." | Reasoning | High | Best | `gpt-4.1` | Complex reasoning detected; highest-capability model. |

- **FR-11** The heuristic classifier MUST produce the exact routes above for S1–S3 so the
  demo is reproducible offline.

### 4.4 Conversation

- **FR-12** The app MUST present a conversation UI using **Syncfusion `SfAIAssistView`**,
  showing user prompts and assistant responses in order.
- **FR-13** Each assistant turn MUST be accompanied by a visible **routing decision**
  (see UI-2) containing Complexity, Route, Model, and Reason.
- **FR-13a** The transparency block shown to the user SHOULD follow this shape:

  ```
  System Analysis:
  ✓ Complexity: High
  ✓ Route: Best Model

  Selected Model:
  gpt-4.1

  Reason:
  Complex reasoning and architectural analysis detected.
  ```

### 4.5 Analytics (optional / stretch)

- **FR-14 (Optional)** The app MAY display an analytics panel tracking total requests and a
  per-route breakdown (Cheap / Fast / Best counts). If implemented, counts MUST update live
  after each prompt.

---

## 5. Non-functional requirements

- **NFR-1** Cross-platform: builds and runs on the repo's standard MAUI targets (net10.0).
- **NFR-2** Security: comply with `constitution.md` §3 — no secrets in source, no secret
  logging, HTTPS only, prompt treated as untrusted data.
- **NFR-3** Responsiveness: UI stays responsive during model calls (async/await, no UI-thread
  blocking); show a busy indicator while awaiting a response.
- **NFR-4** Resilience: network/timeout/HTTP errors are caught and surfaced as a friendly
  message; the app never crashes on a failed call.
- **NFR-5** Determinism of demo: the heuristic path yields stable routes for the documented
  examples and scenarios.
- **NFR-6** MVVM separation and code style consistent with existing repo samples.
- **NFR-7** Configurability: LiteLLM base URL and the three model names are configurable
  without recompiling (config/constants with safe defaults).

---

## 6. UI components

- **UI-1 — AI AssistView.** Primary conversation surface (`SfAIAssistView`). Handles prompt
  entry, request lifecycle, and renders responses.
- **UI-2 — Routing Card.** Shown after every prompt (a Syncfusion card). Displays:
  ```
  Prompt Analysis
  Complexity: <Low|Medium|High>
  Route:      <Cheap|Fast|Best>
  Model:      <gpt-4o-mini|gpt-4.1-mini|gpt-4.1>
  ```
- **UI-3 — Route chips.** Three chips `[Cheap] [Fast] [Best]`; the **selected** route is
  highlighted for the current turn. Chips are **read-only indicators**, not a manual picker.
- **UI-4 — Analytics panel (optional).** Requests processed + per-route counts, e.g.:
  ```
  Requests Processed: 18
  Cheap: 10   Fast: 5   Best: 3
  ```
- **UI-5 — Configuration affordance.** A non-blocking way to provide the API key/endpoint at
  runtime (field or settings), consistent with the no-secrets-in-source rule. Absence of a
  key triggers heuristic mode with a clear visual indicator.

---

## 7. Routing decision data contract

The analyzer/classifier returns an object with this shape (used by UI-2/UI-3 and analytics):

```
RouteDecision {
  Complexity : enum { Low, Medium, High }
  Route      : enum { Cheap, Fast, Best }
  Model      : string   // resolved from Route
  Reason     : string   // short, human-readable
  Source     : enum { Model, Heuristic }   // how the decision was made
}
```

---

## 8. Traceability

Every `FR-*`/`NFR-*`/`UI-*` here is implemented by one or more tasks in `tasks.md` and
verified by a check in `acceptance.md`. No requirement may be left unmapped.
