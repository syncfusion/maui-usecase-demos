# Smart Movie Ticket Booking — Harness Document

**Purpose:** Define the contract an AI coding agent must satisfy to produce a working, design-faithful implementation of the Smart Movie Ticket Booking sample.

**Companion documents:**
- Functional & behavior specification → [SPECIFICATION.md](./SPECIFICATION.md)
- Development task breakdown → [TASKS.md](./TASKS.md)

---

## Table of Contents

1. [Functional Requirements (FR)](#1-functional-requirements-fr)
2. [Non-Functional Requirements (NFR)](#2-non-functional-requirements-nfr)
3. [Assumptions](#3-assumptions)
4. [Dependencies](#4-dependencies)
5. [Screen-Level Implementation Guidelines](#5-screen-level-implementation-guidelines)
6. [User Flow Diagrams](#6-user-flow-diagrams)
7. [State Management Requirements](#7-state-management-requirements)
8. [Service Layer Requirements](#8-service-layer-requirements)
9. [Mock API / Data Contract Definitions](#9-mock-api--data-contract-definitions)
10. [AI Integration Requirements](#10-ai-integration-requirements)
11. [Error Handling Scenarios](#11-error-handling-scenarios)
12. [Validation Requirements](#12-validation-requirements)
13. [Acceptance Criteria](#13-acceptance-criteria)
14. [User Stories](#14-user-stories)
15. [Milestones](#15-milestones)
16. [Appendix A — Screen-to-Control Quick Reference](#appendix-a--screen-to-control-quick-reference)
17. [Appendix B — Theming Tokens](#appendix-b--theming-tokens)
18. [Appendix C — Mock Catalog (Sample Seed)](#appendix-c--mock-catalog-sample-seed)

---

## 1. Functional Requirements (FR)

| ID | Requirement |
|---|---|
| FR-01 | App launches to Home Screen within 2 seconds on a mid-tier device. |
| FR-02 | Home displays a horizontal carousel of Now-Showing movies, each card with poster, title, genre, duration, rating, language. |
| FR-03 | Home provides working search, genre filter, and language filter that update the visible list reactively. |
| FR-04 | Tapping a movie card navigates to Movie Details with the correct movie loaded. |
| FR-05 | Movie Details displays all required fields and the "Book Tickets" CTA is enabled only after data is loaded. |
| FR-06 | Tapping "Book Tickets" navigates to Theater Selection for the selected movie. |
| FR-07 | Theater Selection shows ≥ 3 theaters for the selected movie, each with showtimes for the selected date. |
| FR-08 | Date selector switches showtimes for all theaters. |
| FR-09 | Tapping a showtime navigates to Seat Selection for that showtime. |
| FR-10 | Seat Selection shows a visual grid with at least 80 seats; available, occupied, selected, premium states are visually distinct. |
| FR-11 | At least 1 seat must be selected to enable "Proceed". |
| FR-12 | Pricing summary updates live as seats are toggled. |
| FR-13 | Tapping "Proceed" navigates to Booking Summary with all selections preserved. |
| FR-14 | Booking Summary shows movie, theater, showtime, seats, fees, total, and an "Apply Coupon" input. |
| FR-15 | "WELCOME10" coupon applies a 10% discount; any other input shows an invalid-coupon message. |
| FR-16 | Tapping "Proceed to Pay" navigates to Payment with the total amount. |
| FR-17 | Payment allows selection of UPI, Card, or Wallet and shows the appropriate form. |
| FR-18 | Tapping "Pay" shows a processing animation for ~2.5s. |
| FR-19 | On success, navigates to Booking Confirmation with booking ID, QR code, and ticket details. |
| FR-20 | On failure (5%), shows a retry popup; user can retry or change method. |
| FR-21 | Booking Confirmation allows "Download Ticket" (popup) and "Share Ticket" (share sheet). |
| FR-22 | "Back to Home" pops to root and resets `BookingSession`. |
| FR-23 | My Bookings is reachable from Home; it shows Upcoming and Past tabs. |
| FR-24 | A completed booking appears in My Bookings → Upcoming. |
| FR-25 | The AI Assistant is reachable from every primary screen via a persistent FAB or toolbar entry. |
| FR-26 | The AI Assistant answers all 6 example prompts in the spec without throwing. |
| FR-27 | AI Assistant preserves conversation history for the session. |
| FR-28 | App supports light and dark themes. |
| FR-29 | All async operations show shimmer/loading states. |
| FR-30 | App handles back-button navigation correctly (no orphaned pages). |

---

## 2. Non-Functional Requirements (NFR)

| ID | Requirement |
|---|---|
| NFR-01 | App must compile and run on Android (API 21+), iOS (15+), Mac Catalyst (15+), and Windows (10.0.17763+). |
| NFR-02 | No third-party paid UI library other than Syncfusion. |
| NFR-03 | All animations must run at 60 fps on a mid-tier device. |
| NFR-04 | No PII collection, no network calls beyond image fetches. |
| NFR-05 | All text in resources (no hard-coded user-facing strings in code-behind). |
| NFR-06 | `Nullable` enabled, no warnings in build. |
| NFR-07 | MVVM strictly enforced — no business logic in code-behind. |
| NFR-08 | Mock data loaded once; no re-loads on navigation. |
| NFR-09 | Theming via Syncfusion theme dictionary + custom keys. |
| NFR-10 | Respect platform safe areas (notch, status bar, gesture bar). |

---

## 3. Assumptions

- Users accept that this is a sample, not a production app.
- No real payment processing occurs.
- Movie posters are bundled assets.
- The device has a working network connection only if remote images are used.
- A single mock user profile is sufficient.
- The AI Assistant operates entirely over the in-memory catalog; no external LLM call is required.

---

## 4. Dependencies

| Dependency | Version | Reason |
|---|---|---|
| `Microsoft.Maui.Controls` | 10.0.80 | MAUI runtime. |
| `CommunityToolkit.Mvvm` | 8.4.0 | MVVM source generators. |
| `Syncfusion.Maui.Core` | 33.2.15 | Common Syncfusion controls. |
| `Syncfusion.Maui.ListView` | 33.2.15 | SfListView, SfCollectionView. |
| `Syncfusion.Maui.Buttons` | 33.2.15 | SfButton, SfSegmentedControl, SfRadioButton. |
| `Syncfusion.Maui.Popup` | 33.2.15 | SfPopup. |
| `Syncfusion.Maui.BottomSheet` | 33.2.15 | SfBottomSheet. |
| `Syncfusion.Maui.AIAssistView` | 33.2.15 | SfAIAssistView. |
| `Syncfusion.Maui.Cards` | 33.2.15 | SfCardView. |
| `Syncfusion.Maui.Chips` | 33.2.15 | SfChip. |
| `Syncfusion.Maui.ComboBox` | 33.2.15 | SfComboBox. |
| `Syncfusion.Maui.TabView` | 33.2.15 | SfTabView. |
| `Syncfusion.Maui.Barcode` | 33.2.15 | QR generator. |
| `Syncfusion.Maui.Charts` | 33.2.15 (optional) | Sparkline for popularity. |
| `Microsoft.Extensions.Logging.Debug` | 10.0.9 | Debug logging. |

All package versions are pinned; the AI agent must not bump MAUI major versions without explicit instruction.

---

## 5. Screen-Level Implementation Guidelines

Each page must:

- Use `ContentPage` as base.
- Inject its ViewModel via DI.
- Set `BindingContext` in the constructor.
- Override `OnAppearing` only to call `InitializeAsync` on the ViewModel.
- Use Shell query parameters for IDs (e.g., `?movieId=...`).
- Have a corresponding route registered in `AppShell.xaml`.

### Page Implementation Skeleton (Reference)

```csharp
public partial class MovieDetailsPage : ContentPage
{
    private readonly MovieDetailsViewModel _vm;

    public MovieDetailsPage(MovieDetailsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var movieId = GetQueryParameter("id");
        await _vm.InitializeAsync(movieId);
    }

    private string GetQueryParameter(string key) =>
        Shell.Current?.CurrentItem?.CurrentItem?.QueryParameters.GetValueOrDefault(key);
}
```

### Loading / Empty / Error States

Every list page must define three states:

- **Loading** — `SfShimmer.View` with placeholder shapes.
- **Empty** — illustration + headline + (optional) CTA.
- **Error** — `SfPopup` with Retry / Dismiss.

---

## 6. User Flow Diagrams

### 6.1 Primary Booking Flow

```
┌──────────────┐
│ App Launch   │
└──────┬───────┘
       ▼
┌──────────────┐  search / filter
│  Home Page   │ ─────────────────► (Filtered Home)
└──────┬───────┘
       │ tap card
       ▼
┌──────────────────┐   AI FAB
│ Movie Details    │ ─────────► AI Assistant Sheet
└──────┬───────────┘
       │ Book Tickets
       ▼
┌────────────────────────┐
│ Theater Selection      │  date + showtime chips
└──────┬─────────────────┘
       │ tap showtime
       ▼
┌────────────────────────┐
│ Seat Selection (≥1)    │  SeatControl grid + pricing
└──────┬─────────────────┘
       │ Proceed
       ▼
┌────────────────────────┐
│ Booking Summary        │  Apply coupon (WELCOME10)
└──────┬─────────────────┘
       │ Proceed to Pay
       ▼
┌────────────────────────┐
│ Payment (UPI/Card/Wallet)│
└──────┬─────────────────┘
       │ Pay
       ▼
┌────────────────────────┐
│ Processing Sheet (~2.5s)│
└──────┬─────────────────┘
       │ success
       ▼
┌────────────────────────┐  Download / Share / Back
│ Booking Confirmation   │
└──────┬─────────────────┘
       │ Back to Home
       ▼
┌──────────────┐
│  Home Page   │
└──────────────┘
```

### 6.2 Failure Branch — Payment

```
Payment → Pay
   │
   ▼
Processing Sheet (2.5s)
   │
   ├──── success ────► Booking Confirmation
   │
   └──── failure (5%) ────► SfPopup: "Payment failed"
                                  ├─ Retry ─► re-enter Processing
                                  └─ Change Method ─► back to Payment
```

### 6.3 AI Assistant Flow

```
Any Primary Screen
   │ tap AI FAB
   ▼
SfBottomSheet opens
   │
   ▼
SfAIAssistView shows
   ├─ Suggested chips (6)
   │     └─ tap chip ─► AiAssistantService.ProcessAsync(text)
   └─ Free-text input ─► same path
                          │
                          ▼
                  Intent Engine (keyword scoring)
                          │
                          ▼
                  Pull data from Movie/Booking services
                          │
                          ▼
                  Emit AssistMessage list (text + cards)
                          │
                          ▼
                  History persists in session
```

### 6.4 My Bookings Flow

```
Home → My Bookings icon
   │
   ▼
My Bookings Page (SfTabView)
   ├─ Upcoming tab ─► list of bookings with showtime ≥ now
   └─ Past tab     ─► list of bookings with showtime < now
                          │
                          ▼
                  tap card ─► Read-only ticket view
```

---

## 7. State Management Requirements

- One source of truth: `BookingSession` for in-progress, `BookingRepository` for completed.
- ViewModels must never directly mutate each other.
- Use `MessagingCenter` or an `EventAggregator` (custom) for cross-screen notifications such as "BookingSessionUpdated."
- Pages must never cache state in code-behind fields that survive navigation.
- All observable collections must be `ObservableCollection<T>`; never use `List<T>` for bound data.

### `BookingSession` Lifecycle

| Event | Action |
|---|---|
| App launch | session is `null`. |
| Tap movie card | session not yet created. |
| Tap showtime chip | `BookingSession.SetShowtime(showtime)`. |
| Seat toggled | `BookingSession.ToggleSeat(seat)`. |
| Coupon applied | `BookingSession.ApplyCoupon(code)`. |
| Payment success | `BookingRepository.Add(session.ToBooking())` then `BookingSession.Reset()`. |
| Back from confirmation | session is already reset. |

---

## 8. Service Layer Requirements

- Services are registered in `MauiProgram`.
- Services are stateless and thread-safe.
- Services expose `async` methods returning `Task<T>`.
- JSON deserialization uses `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`.
- All service methods that simulate I/O must include an artificial delay (`Task.Delay`) to make shimmer visible.
- Services must not throw on empty input; they return empty collections.
- Services must log to `Debug.WriteLine` (or `ILogger`) on failure and return safe defaults.

### Threading Rules

- All service methods use `ConfigureAwait(false)` internally.
- UI mutations after `await` are dispatched to the main thread automatically by MAUI bindings; no manual `MainThread.BeginInvokeOnMainThread` is required for property setters on `ObservableObject`.

---

## 9. Mock API / Data Contract Definitions

### 9.1 IMovieService

```csharp
public interface IMovieService
{
    Task<IReadOnlyList<Movie>> GetNowShowingAsync();
    Task<IReadOnlyList<Movie>> GetComingSoonAsync();
    Task<Movie> GetByIdAsync(string id);
    Task<IReadOnlyList<Movie>> SearchAsync(string query, string genre, string language);
    Task<IReadOnlyList<Movie>> GetByGenreAsync(string genre);
    Task<IReadOnlyList<Movie>> GetTopRatedAsync(int count = 5);
}
```

### 9.2 ITheaterService

```csharp
public interface ITheaterService
{
    Task<IReadOnlyList<Theater>> GetTheatersForMovieAsync(string movieId);
    Task<Theater> GetByIdAsync(string id);
}
```

### 9.3 IShowtimeService

```csharp
public interface IShowtimeService
{
    Task<IReadOnlyList<Showtime>> GetShowtimesAsync(string movieId, string theaterId, DateTime date);
    Task<Showtime> GetByIdAsync(string id);
}
```

### 9.4 IBookingService

```csharp
public interface IBookingService
{
    Task<Booking> CreateBookingAsync(BookingDraft draft, string paymentMethod);
    Task CancelAsync(string bookingId);
}
```

### 9.5 IPaymentService

```csharp
public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(decimal amount, string method, object details);
}

public record PaymentResult(bool Success, string TransactionId, string? FailureReason);
```

### 9.6 IBookingRepository

```csharp
public interface IBookingRepository
{
    IReadOnlyList<Booking> All { get; }
    Booking GetById(string id);
    void Add(Booking booking);
    event EventHandler BookingsChanged;
}
```

### 9.7 IAiAssistantService

```csharp
public interface IAiAssistantService
{
    Task<IReadOnlyList<AssistMessage>> ProcessAsync(string userInput, AiContext context);
}

public class AiContext
{
    public string? CurrentMovieId { get; init; }
    public string? CurrentTheaterId { get; init; }
    public string? CurrentShowtimeId { get; init; }
    public string CurrentScreenName { get; init; } = "Home";
}
```

### 9.8 JSON File → Model Mapping

| JSON file | Root | Records |
|---|---|---|
| `movies.json` | array of `Movie` | ≥ 20 |
| `theaters.json` | array of `Theater` | ≥ 8 |
| `showtimes.json` | array of `Showtime` | ≥ 100 |
| `cast.json` | dictionary `movieId → List<CastMember>` | per movie |
| `userprofile.json` | single `UserProfile` | 1 |

All files registered in `csproj` as `<MauiAsset>` with `LogicalName="<filename>"`.

---

## 10. AI Integration Requirements

### 10.1 Intent Coverage

The intent engine MUST handle, at minimum, these intents with ≥ 80% accuracy on the supported prompt list:

| Intent | Trigger keywords | Data source |
|---|---|---|
| Recommendation | recommend, suggest, looking for, what's good | `IMovieService.GetByGenreAsync`, `GetTopRatedAsync` |
| Comparison | compare, vs, versus, difference | `IMovieService.GetByIdAsync` (×2) |
| Movie Info | tell me about, what is, info, about | `IMovieService.GetByIdAsync` |
| Ratings / Top | top rated, highest, best rated | `IMovieService.GetTopRatedAsync` |
| Family / Kids | kids, family, children | filter by Genre="Family" or rating ≥ 7.5 |
| Booking Help | book, ticket, how do I, purchase | static steps |
| Genre Filter | action, comedy, drama, thriller, romance, sci-fi, family | `IMovieService.GetByGenreAsync` |
| Unknown | none matched | fallback message |

### 10.2 Conversation State

- All messages are stored in `AiAssistantViewModel.Messages` (`ObservableCollection<AssistMessage>`).
- The sheet never clears history during a session.
- Tapping a chip or sending a message appends a request message and then a response message.

### 10.3 Context Awareness

- When invoked from Movie Details, the assistant's greeting includes the current movie title.
- When invoked from Seat Selection, the assistant offers to "recommend the best view."
- When invoked from Payment, the assistant offers to "walk you through it."
- This is implemented by setting `AiContext.CurrentMovieId` / `CurrentScreenName` in each page's `OnAppearing`.

### 10.4 Response Templates

| Intent | Template |
|---|---|
| Recommendation | Text summary + 1–5 `MovieAssistMessage` cards. |
| Comparison | Two-column layout, 2 movie cards + comparison text. |
| Movie Info | Text synopsis + cast list + rating badge. |
| Ratings | Bulleted list of top-N movies with rating. |
| Family / Kids | Filtered recommendation list. |
| Booking Help | Numbered steps + "Start Booking" button. |
| Genre Filter | Filtered recommendation list with intro text. |
| Unknown | "I'm not sure I understood. Try one of the suggestions." |

### 10.5 Supported Prompt List (Must Pass)

- "Recommend a family movie."
- "Suggest an action movie."
- "Tell me about this movie."
- "Which movie has the highest rating?"
- "What movie is suitable for kids?"
- "How do I book tickets?"

---

## 11. Error Handling Scenarios

| Scenario | Behavior |
|---|---|
| JSON load failure | Log, show `SfPopup` "Data could not be loaded. Retry?" with retry action. |
| Empty result set | Render empty state with illustration and CTA. |
| Network image 404 | Show placeholder poster (`poster_placeholder.png`). |
| AI Assistant unknown intent | Respond with "I'm not sure I understood. Try one of the suggestions." |
| Payment failure | Show `SfPopup` with Retry / Change Method buttons. |
| Booking session invalid (e.g., navigated back from confirmation) | Reset to Home and clear stack. |
| Shell route missing parameter | Redirect to Home. |
| Concurrent seat toggles | Last write wins (acceptable in sample). |
| Theme toggle during navigation | Apply on next render; no crash. |
| Device back button mid-payment | Confirm via popup before discarding draft. |

---

## 12. Validation Requirements

| Field | Rule | Source of truth |
|---|---|---|
| Card number | 12–19 digits | `ValidationService.IsValidCardNumber` |
| UPI VPA | must contain `@` | `ValidationService.IsValidVpa` |
| CVV | 3–4 digits | `ValidationService.IsValidCvv` |
| Expiry | MM/YY, future date | `ValidationService.IsValidExpiry` |
| Coupon | only `WELCOME10` valid; case-insensitive | `PricingService.ApplyCoupon` |
| Seat count | 1–10 per booking | `SeatSelectionViewModel.MaxSeats` |
| Phone (if collected) | 10 digits | `ValidationService.IsValidPhone` |
| Search text | length 0–64 | enforced by `DebounceSearchBehavior` |

All validation messages must be shown inline next to the field (or below), never as blocking dialogs except for hard errors.

---

## 13. Acceptance Criteria

### 13.1 Build & Run

- **AC-B1:** `dotnet build -f net10.0-android` succeeds.
- **AC-B2:** `dotnet build -f net10.0-ios` succeeds (on macOS).
- **AC-B3:** `dotnet build -f net10.0-maccatalyst` succeeds (on macOS).
- **AC-B4:** `dotnet build -f net10.0-windows10.0.19041.0` succeeds (on Windows).
- **AC-B5:** App launches to Home within 2 s on a clean build.

### 13.2 Functional

- **AC-F1:** A user can complete a full booking in under 60 seconds.
- **AC-F2:** All 9 features from §5 of SPECIFICATION.md are accessible and functional.
- **AC-F3:** AI Assistant answers all 6 example prompts.
- **AC-F4:** A completed booking appears in My Bookings → Upcoming.
- **AC-F5:** Search and filters update the home list reactively.
- **AC-F6:** Coupon `WELCOME10` produces a 10% discount; others show invalid.

### 13.3 Visual / UX

- **AC-V1:** All screens follow the Stitch design language (card-based, generous spacing, brand color).
- **AC-V2:** Light and dark themes are supported and consistent.
- **AC-V3:** All async operations show a shimmer or spinner.
- **AC-V4:** Page transitions are smooth and themed.

### 13.4 Technical

- **AC-T1:** MVVM strictly enforced — no business logic in code-behind.
- **AC-T2:** All services are DI-registered and stateless.
- **AC-T3:** `Nullable` enabled, no build warnings.
- **AC-T4:** All JSON models round-trip through deserialization.
- **AC-T5:** All errors are caught and surfaced via `SfPopup` or inline message.

### 13.5 Content

- **AC-C1:** ≥ 20 movies (10 Now Showing, 10 Coming Soon).
- **AC-C2:** ≥ 8 theaters across ≥ 2 cities.
- **AC-C3:** ≥ 100 showtimes across 7 days.
- **AC-C4:** All movies have ≥ 4 cast members and a synopsis.

---

## 14. User Stories

> All stories follow the format: *As a [persona], I want to [action], so that [benefit].*

### Discovery
- **US-01** — As a casual moviegoer, I want to browse now-playing movies as cards, so that I can quickly pick one that interests me.
- **US-02** — As a casual moviegoer, I want to search by title, so that I can find a specific movie fast.
- **US-03** — As a casual moviegoer, I want to filter by genre and language, so that I can narrow down to movies that fit my taste.
- **US-04** — As a casual moviegoer, I want to see ratings and duration on each card, so that I can decide before opening details.

### Details
- **US-05** — As a cinephile, I want a detailed movie page with synopsis, cast, and languages, so that I can make an informed choice.
- **US-06** — As a cinephile, I want a trailer button, so that I can preview the movie (mocked in this sample).
- **US-07** — As a cinephile, I want to see a "Book Tickets" CTA, so that I can act on my interest.

### Theater & Showtime
- **US-08** — As an on-the-go booker, I want to see nearby theaters and showtimes, so that I can pick a convenient option.
- **US-09** — As a family planner, I want to see theater amenities and screen formats, so that I can pick a comfortable experience for my family.
- **US-10** — As a family planner, I want to switch dates, so that I can find a weekend show that works.

### Seat
- **US-11** — As a family planner, I want a visual seat map, so that I can pick seats together.
- **US-12** — As a casual moviegoer, I want to see seat pricing, so that I can stay within budget.
- **US-13** — As a casual moviegoer, I want the total to update live, so that I can see the impact of my choices.

### Booking & Payment
- **US-14** — As any user, I want to review my booking before paying, so that I can avoid mistakes.
- **US-15** — As a casual moviegoer, I want a coupon option, so that I can save money.
- **US-16** — As any user, I want a simple payment flow, so that I can complete the booking quickly.

### Confirmation
- **US-17** — As any user, I want a confirmation screen with QR code and ticket details, so that I can present it at the theater.
- **US-18** — As any user, I want to share or download my ticket, so that I can have a backup.

### History
- **US-19** — As an on-the-go booker, I want to see all my past and upcoming bookings, so that I can revisit them.
- **US-20** — As any user, I want Upcoming and Past tabs, so that I can quickly find what I need.

### AI Assistant
- **US-21** — As a family planner, I want to ask "recommend a family movie," so that I can save browsing time.
- **US-22** — As a cinephile, I want to compare two movies, so that I can decide between them.
- **US-23** — As a casual moviegoer, I want to see top-rated movies this week, so that I can pick something good.
- **US-24** — As any user, I want the assistant to be reachable from every screen, so that I can ask for help at any time.

---

## 15. Milestones

| Milestone | Tasks (see TASKS.md) | Definition of Done |
|---|---|---|
| **M1 — Foundation** | T1.1–T1.5 | Project builds, theme works, mock data loads. |
| **M2 — Discovery** | T2.1–T2.4 | Home and Movie Details fully functional. |
| **M3 — Booking Flow** | T3.1–T3.6 | Full end-to-end booking works on all platforms. |
| **M4 — AI Assistant** | T4.1–T4.3 | AI assistant answers all example prompts; available globally. |
| **M5 — Polish** | T5.1–T5.3, T6.1–T6.3, T7.1–T7.3, T8.1–T8.4 | All acceptance criteria met. |

---

## Appendix A — Screen-to-Control Quick Reference

| Screen | Syncfusion Controls |
|---|---|
| Home | `SfCollectionView` (carousel, grid), `SfListView` (coming soon), `SfChip` (genre), `SfComboBox` (language), `SfEffectsView`, `SfShimmer`. |
| Movie Details | `SfCardView`, `SfBadgeView`, `SfChip`, `SfCollectionView` (cast), `SfGradientView`, `SfEffectsView`, `SfPopup` (trailer). |
| Theater Selection | `SfListView` / `SfCollectionView`, `SfChip` (date), `SfChip` (showtime), `SfBadgeView`, `SfEffectsView`. |
| Seat Selection | `SfCardView`, `SfEffectsView`, `SfButton` (proceed), custom `SeatControl`. |
| Booking Summary | `SfCardView`, `SfTextInputLayout` (coupon), `SfButton` (apply, proceed). |
| Payment | `SfSegmentedControl`, `SfRadioButton`, `SfTextInputLayout`, `SfBottomSheet` (processing), `SfPopup` (failure). |
| Booking Confirmation | `SfCardView`, `SfEffectsView`, `SfButton` (download, share, back), QR generator. |
| My Bookings | `SfTabView`, `SfListView` / `SfCollectionView`, `SfBadgeView`, `SfEffectsView`. |
| AI Assistant | `SfBottomSheet`, `SfAIAssistView`, `SfChip` (suggestions), `SfCardView` (movie cards in responses). |

---

## Appendix B — Theming Tokens

```
Primary:          #FF3D5A   (CineBook red — buy tickets, primary CTAs)
PrimaryDark:      #C92E47
Secondary:        #1F1F2E   (background, dark surface)
Accent:           #FFB300   (premium tier, highlights)
Success:          #2EC27E
Error:            #E53935
Info:             #1E88E5

BackgroundLight:  #FFFFFF
BackgroundDark:   #121212
SurfaceLight:     #F7F7FA
SurfaceDark:      #1E1E1E
OnSurfaceLight:   #1F1F1F
OnSurfaceDark:    #E6E1E6
```

These tokens are added to `Resources/Styles/Colors.xaml` and consumed via `DynamicResource` in XAML, so the `ThemeService` can swap them at runtime.

---

## Appendix C — Mock Catalog (Sample Seed)

For developer convenience, the JSON should include movies like:

- *Stellar Horizon* — Sci-Fi, English, 2h 28m, rating 8.6.
- *Laughing in Mumbai* — Comedy, Hindi, 2h 12m, rating 8.1.
- *Kandukondain 2* — Drama, Tamil, 2h 35m, rating 8.3.
- *Last Heist* — Thriller, English, 1h 58m, rating 7.4.
- *Sitaara* — Family, Telugu, 1h 50m, rating 8.8.
- *Heartbeats* — Romance, Malayalam, 2h 04m, rating 7.9.
- *Galactic Wars: Dawn* — Sci-Fi, English (3D), 2h 45m, rating 9.1.
- *Veera Saga* — Action, Tamil, 2h 20m, rating 8.0.
- *Yellow Umbrella* — Drama, Kannada, 1h 47m, rating 8.5.
- *Cozy Christmas* — Family, English, 1h 42m, rating 7.2.

(plus 10 more to satisfy the catalog target.)

Theaters in `theaters.json` should be spread across at least 2 cities with realistic Indian names (PVR, INOX, Cinepolis, Carnival) — for **fictional sample data** only.

---

*See [TASKS.md](./TASKS.md) for the full 8-phase development task breakdown that satisfies this harness.*