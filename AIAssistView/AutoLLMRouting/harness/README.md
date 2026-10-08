# Auto LLM Routing — AI Development Harness

This folder is the **spec-driven development harness** for the **Auto LLM Routing**
sample: a .NET MAUI application built with the **Syncfusion AI AssistView** and other
Syncfusion MAUI controls that **automatically routes user prompts to different GPT
models** based on task complexity — optimizing cost, speed, and quality without
manual model selection.

The harness is the single source of truth an AI coding agent (or a human developer)
consumes **before** writing any application code. Every document below is **mandatory**
reading. Implementation must not begin until the spec requirements and task details in
this harness are understood and satisfied.

---

## What this sample demonstrates

- **LiteLLM model routing** — one OpenAI-compatible endpoint fronting three GPT models.
- **AI-powered prompt analysis** — a cheap "analyzer" model classifies each prompt.
- **Cost optimization** — simple prompts go to the cheapest capable model.
- **Performance optimization** — everyday prompts go to a fast, balanced model.
- **Syncfusion AI AssistView integration** — conversation UI plus a visible routing card.

### Key differentiator from the existing LiteLLM sample

| | Existing LiteLLM sample | This sample (Auto LLM Routing) |
|---|---|---|
| Model choice | **User** picks the model manually | **AI** picks the model automatically |
| Goal | Compare models side by side | Optimize cost, speed & quality automatically |

Existing reference sample:
<https://github.com/SyncfusionExamples/How-to-integrate-LiteLLM-with-AI-AssistView>

---

## Harness documents (read in this order)

| # | Document | Purpose | Status |
|---|----------|---------|--------|
| 1 | [`constitution.md`](./constitution.md) | Non-negotiable principles, tech stack, and constraints the implementation must obey. | **Mandatory** |
| 2 | [`spec.md`](./spec.md) | Functional & non-functional **requirements**, routing rules, the three canonical scenarios, and UI component spec. | **Mandatory** |
| 3 | [`plan.md`](./plan.md) | Technical approach: architecture, project layout, services, data flow, and interfaces. | **Mandatory** |
| 4 | [`tasks.md`](./tasks.md) | Ordered, actionable **task details** that implement the spec. Each task has IDs, dependencies, and done-criteria. | **Mandatory** |
| 5 | [`acceptance.md`](./acceptance.md) | Acceptance criteria and the verification checklist used to sign off the sample. | **Mandatory** |

> **Rule:** `spec.md` (requirements) and `tasks.md` (task details) are **mandatory** inputs.
> No implementation task may be marked done unless its linked requirement in `spec.md`
> and its acceptance criterion in `acceptance.md` are both satisfied.

---

## Target placement of the generated app

```
AIAssistView/
  AutoLLMRouting/
    harness/              <-- you are here (specs + tasks)
    AutoLLMRouting/       <-- MAUI project (created during implementation)
    AutoLLMRouting.sln    <-- solution (created during implementation)
```

---

## How an AI agent should use this harness

1. Read `constitution.md` → internalize constraints (MAUI version, Syncfusion, LiteLLM, no secrets in source).
2. Read `spec.md` → understand **what** to build and the exact routing behavior.
3. Read `plan.md` → understand **how** it is architected.
4. Execute `tasks.md` **in order**, honoring dependencies; keep a running task status.
5. Validate against `acceptance.md` before declaring the sample complete.

If any requirement is ambiguous or conflicts with a constraint, **stop and clarify**
rather than guessing.
