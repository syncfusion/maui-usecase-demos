# Smart Movie Ticket Booking — Development Tasks (Windows, 3 pages + 2 popups)

**Purpose:** Lean, UI-first task plan for building the **Smart Movie Ticket Booking** sample as a Windows desktop .NET MAUI app, matching the Stitch design exactly.

**Companion documents:**
- Functional & behavior specification → [SPECIFICATION.md](./SPECIFICATION.md)
- Requirements, validation, error handling, acceptance criteria → [HARNESS.md](./HARNESS.md)

> **Out of scope (handled manually outside this task list):** real images/asset licensing, tests, accessibility audit, marketing screenshots, README polish, i18n, final build/sign/publish.

---

## Task Index

### Phase 1 — Foundation
- [T1.1 Project Bootstrap](#t11-project-bootstrap)
- [T1.2 Theme & Design Tokens](#t12-theme--design-tokens)
- [T1.3 Models, Services, and AppSeed](#t13-models-services-and-appseed)
- [T1.4 Window Chrome (Title Bar, Sidebar, Header, AI FAB)](#t14-window-chrome-title-bar-sidebar-header-ai-fab)

### Phase 2 — Three Pages
- [T2.1 Movie Home Page](#t21-movie-home-page)
- [T2.2 Theaters Page](#t22-theaters-page)
- [T2.3 Bookings Page](#t23-bookings-page)

### Phase 3 — Two Popups
- [T3.1 Seat Selection Popup](#t31-seat-selection-popup)
- [T3.2 Payment Confirmation Popup](#t32-payment-confirmation-popup)

### Phase 4 — Wire-up & Walkthrough
- [T4.1 Navigation, BookingSession, BookingRepository](#t41-navigation-bookingsession-bookingrepository)
- [T4.2 AI Copilot Sheet](#t42-ai-copilot-sheet)
- [T4.3 End-to-End Walkthrough](#t43-end-to-end-walkthrough)

---

## Phase 1 — Foundation

### T1.1 Project Bootstrap
- **Description:** Create the .NET MAUI project `SmartMovieBooking`. Target framework `net10.0-windows10.0.19041.0` only. Add all Syncfusion + CommunityToolkit.Mvvm packages from HARNESS §4.1. Verify `dotnet build` succeeds.
- **Dependencies:** none.
- **Deliverables:** `SmartMovieBooking.csproj`, working `MauiProgram.cs`, empty `App` and `MainWindow`.
- **Acceptance:** `dotnet build` succeeds; `dotnet run -f net10.0-windows10.0.19041.0` launches an empty window.

### T1.2 Theme & Design Tokens
- **Description:** Define the color, typography, spacing, and radius tokens from SPEC §5 in `Resources/Styles/Colors.xaml`, `Typography.xaml`, `Buttons.xaml`, `Chips.xaml`, `Cards.xaml`, `Theme.xaml`. Register fonts in `MauiProgram.ConfigureFonts`. Define reusable `Style` entries: `PrimaryButton`, `OutlinedButton`, `PrimaryChip`, `InactiveChip`, `SurfaceCard`, `InverseSurfaceChip`, `PrimeTimeBadge`, etc.
- **Dependencies:** T1.1.
- **Deliverables:** `Resources/Styles/*.xaml`, `App.xaml` registers a `SyncfusionThemeResourceDictionary VisualTheme="MaterialLight"`, `ThemeService` for light/dark toggle (per existing `EmployeeDirectory` pattern).
- **Acceptance:** A test page with one of every style renders correctly in light mode. Toggling `ThemeService.IsDarkTheme = true` swaps to dark tokens.

### T1.3 Models, Services, and AppSeed
- **Description:** Implement all models from SPEC §8 (`Movie`, `CastMember`, `Theater`, `Showtime`, `Seat`, `SeatTier`, `SeatState`, `Booking`). Implement service interfaces from HARNESS §8 and concrete classes that read from `AppSeed.cs`. Implement `SeatLayoutFactory` (deterministic per showtime ID).
- **Dependencies:** T1.1.
- **Deliverables:** `Models/*.cs`, `Services/*.cs`, `AppSeed.cs` with the 10 movies + 4 theaters (matching Stitch HTML) and a `Showtimes(movieId, theaterId, date)` generator.
- **Acceptance:** Calling `MovieService.GetNowShowingAsync()` returns 10 movies; `TheaterService.GetAllAsync()` returns 4 theaters; `ShowtimeService.GetAsync(...)` returns 4–6 showtimes per (movie, theater, date).

### T1.4 Window Chrome (Title Bar, Sidebar, Header, AI FAB)
- **Description:** Build the desktop window skeleton: 32 px custom title bar (app icon + "Smart Movie Ticket Booking v2.5 (x64)" + 3 window buttons), 256 px fixed sidebar (brand + 3 nav items + "Region Service" footer), 64 px sticky header bar (city, search, bell, avatar), and the AI Copilot FAB.
- **Dependencies:** T1.1, T1.2.
- **Deliverables:** `Controls/TitleBarView.xaml(.cs)`, `Controls/SidebarView.xaml(.cs)`, `Controls/HeaderBarView.xaml(.cs)`, `Controls/AiCopilotFab.xaml(.cs)`, `MainWindow.xaml` composition, `MainWindowViewModel` with `CurrentPage` enum.
- **Acceptance:** App launches with full chrome visible; clicking sidebar items toggles `MainWindowViewModel.CurrentPage`; AI FAB is present and clickable (visual only at this stage).

---

## Phase 2 — Three Pages

### T2.1 Movie Home Page
- **Description:** Build `Views/MovieHomeView.xaml` per SPEC §6.1. Use `SfCollectionView` for the movie grid. Build `Controls/MovieCardView.xaml` for the card. Build a horizontal genre chip strip.
- **Dependencies:** T1.3, T1.4.
- **Deliverables:** `Views/MovieHomeView.xaml(.cs)`, `ViewModels/HomeViewModel.cs`, `Controls/MovieCardView.xaml(.cs)`, `Converters/SelectedToColorConverter.cs`.
- **Acceptance:** Home shows 10 movie cards in a 3-column grid; search filters the list; genre chip selection filters the list; tapping a card sets the selected movie in `BookingSession` and navigates to Theaters.

### T2.2 Theaters Page
- **Description:** Build `Views/TheatersView.xaml` per SPEC §6.2 — context banner, date strip, filters panel, theater cards, top-rated venues row, sort dropdown. Build `Controls/DateChipView.xaml`, `Controls/TheaterCardView.xaml`, `Controls/ShowtimePillView.xaml`.
- **Dependencies:** T1.3, T1.4.
- **Deliverables:** `Views/TheatersView.xaml(.cs)`, `ViewModels/TheatersViewModel.cs`, the three control files, `Converters/ShowtimeStatusToColorConverter.cs`, `Converters/DateChipStateToStyleConverter.cs`.
- **Acceptance:** Context banner shows the selected movie; date strip has 7 cells with the correct labels; selecting a date reloads showtimes; selecting a showtime highlights it; "Select Showtime & Continue" opens the Seat Selection popup with the showtime context.

### T2.3 Bookings Page
- **Description:** Build `Views/BookingsView.xaml` per SPEC §6.3 — header, list of booking cards, empty state. Build `Controls/BookingCardView.xaml`.
- **Dependencies:** T1.3, T1.4.
- **Deliverables:** `Views/BookingsView.xaml(.cs)`, `ViewModels/BookingsViewModel.cs`, `Controls/BookingCardView.xaml(.cs)`.
- **Acceptance:** Empty state renders when no bookings; after a successful booking confirmation, the booking appears in the list; tapping "View Ticket" opens the Payment Confirmation popup in read-only mode.

---

## Phase 3 — Two Popups

### T3.1 Seat Selection Popup
- **Description:** Build `Views/SeatSelectionView.xaml` per SPEC §6.4 — header, screen indicator, seat grid (10×14), legend, sticky pricing footer. Build `Controls/SeatControl.xaml`, `Controls/SeatLegendView.xaml`. Host inside an `SfPopup` opened from the Theaters page.
- **Dependencies:** T2.2.
- **Deliverables:** `Views/SeatSelectionView.xaml(.cs)`, `ViewModels/SeatSelectionViewModel.cs`, the two control files, `Converters/SeatStateToColorConverter.cs`.
- **Acceptance:** Tapping an available seat toggles its selection; pricing updates live; "Confirm Seats" is disabled when 0 seats; tapping "Confirm Seats" creates a `Booking` and opens the Payment Confirmation popup.

### T3.2 Payment Confirmation Popup
- **Description:** Build `Views/PaymentConfirmationView.xaml` per SPEC §6.5 — success header, booking ID, QR code, ticket card, 3 action buttons. Build `Controls/QrCodeView.xaml` using `SfBarcodeGenerator` with QR symbology (or a styled placeholder if the generator is not available).
- **Dependencies:** T3.1.
- **Deliverables:** `Views/PaymentConfirmationView.xaml(.cs)`, `ViewModels/PaymentConfirmationViewModel.cs`, `Controls/QrCodeView.xaml(.cs)`.
- **Acceptance:** Popup shows the booking details + QR; "Back to Home" closes the popup and returns to Movie Home; "Download Ticket" and "Share Ticket" show inline confirmation toasts; opening from Bookings renders the same view in read-only mode.

---

## Phase 4 — Wire-up & Walkthrough

### T4.1 Navigation, BookingSession, BookingRepository
- **Description:** Wire `MainWindowViewModel` to `ContentPresenter` for the three pages. Implement `BookingSession` and `BookingRepository` singletons. Reset `BookingSession` after a successful booking.
- **Dependencies:** T2.1, T2.2, T2.3, T3.1, T3.2.
- **Deliverables:** `Services/BookingSession.cs`, `Services/BookingRepository.cs`, `Services/IBookingService.cs`, `Services/BookingService.cs`, updated `MauiProgram.cs` registrations.
- **Acceptance:** A user can navigate Home → Theaters → Seat → Confirmation → Bookings; the Bookings page shows the new booking; navigating to a different page and back does not lose state.

### T4.2 AI Copilot Sheet
- **Description:** Build `Views/AiCopilotView.xaml` and host it inside an `SfBottomSheet` in `MainWindow`. Implement `Services/AiAssistantService.cs` with a small keyword-based intent engine that returns canned responses seeded from `AppSeed.Movies`. Suggested prompts: "Recommend a movie", "Top rated this week", "Family-friendly", "What's IMAX?".
- **Dependencies:** T1.4, T1.3.
- **Deliverables:** `Views/AiCopilotView.xaml(.cs)`, `ViewModels/AiCopilotViewModel.cs`, `Services/IAiAssistantService.cs`, `Services/AiAssistantService.cs`.
- **Acceptance:** FAB opens the sheet on every page; typing a prompt returns a relevant response; suggested prompt chips populate the input.

### T4.3 End-to-End Walkthrough
- **Description:** Manually walk through the full flow at least once on Windows. Verify light + dark theme, all sidebar links, AI FAB, all popups. Fix any visual or behavioral issues found.
- **Dependencies:** T4.1, T4.2.
- **Deliverables:** Verified app, no functional defects.
- **Acceptance:** All FR-01 to FR-30 from HARNESS §1 are satisfied. All AC-* from HARNESS §13 are met.

---

## Task Dependency Graph

```
T1.1 ──► T1.2 ──► T1.4 ──┬─► T2.1 ──┐
   │                       ├─► T2.2 ──┤
   └─► T1.3 ──────���────────┼─► T2.3 ──┤
                           │          ├─► T3.1 ──► T3.2 ──┐
                           │          │                     ├─► T4.1 ──► T4.3
                           │          │                     │
                           └─► T4.2 ──┴─────────────────────┘
```
