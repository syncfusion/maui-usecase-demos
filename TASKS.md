# Smart Movie Ticket Booking — Development Tasks (Short, UI-Focused)

**Purpose:** Lean, UI-first task plan for building the Smart Movie Ticket Booking sample. Phases for testing/validation, final polish, and data/image asset work are intentionally omitted — those are handled manually outside the task list.

**Companion documents:**
- Functional & behavior specification → [SPECIFICATION.md](./SPECIFICATION.md)
- Requirements, validation, error handling, acceptance criteria → [HARNESS.md](./HARNESS.md)

> **Note on Stitch design:** The `https://stitch.withgoogle.com/projects/14368844853394156269` URL returns a JavaScript SPA shell that requires Google sign-in, so the rendered design cannot be fetched programmatically. Tasks in this file reference "Stitch design" based on the description provided in the project brief (modern card-based mobile UI inspired by TicketNew/BookMyShow, brand color, generous spacing, dark/light theme). Confirm the brand color, type scale, and any unique layout details before implementation.

---

## Task Index

### Phase 1 — Foundation
- [T1.1 Project Bootstrap](#t11-project-bootstrap)
- [T1.2 Theme Service & Colors](#t12-theme-service--colors)
- [T1.3 Project Structure & DI Wiring](#t13-project-structure--di-wiring)
- [T1.4 Models & Mock Data](#t14-models--mock-data)

### Phase 2 — Core UI Development
- [T2.1 App Shell & Navigation](#t21-app-shell--navigation)
- [T2.2 Reusable UI Library](#t22-reusable-ui-library)
- [T2.3 Home Screen](#t23-home-screen)
- [T2.4 Movie Details Screen](#t24-movie-details-screen)

### Phase 3 — Booking Workflow
- [T3.1 Theater Selection Screen](#t31-theater-selection-screen)
- [T3.2 Seat Selection Screen](#t32-seat-selection-screen)
- [T3.3 Booking Summary Screen](#t33-booking-summary-screen)
- [T3.4 Payment Screen](#t34-payment-screen)
- [T3.5 Booking Confirmation Screen](#t35-booking-confirmation-screen)
- [T3.6 My Bookings Screen](#t36-my-bookings-screen)

### Phase 4 — AI Movie Assistant
- [T4.1 Intent Engine](#t41-intent-engine)
- [T4.2 AI AssistView UI](#t42-ai-assistview-ui)
- [T4.3 Global AI Entry Points](#t43-global-ai-entry-points)

### Phase 5 — Wire-up & Smoke Pass
- [T5.1 Booking Session & Flow](#t51-booking-session--flow)
- [T5.2 End-to-End Smoke Walk](#t52-end-to-end-smoke-walk)

---

## Phase 1 — Foundation

### T1.1 Project Bootstrap
- **Description:** Create a new .NET MAUI project `SmartMovieBooking`. Add the required Syncfusion NuGet packages and `CommunityToolkit.Mvvm`. Wire `Syncfusion.Maui.Core` theming in `MauiProgram`. Confirm `dotnet build` succeeds.
- **Dependencies:** none.
- **Deliverables:** Compilable project; empty `HomePage` displays "Smart Movie Ticket Booking".
- **Acceptance:** `dotnet build` succeeds; app launches to the placeholder home.

### T1.2 Theme Service & Colors
- **Description:** Add `ThemeService` for light/dark toggle (reuse the pattern from the repo's `EmployeeDirectory` sample). Author `Resources/Styles/Colors.xaml` and `Styles.xaml` with the Stitch brand palette — pick one accent (e.g. `#E63946` cinematic red or `#6750A4` Material violet) and a neutral surface scale; expose them as `Theme*` keys.
- **Dependencies:** T1.1.
- **Deliverables:** `Services/ThemeService.cs`, `Resources/Styles/Colors.xaml`, `Resources/Styles/Styles.xaml`.
- **Acceptance:** Toggling theme updates both MAUI `UserAppTheme` and Syncfusion `VisualTheme`; primary buttons/cards pick up the brand color.

### T1.3 Project Structure & DI Wiring
- **Description:** Create the folder skeleton (`Models/`, `Services/`, `ViewModels/`, `Views/`, `Controls/`, `Converters/`, `Behaviors/`, `Resources/Raw/`). Register all services and ViewModels in `MauiProgram.cs`. Add a `BaseViewModel : ObservableObject` with `IsBusy` / `IsRefreshing` and an async `InitializeAsync` helper.
- **Dependencies:** T1.1.
- **Deliverables:** Folder layout, DI registrations, `BaseViewModel`.
- **Acceptance:** `MauiApp` builds and resolves all registered services.

### T1.4 Models & Mock Data
- **Description:** Author the POCOs (`Movie`, `CastMember`, `Theater`, `Screen`, `Showtime`, `Seat`, `Booking`, `BookingDraft`, `UserProfile`) and seed them inline as `IReadOnlyList<…>` constants inside their respective services — no JSON loader required. Use realistic Indian-context data (PVR, INOX, Cinepolis theater names; Bollywood, Tollywood, Kollywood titles) so the UI feels grounded.
- **Dependencies:** T1.3.
- **Deliverables:** Models under `Models/`; data factories inside `Services/*Service.cs`.
- **Acceptance:** Each service returns a non-empty collection on first call.

---

## Phase 2 — Core UI Development

### T2.1 App Shell & Navigation
- **Description:** Build `AppShell.xaml` with two tabs (Home, My Bookings). Register Shell routes for `movieDetails`, `theaterSelection`, `seatSelection`, `bookingSummary`, `payment`, `bookingConfirmation`, and a modal route `aiAssistant`. Implement a small `INavigationService` helper that supports query parameters (`?movieId=…`).
- **Dependencies:** T1.3.
- **Deliverables:** `AppShell.xaml(.cs)`, `Services/NavigationService.cs`.
- **Acceptance:** App boots to Home tab; pushing routes navigates correctly with parameters.

### T2.2 Reusable UI Library
- **Description:** Build the shared visuals: `MovieCardView` (poster + title + meta strip), `TheaterCardView`, `ShowtimeChip`, `RatingBadge`, `SeatControl` (square with 4 visual states), `QrCodeView` (simple `GraphicsView` QR generator), and a `MoviePosterPlaceholder` (gradient + initials) for missing images. Add converters: `SeatStateToColorConverter`, `InverseBoolConverter`, `CurrencyConverter`, `DurationConverter`, `FirstValidationErrorConverter`.
- **Dependencies:** T1.2.
- **Deliverables:** Files under `Controls/`, `Converters/`.
- **Acceptance:** Each control renders correctly when previewed against sample data in a debug page.

### T2.3 Home Screen
- **Description:** Build `HomePage` with: top app bar (location chip + AI FAB + bookings icon), a `SfCollectionView` horizontal carousel for **Now Showing** with `MovieCardView` items, a "Coming Soon" row, search bar, and genre / language chip filters. Tapping a card pushes `MovieDetailsPage` with `?movieId=`.
- **Dependencies:** T2.1, T2.2, T1.4.
- **Deliverables:** `Views/HomePage.xaml(.cs)`, `ViewModels/HomeViewModel.cs`.
- **Acceptance:** Carousel scrolls horizontally, search and filters update the list, tapping a card navigates to details.

### T2.4 Movie Details Screen
- **Description:** Build `MovieDetailsPage`: hero banner with gradient overlay, meta chip strip (genre • duration • rating • language), synopsis, horizontal cast row, languages chips, and a sticky bottom action bar with outlined "▶ Trailer" (`SfPopup` "Trailer unavailable in this sample") and filled "Book Tickets".
- **Dependencies:** T2.3, T2.2.
- **Deliverables:** `Views/MovieDetailsPage.xaml(.cs)`, `ViewModels/MovieDetailsViewModel.cs`.
- **Acceptance:** Hero renders, trailer popup opens, "Book Tickets" pushes `TheaterSelectionPage`.

---

## Phase 3 — Booking Workflow

### T3.1 Theater Selection Screen
- **Description:** Build `TheaterSelectionPage`: header with movie thumbnail, horizontal date strip (next 7 days, "Today"/"Tomorrow" labels), list of theaters via `SfListView`. Each theater card shows name, distance, amenities (icons), and `SfChip`-style showtime buttons. Tapping a showtime pushes `SeatSelectionPage`.
- **Dependencies:** T1.4, T2.2.
- **Deliverables:** `Views/TheaterSelectionPage.xaml(.cs)`, `ViewModels/TheaterSelectionViewModel.cs`.
- **Acceptance:** Changing date refreshes showtimes; sold-out chips disabled; tapping a showtime navigates.

### T3.2 Seat Selection Screen
- **Description:** Build `SeatSelectionPage`: header (theater + screen + showtime), "All eyes this way" arc, seat grid built with `Bindable Layout` or `GraphicsView`, legend strip, and a sticky pricing summary card (count • subtotal • convenience fee ₹30 • total). Each `SeatControl` tap toggles selection (max 10). Proceed button disabled until ≥ 1 seat selected.
- **Dependencies:** T3.1, T2.2.
- **Deliverables:** `Views/SeatSelectionPage.xaml(.cs)`, `ViewModels/SeatSelectionViewModel.cs`, `Controls/SeatControl.cs`.
- **Acceptance:** All 4 seat states visually distinct; total updates live; proceed navigates with seats in `BookingSession`.

### T3.3 Booking Summary Screen
- **Description:** Build `BookingSummaryPage`: movie card, theater/showtime card, seat chips card, coupon input ("WELCOME10" = 10% off, anything else shows "Invalid coupon"), price breakdown card, sticky "Proceed to Pay ₹X" button.
- **Dependencies:** T3.2, T2.2.
- **Deliverables:** `Views/BookingSummaryPage.xaml(.cs)`, `ViewModels/BookingSummaryViewModel.cs`.
- **Acceptance:** WELCOME10 applies discount; invalid codes show error message; CTA disabled at total ≤ 0.

### T3.4 Payment Screen
- **Description:** Build `PaymentPage`: payment-method chips (UPI / Card / Wallet) implemented as `SfSegmentedControl`, method-specific form (`SfTextInputLayout` for fields), "Pay ₹X" button. Tap shows an `SfBottomSheet` with a spinner "Processing payment…" for ~2.5 s, then success path → `BookingConfirmationPage`. ~5% simulated failure shows an `SfPopup` with Retry / Change Method.
- **Dependencies:** T3.3.
- **Deliverables:** `Views/PaymentPage.xaml(.cs)`, `ViewModels/PaymentViewModel.cs`.
- **Acceptance:** Method switching swaps form; processing sheet animates; success and failure both reachable.

### T3.5 Booking Confirmation Screen
- **Description:** Build `BookingConfirmationPage`: success checkmark, Booking ID `BK-XXXXXX`, QR code (placeholder image OK), ticket recap card, action row with "Download Ticket" and "Share Ticket" buttons that simply show `SfPopup` confirmations, "Back to Home" navigates to root.
- **Dependencies:** T3.4.
- **Deliverables:** `Views/BookingConfirmationPage.xaml(.cs)`, `ViewModels/BookingConfirmationViewModel.cs`.
- **Acceptance:** Booking persists in `BookingRepository`; tap "Back to Home" returns to root and resets `BookingSession`.

### T3.6 My Bookings Screen
- **Description:** Build `MyBookingsPage` as the second Shell tab: `SfTabView` with Upcoming/Past, `SfListView` of booking cards (poster thumb, title, theater, date/time, seat count, status badge), empty state with CTA back to Home. Tapping a card opens a read-only ticket view.
- **Dependencies:** T1.4, T2.2.
- **Deliverables:** `Views/MyBookingsPage.xaml(.cs)`, `ViewModels/MyBookingsViewModel.cs`.
- **Acceptance:** Completed bookings show under Upcoming; empty state renders when none; tabs switch.

---

## Phase 4 — AI Movie Assistant

### T4.1 Intent Engine
- **Description:** Build a domain-aware `IAiAssistantService` that classifies user input into intents (`RecommendByGenre`, `TopRated`, `CompareTwo`, `ExplainMovie`, `FamilyFriendly`, `BookingHelp`, `Unknown`) using simple keyword matching over the in-memory catalog. Return a list of `AssistMessage` (plain text, movie cards, or comparisons) without calling any external LLM.
- **Dependencies:** T1.4.
- **Deliverables:** `Services/AiAssistantService.cs`, `Models/MovieAssistMessage.cs`.
- **Acceptance:** All 6 example prompts from the brief return a non-empty response with no crashes.

### T4.2 AI AssistView UI
- **Description:** Build `AiAssistantSheet.xaml` as a modal `SfBottomSheet` containing an `SfAIAssistView` styled to the Stitch design (brand color for user bubbles, surface variant for assistant bubbles). Render recommended movies using a custom `DataTemplate` that shows poster, title, rating, and a "View Details" button.
- **Dependencies:** T4.1.
- **Deliverables:** `Views/AiAssistantSheet.xaml(.cs)`, `ViewModels/AiAssistantViewModel.cs`.
- **Acceptance:** Sheet opens, suggested prompts inject into the input, responses render correctly.

### T4.3 Global AI Entry Points
- **Description:** Wire an AI FAB on Home and toolbar AI buttons on Movie Details, Theater, Seat, Summary, and Payment. All entry points open the same singleton `AiAssistantSheet`. The ViewModel receives a context object (current movie, screen) so the greeting is contextual.
- **Dependencies:** T4.2, T2.3.
- **Deliverables:** FAB control + per-screen toolbar bindings.
- **Acceptance:** AI opens from every primary screen; conversation history persists while the sheet is open.

---

## Phase 5 — Wire-up & Smoke Pass

### T5.1 Booking Session & Flow
- **Description:** Implement `BookingSession` (singleton) and `BookingRepository` (in-memory list). Wire them so each step in the booking flow reads the draft and writes the next decision; on payment success the draft becomes a persisted `Booking`.
- **Dependencies:** T3.5.
- **Deliverables:** `Services/BookingSession.cs`, `Services/BookingRepository.cs`.
- **Acceptance:** A completed booking appears in `MyBookingsPage` immediately after payment success.

### T5.2 End-to-End Smoke Walk
- **Description:** Launch the app and walk through Home → Details → Theater → Seats → Summary → Payment → Confirmation → My Bookings on at least one platform. Verify navigation, persistence, theme toggle, and AI sheet all work. Fix any visual regressions found.
- **Dependencies:** T1.1 → T5.1.
- **Deliverables:** Smoke walk notes (informal, in commit messages).
- **Acceptance:** Happy path completes without crashes; the resulting booking is visible in My Bookings.

---

## Out of Scope (handled manually)

The following are intentionally **not** part of this task list — they are done outside the AI agent's responsibility:

- Authoring/bundling real movie poster images and banner art.
- Sourcing real Indian-context theater brand assets (PVR/INOX logos, etc.).
- Unit tests, integration tests, UI automation tests.
- Final accessibility audit, Lighthouse/performance profiling.
- README/sample write-up and screenshots for marketing.
- Multi-language localization (English-only is acceptable for this sample).