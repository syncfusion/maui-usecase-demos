# Technical Plan — Auto LLM Routing

**Status:** Mandatory. Defines **how** the sample is architected to satisfy `spec.md`
within the limits of `constitution.md`. Tasks in `tasks.md` implement this plan.

---

## 1. Solution & project layout

```
AIAssistView/AutoLLMRouting/
  AutoLLMRouting.sln
  AutoLLMRouting/
    AutoLLMRouting.csproj
    MauiProgram.cs                 # DI registration (HttpClient, services, VM, pages)
    App.xaml(.cs)
    AppShell.xaml(.cs)
    Models/
      ComplexityLevel.cs           # enum: Low, Medium, High
      ModelRoute.cs                # enum: Cheap, Fast, Best
      DecisionSource.cs            # enum: Model, Heuristic
      RouteDecision.cs            # Complexity, Route, Model, Reason, Source
      ChatMessage.cs               # role, text, optional RouteDecision
      RoutingOptions.cs            # base URL + 3 model names (configurable)
    Services/
      IPromptClassifier.cs
      ModelPromptClassifier.cs     # uses gpt-4o-mini via LiteLLM to classify
      HeuristicPromptClassifier.cs # offline keyword/length fallback
      IRoutingEngine.cs
      RoutingEngine.cs             # complexity -> route -> model, default Fast
      ILiteLLMClient.cs
      LiteLLMClient.cs             # OpenAI-compatible chat/completions over HTTP
      IChatOrchestrator.cs
      ChatOrchestrator.cs          # analyze -> route -> call -> build response
      IAppConfig.cs / AppConfig.cs # resolves key/endpoint at runtime (no secrets in src)
    ViewModels/
      ChatViewModel.cs             # bound to AI AssistView; exposes RouteDecision + analytics
    Views/
      ChatPage.xaml(.cs)           # SfAIAssistView + Routing Card + chips (+ analytics)
    Converters/
      RouteToColorConverter.cs     # highlight selected chip
      BoolToVisibility / etc. as needed
    Resources/                     # icons, fonts, styles (standard MAUI)
```

Mirrors the MVVM folder layout used by existing `maui-usecase-demos` samples.

---

## 2. Dependencies (`.csproj`)

- `Microsoft.Maui.Controls` (`$(MauiVersion)`)
- `Microsoft.Extensions.Http` (for `IHttpClientFactory`)
- `Microsoft.Extensions.Logging.Debug`
- `Syncfusion.Maui.AIAssistView` (`*`) — conversation UI (**required**)
- `Syncfusion.Maui.Core` (`*`)
- `Syncfusion.Maui.Cards` (`*`) — routing card (UI-2)
- `Syncfusion.Maui.Buttons` (`*`) — chips/selection (UI-3)
- `Syncfusion.Maui.Charts` (`*`) — optional analytics (UI-4)

Register the Syncfusion license + `ConfigureSyncfusionCore()` as per Syncfusion MAUI setup.

---

## 3. Component responsibilities

### 3.1 `IPromptClassifier` → complexity + route
- **ModelPromptClassifier** (primary, FR-4): sends the user prompt to **`gpt-4o-mini`**
  via LiteLLM with a system prompt instructing it to reply with **strict JSON**:
  `{ "complexity": "...", "route": "...", "reason": "..." }`. Parse defensively; on any
  parse/HTTP failure, signal fallback.
- **HeuristicPromptClassifier** (fallback, FR-5/FR-11): pure-C# rules —
  - **Cheap:** short prompt and/or keywords like *summarize, summarise, tl;dr, correct,
    grammar, rewrite, rephrase, fix this sentence*.
  - **Best:** keywords like *architecture, compare, design, implement, code, algorithm,
    microservices, monolith, reasoning, trade-off, step-by-step, plan*, or long/multi-sentence
    prompts.
  - **Fast:** everything else (default), incl. *draft, email, write, generate, create, summary
    of a project* at medium length.
  - Must deterministically yield S1→Cheap, S2→Fast, S3→Best.

### 3.2 `IRoutingEngine` (FR-2/FR-3/FR-6)
- Pure mapping with no I/O: complexity/route → concrete model name from `RoutingOptions`.
- Unknown/low-confidence → **Fast**.

