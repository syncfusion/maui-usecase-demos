# Acceptance Criteria — Auto LLM Routing

**Status:** Mandatory. The sample is **done** only when every check below passes (optional
items are noted). Each check references the `spec.md` requirement(s) it verifies. Use the
Result column to record PASS/FAIL during the acceptance pass (task **T-20**).

Legend: **M** = mandatory · **O** = optional/stretch

---

## A. Routing correctness (canonical scenarios)

| # | Check | Spec | M/O | Result |
|---|-------|------|-----|--------|
| A1 | Input "Summarize this paragraph." → Complexity **Low**, Route **Cheap**, Model **gpt-4o-mini**. | S1, FR-8, FR-11 | M | |
| A2 | Input "Draft a professional email requesting project approval." → Complexity **Medium**, Route **Fast**, Model **gpt-4.1-mini**. | S2, FR-9, FR-11 | M | |
| A3 | Input "Compare microservices and monolithic architectures and provide implementation recommendations." → Complexity **High**, Route **Best**, Model **gpt-4.1**. | S3, FR-10, FR-11 | M | |
| A4 | Each route maps to exactly its fixed model (Cheap→gpt-4o-mini, Fast→gpt-4.1-mini, Best→gpt-4.1). | FR-3 | M | |
| A5 | Unparseable / low-confidence classifier output defaults to **Fast**. | FR-6 | M | |
| A6 | Additional "Cheap" examples (correct a sentence / rewrite professionally) route to Cheap; "Best" examples (design auth architecture / compare Azure & AWS / MAUI implementation plan) route to Best. | FR-8, FR-10 | M | |

---

## B. Analysis & model invocation

| # | Check | Spec | M/O | Result |
|---|-------|------|-----|--------|
| B1 | Every prompt is classified into Low/Medium/High complexity. | FR-1 | M | |
| B2 | When a key/endpoint is configured, the **gpt-4o-mini** classifier is used and its decision carries `Source=Model`. | FR-4 | M | |
| B3 | The resolved model is invoked **via LiteLLM** (single OpenAI-compatible base URL). | FR-7 | M | |
| B4 | With **no** key/endpoint (or on classifier failure), the **heuristic** classifier is used (`Source=Heuristic`) and routing still works. | FR-5 | M | |

---

## C. Transparency UI

| # | Check | Spec | M/O | Result |
|---|-------|------|-----|--------|
| C1 | Conversation renders in **Syncfusion `SfAIAssistView`** with prompts and responses in order. | UI-1, FR-12 | M | |
| C2 | Each assistant turn shows a routing decision (Complexity, Route, Model, Reason). | FR-13 | M | |
| C3 | The analysis block matches the FR-13a shape (System Analysis / Selected Model / Reason). | FR-13a | M | |
| C4 | **Routing Card** displays Complexity / Route / Model and updates after every prompt. | UI-2 | M | |
| C5 | **Route chips** `[Cheap][Fast][Best]` show, with the selected route highlighted; chips are read-only (no manual override). | UI-3 | M | |
| C6 | Heuristic/offline mode shows a visible indicator; a non-blocking config affordance exists for key/endpoint. | UI-5 | M | |
| C7 | **Analytics panel** shows Requests Processed + Cheap/Fast/Best counts, updating live. | UI-4, FR-14 | O | |

---

## D. Non-functional

| # | Check | Spec | M/O | Result |
|---|-------|------|-----|--------|
| D1 | No secrets (API keys/endpoints) are hard-coded or committed; keys are never logged. | NFR-2, Constitution §3 | M | |
| D2 | UI stays responsive during calls (async) with a busy indicator. | NFR-3 | M | |
| D3 | Network/timeout/HTTP/parse errors are caught and surfaced as a friendly message; no crash. | NFR-4 | M | |
| D4 | LiteLLM base URL and the three model names are configurable without recompiling. | NFR-7 | M | |
| D5 | Heuristic path is deterministic for the documented examples/scenarios. | NFR-5 | M | |
| D6 | MVVM separation and style consistent with existing repo samples; builds cleanly. | NFR-6, NFR-1 | M | |

---

## E. Engineering hygiene

| # | Check | Spec | M/O | Result |
|---|-------|------|-----|--------|
| E1 | Unit tests cover routing engine + heuristic classifier (S1/S2/S3 + default-Fast) and pass offline. | T-07, FR-11 | M | |
| E2 | App README documents purpose, routing behavior, config (env var / in-app), and offline mode — **no secrets**. | T-19 | M | |
| E3 | All mandatory tasks in `tasks.md` are DONE; any skipped optional task is explicitly noted. | tasks.md | M | |

---

## Sign-off

- All **M** checks PASS → sample accepted.
- Any **M** check FAIL → return to the owning task in `tasks.md`, fix, and re-run this pass.
- **O** checks may be deferred; record them as "skipped (optional)" if not implemented.
