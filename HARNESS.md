# Smart Movie Ticket Booking — Harness Document

**Purpose:** Define the contract an AI coding agent must satisfy to produce a working, design-faithful implementation of the **Smart Movie Ticket Booking** sample on Windows desktop.

**Companion documents:**
- Functional & behavior specification → [SPECIFICATION.md](./SPECIFICATION.md)
- Development task breakdown → [TASKS.md](./TASKS.md)

---

## Table of Contents

1. [Functional Requirements (FR)](#1-functional-requirements-fr)
2. [Non-Functional Requirements (NFR)](#2-non-functional-requirements-nfr)
3. [Assumptions](#3-assumptions)
4. [Dependencies](#4-dependencies)
5. [Page-Level Implementation Guidelines](#5-page-level-implementation-guidelines)
6. [User Flow](#6-user-flow)
7. [State Management Requirements](#7-state-management-requirements)
8. [Service Layer Requirements](#8-service-layer-requirements)
9. [Mock Data Contracts](#9-mock-data-contracts)
10. [AI Copilot Requirements](#10-ai-copilot-requirements)
11. [Error Handling Scenarios](#11-error-handling-scenarios)
12. [Validation Requirements](#12-validation-requirements)
13. [Acceptance Criteria](#13-acceptance-criteria)
14. [User Stories](#14-user-stories)
15. [Milestones](#15-milestones)
16. [Appendix A — Page-to-Control Map](#appendix-a--page-to-control-map)
17. [Appendix B — Theming Tokens](#appendix-b--theming-tokens)
18. [Appendix C — AppSeed Sample Movies](#appendix-c--appseed-sample-movies)

---

## 1. Functional Requirements (FR)

| ID | Requirement |
|---|---|
| FR-01 | App launches to Movie Home within 2 seconds on Windows. |
| FR-02 | Movie Home displays a grid of now-playing movies, each card with poster, title, genre, runtime, rating, language, IMAX 3D chip. |
| FR-03 | Movie Home provides working search and genre chip filter that update the visible list reactively. |
| FR-04 | Tapping a movie card navigates to the Theaters page with the selected movie loaded in the context banner. |
| FR-05 | The Theaters page displays the selected movie's poster, title, synopsis, runtime, audio, accessibility in the context banner. |
| FR-06 | The Theaters page shows a 7-day date strip with Today, Tomorrow, Weekend, Weekday labels. The active date is highlighted with `primary-container` bg and an animated dot. |
| FR-07 | The Theaters page shows a Cinema Experience Filters panel with Format chips, Audio & Sub chips, Price dropdown, Recliner Seats toggle, Open Now toggle. |
| FR-08 | Selecting a date chip updates all theater cards' showtime grids to that date. |
| FR-09 | Toggling a format filter narrows the visible showtime pills. |
| FR-10 | Tapping a showtime pill marks it as `SelectedShowtime` and enables the theater's "Select Showtime & Continue" button. |
| FR-11 | Each theater card shows: icon tile, name, category chip, location + distance, "From $X" + "Y seats available", favorite heart button, amenities row, showtime grid, footer "Select Showtime & Continue" button. |
| FR-12 | Tapping a showtime pill's footer button opens the **Seat Selection popup** with the chosen showtime. |
| FR-13 | The Seat Selection popup shows a 10×14 seat grid with Available / Occupied / Selected / Premium states, a legend, and a sticky pricing footer. |
| FR-14 | The pricing footer updates live as seats are toggled; subtotal, convenience fee, total. |
| FR-15 | The "Confirm Seats" button is disabled when 0 seats are selected. |
| FR-16 | Tapping "Confirm Seats" creates a `Booking`, persists it to `BookingRepository`, and opens the **Payment Confirmation popup**. |
| FR-17 | The Payment Confirmation popup shows: success header, booking ID, QR code, ticket card (movie, theater, screen, date, time, seats, total), Download / Share / Back to Home buttons. |
| FR-18 | Tapping "Back to Home" closes the popup and navigates to Movie Home. |
| FR-19 | The Bookings page lists all confirmed bookings as cards with title, status badge, theater/date/seat count, total, and "View Ticket" button. |
| FR-20 | Tapping "View Ticket" opens the Payment Confirmation popup in read-only mode for that booking. |
| FR-21 | The Bookings page shows a designed empty state when there are no bookings. |
| FR-22 | The AI Copilot FAB is visible in the bottom-right corner of every page. |
| FR-23 | Tapping the AI Copilot FAB opens an `SfBottomSheet` with an `SfAIAssistView` that answers the sample prompts. |
| FR-24 | Light and dark themes are both supported and the design tokens are honored in both modes. |
| FR-25 | The sidebar nav (Movie Home / Theaters / Bookings) highlights the active page with `primary-container` bg. |
| FR-26 | The app bar shows: city selector ("New York, NY"), search field, notifications bell, user avatar ("Alex Vance • Platinum Member"). |
| FR-27 | The custom title bar shows: app icon + "Smart Movie Ticket Booking v2.5 (x64)" on the left, minimize / maximize / close buttons on the right. |
| FR-28 | The sidebar shows the brand "Smart Cinema" + "MAUI Pro Engine" caption, the "Explore Hub" section header, the three nav items, and a "Region Service — Live Synced • Active v2.5" status footer. |
| FR-29 | The "Change Movie" button on Theaters page navigates back to Movie Home. |
| FR-30 | The "Reset All" link in the filters panel resets every filter to its default. |

---

## 2. Non-Functional Requirements (NFR)

| ID | Requirement |
|---|---|
| NFR-01 | All Syncfusion design tokens (colors, radii, typography, spacing) from SPEC §5 are defined as `StaticResource` keys in `Resources/Styles/Colors.xaml` and used throughout. |
| NFR-02 | All ViewModels inherit `BaseViewModel : ObservableObject`. Bindings use `[ObservableProperty]` / `[RelayCommand]`. No business logic in code-behind. |
| NFR-03 | `Nullable` is enabled in the .csproj; no build warnings. |
| NFR-04 | `dotnet build` succeeds on `net10.0-windows10.0.19041.0`. |
| NFR-05 | All services and ViewModels are registered in `MauiProgram.cs` via `IServiceCollection`. Pages receive their ViewModel via constructor injection. |
| NFR-06 | All in-app data comes from `AppSeed.cs` — no JSON files, no network calls, no real APIs. |
| NFR-07 | Page transitions do not block the UI thread. All data access is `async`. |
| NFR-08 | The sample is fully offline. It must run with no network access (poster image URLs may render empty placeholders when offline). |
| NFR-09 | The seat layout is deterministic per (showtime, theater) — the same seats must show as occupied across launches for the same showtime. |
| NFR-10 | All fonts (Plus Jakarta Sans, Inter, Material Symbols Outlined) are bundled in `Resources/Fonts/` and registered in `MauiProgram.ConfigureFonts`. |

---

## 3. Assumptions

- The sample is for **Windows desktop only** in this build. Cross-platform can be re-enabled by adding the other `<TargetFramework>`s back in the .csproj, but the layout is not tested for mobile form factors.
- The user has the Syncfusion .NET MAUI license and NuGet feed configured. If not, packages will still restore but will run in trial mode with a license banner.
- Posters are loaded from the public URLs in `AppSeed` (matching the Stitch HTML). If the network is unavailable, an `Image` with a fallback brush (`surface-container-high`) is acceptable.
- "Booking ID" is a generated string like `BK-123456` created at confirmation time. No persistence to disk.
- The QR code is purely visual — a `SfBarcodeGenerator` with QR symbology is the preferred implementation; a styled placeholder is acceptable.
- The AI Copilot answers are deterministic, drawn from `AppSeed` data — no LLM call.
- All currency is in USD to match the Stitch design ($18.50, $24.00 etc.).
- The user has Visual Studio 2022 17.x+ with the .NET MAUI workload installed.

---

## 4. Dependencies

### 4.1 NuGet Packages (target `33.2.x` to match the rest of the repo)

```xml
<PackageReference Include="Microsoft.Maui.Controls" Version="10.0.80" />
<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.9" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
<PackageReference Include="Syncfusion.Maui.Core"           Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.Buttons"         Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.Cards"           Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.Chips"           Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.ListView"        Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.Popup"           Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.BottomSheet"     Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.AIAssistView"    Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.Barcode"         Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.SegmentedControl" Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.ToggleButton"    Version="33.2.15" />
<PackageReference Include="Syncfusion.Maui.ComboBox"        Version="33.2.15" />
```

(Omit any package that is not needed for the three pages + two popups; for example, AI AssistView and BottomSheet can be deferred if T4 is not in scope.)

### 4.2 Fonts

Bundle under `Resources/Fonts/`:
- `PlusJakartaSans-SemiBold.ttf` (600)
- `PlusJakartaSans-Bold.ttf` (700)
- `PlusJakartaSans-ExtraBold.ttf` (800)
- `Inter-Regular.ttf` (400)
- `Inter-Medium.ttf` (500)
- `Inter-SemiBold.ttf` (600)
- `MaterialSymbolsOutlined.ttf` (variable)

Register in `MauiProgram.ConfigureFonts`:
```csharp
fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "PlusJakarta");
fonts.AddFont("PlusJakartaSans-Bold.ttf",     "PlusJakartaBold");
fonts.AddFont("PlusJakartaSans-ExtraBold.ttf","PlusJakartaExtra");
fonts.AddFont("Inter-Regular.ttf",   "Inter");
fonts.AddFont("Inter-Medium.ttf",    "InterMedium");
fonts.AddFont("Inter-SemiBold.ttf",  "InterSemi");
fonts.AddFont("MaterialSymbolsOutlined.ttf", "MaterialIcons");
```

---

## 5. Page-Level Implementation Guidelines

### 5.1 MainWindow (root composition)

- Single `Window` set in `App.xaml.cs` `CreateWindow`.
- 2-row `Grid`: `Auto, *` for title bar (32 px) and the rest.
- The second row is a 2-column `Grid`: `256, *` for sidebar and main.
- `MainWindowViewModel.CurrentPage` (enum `MovieHome | Theaters | Bookings`) drives a `ContentPresenter` that renders the right `ContentView`.
- AI FAB is positioned with `Grid` cell, `HorizontalOptions="End"`, `VerticalOptions="End"`, `Margin="0,0,24,24"`, `ZIndex="99"`.
- A single `SfBottomSheet` host at the bottom of main with `IsOpen` bound to `MainWindowViewModel.IsAiSheetOpen`.

### 5.2 Movie Home

- Use `SfCollectionView` with `ItemsLayout="VerticalGrid, 3"` (Span 2 on small widths).
- `SfShimmer` placeholder while `IsLoading == true`. Wrap the grid in a `Grid` and toggle visibility.
- Search field is `SfTextInputLayout` wrapping an `Entry`. `TextChanged` updates `SearchText` on the VM.
- Genre chips row is a `ScrollView` containing a horizontal `FlexLayout` (or `BindableLayout` over a `SfListView` with `LayoutManager="Horizontal"`).
- Movie card control is `Controls/MovieCardView.xaml` with `BindableProperty` for `Movie`.
- Empty state: a centered `VerticalStackLayout` with an icon (`movie` from `MaterialIcons`), text, and a button.

### 5.3 Theaters

- The page is a `ScrollView` wrapping a `VerticalStackLayout`.
- **Context banner:** `Grid` with absolute-positioned `SfGradientView` blobs (top-right, bottom-right). Main content row: poster (128×192) + meta `VerticalStackLayout` + Change Movie button. Use `Margin="-24,-24,-24,0"` so the banner can extend visually if needed.
- **Date strip:** a 7-column `Grid`. Each cell is `Controls/DateChipView.xaml` (BindableProperty for `DateTime`, `IsSelected`). `IsSelected` triggers the active style (primary-container bg, animated dot via `SfEffectsView` or a plain `BoxView` with `SfShimmer`).
- **Filters panel:** `Grid` with rows for Format chips, Audio & Sub chips, then a row with Price dropdown + toggles. Use `SfChipGroup` with `SelectionMode="Single"` for Format, "SingleOrNone" or "Multiple" per design intent.
- **Theater cards:** `SfCardView` wrapping a `VerticalStackLayout` per the design. Use `Controls/TheaterCardView.xaml` with `BindableProperty` for `TheaterWithShowtimes` and a `ShowtimeSelectedCommand`.
- **Top-Rated Partner Venues row:** `SfCollectionView` with `ItemsLayout="VerticalGrid, 4"`. Each card is a simple image + title + distance mini-card.
- "Select Showtime & Continue" button per card is `SfButton` with `Style="{StaticResource PrimaryButton}"`. `IsEnabled` is bound to `TheaterViewModel.HasSelectedShowtime` for that card.
- **"Sort by" dropdown:** `SfComboBox` with "Proximity (Distance)", "Rating", "Price (Low to High)", "Price (High to Low)".

### 5.4 Bookings

- Vertical `SfListView` or `VerticalStackLayout` over an `ObservableCollection<Booking>`.
- `Controls/BookingCardView.xaml` per booking, with a `ViewTicketCommand` returning the booking.
- Empty state with icon + CTA.

### 5.5 Seat Selection popup

- `SfPopup` with `SfPopupHost` containing the `SeatSelectionView` (`ContentView`).
- Header: `Grid` with back button, title, theater name, showtime.
- Screen indicator: a horizontal `BoxView` with a slight curve (or an SVG).
- Seat grid: a `BindableLayout` over a `Grid` of `Grid`s, OR a `Canvas` with positioned `Rectangle`s. For simplicity, build a `Grid` programmatically (10 rows × 14 cols) and set the cell's `BackgroundColor` via a `SeatStateToBrushConverter`.
- Each seat is a small clickable `Border` (`RoundRectangle` `CornerRadius="6"`, `WidthRequest="22"`, `HeightRequest="22"`).
- Legend is a horizontal `BindableLayout` over a static list of (Color, Label) tuples.
- Footer: `Border` with `Radius="16,16,0,0"`, `BackgroundColor="{StaticResource SurfaceContainerLowest}"`, containing totals and the `SfButton`.

### 5.6 Payment Confirmation popup

- `SfPopup` with `ContentView` `PaymentConfirmationView`.
- Success header is centered `VerticalStackLayout`: emoji or check, "Booking Confirmed!" `display-hero`, subtitle.
- QR code: `Syncfusion.Maui.Barcode.SfBarcodeGenerator` configured with `Symbology="QR"`, `Value="{Binding QrPayload}"`, sized 200×200, centered.
- Ticket card: bordered `VerticalStackLayout` per the design.
- Action row: 3 buttons in a `Grid` with 3 columns. "Download Ticket" and "Share Ticket" use `Style="{StaticResource OutlinedButton}"`. "Back to Home" uses `Style="{StaticResource PrimaryButton}"`.

### 5.7 AI Copilot FAB + Sheet

- `Controls/AiCopilotFab.xaml` is a 48×48 circular `Border` with a `LinearGradientBrush` (primary → secondary), the `auto_awesome` icon, and a 12×12 dot top-right.
- Tooltip is a sibling `Border` with `Opacity="0"` and an `SfEffectsView` ripple on hover.
- The `SfBottomSheet` is hosted in `MainWindow.xaml`, full-width, `HeightRequest="420"`, with `SfAIAssistView` inside.

---

## 6. User Flow

```
[App launch]
    │
    ▼
[Movie Home]  ──search/filter──► [Filtered Movie Home]
    │  tap card
    ▼
[Theaters ?movieId=...]  ──select date──► [Theaters w/ showtimes for date]
    │  tap showtime + Continue
    ▼
[Seat Selection popup]
    │  pick seats, Confirm
    ▼
[Payment Confirmation popup]
    │  Back to Home
    ▼
[Movie Home]

[Sidebar] Theaters   → [Theaters w/ current selection or first movie]
[Sidebar] Bookings   → [Bookings]
[AI FAB on any page] → [AI Copilot sheet]
[Bookings card]      → [Payment Confirmation read-only]
```

---

## 7. State Management Requirements

- **One source of truth for the in-progress booking:** `BookingSession` singleton.
- **One source of truth for completed bookings:** `BookingRepository` singleton.
- ViewModels never reference each other directly; they communicate via the singletons.
- The AI Copilot state (`IsOpen`, `Messages`) lives on its own VM.
- Page transitions do not destroy ViewModels unnecessarily; the navigation `enum` on `MainWindowViewModel` re-binds the `ContentPresenter`.
- When the user taps "Back to Home" on the confirmation, `BookingSession` is reset.

---

## 8. Service Layer Requirements

- Stateless services are safe to register as singletons.
- `IMovieService.GetNowShowingAsync()` returns a list of `Movie` (10 entries from `AppSeed`).
- `ITheaterService.GetTheatersForMovieAsync(string movieId)` returns the list of theaters for the given movie. Theaters are not movie-specific in the design; this method returns the full list.
- `IShowtimeService.GetShowtimesAsync(string movieId, string theaterId, DateTime date)` returns the showtimes for the given triple.
- `IBookingService.CreateAsync(BookingDraft)` returns a `Booking` with a generated `BookingId` and a `CreatedAt` timestamp.
- All services are async, returning `Task<T>`. They may include a small artificial delay (200–600 ms) to simulate network latency for the shimmer effect.

### Service Interfaces

```csharp
public interface IMovieService
{
    Task<IReadOnlyList<Movie>> GetNowShowingAsync();
    Task<Movie> GetByIdAsync(string id);
    Task<IReadOnlyList<Movie>> SearchAsync(string query, string genre);
}

public interface ITheaterService
{
    Task<IReadOnlyList<Theater>> GetAllAsync();
    Task<Theater> GetByIdAsync(string id);
}

public interface IShowtimeService
{
    Task<IReadOnlyList<Showtime>> GetAsync(string movieId, string theaterId, DateTime date);
    Task<Showtime> GetByIdAsync(string id);
}

public interface IBookingService
{
    Task<Booking> CreateAsync(BookingDraft draft);
}

public interface IAiAssistantService
{
    Task<string> RespondAsync(string prompt, AiContext context);
}
```

---

## 9. Mock Data Contracts

### 9.1 `AppSeed.cs`

A static class in `Services/` that returns the in-memory data. Movies are hard-coded to match the Stitch design (10 Now Showing). Theaters and showtimes are derived deterministically:

- Theaters: 4 hard-coded entries (AMC Empire 25, Regal E-Walk 4DX, AMC Lincoln Square 13, Alamo Drafthouse Downtown).
- Showtimes: for each (movie, theater, date in next 7 days) generate 4–6 showtimes at 10:00, 11:15, 12:00, 13:30, 14:45, 16:00, 17:15, 18:30, 19:45, 20:00, 21:00, 22:45, 23:30 with format + price + status derived from the design.

### 9.2 `SeatLayoutFactory.cs`

```csharp
public static class SeatLayoutFactory
{
    public static IReadOnlyList<Seat> Build(string showtimeId)
    {
        // 10 rows × 14 cols, premium rows = last 2 rows, deterministic
        // occupancy by hashing the showtimeId.
    }
}
```

### 9.3 Seeded Data Per Theater (mirrors Stitch HTML)

| Theater | Icon | Category | Sample Showtimes |
|---|---|---|---|
| AMC Empire 25 & IMAX | `theaters` | Flagship Venue | 10:00, 13:30, 16:45, 19:15 (Prime Time), 22:00, 23:30 |
| Regal E-Walk 4DX & RPX | `cinematic_blur` | Motion Immersion | 11:15, 14:45, 18:00, 20:30, 22:45 |
| AMC Lincoln Square 13 & IMAX | `fullscreen` | Original 70mm Grand Screen | 12:00, 15:45, 19:30, 23:00 |
| Alamo Drafthouse Cinema Downtown | `dinner_dining` | Curated Dining | 13:00, 17:15, 20:45 |

The selected "Chronicles of Nebula: Horizon" movie in the Stitch HTML uses ID `m-nebula` with synopsis, cast, and meta as written. AppSeed should keep this as a primary record so the demo matches the design exactly.

---

## 10. AI Copilot Requirements

- A single `AiAssistantService` with a `RespondAsync(prompt, context)` method.
- The intent engine is a small set of regex / keyword matchers that map to canned responses. Example intents:
  - "recommend", "suggest" → top 3 by rating
  - "compare" + movie title → two-column comparison
  - "imax" / "4dx" / "dolby" → format explanation
  - "family" / "kids" → filter by genre Family
  - "highest rating" / "best" → top-rated
- Every response can include up to 3 `AssistMessage` items: a text message + 0–2 movie recommendation cards.
- The sheet uses `SfAIAssistView` with `ShowHeader="True"`, header text "Cinema AI Copilot", suggested prompts shown as `AssistItem`s at the bottom.
- The assistant maintains a session history in `AiCopilotViewModel.Messages`.

---

## 11. Error Handling Scenarios

| Scenario | Behavior |
|---|---|
| Movie card image fails to load | Fall back to a `Border` with `surface-container-high` bg and a `movie` icon. |
| Showtime ID is invalid | Show inline "Showtime not available" on the Theaters page and disable the "Continue" button. |
| Seat grid generation throws | Show a `SfPopup` "Could not load seats. Try again." with a retry. |
| Booking creation throws | Show a `SfPopup` "Could not confirm booking. Please try again." |
| AI intent is not recognized | Reply with "I'm not sure I understood. Try one of the suggestions." |
| Bookings list is empty | Render the designed empty state. |

---

## 12. Validation Requirements

- Search input: trimmed, lowercased, ignored if empty.
- Genre chip selection: at most one active at a time.
- Format chip selection: at most one active at a time (matches the design where the active one is "All Formats").
- Date chip selection: exactly one active.
- Showtime selection: zero or one per theater.
- Seat selection: 0 to 10 seats per booking.
- "Confirm Seats" button: enabled only when `SelectedSeats.Count > 0`.

---

## 13. Acceptance Criteria

### 13.1 Build

- AC-B1: `dotnet build -f net10.0-windows10.0.19041.0` succeeds with no errors or warnings.
- AC-B2: App launches to Movie Home in under 2 seconds on Windows.
- AC-B3: Title bar, sidebar, header bar, and main content are all visible at launch.

### 13.2 Functional

- AC-F1: A reviewer can complete Home → Theaters → Seat → Confirmation in under 60 seconds.
- AC-F2: All three pages match the Stitch design tokens (colors, typography, spacing, radius).
- AC-F3: The Seat Selection popup and Payment Confirmation popup both render and function.
- AC-F4: A confirmed booking appears in the Bookings page.
- AC-F5: The AI FAB is visible on all three pages and opens the assistant sheet.
- AC-F6: Date chip selection reloads showtimes for the chosen date.
- AC-F7: Format chip selection narrows the visible showtime pills.
- AC-F8: "Change Movie" navigates back to Movie Home.
- AC-F9: "Back to Home" from the confirmation navigates to Movie Home and closes the popup.
- AC-F10: Sidebar nav highlights the active page.

### 13.3 Visual

- AC-V1: All design tokens from SPEC §5 are applied throughout.
- AC-V2: The movie card on Movie Home shows the IMAX 3D chip top-left, poster, title, genre, runtime, rating row.
- AC-V3: The Theaters page context banner shows the poster, meta chips, title, synopsis, runtime/audio/accessibility row, and Change Movie button.
- AC-V4: The date strip has 7 cells with the correct labels (Today, Tomorrow, Weekend/Weekday).
- AC-V5: The active date cell is filled with `primary-container` and shows the animated dot.
- AC-V6: The theater card's "Select Showtime & Continue" button uses the primary brand color.
- AC-V7: The Payment Confirmation popup shows a QR code, ticket card, and 3 action buttons.
- AC-V8: Light and dark themes both render correctly.

### 13.4 Technical

- AC-T1: MVVM strictly enforced; no business logic in code-behind.
- AC-T2: All services are DI-registered; ViewModels are constructor-injected.
- AC-T3: All mock data comes from `AppSeed.cs`.
- AC-T4: `Nullable` enabled; no warnings.
- AC-T5: All fonts and design tokens are bundled in the project.

---

## 14. User Stories

- **US-01** — As a moviegoer, I want to browse now-playing movies on the home page, so I can pick one.
- **US-02** — As a moviegoer, I want to search by movie title, so I can find a specific film.
- **US-03** — As a moviegoer, I want to filter by genre, so I can narrow the list.
- **US-04** — As a moviegoer, I want to see ratings on each card, so I can decide before opening details.
- **US-05** — As a moviegoer, I want to see which theaters are showing my movie, so I can pick one nearby.
- **US-06** — As a moviegoer, I want to switch dates, so I can find a show that fits my schedule.
- **US-07** — As a moviegoer, I want to see showtime prices and seat availability, so I can choose the best option.
- **US-08** — As a moviegoer, I want to filter by format (IMAX, 4DX, Standard), so I get the experience I want.
- **US-09** — As a moviegoer, I want to pick my seats visually, so I can sit with my group.
- **US-10** — As a moviegoer, I want to see the total cost update live, so I stay within budget.
- **US-11** — As a moviegoer, I want a confirmation with a QR code, so I can present it at the theater.
- **US-12** — As a moviegoer, I want to see my past bookings, so I can revisit a ticket.
- **US-13** — As a moviegoer, I want to ask the AI Copilot for recommendations, so I can discover new movies.
- **US-14** — As a moviegoer, I want the AI Copilot to be available on every page, so I can ask without losing my place.

---

## 15. Milestones

| Milestone | Deliverable | Definition of Done |
|---|---|---|
| **M1 — Window + Chrome** | Title bar, sidebar, header bar, AI FAB, navigation enum. Movie Home placeholder. | App launches, navigates between placeholder pages, FAB is present. |
| **M2 — Movie Home + Theaters** | Both pages render the Stitch design with mock data; date strip, filters, theater cards, showtime selection. | Reviewer can browse, search, filter, switch date, and select a showtime. |
| **M3 — Booking Popups** | Seat Selection popup, Payment Confirmation popup, booking persisted in repository. | Reviewer can pick seats, see total, confirm, and land on the confirmation. |
| **M4 — Bookings + AI Copilot** | Bookings list, AI Copilot sheet, light/dark theme polish. | Reviewer can view past bookings, open ticket, and chat with the AI. |

---

## 16. Appendix A — Page-to-Control Map

| Element | Control |
|---|---|
| Title bar | Custom `Border` + `Image` (movie icon) + `Label` + 3 `SfButton` (min/max/close) |
| Sidebar nav item | `Border` + `Label` (Material icon) + `Label` (label) bound to a `Command` |
| App bar search | `SfTextInputLayout` wrapping `Entry` with leading icon `Border` |
| Movie card | `SfCardView` with `Image` + `Label`s + small `Border` chip |
| Genre chip | `SfChip` (selectable) with `IsSelected` styling |
| Date chip | `Border` with 3 `Label`s and an animated `BoxView` (dot) |
| Theater card | `SfCardView` with header row, amenities row, `SfCollectionView` of showtime pills, footer `SfButton` |
| Showtime pill | `Border` with time, format label, price, status badge |
| Filters panel | `Border` + rows of `SfChip`s, `SfComboBox`, `SfToggleButton` |
| Seat | Small clickable `Border` with `CornerRadius="6"` |
| Seat legend | Horizontal `BindableLayout` of (swatch, label) pairs |
| Ticket card | `Border` with `VerticalStackLayout` |
| QR code | `SfBarcodeGenerator` with `Symbology="QR"` |
| AI FAB | 48 px circular `Border` with `LinearGradientBrush` + icon + 12 px dot |
| AI sheet | `SfBottomSheet` with `SfAIAssistView` inside |

---

## 17. Appendix B — Theming Tokens

```xml
<!-- Colors.xaml -->
<Color x:Key="Primary">#3525cd</Color>
<Color x:Key="PrimaryContainer">#4f46e5</Color>
<Color x:Key="OnPrimary">#ffffff</Color>
<Color x:Key="OnPrimaryContainer">#dad7ff</Color>

<Color x:Key="Secondary">#712ae2</Color>
<Color x:Key="SecondaryContainer">#8a4cfc</Color>
<Color x:Key="SecondaryFixed">#eaddff</Color>
<Color x:Key="OnSecondary">#ffffff</Color>

<Color x:Key="Tertiary">#00505f</Color>
<Color x:Key="TertiaryContainer">#006a7c</Color>
<Color x:Key="OnTertiary">#ffffff</Color>
<Color x:Key="OnTertiaryContainer">#93e8ff</Color>

<Color x:Key="Error">#ba1a1a</Color>
<Color x:Key="ErrorContainer">#ffdad6</Color>
<Color x:Key="OnError">#ffffff</Color>

<Color x:Key="Surface">#f8f9ff</Color>
<Color x:Key="SurfaceContainerLowest">#ffffff</Color>
<Color x:Key="SurfaceContainerLow">#eff4ff</Color>
<Color x:Key="SurfaceContainer">#e5eeff</Color>
<Color x:Key="SurfaceContainerHigh">#dce9ff</Color>
<Color x:Key="SurfaceContainerHighest">#d3e4fe</Color>
<Color x:Key="SurfaceVariant">#d3e4fe</Color>

<Color x:Key="Outline">#777587</Color>
<Color x:Key="OutlineVariant">#c7c4d8</Color>

<Color x:Key="OnSurface">#0b1c30</Color>
<Color x:Key="OnSurfaceVariant">#464555</Color>
<Color x:Key="InverseSurface">#213145</Color>
<Color x:Key="InverseOnSurface">#eaf1ff</Color>
<Color x:Key="InversePrimary">#c3c0ff</Color>

<Color x:Key="Background">#f8f9ff</Color>
<Color x:Key="OnBackground">#0b1c30</Color>

<Color x:Key="Amber400">#fbbf24</Color>
<Color x:Key="Amber500">#f59e0b</Color>
<Color x:Key="Amber600">#d97706</Color>
```

Dark variant mirrors the same keys with dark equivalents. All styles (`PrimaryButton`, `OutlinedButton`, `SurfaceCard`, `PrimaryChip`, `InactiveChip`, `DateChipActive`, `DateChipInactive`, etc.) reference these tokens — never hex literals in pages.

---

## 18. Appendix C — AppSeed Sample Movies

```csharp
public static class AppSeed
{
    public static IReadOnlyList<Movie> Movies => new[]
    {
        new Movie {
            Id = "m-nebula", Title = "Chronicles of Nebula: Horizon",
            Synopsis = "Captain Alyssa Vance leads an intrepid exploration vessel beyond the known Orion Reach to investigate an ancient hyperspace beacon sending harmonious resonance signals.",
            Genre = "Sci-Fi • Epic Adventure",
            Genres = new() { "Sci-Fi", "Action", "Adventure" },
            Language = "English",
            Languages = new() { "English", "Spanish", "Hindi" },
            DurationMinutes = 168, Rating = 4.9, VoteCount = 28400,
            RatingLabel = "PG-13", Format = "IMAX 3D",
            PosterUrl = "https://images.unsplash.com/...",
            Cast = new() { /* 4 cast members */ }
        },
        // + 9 more movies
    };

    public static IReadOnlyList<Theater> Theaters => new[]
    {
        new Theater { Id = "t-amc-empire", Name = "AMC Empire 25 & IMAX",
            Location = "234 W 42nd St, Manhattan", Category = "Flagship Venue",
            Icon = "theaters", DistanceKm = 0.8, /* ... */ },
        // + 3 more
    };

    public static IReadOnlyList<Showtime> Showtimes(string movieId, string theaterId, DateTime date) { /* generated */ }
}
```

The full seed of 10 movies can reuse public-domain movie titles or fictional ones — the design doesn't dictate specific titles, but `m-nebula` should be the first one so the Theaters page matches the Stitch HTML exactly when launched.