### 3.3 `ILiteLLMClient` (FR-7)
- Thin OpenAI-compatible client: `POST {baseUrl}/chat/completions` with `model`, `messages`.
- `IHttpClientFactory`-managed `HttpClient`; `Authorization: Bearer <key>` only when a key
  is present. Timeouts + try/catch → typed result (NFR-4). Never logs the key (NFR-2).

### 3.4 `IChatOrchestrator`
- Orchestrates one turn: `classify(prompt)` → `route` → `LiteLLM.Complete(model, prompt)` →
  assemble `ChatMessage` + `RouteDecision`. Chooses model vs heuristic classifier and sets
  `RouteDecision.Source`. In offline mode returns a clearly-labeled stub answer but a **real**
  routing decision.

### 3.5 `ChatViewModel`
- Commands: `SendPrompt`. Observable: `Messages`, `CurrentDecision`, `IsBusy`,
  analytics counters (`TotalRequests`, `CheapCount`, `FastCount`, `BestCount`), `IsHeuristicMode`.
- Updates analytics after each turn (FR-14).

### 3.6 Config (`IAppConfig`, NFR-7, security)
- Resolution order: in-app settings field (UI-5) → environment variable
  (`LITELLM_BASE_URL`, `LITELLM_API_KEY`, optional model-name overrides) → safe defaults
  (base URL default + the three fixed model names; **no key default**).
- No key present ⇒ `IsHeuristicMode = true`.

---

## 4. Data flow (one turn)

```
ChatPage (SfAIAssistView) --prompt--> ChatViewModel.SendPrompt
   -> ChatOrchestrator.ProcessAsync(prompt)
        -> IPromptClassifier.ClassifyAsync(prompt)   // model or heuristic
             -> RouteDecision{ Complexity, Route, Reason, Source }
        -> IRoutingEngine.ResolveModel(Route)        // -> model name
        -> ILiteLLMClient.CompleteAsync(model, prompt) // or stub if offline
   <- ChatMessage{ assistant text } + RouteDecision
ChatViewModel -> update Messages, CurrentDecision, analytics
ChatPage -> render response, Routing Card (UI-2), highlight chip (UI-3), analytics (UI-4)
```

---

## 5. UI composition (`ChatPage.xaml`)

- Root layout: AI AssistView fills the conversation area.
- Routing Card (`SfCard`) bound to `CurrentDecision` — visible after each prompt (UI-2).
- Chips row (three `SfChip`/toggle buttons) bound to `CurrentDecision.Route` via
  `RouteToColorConverter` to highlight the selected one (UI-3, read-only).
- Optional analytics panel bound to the counters (UI-4) — a compact card or small chart.
- Busy indicator bound to `IsBusy` (NFR-3); heuristic-mode banner bound to `IsHeuristicMode`.
- Consider responsive desktop/mobile layouts as other repo samples do (not mandatory).

---

## 6. Classifier contract (model system prompt)

System prompt instructs `gpt-4o-mini` to return **only** JSON:

```
You are a routing classifier. Classify the user prompt.
Return ONLY compact JSON: {"complexity":"Low|Medium|High","route":"Cheap|Fast|Best","reason":"<=15 words"}.
Cheap = short/simple Q&A, summarization, grammar, basic rewrite.
Fast  = everyday productivity, email/content drafting, general help.
Best  = technical reasoning, architecture, code generation, complex multi-step analysis.
```

Parser validates enums; invalid → heuristic/Fast (FR-6).

---

## 7. Error handling & degraded mode

- No key/endpoint ⇒ heuristic classifier + stubbed answer labeled
  "(Offline demo — model not called)"; routing UI fully functional (FR-5, NFR-5).
- HTTP/timeout/parse errors ⇒ caught, friendly message, no crash (NFR-4); classifier errors
  fall back to heuristic, not failure.

---

## 8. Testing approach

- Heuristic classifier and routing engine are pure/deterministic → unit-testable without
  network. At minimum verify S1→Cheap, S2→Fast, S3→Best and the default-Fast rule.
- Manual run-through on at least one platform to confirm AI AssistView + Routing Card +
  chips + (optional) analytics render and update per turn.

---

## 9. Build / run notes (sandbox caveat)

- MAUI workloads/emulators are **not** expected to run in the Linux sandbox. The agent should
  verify a build where feasible (e.g. `dotnet build` for a buildable TFM) and otherwise rely on
  static correctness + unit tests for the pure logic. Full device/emulator run is a human step.
