# Smart Movie Ticket Booking — Development Tasks

**Purpose:** Granular, actionable task breakdown for AI-assisted implementation of the sample. Every task includes Title, Description, Dependencies, Expected Deliverables, and Acceptance Criteria.

**Companion documents:**
- Functional & behavior specification → [SPECIFICATION.md](./SPECIFICATION.md)
- Requirements, validation, error handling, acceptance criteria → [HARNESS.md](./HARNESS.md)

---

## Task Index

### Phase 1 — Foundation
- [T1.1 Project Bootstrap](#t11-project-bootstrap)
- [T1.2 Theme Service & Resource Dictionaries](#t12-theme-service--resource-dictionaries)
- [T1.3 Project Structure & DI Wiring](#t13-project-structure--di-wiring)
- [T1.4 Mock Data Authoring](#t14-mock-data-authoring)
- [T1.5 Models & JSON Loading](#t15-models--json-loading)

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

### Phase 5 — Business Logic
- [T5.1 Booking Session & Repository](#t51-booking-session--repository)
- [T5.2 Pricing & Coupon Logic](#t52-pricing--coupon-logic)
- [T5.3 Validation Service](#t53-validation-service)

### Phase 6 — Data Integration
- [T6.1 Image Asset Bundling](#t61-image-asset-bundling)
- [T6.2 Localized Strings](#t62-localized-strings)
- [T6.3 Mock Realism Layer](#t63-mock-realism-layer)

### Phase 7 — Testing & Validation
- [T7.1 Manual Test Pass](#t71-manual-test-pass)
- [T7.2 Theming Audit](#t72-theming-audit)
- [T7.3 Performance Pass](#t73-performance-pass)

### Phase 8 — Final Sample Polish
- [T8.1 Empty States & Illustrations](#t81-empty-states--illustrations)
- [T8.2 Animations & Transitions](#t82-animations--transitions)
- [T8.3 README & Sample Documentation](#t83-readme--sample-documentation)
- [T8.4 Final Build & Smoke Test](#t84-final-build--smoke-test)

---

## Phase 1 — Foundation

### T1.1 Project Bootstrap
- **Description:** Create new .NET MAUI project `SmartMovieBooking`, add Syncfusion NuGet packages, configure `MauiProgram`, register Syncfusion theme, verify build on all target frameworks.
- **Dependencies:** none.
- **Expected Deliverables:** Compilable project with all Syncfusion packages and a "Hello" home page.
- **Acceptance Criteria:** `dotnet build` succeeds; app launches to "Hello" page.

### T1.2 Theme Service & Resource Dictionaries
- **Description:** Implement `ThemeService` (light/dark toggle) following the `EmployeeDirectory` pattern. Add `Colors.xaml`, `Styles.xaml`, and `Theme*` keys.
- **Dependencies:** T1.1.
- **Expected Deliverables:** `Services/ThemeService.cs`, `Resources/Styles/Colors.xaml`, `Resources/Styles/Styles.xaml`.
- **Acceptance Criteria:** Toggling theme updates MAUI `UserAppTheme` and Syncfusion `VisualTheme`; all colors update at runtime.

### T1.3 Project Structure & DI Wiring
- **Description:** Create folder skeleton (Models, Services, ViewModels, Views, Controls, Converters, Behaviors). Register all services and ViewModels in `MauiProgram.cs`.
- **Dependencies:** T1.1.
- **Expected Deliverables:** Empty folders, registration block, `BaseViewModel`.
- **Acceptance Criteria:** `MauiProgram.CreateMauiApp()` resolves without errors.

### T1.4 Mock Data Authoring
- **Description:** Create `movies.json`, `theaters.json`, `showtimes.json`, `cast.json`, `userprofile.json` per SPECIFICATION.md §13.
- **Dependencies:** T1.3.
- **Expected Deliverables:** JSON files committed under `Resources/Raw/`.
- **Acceptance Criteria:** Each file deserializes into the model with no errors.

### T1.5 Models & JSON Loading
- **Description:** Implement all models from SPECIFICATION.md §9 and the JSON loader in services.
- **Dependencies:** T1.4.
- **Expected Deliverables:** All model files, `MovieService.GetNowShowingAsync()` returns ≥ 10 movies.
- **Acceptance Criteria:** Unit-testable (or sample load) returns expected counts.

---

## Phase 2 — Core UI Development

### T2.1 App Shell & Navigation
- **Description:** Configure `AppShell.xaml` with tabs (Home, My Bookings) and routes for push pages. Implement `NavigationService` to support query params.
- **Dependencies:** T1.3.
- **Expected Deliverables:** `AppShell.xaml(.cs)`, `Services/NavigationService.cs`.
- **Acceptance Criteria:** App launches to Home tab; routes resolve.

### T2.2 Reusable UI Library
- **Description:** Build `MovieCardView`, `TheaterCardView`, `ShowtimeChip`, `RatingBadge`, `QrCodeView`, and all converters/behaviors.
- **Dependencies:** T1.2.
- **Expected Deliverables:** Files under `Controls/`, `Converters/`, `Behaviors/`.
- **Acceptance Criteria:** All controls render with sample data in a test page.

### T2.3 Home Screen
- **Description:** Implement `HomePage.xaml` and `HomeViewModel` per SPECIFICATION.md §6.1.
- **Dependencies:** T2.1, T2.2, T1.5.
- **Expected Deliverables:** Working home with carousels, search, filters, shimmer loading.
- **Acceptance Criteria:** All cards render; search and filter update list; tap navigates to details.

### T2.4 Movie Details Screen
- **Description:** Implement per SPECIFICATION.md §6.2.
- **Dependencies:** T2.3, T2.2.
- **Expected Deliverables:** `MovieDetailsPage.xaml(.cs)`, `MovieDetailsViewModel.cs`.
- **Acceptance Criteria:** Hero, cast row, languages, and CTAs all work; trailer shows popup; "Book Tickets" navigates to theater.

---

## Phase 3 — Booking Workflow

### T3.1 Theater Selection Screen
- **Description:** Implement per SPECIFICATION.md §6.3.
- **Dependencies:** T1.5, T2.2.
- **Expected Deliverables:** `TheaterSelectionPage`, `TheaterSelectionViewModel`.
- **Acceptance Criteria:** Date changes update showtimes; tap showtime navigates to seat.

### T3.2 Seat Selection Screen
- **Description:** Implement per SPECIFICATION.md §6.4, including `SeatControl` and `SeatStateToColorConverter`.
- **Dependencies:** T3.1, T2.2.
- **Expected Deliverables:** `SeatSelectionPage`, `SeatSelectionViewModel`, `SeatControl`.
- **Acceptance Criteria:** Seats render in correct state; selection updates pricing; proceed disabled at 0 seats.

### T3.3 Booking Summary Screen
- **Description:** Implement per SPECIFICATION.md §6.5 with coupon logic.
- **Dependencies:** T3.2, T2.2.
- **Expected Deliverables:** `BookingSummaryPage`, `BookingSummaryViewModel`.
- **Acceptance Criteria:** WELCOME10 works; total recomputes; proceed navigates to payment.

### T3.4 Payment Screen
- **Description:** Implement per SPECIFICATION.md §6.6 with method selection, forms, processing sheet.
- **Dependencies:** T3.3.
- **Expected Deliverables:** `PaymentPage`, `PaymentViewModel`, `PaymentService`.
- **Acceptance Criteria:** Processing animates; success routes to confirmation; failure shows popup with retry.

### T3.5 Booking Confirmation Screen
- **Description:** Implement per SPECIFICATION.md §6.7 with QR code, download/share/back actions.
- **Dependencies:** T3.4.
- **Expected Deliverables:** `BookingConfirmationPage`, `BookingConfirmationViewModel`, `QrCodeView`, `BookingRepository.Add(...)`.
- **Acceptance Criteria:** Confirmation displays booking; download popup works; share uses system share sheet; back resets session.

### T3.6 My Bookings Screen
- **Description:** Implement per SPECIFICATION.md §6.8 with Upcoming/Past tabs and empty state.
- **Dependencies:** T3.5, T2.2.
- **Expected Deliverables:** `MyBookingsPage`, `MyBookingsViewModel`.
- **Acceptance Criteria:** A completed booking appears in Upcoming; after showtime, in Past.

---

## Phase 4 — AI Movie Assistant

### T4.1 Intent Engine
- **Description:** Implement `AiAssistantService.ProcessAsync` with intent classification, keyword scoring, and data lookups.
- **Dependencies:** T1.5.
- **Expected Deliverables:** `AiAssistantService.cs`, `AiContext.cs`, message templates.
- **Acceptance Criteria:** All 6 example prompts return non-empty, on-topic results.

### T4.2 AI AssistView UI
- **Description:** Implement `AiAssistantSheet` and `AiAssistantViewModel`. Wire suggested prompts, input bar, history.
- **Dependencies:** T4.1, T2.2.
- **Expected Deliverables:** `Views/AiAssistantSheet.xaml(.cs)`, `ViewModels/AiAssistantViewModel.cs`.
- **Acceptance Criteria:** Sheet opens from Home; suggestions work; messages render with correct templates.

### T4.3 Global AI Entry Points
- **Description:** Add AI FAB on Home and toolbar entries on every primary page. Wire to `BookingSession`-aware context.
- **Dependencies:** T4.2, all primary pages.
- **Expected Deliverables:** Updated `*Page.xaml` files.
- **Acceptance Criteria:** AI sheet opens from every primary screen with appropriate context greeting.

---

## Phase 5 — Business Logic

### T5.1 Booking Session & Repository
- **Description:** Implement `BookingSession` (singleton) and `IBookingRepository` with add/list events.
- **Dependencies:** T1.3.
- **Expected Deliverables:** `BookingSession.cs`, `BookingRepository.cs`.
- **Acceptance Criteria:** Session clears on confirm; repository raises event on add.

### T5.2 Pricing & Coupon Logic
- **Description:** Centralize seat-tier pricing, fee, tax, and coupon math in `PricingService`.
- **Dependencies:** T3.2, T3.3.
- **Expected Deliverables:** `Services/PricingService.cs`.
- **Acceptance Criteria:** All screens agree on totals; coupon is applied only once.

### T5.3 Validation Service
- **Description:** Centralize input validation (card, UPI, CVV, expiry).
- **Dependencies:** T3.4.
- **Expected Deliverables:** `Services/ValidationService.cs`.
- **Acceptance Criteria:** Invalid inputs surface inline error messages and disable Pay.

---

## Phase 6 — Data Integration

### T6.1 Image Asset Bundling
- **Description:** Bundle 20+ placeholder posters, cast avatars, and theater logos as MAUI assets.
- **Dependencies:** T1.4.
- **Expected Deliverables:** `Resources/Images/Posters/*`, `Resources/Images/Cast/*`, `Resources/Images/Theaters/*`.
- **Acceptance Criteria:** All images load in app.

### T6.2 Localized Strings
- **Description:** Move all user-facing text to `Resources/Strings/Strings.resx` (or simple constants file) for i18n-readiness.
- **Dependencies:** T3.5.
- **Expected Deliverables:** Strings resource.
- **Acceptance Criteria:** No literal user-facing strings in XAML code-behind.

### T6.3 Mock Realism Layer
- **Description:** Add artificial delays, randomized seat occupancy, and 5% payment failure to make the app feel real.
- **Dependencies:** T1.5, T3.4.
- **Expected Deliverables:** `Services/MockLatency.cs` (or inline delays), seeded RNG.
- **Acceptance Criteria:** Loading states visible; seats differ between launches; payment failure reproducible.

---

## Phase 7 — Testing & Validation

### T7.1 Manual Test Pass
- **Description:** Walk through every screen on every target framework; file bugs.
- **Dependencies:** T1–T6.
- **Expected Deliverables:** Test report.
- **Acceptance Criteria:** All FRs (HARNESS.md §1) pass.

### T7.2 Theming Audit
- **Description:** Verify all screens in light and dark themes.
- **Dependencies:** T7.1.
- **Expected Deliverables:** Theme audit checklist.
- **Acceptance Criteria:** No contrast or color regressions.

### T7.3 Performance Pass
- **Description:** Verify 60 fps on carousels, seat grid, AI sheet.
- **Dependencies:** T7.1.
- **Expected Deliverables:** Performance notes.
- **Acceptance Criteria:** No visible jank.

---

## Phase 8 — Final Sample Polish

### T8.1 Empty States & Illustrations
- **Description:** Add illustrations to all empty states.
- **Dependencies:** T7.1.
- **Expected Deliverables:** SVG / PNG illustrations.
- **Acceptance Criteria:** Every list has a designed empty state.

### T8.2 Animations & Transitions
- **Description:** Add page-transition animations, success animation, processing animation.
- **Dependencies:** T3.5, T3.6.
- **Expected Deliverables:** XAML animations / Lottie files.
- **Acceptance Criteria:** Smooth, no jank, theme-aware.

### T8.3 README & Sample Documentation
- **Description:** Write a sample README that explains the app, architecture, AI assistant, and how to run.
- **Dependencies:** T7.1.
- **Expected Deliverables:** `README.md` at project root.
- **Acceptance Criteria:** A new developer can run the app following only the README.

### T8.4 Final Build & Smoke Test
- **Description:** Clean, rebuild, and smoke-test all four target frameworks.
- **Dependencies:** T8.1–T8.3.
- **Expected Deliverables:** Build report.
- **Acceptance Criteria:** All targets build and launch.

---

## Task Dependency Graph (Summary)

```
T1.1 ──► T1.2 ──► T2.2 ──► T2.3 ──► T2.4
   │           │           │           │
   ▼           ▼           ▼           ▼
 T1.3 ──► T1.4 ──► T1.5 ──► T3.1 ──► T3.2 ──► T3.3 ──► T3.4 ──► T3.5 ──► T3.6
   │                       │                   │           │
   ▼                       └─► T4.1 ──► T4.2 ──┘           ▼
 T2.1                                                  T4.3 (after T4.2)
                                                            │
                                                            ▼
                                                       T5.1, T5.2, T5.3
                                                            │
                                                            ▼
                                                       T6.1, T6.2, T6.3
                                                            │
                                                            ▼
                                                       T7.1, T7.2, T7.3
                                                            │
                                                            ▼
                                                       T8.1, T8.2, T8.3, T8.4
```

---

*See [SPECIFICATION.md](./SPECIFICATION.md) for the functional specification and [HARNESS.md](./HARNESS.md) for the requirements, acceptance criteria, user stories, and milestones.*