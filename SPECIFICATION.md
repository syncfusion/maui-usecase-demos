# Smart Movie Ticket Booking — Functional Specification

**Project Type:** .NET MAUI Showcase Sample (Windows desktop)
**UI Toolkit:** Syncfusion .NET MAUI Controls (v33.2.x)
**Architecture:** MVVM with CommunityToolkit.Mvvm
**Data Source:** Mock data — `AppSeed.cs` static class (no JSON, no backend)
**Design Reference:** Stitch Windows desktop design (provided HTML)
**Target framework:** `net10.0-windows10.0.19041.0` (primary). Cross-platform target frameworks can be added in csproj.

---

## Companion Documents

- **`HARNESS.md`** — Functional & Non-Functional Requirements, assumptions, screen-level guidelines, state management, validation, acceptance criteria, user stories, milestones.
- **`TASKS.md`** — Lean, UI-first 4-phase development plan.

---

## Table of Contents

1. [Sample Overview](#1-sample-overview)
2. [Business Objective](#2-business-objective)
3. [Sample Objective](#3-sample-objective)
4. [Application Scope](#4-application-scope)
5. [Design System (from Stitch)](#5-design-system-from-stitch)
6. [Page-by-Page Specifications](#6-page-by-page-specifications)
   - 6.1 [Page 1 — Movie Home](#61-page-1--movie-home)
   - 6.2 [Page 2 — Theaters](#62-page-2--theaters)
   - 6.3 [Page 3 — Bookings](#63-page-3--bookings)
   - 6.4 [Popup 1 — Seat Selection](#64-popup-1--seat-selection)
   - 6.5 [Popup 2 — Payment Confirmation](#65-popup-2--payment-confirmation)
7. [Navigation Flow](#7-navigation-flow)
8. [Data Models](#8-data-models)
9. [Project Structure](#9-project-structure)
10. [Syncfusion Control Usage](#10-syncfusion-control-usage)
11. [AI Copilot (Floating)](#11-ai-copilot-floating)

---

## 1. Sample Overview

**Smart Movie Ticket Booking** is a Windows-targeted .NET MAUI showcase sample that demonstrates how to compose Syncfusion's .NET MAUI controls into a modern, content-commerce desktop application modeled on the TicketNew / BookMyShow experience.

The sample ships a complete booking flow built on **mock in-memory data**:

1. **Movie Home** — browse now-playing movies
2. **Theaters** — pick date, filters, theater and showtime for the selected movie
3. **Seat Selection (popup)** — visual seat picker
4. **Payment Confirmation (popup)** — booking confirmation
5. **Bookings** — view past bookings

A floating **AI Cinema Copilot** button sits in the bottom-right corner of the desktop window, available on every page.

**Key constraints (per project brief):**
- Use case sample only — not a production system
- No real payment, no backend, no real auth
- No testing phase, no data-asset work, no final-polish phase (handled manually)
- UI must mirror the Stitch Windows desktop design (sidebar + 3-column main + top app bar)

---

## 2. Business Objective

Provide a compact, runnable reference of a modern movie-ticketing desktop UI built entirely with Syncfusion .NET MAUI controls. The sample's value is:

- Demonstrates Syncfusion's desktop-friendly controls in a real composition.
- Provides a copy-paste architecture (MVVM + CommunityToolkit.Mvvm + DI) for content-commerce desktop apps.
- Validates the AI Copilot floating-button pattern over a domain model.

---

## 3. Sample Objective

Success criteria (use-case sample):

- App launches to Movie Home within 2 s on Windows.
- A reviewer can complete a full booking (Home → Theaters → Seat → Confirmation) in under 60 seconds.
- All three pages plus both popups match the Stitch design tokens and layout.
- AI Copilot button is present and opens an assistant sheet on every page.

---

## 4. Application Scope

### In Scope
- 3 primary pages: **Movie Home**, **Theaters**, **Bookings**
- 2 popup dialogs: **Seat Selection**, **Payment Confirmation**
- Sidebar navigation (Movie Home, Theaters, Bookings)
- Top app bar (title + city + search + notifications + user avatar)
- Floating AI Copilot button (bottom-right)
- Mock in-memory data via `AppSeed.cs`
- Light & dark theme

### Out of Scope (handled manually)
- Real payment integration
- Real auth, real user profile
- Real AI/LLM integration
- Image asset management / licensing
- Internationalization
- Unit tests, integration tests, performance audits
- Accessibility audit
- Marketing copy / production screenshots

---

## 5. Design System (from Stitch)

All values below are taken from the provided Stitch HTML and must be honored throughout the build.

### 5.1 Color Tokens (Light)

| Token | Hex | Usage |
|---|---|---|
| `primary` | `#3525cd` | Brand color; primary buttons, selected date chip, active nav |
| `primary-container` | `#4f46e5` | Active nav bg, selected items |
| `on-primary` | `#ffffff` | Text on primary bg |
| `on-primary-container` | `#dad7ff` | Text on primary-container |
| `secondary` | `#712ae2` | Secondary accents, AI gradient stop |
| `secondary-container` | `#8a4cfc` | Secondary highlights |
| `tertiary` | `#00505f` | Location pin, dining accents |
| `tertiary-container` | `#006a7c` | "Live Synced" dot |
| `error` | `#ba1a1a` | Error / sold-out |
| `error-container` | `#ffdad6` | Error bg |
| `surface` | `#f8f9ff` | App background |
| `surface-container-lowest` | `#ffffff` | Card background |
| `surface-container-low` | `#eff4ff` | Subtle input/chip bg |
| `surface-container` | `#e5eeff` | Selected filter chip |
| `surface-container-high` | `#dce9ff` | Hover surfaces |
| `surface-container-highest` | `#d3e4fe` | Stronger hover |
| `surface-variant` | `#d3e4fe` | Tag bg |
| `outline` | `#777587` | Borders, separators |
| `outline-variant` | `#c7c4d8` | Dot separators |
| `on-surface` | `#0b1c30` | Body text |
| `on-surface-variant` | `#464555` | Muted text |
| `inverse-surface` | `#213145` | Dark badges (IMAX chip) |
| `inverse-on-surface` | `#eaf1ff` | Text on inverse bg |
| `background` | `#f8f9ff` | Window background |
| `on-background` | `#0b1c30` | Text on background |
| `amber-500` | `#f59e0b` | Star rating icon |
| `amber-400` | `#fbbf24` | Star icon in dark badge |

### 5.2 Color Tokens (Dark)

The Stitch file does not define explicit dark tokens, but they should mirror Material 3 conventions. Suggested starting values (refine during build):

| Token | Hex |
|---|---|
| `primary` | `#c3c0ff` |
| `primary-container` | `#4f378b` |
| `on-primary` | `#1f1f1f` |
| `surface` | `#0b1c30` |
| `surface-container-lowest` | `#0a1828` |
| `surface-container-low` | `#11253a` |
| `surface-container` | `#1a3049` |
| `on-surface` | `#eaf1ff` |
| `on-surface-variant` | `#c3c8d4` |
| `outline` | `#8a8a99` |
| `outline-variant` | `#3a4554` |

### 5.3 Typography

- **Display / Headlines / Labels:** **Plus Jakarta Sans** (weights 600, 700, 800)
- **Body:** **Inter** (weights 400, 500, 600)

| Token | Size / Lh / Wt | Usage |
|---|---|---|
| `display-hero` | 44 / 52 / 800, -0.03em | Hero numbers, day "24" on date chip |
| `headline-lg` | 32 / 40 / 700, -0.025em | Page hero titles |
| `headline-md` | 24 / 32 / 600, -0.02em | Section headers |
| `headline-sm` | 18 / 26 / 600, -0.015em | Card titles, theater names |
| `body-lg` | 16 / 24 / 400, -0.01em | Long body |
| `body-md` | 14 / 20 / 400, -0.005em | Default body |
| `body-sm` | 12 / 18 / 400 | Metadata, captions |
| `label-lg` | 14 / 20 / 600, 0.01em | Buttons |
| `label-md` | 12 / 16 / 600, 0.02em | Chips, badge labels |
| `label-sm` | 10 / 14 / 700, 0.05em | Micro labels, all-caps tags |

### 5.4 Spacing

| Token | Value |
|---|---|
| `space-xs` | 0.25rem (4) |
| `space-sm` | 0.5rem (8) |
| `space-md` | 1rem (16) |
| `space-lg` | 1.5rem (24) |
| `space-xl` | 2rem (32) |
| `gutter` | 1.5rem |
| `margin` | 2.5rem |

### 5.5 Radius

| Token | Value |
|---|---|
| `radius-sm` | 0.25rem (4) |
| `radius-md` | 0.5rem (8) |
| `radius-lg` | 0.75rem (12) |
| `radius-2xl` | 1rem (16) |
| `radius-3xl` | 1.5rem (24) |
| `radius-full` | 9999px (pills, FAB) |

### 5.6 Elevation / Shadow

- Card: `0 1px 8px rgba(15,23,42,0.04)` (light) and stronger on hover
- Active / selected: `0 4px 12px rgba(53,37,205,0.25)` (primary glow)
- Modal/Popup: `0 24px 64px rgba(11,28,48,0.2)` plus primary-tinted glow

### 5.7 Iconography

- **Material Symbols Outlined** is the source font. Use Unicode ligatures in `<Label FontFamily="MaterialSymbolsOutlined" Text="movie" />` or via the Syncfusion icon helper.
- Icons referenced in the design: `movie`, `minimize`, `crop_square`, `close`, `theaters`, `domain`, `confirmation_number`, `local_activity`, `pin_drop`, `location_on`, `expand_more`, `search`, `notifications`, `person`, `keyboard_arrow_down`, `calendar_month`, `chevron_left`, `chevron_right`, `tune`, `schedule`, `surround_sound`, `closed_caption`, `swap_horiz`, `hd`, `airline_seat_recline_extra`, `fastfood`, `waves`, `videocam`, `local_parking`, `aspect_ratio`, `wb_iridescent`, `accessible`, `cinematic_blur`, `fullscreen`, `dinner_dining`, `restaurant`, `cloud_download`, `volume_off`, `local_bar`, `check_circle`, `verified`, `arrow_forward`, `star`, `auto_awesome`, `favorite`, `favorite_border`, `event_seat`.

### 5.8 Layout Skeleton (Windows Desktop)

```
+----------------------------------------------------------+
|  [Title bar] Smart Movie Ticket Booking v2.5 (x64)  □ X  |  32 px
+------+---------------------------------------------------+
|      | [Header bar]   [City]  [Search bar]   [bell] [👤] |  64 px
| S    +---------------------------------------------------+
| i    |                                                   |
| d    |  [Main content area]                              |
| e    |  (3 pages switch here)                            |
| b    |                                                   |
| a    |                                                   |
| r    |                                                   |
|      |                                                   |
| 256  |                                              [✨] |
+------+---------------------------------------------------+
```

- **Sidebar width:** 256 px, fixed left, full height (below title bar)
- **Header height:** 64 px, fixed top, blurs background, pl-256 to start
- **Main area:** `padding-top: 96px` (32 + 64), `padding-left: 256px`, `padding: 24px`
- **AI FAB:** bottom-right, 24 px from edges, 48 px circle, primary→secondary gradient

---

## 6. Page-by-Page Specifications

### 6.1 Page 1 — Movie Home

**Route:** `//movie-home`
**Purpose:** Browse now-playing movies and pick one to view theaters for.

**Layout (top → bottom):**

1. **App bar** (per layout skeleton)
2. **Page header row:**
   - Title: "Now Showing in New York" (`headline-md`)
   - Subtitle: "Discover the latest blockbusters playing in your area" (`body-md` muted)
3. **Search bar (compact, in-page)** — placeholder "Search by movie, genre, or theater", `surface-container-low` bg, `radius-md`
4. **Genre chip strip** — horizontal scroll: "All", "Action", "Sci-Fi", "Drama", "Comedy", "Family", "Thriller". Selected = `primary-container` bg; unselected = `surface-container-low` bg.
5. **Movie grid** — 2–4 columns responsive (`CollectionView` with `ItemsLayout="VerticalGrid, 3"`). Each card:
   - 2:3 poster image (192 px height, `radius-lg`, object-cover)
   - `IMAX 3D` chip top-left (dark `inverse-surface` bg, `label-sm` uppercase, `inverse-on-surface` text)
   - Title (`headline-sm`, 2-line clamp)
   - Genre • Runtime (`body-sm` muted)
   - Rating row: ⭐ 4.9 · (28.4k votes) (`label-md`)

**State (HomeViewModel):**
- `ObservableCollection<Movie> Movies`
- `ObservableCollection<Movie> FilteredMovies`
- `ObservableCollection<string> Genres`
- `string SearchText`, `string SelectedGenre`
- `bool IsLoading` → `SfShimmer` while true

**Behaviors:**
- Type into search → debounce 250 ms → re-filter.
- Tap a chip → re-filter by genre.
- Tap a card → push `//theaters?movieId=...` (the movie is set on the booking session).

**Empty state:** centered "No movies match your filters" + "Clear filters" button.

---

### 6.2 Page 2 — Theaters

**Route:** `//theaters?movieId=...`
**Purpose:** Pick a date, optionally apply filters, and choose a theater + showtime for the selected movie.

**Layout (top → bottom):**

1. **App bar**
2. **Selected Movie Context Banner** (`surface-container-lowest` bg, `radius-2xl`, two decorative blurred blobs top-right corner, p-24):
   - 2:3 poster thumbnail (left, 128 × 192, `radius-xl`)
   - IMAX 3D chip top-left of poster (`inverse-surface` bg, `label-sm` uppercase)
   - Meta strip: `PG-13` chip (primary text) | `Sci-Fi • Epic Adventure` chip (muted) | ��� 4.9 (28.4k votes) chip
   - Title (`display-hero`)
   - Synopsis (`body-md` muted, 2-line clamp)
   - Meta row: ⏱ 2h 48m Runtime · Dolby Atmos & Laser 4K · CC & Audio Description (with `primary` icons)
   - Right side: "Change Movie" outlined button (`swap_horiz` icon, rotates 180° on hover); "Currently Browsing" + count text
3. **Date Selection section** (mb-32):
   - Header: 📅 "Select Date" + month caption (e.g. "• October 2024")
   - ‹ › navigation buttons (`surface-container-lowest` chips, 32×32)
   - 7-column date grid (2 cols on mobile, 4 on sm, 7 on md+). Each chip is a `radius-2xl` card with 3 stacked lines:
     - Top: `label-sm` uppercase tag (e.g. "Today", "Tomorrow", "Weekend", "Weekday")
     - Middle: day number in `display-hero` size
     - Bottom: weekday + month (e.g. "Thu • Oct")
   - Active chip: `primary-container` bg, animated `ping` dot top-right in `secondary-fixed` color
4. **Cinema Experience Filters** (`surface-container-lowest` bg, `radius-xl`, p-16, mb-32):
   - Header: ⚙ "Cinema Experience Filters" + "Showing 4 partner venues nearby" + "Reset All" link
   - **Row A — Formats** (label "Formats:"):
     `All Formats` (active) | `IMAX Laser` | `Dolby Cinema` | `3D Experience` | `4DX Motion` | `Standard 2D`
   - **Row B — Audio & Sub** + right-aligned Price + Recliner Seats toggle + Open Now toggle:
     `All Languages` (active) | `English (Original)` | `Spanish Dub` | `Subtitled (CC)`
     Price dropdown: "Any Price" with ⌄
     Recliner Seats ☐✓
     Open Now ☐✓
5. **Available Theaters & Showtimes** (mb-32):
   - Section header: "Available Theaters & Showtimes" (`headline-md`) + subtitle "Real-time certified seating allocations for Thursday, Oct 24"
   - Right side: "Sort by:" + "Proximity (Distance)" dropdown
   - **Theater cards** (`surface-container-lowest` bg, `radius-3xl`, p-24, `shadow-sm`):
     - Header row: 56×56 icon tile (`surface-container-low` bg, primary-tint icon) + name (`headline-sm` bold) + category chip (e.g. "Flagship Venue" in `surface-container-high` bg, primary text) + location (📍 "Times Square • 234 W 42nd St, Manhattan") + (0.8 mi away) primary bold
     - Right side: "From $18.50" + "142 seats available" + ♥ favorite icon button
     - Facility badges row: pill chips with icon + label (e.g. "🎬 IMAX Laser 4K" primary, "🔊 Dolby Atmos" secondary, "💺 Plush VIP Recliners" muted)
     - "Select Screening Time" label
     - **Showtime grid** (2–6 cols responsive): each pill is a `radius-xl` card with 3 stacked rows:
       - Time (`headline-sm` bold)
       - Format label (e.g. "Standard 2D", color-coded: IMAX=primary, Dolby=tertiary, 4DX=secondary)
       - Price (primary bold) + status (e.g. "45 seats", "Fast Filling" amber, "Available" muted, "Prime Time" badge on selected)
     - **Selected showtime** (e.g. 07:15 PM IMAX Laser): `primary-container` bg, `on-primary` text, `check_circle` icon, "Prime Time" sub-badge
     - **Footer action bar** (`surface-container-low/50` bg, full-width strip): "Dolby Vision 4K projection certified for auditorium 1 & 3" text on left, "Select Showtime & Continue" filled primary button (with `arrow_forward` icon) on right
6. **Top-Rated Partner Venues** (mb-32, optional but matches design):
   - Header: "Top-Rated Partner Venues" + "View all 32 cinemas" link
   - 4-column grid of mini venue cards (image top with `inverse-surface` ⭐4.9 badge, name + description + area + distance below)

**State (TheatersViewModel):**
- `Movie SelectedMovie`
- `DateTime SelectedDate`, `List<DateTime> DateStrip` (7 days from today)
- `string SelectedFormat`, `string SelectedLanguage`, `string SelectedPrice`
- `bool ReclinerOnly`, `bool OpenNow`
- `string SortBy` (default "Proximity")
- `ObservableCollection<TheaterWithShowtimes> Theaters` — each `Theater` carries its list of `Showtime` for the current date
- `Showtime? SelectedShowtime`

**Behaviors:**
- Tap a date chip → swap active styles, reload showtimes for that date.
- Tap a showtime pill → highlight as `SelectedShowtime`, enable the footer "Select Showtime & Continue" button.
- Tap footer CTA → push **Seat Selection popup** (or full-page modal) with the showtime context.
- Tap "Change Movie" → pop back to `//movie-home`.

---

### 6.3 Page 3 — Bookings

**Route:** `//bookings`
**Purpose:** Show past confirmed bookings (sample use case — no separate "upcoming/past" tabs).

**Layout (top → bottom):**

1. **App bar**
2. **Page header row:**
   - Title: "My Bookings" (`headline-md`)
   - Subtitle: "Confirmed tickets from your recent sessions" (`body-md` muted)
3. **Bookings list** — vertical stack of booking cards (`surface-container-lowest` bg, `radius-2xl`, p-16, mb-12). Each card has:
   - Top row: movie title (`headline-sm`) on left, status badge on right ("Confirmed" in primary-container, "Cancelled" in error-container)
   - Meta row: 🏛 theater · 🕐 date · 💺 seat count (icons in `primary` color, text in `body-sm` muted)
   - Bottom row: total amount (`headline-sm` bold) on left, "View Ticket" outlined button on right
4. **Empty state:** centered "No bookings yet. Book your first movie!" + "Browse Movies" button (navigates to `//movie-home`)

**State (BookingsViewModel):**
- `ObservableCollection<Booking> Bookings` — sourced from `BookingRepository`

**Behaviors:**
- Tap a card or "View Ticket" → push **Payment Confirmation popup** in read-only mode with the booking details.

---

### 6.4 Popup 1 — Seat Selection

**Trigger:** Tapping "Select Showtime & Continue" on the Theaters page.
**Surface:** `SfPopup` or full-screen modal page (use whichever the design allows; preferred is `SfPopup` for desktop).

**Layout (top → bottom):**

1. **Header strip:** back chevron + "Select Seats" + theater name + showtime (movie title on the right as context). Sub-strip with screen name.
2. **Screen indicator:** "All eyes this way" curved line above the seat grid.
3. **Seat grid:** rows A��J × columns 1–14. Aisle gap between 6 and 7. Each seat is a small `radius-md` cell. States:
   - **Available:** `surface-container-low` bg, `on-surface` text
   - **Occupied:** `outline-variant` bg, `outline` text, disabled
   - **Selected:** `primary` bg, `on-primary` text, slight elevation
   - **Premium:** gold border (`amber-500`), `surface-container-lowest` bg, gold text
4. **Legend strip:** small color-swatch list (Available · Selected · Occupied · Premium).
5. **Sticky footer card** (`surface-container-lowest` bg, `radius-2xl` top corners, p-16):
   - Left: ticket count, subtotal, convenience fee, total
   - Right: "Confirm Seats" filled `SfButton` (primary, with `arrow_forward` icon). Disabled when count = 0.

**State (SeatSelectionViewModel):**
- `Showtime Showtime`
- `int Rows = 10`, `int Columns = 14`
- `ObservableCollection<Seat> Seats` (built deterministically from `(ShowtimeId + ScreenLayout)` seed)
- `ObservableCollection<Seat> SelectedSeats`
- `decimal Subtotal`, `decimal ConvenienceFee = 30m`, `decimal Total`
- `bool CanConfirm => SelectedSeats.Count > 0`

**Behaviors:**
- Tap an available seat → toggle selection (re-compute totals).
- Tap an occupied seat → no-op.
- Tap "Confirm Seats" → close popup, push **Payment Confirmation popup** with the booking details prefilled.

---

### 6.5 Popup 2 — Payment Confirmation

**Trigger:** Tapping "Confirm Seats" on the Seat Selection popup, OR tapping "View Ticket" on a booking in Bookings.

**Layout (top → bottom):**

1. **Success header:** centered 🎉 large emoji or checkmark + "Booking Confirmed!" (`display-hero` size) + "Your tickets are locked in. Enjoy the show!" subtitle
2. **Booking ID strip:** "Booking ID: BK-123456" in `label-md` with copy icon
3. **QR code area:** 200×200 placeholder square (`surface-container-low` bg, "QR" label) — `Syncfusion.Maui.Barcode.SfBarcodeGenerator` with QR symbology if available; otherwise a static image asset
4. **Ticket card** (`surface-container-lowest` bg, `radius-2xl`, p-16):
   - Movie title (`headline-sm`)
   - Theater · screen (📍)
   - Date · time (🗓) with `primary` icon
   - Seats (💺)
   - Total amount (`headline-sm` bold)
5. **Action row** (button group):
   - "Download Ticket" outlined `SfButton` (mock — shows inline toast "Ticket saved to device")
   - "Share Ticket" outlined `SfButton` (mock — shows inline toast "Share link copied")
   - "Back to Home" filled primary `SfButton` (closes popup, navigates to `//movie-home`)

**State (PaymentConfirmationViewModel):**
- `Booking Booking` (the saved booking)
- `string QrPayload` (e.g. the booking ID + a hash)
- `RelayCommand DownloadCommand`, `RelayCommand ShareCommand`, `RelayCommand BackHomeCommand`

---

## 7. Navigation Flow

```
[App launch]
    │
    ▼
[Movie Home]  ──tap card──►  [Theaters ?movieId=...]
                                  │  tap showtime CTA
                                  ▼
                            [Seat Selection popup]
                                  │  Confirm Seats
                                  ▼
                            [Payment Confirmation popup]
                                  │  Back to Home
                                  ▼
                            [Movie Home]

[Sidebar] Theaters  → [Theaters ?movieId=<currently selected or first movie>]
[Sidebar] Bookings  → [Bookings]
[App bar search]    → filters Movie Home in-place
[AI FAB]            → opens AI Copilot sheet on every page
```

Use a single `Window` with a 2-row outer `Grid` (title bar 32 + content) and an inner 2-column `Grid` (sidebar 256 + main). Within main, swap pages by toggling `IsVisible` on three `ContentView` instances OR by switching `Content` on a `ContentPresenter` bound to a navigation-state enum. **No Shell is required** for the desktop layout — the sidebar acts as the navigation chrome.

---

## 8. Data Models

All models live under `/Models`. Mock data is provided by `AppSeed.cs` (static class). Each model derives from `ObservableObject` only when it needs change notification; otherwise plain POCO with `[JsonPropertyName]` (kept for future JSON hydration).

### 8.1 Movie

```csharp
public partial class Movie : ObservableObject
{
    public string Id { get; set; }              = "";
    public string Title { get; set; }           = "";
    public string Tagline { get; set; }         = "";
    public string Synopsis { get; set; }        = "";
    public string PosterUrl { get; set; }       = "";
    public string BannerUrl { get; set; }       = "";
    public string Genre { get; set; }           = "";
    public List<string> Genres { get; set; }    = new();
    public string Language { get; set; }        = "English";
    public List<string> Languages { get; set; } = new();
    public int DurationMinutes { get; set; }
    public double Rating { get; set; }
    public int VoteCount { get; set; }
    public string RatingLabel { get; set; }     = "PG-13";
    public string Format { get; set; }          = "IMAX 3D";
    public List<CastMember> Cast { get; set; }  = new();
    public bool IsNowShowing { get; set; }      = true;
}

public class CastMember
{
    public string Name { get; set; }      = "";
    public string Character { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
}
```

### 8.2 Theater & Showtime

```csharp
public partial class Theater : ObservableObject
{
    public string Id { get; set; }          = "";
    public string Name { get; set; }        = "";
    public string Location { get; set; }    = "";
    public string City { get; set; }        = "New York, NY";
    public double DistanceKm { get; set; }
    public string Category { get; set; }    = "";     // "Flagship Venue", "Motion Immersion", "Curated Dining", "Original 70mm Grand Screen"
    public string Icon { get; set; }        = "theaters";
    public List<string> Amenities { get; set; } = new();
    public string Note { get; set; }        = "";     // footer note
}

public class Showtime
{
    public string Id { get; set; }              = "";
    public string MovieId { get; set; }         = "";
    public string TheaterId { get; set; }       = "";
    public DateTime Date { get; set; }          // local date
    public string StartTime { get; set; }       = "";  // "07:15 PM"
    public string Format { get; set; }          = "Standard 2D";
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
    public string StatusLabel { get; set; }     = "Available";  // "Available" | "Fast Filling" | "Almost Full" | "Selling Fast" | "Prime Time"
    public bool IsPrimeTime { get; set; }
}
```

### 8.3 Seat

```csharp
public class Seat
{
    public string Id { get; set; }        = "";   // e.g. "A5"
    public int Row { get; set; }
    public int Column { get; set; }
    public SeatTier Tier { get; set; }    = SeatTier.Normal;
    public SeatState State { get; set; }  = SeatState.Available;
    public decimal Price { get; set; }
}

public enum SeatTier   { Normal, Premium, Executive }
public enum SeatState  { Available, Occupied, Selected }
```

### 8.4 Booking

```csharp
public partial class Booking : ObservableObject
{
    public string BookingId { get; set; }      = "";   // "BK-123456"
    public DateTime CreatedAt { get; set; }
    public string MovieId { get; set; }        = "";
    public string MovieTitle { get; set; }     = "";
    public string PosterUrl { get; set; }       = "";
    public string TheaterName { get; set; }    = "";
    public string ScreenName { get; set; }     = "";
    public string ShowTime { get; set; }       = "";
    public DateTime ShowDate { get; set; }
    public List<string> Seats { get; set; }    = new();
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }         = "Confirmed";   // Confirmed | Cancelled
    public string QrPayload { get; set; }       = "";
}
```

### 8.5 AppSeed.cs

A static class that returns `IReadOnlyList<T>` for movies, theaters, and showtimes. Showtimes are generated programmatically for 7 days × each (movie, theater) pair. Seat occupancy is seeded per (showtime, theater) and the seed string is the showtime ID so the same showtime always shows the same occupied seats across launches.

---

## 9. Project Structure

```
SmartMovieBooking/
├── App.xaml / App.xaml.cs
├── MainWindow.xaml / .cs         ← hosts TitleBar + Sidebar + Main + AI FAB
├── MauiProgram.cs
├── SmartMovieBooking.csproj
│
├── Models/
│   ├── Movie.cs
│   ├── CastMember.cs
│   ├── Theater.cs
│   ├── Showtime.cs
│   ├── Seat.cs
│   └── Booking.cs
│
├── Services/
│   ├── IMovieService.cs / MovieService.cs
│   ├── ITheaterService.cs / TheaterService.cs
│   ├── IShowtimeService.cs / ShowtimeService.cs
│   ├── IBookingService.cs / BookingService.cs
│   ├── BookingRepository.cs     ← singleton in-memory store
│   ├── BookingSession.cs        ← singleton holds the in-progress booking
│   ├── AppSeed.cs               ← static mock data
│   └── SeatLayoutFactory.cs     ← deterministic seat grids
│
├── ViewModels/
│   ├── BaseViewModel.cs
│   ├── MainWindowViewModel.cs   ← holds the current page, AI state
│   ├── HomeViewModel.cs
│   ├── TheatersViewModel.cs
│   ├── BookingsViewModel.cs
│   ├── SeatSelectionViewModel.cs
│   └── PaymentConfirmationViewModel.cs
│
├── Views/
│   ├── MovieHomeView.xaml(.cs)
│   ├── TheatersView.xaml(.cs)
│   ���── BookingsView.xaml(.cs)
│   ├── SeatSelectionView.xaml(.cs)        ← hosted inside SfPopup
│   └── PaymentConfirmationView.xaml(.cs)  ← hosted inside SfPopup
│
├── Controls/
│   ├── TitleBarView.xaml(.cs)
│   ├── SidebarView.xaml(.cs)
│   ├── HeaderBarView.xaml(.cs)
│   ├── MovieCardView.xaml(.cs)
│   ├── DateChipView.xaml(.cs)
│   ├── TheaterCardView.xaml(.cs)
│   ├��─ ShowtimePillView.xaml(.cs)
│   ├── SeatControl.xaml(.cs)
│   ├── SeatLegendView.xaml(.cs)
│   ├── BookingCardView.xaml(.cs)
│   ├── AiCopilotFab.xaml(.cs)
│   └── QrCodeView.xaml(.cs)
│
├── Converters/
│   ├── BoolToVisibilityConverter.cs
│   ├── InverseBoolConverter.cs
│   ├── SelectedToColorConverter.cs
│   ├── ShowtimeStatusToColorConverter.cs
│   ├── DateChipStateToStyleConverter.cs
│   └── CurrencyConverter.cs
│
└── Resources/
    ├── Styles/
    │   ├─��� Colors.xaml
    │   ├── Typography.xaml
    │   ├── Buttons.xaml
    │   ├── Chips.xaml
    │   ├── Cards.xaml
    │   └── Theme.xaml
    └── Fonts/  (PlusJakartaSans, Inter, MaterialSymbolsOutlined)
```

### DI registration (MauiProgram.cs)

```csharp
builder.Services.AddSingleton<IMovieService, MovieService>();
builder.Services.AddSingleton<ITheaterService, TheaterService>();
builder.Services.AddSingleton<IShowtimeService, ShowtimeService>();
builder.Services.AddSingleton<IBookingService, BookingService>();
builder.Services.AddSingleton<BookingRepository>();
builder.Services.AddSingleton<BookingSession>();

builder.Services.AddTransient<MainWindowViewModel>();
builder.Services.AddTransient<HomeViewModel>();
builder.Services.AddTransient<TheatersViewModel>();
builder.Services.AddTransient<BookingsViewModel>();
builder.Services.AddTransient<SeatSelectionViewModel>();
builder.Services.AddTransient<PaymentConfirmationViewModel>();

builder.Services.AddTransient<MainWindow>();
builder.Services.AddTransient<MovieHomeView>();
builder.Services.AddTransient<TheatersView>();
builder.Services.AddTransient<BookingsView>();
builder.Services.AddTransient<SeatSelectionView>();
builder.Services.AddTransient<PaymentConfirmationView>();
```

### State Management

- `BookingSession` (singleton) holds the in-progress `Movie`, `Theater`, `Showtime`, `Seats`, `Total`.
- `BookingRepository` (singleton) holds the in-memory list of confirmed `Booking`s.
- Pages never mutate each other; they write to `BookingSession` and the next page reads from it.
- AI Copilot uses a separate singleton state so it persists across page navigations.

---

## 10. Syncfusion Control Usage

| Control | Package | Where |
|---|---|---|
| `SfButton` | `Syncfusion.Maui.Buttons` | All primary CTAs (Select Showtime, Confirm Seats, Back to Home, Book Tickets), outlined secondary buttons (Change Movie, View Ticket, Download, Share) |
| `SfChip` / `Syncfusion.Maui.Chips` | Genre chips, format filters, audio & sub filters, status badges, amenities |
| `SfCardView` / `Syncfusion.Maui.Cards` | Movie cards, theater cards, ticket card, booking cards, filter panel, date chips |
| `SfListView` | Theater list, booking list |
| `SfCollectionView` | Movie grid, showtime grid, amenities row |
| `SfPopup` | Seat Selection popup, Payment Confirmation popup |
| `SfBottomSheet` | AI Copilot sheet (alternative to popup) |
| `SfTextInputLayout` | Search bar, copy-to-clipboard snackbar input |
| `SfShimmer` | Initial page loading |
| `SfSegmentedControl` | Format filter (single select), audio & sub filter |
| `SfEffectsView` | Ripple on chips, cards |
| `SfGradientView` | Decorative blurred blobs on movie banner, AI FAB gradient |
| `SfBadgeView` | Prime Time chip on selected showtime, status badges |
| `SfToggleButton` | Recliner Seats, Open Now toggles |
| `SfQRCodeGenerator` / `Syncfusion.Maui.Barcode` | QR code in Payment Confirmation |
| `SfToolbar` (or built-in MAUI) | App title bar |
| `SfNavigationDrawer` (optional) | Could replace the fixed sidebar if a collapsible drawer is preferred |
| `SfRadioButton` | Sort by dropdown options |

### Theming

A single `SyncfusionThemeResourceDictionary` (MaterialLight / MaterialDark) is registered in `App.xaml`. The brand `primary` / `secondary` / `tertiary` colors from §5.1 are applied as `ThemePrimaryColor`, `ThemeSecondaryColor`, etc. via the `ThemeService` pattern from the existing `EmployeeDirectory` reference.

---

## 11. AI Copilot (Floating)

A persistent floating action button (FAB) sits in the bottom-right corner of the desktop window on every page. It is a 48 px circular button with a primary→secondary gradient and the `auto_awesome` icon. A small `secondary-fixed` dot is overlaid on the top-right corner of the FAB to indicate readiness.

On hover, a tooltip "Cinema AI Copilot • Ready" fades in to the left of the FAB.

Tapping the FAB opens an `SfBottomSheet` (preferred for desktop) with `SfAIAssistView` inside. The assistant is wired to a simple in-memory intent engine over the seeded `AppSeed` data — it answers prompts like:

- "Recommend an action movie"
- "What is playing in Times Square?"
- "Compare IMAX vs 4DX"
- "Best-rated movie this week"
- "Family-friendly movies"

The assistant also acts as a **navigation helper**: every response can carry a "Go to Movie" or "Book Seats" deep-link button that pushes the corresponding page/popup.

**State (AiCopilotViewModel):**
- `bool IsOpen`
- `ObservableCollection<AssistMessage> Messages`
- `RelayCommand OpenCommand`, `RelayCommand SendCommand`

This is a sample — no real LLM is required. A keyword/pattern-matching service in `Services/AiAssistantService.cs` returns canned responses seeded with the current `AppSeed.Movies`.
