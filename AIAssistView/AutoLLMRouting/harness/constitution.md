# Constitution — Auto LLM Routing

These are the **non-negotiable principles and constraints** for the Auto LLM Routing
sample. Every design decision, every task, and every line of code must comply. When a
later document (spec/plan/tasks) conflicts with this constitution, **this document wins**
and the conflict must be raised before proceeding.

---

## 1. Mission

Build a .NET MAUI sample that **automatically routes a user's prompt to one of three GPT
models** based on analyzed task complexity, and makes that routing decision **visible**
inside a Syncfusion **AI AssistView** conversation. The sample must feel like a polished,
demoable product — the "wow factor" is that the user never picks a model, yet always sees
*why* a model was chosen.

---

## 2. Technology constraints

| Area | Constraint |
|------|------------|
| Framework | .NET MAUI, targeting **net10.0** (match the repo's existing samples — Android primary; iOS/MacCatalyst on non-Linux; Windows on Windows). |
| Language | C# with `ImplicitUsings` enabled and `Nullable` enabled. |
| UI toolkit | **Syncfusion .NET MAUI** controls. The conversation surface **must** be `Syncfusion.Maui.AIAssistView` (`SfAIAssistView`). |
| Supporting Syncfusion controls | Use Syncfusion controls for the routing card, chips, and analytics (e.g. `Syncfusion.Maui.Cards`, `Syncfusion.Maui.Buttons` / chips, `Syncfusion.Maui.Charts` for optional analytics). Prefer Syncfusion over hand-rolled UI. |
| Package versioning | Syncfusion packages referenced with `Version="*"` to match the repo convention. |
| LLM gateway | **LiteLLM** acting as an OpenAI-compatible proxy. The app talks to a single base URL and selects models by name. |
| Models | Exactly three GPT-family models (see §4). |
| Architecture | **MVVM**. Folders: `Views/`, `ViewModels/`, `Services/`, `Models/`, `Converters/` — mirroring the existing repo samples. |
| HTTP | Use `Microsoft.Extensions.Http` / `IHttpClientFactory` and dependency injection registered in `MauiProgram.cs`. |

---

## 3. Security principles (OWASP-aligned, mandatory)

1. **No secrets in source.** API keys, LiteLLM master keys, and endpoints must **never**
   be hard-coded or committed. Load from runtime configuration (environment variable,
   user-entered field, or an untracked local settings file that is `.gitignore`d).
2. **No secret logging.** Never log keys or full auth headers.
3. **Fail safe.** On missing/invalid credentials, the app degrades gracefully (see §5),
   it does not crash and does not leak error internals to the UI verbatim.
4. **Input is untrusted.** Treat prompt text as data, never interpolate it into code,
   shell, or SQL. The analyzer must not execute model output.
5. **Transport security.** All LLM calls over HTTPS.

---

## 4. The three models (fixed)

| Role | Model | Use for | Benefit |
|------|-------|---------|---------|
| **Cheap** | `gpt-4o-mini` | Simple Q&A, summaries, grammar/rewrite | Lowest cost, high volume |
| **Fast** | `gpt-4.1-mini` | General conversation, email/content drafting, everyday productivity | Fast, balanced quality — **default** |
| **Best** | `gpt-4.1` | Deep reasoning, technical/architecture, coding, complex multi-step | Highest quality & accuracy |

The **Cheap** model doubles as the **prompt analyzer / classifier agent**.

---

## 5. Behavioral principles

1. **AI chooses, not the user.** There is no manual model picker in the primary flow.
   (A read-only indicator of the chosen route is required; a manual override is **out of scope**.)
2. **Transparency is a feature.** Every assistant turn must be preceded/accompanied by a
   routing decision showing *Complexity*, *Route*, *Model*, and a short *Reason*.
3. **Graceful offline/degraded mode.** If no API key/endpoint is configured, the classifier
   must fall back to a **deterministic heuristic** (keyword/length based) so the routing UX
   is fully demoable without network. Responses in this mode may be stubbed/explained.
4. **Deterministic routing contract.** The classifier returns one of exactly three routes:
   `Cheap` | `Fast` | `Best`. Unknown/low-confidence results default to **Fast**.
5. **Cross-platform.** No platform-specific hacks that break other targets.

---

## 6. Quality bar

- Clean MVVM separation; no business logic in code-behind beyond view wiring.
- Builds without errors; no unused Syncfusion packages.
- UI is modern, clean, and accessible (labels, contrast, touch targets).
- Code matches the style/naming of existing samples in `maui-usecase-demos`.

---

## 7. Explicitly out of scope (v1)

- Manual model override UI.
- Streaming token-by-token responses (nice-to-have, not required).
- Persisting conversation history across launches.
- Authentication/user accounts.
- Non-GPT providers.
