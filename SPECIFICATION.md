# Smart Movie Ticket Booking — Specification, Harness & Development Plan

**Project Type:** .NET MAUI Showcase Sample
**UI Toolkit:** Syncfusion .NET MAUI Controls (v33.2.x)
**Architecture:** MVVM with CommunityToolkit.Mvvm
**Data Source:** Mock JSON (no backend / no real payment)
**Design Reference:** Stitch — `https://stitch.withgoogle.com/projects/14368844853394156269`
**Specification Reference (Loop):** `https://loop.cloud.microsoft/p/eyJ1IjoiaHR0cHM6Ly9zeW5jZnVzaW9uLnNoYXJlcG9pbnQuY29tL2NvbnRlbnRzdG9yYWdlL0NTUF8xMDc3OTg2Zi1mODE2LTRiMmItYTI3OC04ODM5NzhkNGU5Y2U`

---

## Table of Contents

1. [Sample Overview](#1-sample-overview)
2. [Business Objective](#2-business-objective)
3. [Sample Objective](#3-sample-objective)
4. [User Personas](#4-user-personas)
5. [Application Features](#5-application-features)
6. [Screen-by-Screen Specifications](#6-screen-by-screen-specifications)
7. [User Journey](#7-user-journey)
8. [Navigation Flow](#8-navigation-flow)
9. [Data Models](#9-data-models)
10. [MVVM Architecture](#10-mvvm-architecture)
11. [Syncfusion Control Usage](#11-syncfusion-control-usage)
12. [AI Assistant Integration](#12-ai-assistant-integration)
13. [Mock Data Requirements](#13-mock-data-requirements)
14. [Harness Requirements](#14-harness-requirements)
15. [Development Tasks](#15-development-tasks)
16. [Acceptance Criteria](#16-acceptance-criteria)
17. [User Stories](#17-user-stories)
18. [Milestones](#18-milestones)

---

## 1. Sample Overview

**Smart Movie Ticket Booking** is a modern, cross-platform .NET MAUI showcase sample that replicates a complete movie ticket booking workflow comparable to TicketNew, BookMyShow, and Fandango. The sample is built entirely on Syncfusion's .NET MAUI control suite and demonstrates how to compose them into a production-quality mobile UI with mock data.

The sample provides a full end-to-end booking flow: discover a movie → review details → pick a theater/showtime → select seats → review summary → mock-pay → see confirmation → revisit from "My Bookings". A conversational **AI Movie Assistant** (Syncfusion `AIAssistView`) is available from every primary screen to answer movie-related questions and recommend titles.

**Platforms supported:** Android, iOS, Windows (WinUI), Mac Catalyst.

**Non-production disclaimer:** All data, transactions, and AI responses are simulated. The sample is for educational, evaluation, and demonstration purposes only.

### High-level Value Statement

- Demonstrates Syncfusion MAUI controls in a real-world context.
- Provides a complete, runnable reference for a content-commerce app.
- Showcases modern mobile UX patterns: shimmer loading, bottom sheets, popups, animated transitions, and an embedded AI assistant.

---

## 2. Business Objective

The reference applications (TicketNew, BookMyShow) succeed because they reduce the friction of movie discovery and ticket purchase. The objective of this sample is to:

1. **Demonstrate a complete content-to-checkout journey** in MAUI without backend dependencies.
2. **Showcase Syncfusion's mobile-first controls** (ListView, CollectionView, BottomSheet, Popup, TabView, Shimmer, AIAssistView, etc.) in a meaningful, non-trivial composition.
3. **Provide a copy-paste architecture** that MAUI developers can use to bootstrap their own ticket-booking or content-commerce applications.
4. **Validate the AI AssistView integration pattern** for conversational UX over a domain model (movies, theaters, bookings).

---

## 3. Sample Objective

The sample is intentionally designed as a **showcase / reference application** — not a production system. Success looks like this:

- A developer or reviewer can run the sample on any MAUI target platform and complete a full booking in under 60 seconds.
- Every screen follows the Stitch design language (cards, generous spacing, type hierarchy, brand color, dark/light theme).
- AI Assistant returns contextually relevant answers drawn from the in-app movie catalog.
- The codebase is small enough to read end-to-end in an afternoon and large enough to be representative of a real product.

**Definition of Done (Sample):**

- Build succeeds for all four target frameworks.
- App launches to Home within 2 seconds on a mid-tier device.
- Booking flow can be completed end-to-end without crashes.
- AI Assistant responds to every question in the supported list.

---

## 4. User Personas

| Persona | Description | Primary Goals | Key Screens |
|---|---|---|---|
| **Casual Moviegoer — Riya (24)** | Plans movie outings on weekends with friends. Browses by genre. | Discover what's playing, see ratings, book quickly. | Home, Movie Details, Theater, Seat, Confirmation. |
| **Family Planner — Anand (38)** | Books for family of 4. Prefers matinee shows, kid-friendly films. | Find family movies, pick comfortable seats, manage multiple tickets. | Home (with genre filter), Seat, Booking Summary, My Bookings. |
| **Cinephile — Megha (29)** | Wants details, trailers, ratings, comparisons. | Deep dive on movie info, compare titles, plan ahead. | Movie Details, AI Assistant, Theater, My Bookings. |
| **On-the-go Booker — Karthik (32)** | Books last-minute after work. Values speed. | Quick search, recent movies, one-tap rebook. | Home, Search, My Bookings (re-book). |

---

## 5. Application Features

The sample is organized into **9 features**, mapped to **9 primary screens**.

### Feature 1: Movie Discovery
- Browse now-playing movies as cards.
- Search by title.
- Filter by genre and language.
- Tap a card to open movie details.

### Feature 2: Movie Details
- Banner + poster.
- Synopsis, cast, runtime, rating, release date, languages.
- "Trailer" mock action.
- "Book Tickets" CTA.

### Feature 3: Theater & Showtime Selection
- List of nearby theaters for the selected movie.
- Date selector (next 7 days).
- Showtime chips per theater.

### Feature 4: Seat Selection
- Visual seat layout (rows × columns) per screen.
- States: Available, Occupied, Selected, Aisle.
- Tiered pricing (Premium / Executive / Normal).
- Live total, ticket count, "Proceed" CTA.

### Feature 5: Booking Summary
- Recap: movie, theater, showtime, seats, convenience fee, total.
- Edit seat action.
- "Proceed to Pay" CTA.

### Feature 6: Mock Payment
- Payment method chips: UPI, Credit Card, Debit Card, Wallet.
- Method-specific entry forms (mock — no validation beyond format).
- "Pay" button with processing animation.

### Feature 7: Booking Confirmation
- Success animation.
- Booking ID + QR code (mock).
- Ticket details card.
- Download / Share / Back to Home actions.

### Feature 8: My Bookings
- Tabs: Upcoming / Past.
- Booking cards with status badge.
- Tap to view ticket details.

### Feature 9: AI Movie Assistant
- Syncfusion `AIAssistView`.
- Persistent FAB / toolbar entry from all primary screens.
- Domain-aware responses (recommendations, comparisons, info, ratings, booking help).
- Suggested prompts chip row.

---

## 6. Screen-by-Screen Specifications

Every screen below is described as an AI agent would implement it: layout, controls, data binding, validation, navigation, and states.

### 6.1 Home Screen

**Purpose:** Primary discovery surface. Showcases Syncfusion's modern card-based mobile layout.

**Layout (top to bottom):**

1. **App Bar (SfButton + Brand)**
   - Brand logo / "CineBook" wordmark.
   - "My Bookings" icon button → My Bookings.
   - AI Assistant FAB.

2. **Search Bar (SfTextInputLayout / Entry)**
   - Placeholder: "Search movies, actors, directors…"
   - Live filter on `Movies` collection (debounced 250 ms).

3. **Genre Filter Chips (`SfSegmentedControl` or chip layout)**
   - All, Action, Comedy, Drama, Thriller, Romance, Family, Sci-Fi.
   - Tapping replaces `Movies` view with the filtered subset.

4. **Language Filter (SfComboBox / dropdown)**
   - All, English, Hindi, Tamil, Telugu, Malayalam, Kannada.

5. **Now Showing — Horizontal Carousel (`SfCollectionView` with `LinearLayout`, Horizontal)**
   - Movie cards (banner, title, genre chip, rating badge, duration).

6. **Coming Soon — Vertical List (`SfListView`)**
   - Compact rows: poster thumbnail, title, release date, "Notify Me" button.

7. **Recommended For You — 2-column grid (`SfCollectionView`)**
   - Cards identical to Now Showing but smaller.

**State Management:**

- `HomeViewModel.Movies` (full list).
- `HomeViewModel.FilteredMovies` (after search + filter).
- `HomeViewModel.SelectedGenre`, `SelectedLanguage`, `SearchText`.

**Loading State:** `SfShimmer.View` showing 4–6 card-shaped shimmer placeholders for ~800 ms while mock data is "fetched."

**Empty State:** Centered illustration + "No movies found. Try a different filter."

**Navigation:**

- Tap card → `MovieDetailsPage(movieId)`.
- Tap "My Bookings" → `MyBookingsPage`.
- Tap AI FAB → opens `AIAssistView` overlay (modal sheet).

---

### 6.2 Movie Details Screen

**Purpose:** Provide complete information and convert interest into booking intent.

**Layout:**

1. **Hero Banner** (top, ~40% of viewport).
   - Poster (left) + gradient overlay.
   - Movie title (display).
   - Tagline (optional).

2. **Meta Strip** (chips row).
   - Genre • Runtime • Rating (e.g., 8.4/10) • Language.

3. **Synopsis Block.**
   - Long-form description (2–3 paragraphs from JSON).

4. **Cast List** (horizontal `SfCollectionView`).
   - Circular avatar, actor name, character.

5. **Available Languages** (chips).

6. **Action Bar (sticky bottom):**
   - Secondary "▶ Watch Trailer" (`SfButton` outlined).
   - Primary "Book Tickets" (`SfButton` filled, brand color) → `TheaterSelectionPage`.

**State:**

- `MovieDetailsViewModel.Movie`, `Cast`, `AvailableLanguages`.

**Behaviors:**

- Trailer button: shows a `SfPopup` with "Trailer is not available in this sample."
- "Book Tickets" requires `Movie` to be loaded; disabled until then.

---

### 6.3 Theater Selection Screen

**Purpose:** Help the user pick a theater, date, and showtime.

**Layout:**

1. **Header:** Back + "Select Theater" + Movie poster thumbnail + title.

2. **Date Selector (horizontal `SfListView` or chip strip).**
   - 7 days starting today, "Today" / "Tomorrow" / formatted date.
   - Selected day highlighted with brand color.

3. **Theater List (`SfListView` or `SfCollectionView`).** For each theater:
   - Theater name (bold).
   - Location / area + distance ("3.2 km").
   - Amenities (Parking, Food, Recliner, Dolby) as small icons.
   - Showtime chips (e.g., 10:30 AM, 01:15 PM, 06:45 PM, 10:00 PM).
   - Each chip → seats available count badge.

4. **Empty state:** "No theaters available for the selected date."

**State:**

- `TheaterSelectionViewModel.MovieId`, `SelectedDate`, `Theaters[]`, each with `Showtimes[]`.

**Behaviors:**

- Tapping a showtime chip stores the selection in `BookingSession` and navigates to `SeatSelectionPage`.
- Disabled chips for sold-out shows.

---

### 6.4 Seat Selection Screen

**Purpose:** Visual, interactive seat picker with tiered pricing.

**Layout:**

1. **Header:** Back + "Select Seats" + Theater + Screen name + Showtime.

2. **Screen Indicator:** "All eyes this way" arc/line at top of seat grid.

3. **Seat Grid (custom `Grid` with `Bindable Layout` or canvas).**
   - Rows labeled A–J, columns 1–14 (configurable per screen).
   - Aisle gap between columns 6–7.
   - Seat states:
     - Available (light surface).
     - Occupied (gray, disabled).
     - Selected (brand color, raised).
     - Premium (gold border + higher price).
   - Tap toggles selection (max 10 seats).

4. **Legend Strip:** Available / Selected / Occupied / Premium.

5. **Pricing Summary (sticky bottom card):**
   - Ticket count.
   - Subtotal.
   - Convenience fee (₹30 flat).
   - Total.
   - "Proceed" `SfButton`.

**State:**

- `SeatSelectionViewModel.ShowtimeId`, `Seats[]`, `SelectedSeats[]`, `Pricing`.

**Validation:**

- At least 1 seat required to proceed.
- If 0 seats selected, "Proceed" is disabled.

**Behaviors:**

- Each selection triggers re-calculation of pricing.
- Scrolls horizontally on small screens if grid is wide.

---

### 6.5 Booking Summary Screen

**Purpose:** Final review before payment.

**Layout:**

1. **Header:** Back + "Booking Summary."

2. **Movie Card:** Poster, title, language, format.

3. **Theater & Showtime Card:** Theater name, screen, date, time.

4. **Seat List Card:** A1, A2, B5, B6 (chips) + "Edit" link → back to `SeatSelectionPage`.

5. **Price Breakdown Card:** Subtotal, convenience fee, GST (5%), Total.

6. **Apply Coupon** (mock input + button — accepts "WELCOME10" for 10% off, others show invalid).

7. **Sticky Bottom:** "Proceed to Pay ₹XXX" `SfButton`.

**State:** `BookingSummaryViewModel.BookingDraft`.

**Validation:** Cannot proceed if total ≤ 0 or seats empty.

---

### 6.6 Payment Screen

**Purpose:** Simulate payment method selection and processing.

**Layout:**

1. **Header:** Back + "Payment."

2. **Payment Method Selector (chip group / `SfSegmentedControl`).**
   - UPI • Card • Wallet.

3. **Method-Specific Form:**
   - UPI: VPA input + "Verify" (mock).
   - Card: Number, Expiry, CVV, Cardholder Name (with `SfTextInputLayout`).
   - Wallet: Paytm / PhonePe / Amazon Pay logos (static), "Pay with Wallet" button.

4. **Order Recap (collapsed):** Total amount, "View Details."

5. **Pay Button** with shimmer/processing state.

**Processing Animation:**

- Bottom sheet (`SfBottomSheet`) opens on tap of Pay.
- Lottie or built-in spinner: "Processing payment…"
- After ~2.5 seconds, success path:
  - Generate Booking ID.
  - Generate QR code.
  - Persist booking to in-memory store.
  - Navigate to `BookingConfirmationPage`.

**Failure Simulation (5%):** Show a `SfPopup` "Payment failed. Please try again." with Retry / Change Method.

---

### 6.7 Booking Confirmation Screen

**Purpose:** Deliver the "delight moment."

**Layout:**

1. **Success Animation (top):** Animated checkmark or Lottie.

2. **Booking ID + QR Code** (mock — `Syncfusion.SfImageEditor` or simple QR generator).

3. **Ticket Card:**
   - Movie, theater, screen, date, time, seats, total.

4. **Action Row:**
   - "Download Ticket" (mock — opens `SfPopup` "Downloaded to device").
   - "Share Ticket" (mock — opens share sheet via `Share.RequestAsync`).
   - "Back to Home" → `HomePage` (pops to root).

**State:** `BookingConfirmationViewModel.Booking` (read-only after creation).

---

### 6.8 My Bookings Screen

**Purpose:** Let users revisit past and upcoming tickets.

**Layout:**

1. **App Bar:** "My Bookings" + back.

2. **`SfTabView`:** "Upcoming" / "Past" tabs.

3. **Booking List (`SfListView` or `SfCollectionView`):**
   - For each booking: poster thumb, title, theater, date/time, seat count, status badge, "View Ticket" button.

4. **Empty State:** Illustration + "No upcoming bookings. Book one now!" CTA.

**State:** `MyBookingsViewModel.UpcomingBookings`, `PastBookings`.

---

### 6.9 AI Movie Assistant (Overlay)

**Purpose:** Conversational layer over the catalog.

**Layout:**

- Opens as a modal bottom sheet (`SfBottomSheet`) or full-screen modal.
- Syncfusion `SfAIAssistView` configured for the cinema domain.
- Header: "AI Movie Assistant — Hi, how can I help?"
- Suggested prompts chips: "Recommend a family movie", "Top rated this week", "Compare two movies", etc.
- Input bar at bottom.

**Behavior:** see §12.

---

## 7. User Journey

Below is the canonical happy-path journey, with branch points for the AI assistant.

```
[App Launch]
     │
     ▼
[Home Screen] ──search/filter──► [Filtered Home]
     │ tap card
     ▼
[Movie Details] ──AI FAB──► [AI Assistant Overlay]
     │ Book Tickets
     ▼
[Theater Selection] ──pick date + showtime──►
     │ tap showtime
     ▼
[Seat Selection] ──pick seats (≥1)──►
     │ Proceed
     ▼
[Booking Summary] ──optional: edit seats / apply coupon──►
     │ Proceed to Pay
     ▼
[Payment] ──method + form + tap Pay──►
     │ 2.5s processing
     ��
[Booking Confirmation] ──Download / Share / Back to Home──►
     │
     ▼
[Home Screen]
```

### Branch: AI Assistant
Accessible from Home, Movie Details, Theater, Seat, Summary, and Payment screens. AI responses can:
- Recommend a movie → offers "View Details" deep link.
- Compare two movies → opens comparison popup.
- Show ratings → lists catalog movies by rating.
- Booking help → guides user back into the booking flow.

### Branch: My Bookings
Accessible from Home's app bar. Lists past + upcoming; tapping any opens a read-only ticket view.

---

## 8. Navigation Flow

Use `Shell` for top-level navigation. Bookings, My Bookings, and Home are tab-level destinations. Detail pages are pushed onto the navigation stack.

### Shell Routes

| Route | Type | Purpose |
|---|---|---|
| `//home` | Tab | Home (root) |
| `//bookings` | Tab | My Bookings |
| `movieDetails` | Push | Movie Details (`?id=...`) |
| `theaterSelection` | Push | Theater Selection (`?movieId=...`) |
| `seatSelection` | Push | Seat Selection (`?showtimeId=...`) |
| `bookingSummary` | Push | Booking Summary |
| `payment` | Push | Payment |
| `bookingConfirmation` | Push | Confirmation (`?bookingId=...`) |
| `aiAssistant` | Modal | AI Assistant overlay |

### Navigation Diagram

```
Shell
├── Tab: Home       → HomePage
├── Tab: My Bookings → MyBookingsPage
└── Push stack
    ├── MovieDetailsPage
    ├── TheaterSelectionPage
    ├── SeatSelectionPage
    ├── BookingSummaryPage
    ├── PaymentPage
    └── BookingConfirmationPage

[Modal overlay]
└── AIAssistantSheet (SfBottomSheet with SfAIAssistView)
```

### State Management Across Navigation

- `BookingSession` (singleton service) holds the in-progress booking.
- Each step reads from `BookingSession` and writes the next decision.
- A successful payment moves the booking from `BookingSession` to `BookingRepository`.

---

## 9. Data Models

All models live under `/Models`. They use `CommunityToolkit.Mvvm.ComponentModel.ObservableObject` for `INotifyPropertyChanged` and `[JsonPropertyName]` for deserialization.

### 9.1 Movie

```csharp
public partial class Movie : ObservableObject
{
    [JsonPropertyName("id")]              public string Id { get; set; }
    [JsonPropertyName("title")]           public string Title { get; set; }
    [JsonPropertyName("tagline")]         public string Tagline { get; set; }
    [JsonPropertyName("synopsis")]        public string Synopsis { get; set; }
    [JsonPropertyName("posterUrl")]       public string PosterUrl { get; set; }
    [JsonPropertyName("bannerUrl")]       public string BannerUrl { get; set; }
    [JsonPropertyName("genre")]           public string Genre { get; set; }
    [JsonPropertyName("genres")]          public List<string> Genres { get; set; }
    [JsonPropertyName("languages")]       public List<string> Languages { get; set; }
    [JsonPropertyName("durationMinutes")] public int DurationMinutes { get; set; }
    [JsonPropertyName("rating")]          public double Rating { get; set; }
    [JsonPropertyName("releaseDate")]     public string ReleaseDate { get; set; }
    [JsonPropertyName("format")]          public string Format { get; set; }   // 2D, 3D, IMAX
    [JsonPropertyName("cast")]            public List<CastMember> Cast { get; set; }
    [JsonPropertyName("crew")]            public List<CrewMember> Crew { get; set; }
    [JsonPropertyName("isNowShowing")]    public bool IsNowShowing { get; set; }
    [JsonPropertyName("trailerUrl")]      public string TrailerUrl { get; set; }
}

public class CastMember
{
    [JsonPropertyName("name")]      public string Name { get; set; }
    [JsonPropertyName("character")] public string Character { get; set; }
    [JsonPropertyName("avatarUrl")] public string AvatarUrl { get; set; }
}
```

### 9.2 Theater

```csharp
public partial class Theater : ObservableObject
{
    [JsonPropertyName("id")]            public string Id { get; set; }
    [JsonPropertyName("name")]          public string Name { get; set; }
    [JsonPropertyName("location")]      public string Location { get; set; }
    [JsonPropertyName("city")]          public string City { get; set; }
    [JsonPropertyName("distanceKm")]    public double DistanceKm { get; set; }
    [JsonPropertyName("amenities")]     public List<string> Amenities { get; set; }
    [JsonPropertyName("screens")]       public List<Screen> Screens { get; set; }
}

public class Screen
{
    [JsonPropertyName("id")]            public string Id { get; set; }
    [JsonPropertyName("name")]          public string Name { get; set; }   // "Screen 1", "IMAX"
    [JsonPropertyName("totalSeats")]    public int TotalSeats { get; set; }
    [JsonPropertyName("rows")]          public int Rows { get; set; }
    [JsonPropertyName("columns")]       public int Columns { get; set; }
    [JsonPropertyName("seatLayout")]    public List<SeatLayoutCell> Layout { get; set; }
    [JsonPropertyName("tiers")]         public List<SeatTier> Tiers { get; set; }
}

public class SeatLayoutCell
{
    [JsonPropertyName("row")]      public string Row { get; set; }
    [JsonPropertyName("number")]   public int Number { get; set; }
    [JsonPropertyName("tier")]     public string Tier { get; set; }   // "Premium" | "Executive" | "Normal"
    [JsonPropertyName("aisle")]     public bool Aisle { get; set; }
    [JsonPropertyName("occupied")]  public bool Occupied { get; set; }
}

public class SeatTier
{
    [JsonPropertyName("name")]      public string Name { get; set; }
    [JsonPropertyName("price")]     public decimal Price { get; set; }
    [JsonPropertyName("color")]     public string Color { get; set; }
}
```

### 9.3 Showtime

```csharp
public partial class Showtime : ObservableObject
{
    [JsonPropertyName("id")]              public string Id { get; set; }
    [JsonPropertyName("movieId")]         public string MovieId { get; set; }
    [JsonPropertyName("theaterId")]       public string TheaterId { get; set; }
    [JsonPropertyName("screenId")]        public string ScreenId { get; set; }
    [JsonPropertyName("date")]            public string Date { get; set; }        // ISO yyyy-MM-dd
    [JsonPropertyName("startTime")]       public string StartTime { get; set; }   // HH:mm
    [JsonPropertyName("endTime")]         public string EndTime { get; set; }
    [JsonPropertyName("availableSeats")]  public int AvailableSeats { get; set; }
    [JsonPropertyName("priceFrom")]       public decimal PriceFrom { get; set; }
    [JsonPropertyName("format")]          public string Format { get; set; }
}
```

### 9.4 BookingDraft (in-memory, not serialized)

```csharp
public partial class BookingDraft : ObservableObject
{
    public Movie Movie { get; set; }
    public Theater Theater { get; set; }
    public Screen Screen { get; set; }
    public Showtime Showtime { get; set; }
    public DateTime SelectedDate { get; set; }
    public ObservableCollection<Seat> SelectedSeats { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal ConvenienceFee { get; set; } = 30m;
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string AppliedCoupon { get; set; }
    public decimal Discount { get; set; }
}
```

### 9.5 Booking (persisted in `BookingRepository`)

```csharp
public partial class Booking : ObservableObject
{
    [JsonPropertyName("bookingId")]      public string BookingId { get; set; }   // BK-XXXXXX
    [JsonPropertyName("createdAt")]      public DateTime CreatedAt { get; set; }
    [JsonPropertyName("userId")]         public string UserId { get; set; }
    [JsonPropertyName("movieId")]        public string MovieId { get; set; }
    [JsonPropertyName("theaterId")]      public string TheaterId { get; set; }
    [JsonPropertyName("showtimeId")]     public string ShowtimeId { get; set; }
    [JsonPropertyName("seats")]          public List<string> Seats { get; set; }
    [JsonPropertyName("totalAmount")]    public decimal TotalAmount { get; set; }
    [JsonPropertyName("paymentMethod")]  public string PaymentMethod { get; set; }
    [JsonPropertyName("status")]         public string Status { get; set; }   // Confirmed | Cancelled | Completed
    [JsonPropertyName("qrPayload")]      public string QrPayload { get; set; }
}
```

### 9.6 UserProfile

```csharp
public partial class UserProfile : ObservableObject
{
    [JsonPropertyName("id")]          public string Id { get; set; }
    [JsonPropertyName("name")]        public string Name { get; set; }
    [JsonPropertyName("email")]       public string Email { get; set; }
    [JsonPropertyName("phone")]       public string Phone { get; set; }
    [JsonPropertyName("city")]        public string City { get; set; }
    [JsonPropertyName("avatarUrl")]   public string AvatarUrl { get; set; }
}
```

### 9.7 AIChatMessage (used by `AIAssistView`)

The Syncfusion `AssistMessage` class is reused. A custom subclass can attach movie recommendation cards:

```csharp
public class MovieAssistMessage : AssistMessage
{
    public string MovieId { get; set; }
    public string MovieTitle { get; set; }
    public string PosterUrl { get; set; }
    public double Rating { get; set; }
}
```

---

## 10. MVVM Architecture

The sample strictly follows **MVVM** with `CommunityToolkit.Mvvm`.

### 10.1 Project Structure

```
SmartMovieBooking/
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── MauiProgram.cs
├── SmartMovieBooking.csproj
│
├── Models/                     # POCOs + ObservableObjects
│   ├── Movie.cs
│   ├── CastMember.cs
│   ├── Theater.cs
│   ├── Screen.cs
│   ├── SeatLayoutCell.cs
│   ├── SeatTier.cs
│   ├── Showtime.cs
│   ├── Booking.cs
│   ├── BookingDraft.cs
│   ├── UserProfile.cs
│   └── MovieAssistMessage.cs
│
├── Services/                   # Business + Data layer
│   ├── IMovieService.cs
│   ├── MovieService.cs
│   ├── ITheaterService.cs
│   ├── TheaterService.cs
│   ├── IShowtimeService.cs
│   ├── ShowtimeService.cs
│   ├── IBookingService.cs
│   ├── BookingService.cs
│   ├── IPaymentService.cs
│   ├── PaymentService.cs
│   ├── IAiAssistantService.cs
│   ├── AiAssistantService.cs
│   ├── IBookingRepository.cs
│   ├── BookingRepository.cs
│   ├── BookingSession.cs       # Singleton
│   └── NavigationService.cs
│
├── ViewModels/
│   ├── HomeViewModel.cs
│   ├── MovieDetailsViewModel.cs
│   ├── TheaterSelectionViewModel.cs
│   ├── SeatSelectionViewModel.cs
│   ├── BookingSummaryViewModel.cs
│   ├── PaymentViewModel.cs
│   ├── BookingConfirmationViewModel.cs
│   ├── MyBookingsViewModel.cs
│   ├── AiAssistantViewModel.cs
│   └── BaseViewModel.cs
│
├── Views/
│   ├── HomePage.xaml(.cs)
│   ├── MovieDetailsPage.xaml(.cs)
│   ├── TheaterSelectionPage.xaml(.cs)
│   ├── SeatSelectionPage.xaml(.cs)
│   ├── BookingSummaryPage.xaml(.cs)
│   ├── PaymentPage.xaml(.cs)
│   ├── BookingConfirmationPage.xaml(.cs)
│   ├── MyBookingsPage.xaml(.cs)
│   └── AiAssistantSheet.xaml(.cs)
│
├── Controls/                   # Custom controls / templates
│   ├── MovieCardView.cs
│   ├── TheaterCardView.cs
│   ├── SeatControl.cs
│   ├── ShowtimeChip.cs
│   ├── RatingBadge.cs
│   └── QrCodeView.cs
│
├── Converters/
│   ├── BoolToColorConverter.cs
│   ├── SeatStateToColorConverter.cs
│   ├── InverseBoolConverter.cs
│   ├── CurrencyConverter.cs
│   ├── DurationConverter.cs
│   ├── FirstValidationErrorConverter.cs
│   ├── StringToColorConverter.cs
│   └── RatingToStarsConverter.cs
│
├── Behaviors/
│   ├── DebounceSearchBehavior.cs
│   ├── NumericInputBehavior.cs
│   └── TapAnimationBehavior.cs
│
├── Resources/
│   ├── AppIcon/
│   ├── Fonts/
│   ├── Images/
│   ├── Raw/
│   │   ├── movies.json
│   │   ├── theaters.json
│   │   ├── showtimes.json
│   │   ├── cast.json
│   │   └── userprofile.json
│   ├── Splash/
│   └── Styles/
│       ├── Colors.xaml
│       └── Styles.xaml
│
└── Platforms/
    ├── Android/
    ├── iOS/
    ├── MacCatalyst/
    └── Windows/
```

### 10.2 Layer Responsibilities

| Layer | Responsibility | Allowed Dependencies |
|---|---|---|
| **Models** | Plain data + observable properties. No behavior beyond change notification. | none |
| **Services** | Data access, business rules, mock APIs, AI logic. Stateless. | Models, JSON |
| **ViewModels** | UI state, commands, validation, orchestration. | Services, Models, CommunityToolkit.Mvvm |
| **Views** | XAML + minimal code-behind for navigation and binding. | ViewModels, Controls, Converters, Behaviors |
| **Converters/Behaviors/Controls** | Reusable UI primitives. | Views, Models |

### 10.3 ViewModel Pattern

All ViewModels inherit `BaseViewModel` (which itself inherits `ObservableObject`) and use `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm.

```csharp
public partial class HomeViewModel : BaseViewModel
{
    private readonly IMovieService _movieService;
    private readonly IAiAssistantService _aiService;

    [ObservableProperty] private string searchText;
    [ObservableProperty] private string selectedGenre;
    [ObservableProperty] private string selectedLanguage;
    [ObservableProperty] private bool isLoading;

    public ObservableCollection<Movie> Movies { get; } = new();
    public ObservableCollection<Movie> FilteredMovies { get; } = new();
    public ObservableCollection<string> Genres { get; } = new();
    public ObservableCollection<string> Languages { get; } = new();

    public HomeViewModel(IMovieService movieService, IAiAssistantService aiService)
    {
        _movieService = movieService;
        _aiService = aiService;
    }

    [RelayCommand]
    public async Task LoadAsync() { ... }

    [RelayCommand]
    private async Task MovieSelected(Movie movie) { ... }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedGenreChanged(string value) => ApplyFilters();

    private void ApplyFilters() { ... }
}
```

### 10.4 Dependency Injection

Register all services in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<IMovieService, MovieService>();
builder.Services.AddSingleton<ITheaterService, TheaterService>();
builder.Services.AddSingleton<IShowtimeService, ShowtimeService>();
builder.Services.AddSingleton<IBookingService, BookingService>();
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IAiAssistantService, AiAssistantService>();
builder.Services.AddSingleton<IBookingRepository, BookingRepository>();
builder.Services.AddSingleton<BookingSession>();
builder.Services.AddSingleton<INavigationService, NavigationService>();

builder.Services.AddTransient<HomeViewModel>();
builder.Services.AddTransient<MovieDetailsViewModel>();
builder.Services.AddTransient<TheaterSelectionViewModel>();
builder.Services.AddTransient<SeatSelectionViewModel>();
builder.Services.AddTransient<BookingSummaryViewModel>();
builder.Services.AddTransient<PaymentViewModel>();
builder.Services.AddTransient<BookingConfirmationViewModel>();
builder.Services.AddTransient<MyBookingsViewModel>();
builder.Services.AddTransient<AiAssistantViewModel>();

builder.Services.AddTransient<HomePage>();
builder.Services.AddTransient<MovieDetailsPage>();
// ...
```

Pages receive their ViewModel via constructor injection.

### 10.5 State Management

- **`BookingSession`** is a singleton holding the in-progress `BookingDraft`. It exposes a `Reset()` method and emits `PropertyChanged` on every change.
- **`BookingRepository`** is a singleton in-memory store of all completed bookings. Persists for app session lifetime.
- **No static state on pages.** All view-state is on the ViewModel.
- **Cross-page parameters** (e.g., `movieId`, `showtimeId`) are passed via Shell query parameters and read in `OnAppearing` / `InitializeAsync`.

---

## 11. Syncfusion Control Usage

Below is the canonical mapping of Syncfusion controls to UI elements. All controls are from the **Syncfusion .NET MAUI** package family (v33.2.x).

| Control | Package | Where Used |
|---|---|---|
| `SfCollectionView` | `Syncfusion.Maui.ListView` | Movie carousels, cast row, search results, seat list, payment methods. |
| `SfListView` | `Syncfusion.Maui.ListView` | Coming Soon list, Theater list, My Bookings list. |
| `SfTabView` | `Syncfusion.Maui.TabView` | My Bookings Upcoming/Past tabs. |
| `SfButton` | `Syncfusion.Maui.Buttons` | All primary CTAs (Book, Pay, Confirm). |
| `SfPopup` | `Syncfusion.Maui.Popup` | Trailer "not available," payment failure, downloaded ticket, share confirmations. |
| `SfBottomSheet` | `Syncfusion.Maui.BottomSheet` | AI Assistant overlay, Payment processing sheet. |
| `SfTextInputLayout` | `Syncfusion.Maui.Core` | Card number, UPI VPA, coupon field. |
| `SfShimmer` | `Syncfusion.Maui.Core` | Home, Movie Details, Theater, Seat, My Bookings loaders. |
| `SfSegmentedControl` | `Syncfusion.Maui.Buttons` | Payment method selection, genre filter (optional). |
| `SfAIAssistView` | `Syncfusion.Maui.AIAssistView` | AI Movie Assistant. |
| `SfEffectsView` | `Syncfusion.Maui.Core` | Ripple / scale on cards and chips. |
| `SfGradientView` | `Syncfusion.Maui.Core` | Movie details hero overlay. |
| `SfBadgeView` | `Syncfusion.Maui.Core` | Rating badges, seat counts. |
| `SfRadioButton` | `Syncfusion.Maui.Buttons` | Payment method options, seat tier. |
| `SfComboBox` | `Syncfusion.Maui.ComboBox` | Language filter. |
| `SfDatePicker` | `Syncfusion.Maui.Calendar` (optional) | Date selector. |
| `SfLinearProgress` (or built-in `ActivityIndicator`) | `Syncfusion.Maui.Core` | Payment processing. |
| `SfQRCodeGenerator` / `Syncfusion.Maui.Barcode` | `Syncfusion.Maui.Barcode` | Booking confirmation QR code. |
| `SfCardView` / `Syncfusion.Maui.Cards` | `Syncfusion.Maui.Cards` | Movie cards, theater cards, summary cards. |
| `SfChip` / `Syncfusion.Maui.Chips` | `Syncfusion.Maui.Chips` | Genre, language, showtime, seats. |
| `SfSparkline` (optional) | `Syncfusion.Maui.Charts` | Movie popularity trend. |

### 11.1 Theming

- Use a single `SyncfusionThemeResourceDictionary` in `App.xaml` (MaterialLight or MaterialDark).
- Brand color is layered on top of the Syncfusion theme via custom `Theme*` keys (per existing `EmployeeDirectory` pattern).
- The `ThemeService` toggles dark/light and swaps both MAUI `UserAppTheme` and the `SyncfusionThemeResourceDictionary.VisualTheme`.

### 11.2 Card Pattern (Reference)

Each movie card uses:

```xml
<sf:SfCardView CornerRadius="16" HasShadow="True">
  <Grid RowDefinitions="180,Auto,Auto">
    <Image Source="{Binding BannerUrl}" Aspect="AspectFill" />
    <sf:SfBadgeView Grid.Row="0" BadgeText="{Binding Rating}" Position="TopRight" />
    <VerticalStackLayout Grid.Row="1" Padding="12">
      <Label Text="{Binding Title}" FontSize="16" FontAttributes="Bold" />
      <Label Text="{Binding Genre}" FontSize="12" />
    </VerticalStackLayout>
    <HorizontalStackLayout Grid.Row="2" Padding="12">
      <sf:SfChip Text="{Binding Language}" />
      <sf:SfChip Text="{Binding Duration}" />
    </HorizontalStackLayout>
  </Grid>
</sf:SfCardView>
```

### 11.3 Seat Control (Reference)

A `SeatControl` is a custom view with bindable properties:

- `SeatId` (e.g., "A5")
- `Tier` (Normal / Executive / Premium)
- `State` (Available / Occupied / Selected)
- `Command` (RelayCommand raised with the seat's id on tap)

Rendered as a `Border` or `sfEffectsView.SfEffectsView` wrapping a `Label`. The state-to-color mapping uses `SeatStateToColorConverter`.

---

## 12. AI Assistant Integration

The AI Movie Assistant is implemented with Syncfusion's **`SfAIAssistView`** and a domain-aware **response engine** that does not require any external LLM — it works entirely against the in-memory movie catalog.

### 12.1 Component Layout

```xml
<sf:SfBottomSheet x:Name="AssistantSheet" IsOpen="{Binding IsAssistantOpen}">
  <sf:SfAIAssistView
      x:Name="AssistantView"
      AssistItems="{Binding Messages}"
      ShowHeader="True"
      HeaderTitle="AI Movie Assistant"
      HeaderSubtitle="Powered by Syncfusion"
      SuggestionTemplate="..."
      RequestTemplate="..."
      ResponseTemplate="..." />
</sf:SfBottomSheet>
```

### 12.2 Intent Engine

`AiAssistantService.ProcessAsync(string userInput)` returns a list of `AssistMessage` objects. Internally it:

1. **Normalizes** the input (lowercase, trim, remove punctuation).
2. **Classifies intent** via lightweight keyword scoring:
   - `recommend`, `suggest`, `looking for` → Recommendation
   - `compare`, `vs`, `versus` → Comparison
   - `tell me about`, `what is`, `info` → Movie Info
   - `rating`, `top rated`, `highest` → Ratings
   - `kids`, `family`, `children` → Family filter
   - `book`, `ticket`, `how to book` → Booking help
   - `action`, `comedy`, `drama`, `thriller`, `romance`, `sci-fi`, `family` → Genre filter
3. **Pulls data** from the injected `IMovieService` and `IBookingService`.
4. **Builds responses** with rich text and (for recommend/compare) embedded `MovieAssistMessage` cards.

### 12.3 Suggested Prompts (Chip Row)

On open, show 5–6 suggested chips:

- "Recommend a family movie"
- "Suggest an action movie"
- "Top rated this week"
- "Compare two movies"
- "What movie is suitable for kids?"
- "How do I book tickets?"

Tapping a chip injects that text as a user request and processes it.

### 12.4 Response Templates

- **Text:** plain Markdown-style text rendered via `FormattedText`.
- **Movie Card:** a `MovieAssistMessage` whose template renders a poster + title + rating + "View Details" button.
- **Comparison Table:** a two-column layout of two movie cards with side-by-side stats.
- **List:** bullet list with avatar thumbnails.
- **Booking Help:** numbered steps + a primary button "Start Booking."

### 12.5 Global Availability

A persistent AI FAB on Home and toolbar entries on every primary screen open the same bottom sheet. The same singleton `AiAssistantViewModel` is reused; the assistant maintains conversation history for the session.

### 12.6 Cross-Context Awareness

- On Movie Details: greeting includes "Want me to summarize *<title>* or compare it with similar movies?"
- On Seat Selection: "Need help picking seats? I can recommend the best view."
- On Payment: "Having trouble? I can walk you through it."

This context is provided to the assistant via the page's ViewModel on appearing.

---

## 13. Mock Data Requirements

All data is loaded from local JSON files in `Resources/Raw`. These are loaded once at startup (and cached in services).

### 13.1 JSON Files

| File | Records | Shape |
|---|---|---|
| `movies.json` | ≥ 20 | Array of `Movie` (10 Now Showing, 10 Coming Soon). |
| `theaters.json` | ≥ 8 | Array of `Theater` with 2–4 screens each. |
| `showtimes.json` | ≥ 100 | One per (movie, theater, screen, date) for next 7 days. |
| `cast.json` | per movie | 4–6 `CastMember` entries. |
| `userprofile.json` | 1 | A default profile for the mock user. |

### 13.2 Movie Catalog Targets

- At least 5 genres represented.
- At least 5 languages.
- Mix of durations 90–180 min.
- Ratings 6.0 – 9.5.
- Mix of formats (2D, 3D, IMAX).

### 13.3 Image Strategy

Posters and banners use stable, public image URLs (e.g., `https://image.tmdb.org/t/p/...`) OR are bundled as MAUI assets under `Resources/Images/Posters/`. Bundled images are preferred for offline reliability; URLs are cached by the platform's HTTP cache.

For the sample, **bundled placeholder posters** (`Resources/Images/Posters/movie_01.png` … `movie_20.png`) are used. The `Movie.PosterUrl` field is a `ms-appx` resource path.

### 13.4 Service Contracts

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

public interface ITheaterService
{
    Task<IReadOnlyList<Theater>> GetTheatersForMovieAsync(string movieId);
    Task<Theater> GetByIdAsync(string id);
}

public interface IShowtimeService
{
    Task<IReadOnlyList<Showtime>> GetShowtimesAsync(string movieId, string theaterId, DateTime date);
    Task<Showtime> GetByIdAsync(string id);
}

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(BookingDraft draft, string paymentMethod);
    Task CancelAsync(string bookingId);
}

public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(decimal amount, string method, object details);
}

public interface IBookingRepository
{
    IReadOnlyList<Booking> All { get; }
    Booking GetById(string id);
    void Add(Booking booking);
    event EventHandler BookingsChanged;
}

public interface IAiAssistantService
{
    Task<IReadOnlyList<AssistMessage>> ProcessAsync(string userInput, AiContext context);
}
```

### 13.5 Mock Realism

- `IMovieService.GetNowShowingAsync()` delays 600–900 ms to mimic a network call.
- `IPaymentService.ProcessAsync()` delays ~2.5 seconds and returns failure ~5% of the time (seeded by a per-call random, controllable via a "Force Success" debug switch in DevSettings).
- Seat occupancy is randomized per (showtime, screen) at app start so two app launches give different available seats.

---

## 14. Harness Requirements

The "harness" is the contract an AI agent must satisfy to produce a working, design-faithful implementation.

### 14.1 Functional Requirements (FR)

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

### 14.2 Non-Functional Requirements (NFR)

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

### 14.3 Assumptions

- Users accept that this is a sample, not a production app.
- No real payment processing occurs.
- Movie posters are bundled assets.
- The device has a working network connection only if remote images are used.
- A single mock user profile is sufficient.

### 14.4 Dependencies

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

### 14.5 Screen-Level Implementation Guidelines

Each page must:

- Use `ContentPage` as base.
- Inject its ViewModel via DI.
- Set `BindingContext` in the constructor.
- Override `OnAppearing` only to call `InitializeAsync` on the ViewModel.
- Use Shell query parameters for IDs (e.g., `?movieId=...`).
- Have a corresponding route registered in `AppShell.xaml`.

### 14.6 State Management Requirements

- One source of truth: `BookingSession` for in-progress, `BookingRepository` for completed.
- ViewModels must never directly mutate each other.
- Use `MessagingCenter` or an `EventAggregator` (custom) for cross-screen notifications such as "BookingSessionUpdated."

### 14.7 Service Layer Requirements

- Services are registered in `MauiProgram`.
- Services are stateless and thread-safe.
- Services expose `async` methods returning `Task<T>`.
- JSON deserialization uses `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`.

### 14.8 Error Handling Scenarios

| Scenario | Behavior |
|---|---|
| JSON load failure | Log, show `SfPopup` "Data could not be loaded. Retry?" with retry action. |
| Empty result set | Render empty state with illustration and CTA. |
| Network image 404 | Show placeholder poster (`poster_placeholder.png`). |
| AI Assistant unknown intent | Respond with "I'm not sure I understood. Try one of the suggestions." |
| Payment failure | Show `SfPopup` with Retry / Change Method buttons. |
| Booking session invalid (e.g., navigated back from confirmation) | Reset to Home and clear stack. |

### 14.9 Validation Requirements

- Card number: 12–19 digits (mock — accept anything in that range).
- UPI VPA: must contain `@`.
- CVV: 3–4 digits.
- Expiry: MM/YY, future date.
- Coupon: only `WELCOME10` is valid; case-insensitive.
- Seat count: 1–10 per booking.
- Phone (if collected): 10 digits.

### 14.10 Acceptance Criteria Summary

See §16.

---

## 15. Development Tasks

Each task is granular enough to be completed in 0.5–2 days and produces verifiable deliverables.

### Phase 1 — Foundation

#### T1.1 Project Bootstrap
- **Description:** Create new .NET MAUI project `SmartMovieBooking`, add Syncfusion NuGet packages, configure `MauiProgram`, register Syncfusion theme, verify build on all target frameworks.
- **Dependencies:** none.
- **Deliverables:** Compilable project with all Syncfusion packages and a "Hello" home page.
- **Acceptance:** `dotnet build` succeeds; app launches to "Hello" page.

#### T1.2 Theme Service & Resource Dictionaries
- **Description:** Implement `ThemeService` (light/dark toggle) following the `EmployeeDirectory` pattern. Add `Colors.xaml`, `Styles.xaml`, and `Theme*` keys.
- **Dependencies:** T1.1.
- **Deliverables:** `Services/ThemeService.cs`, `Resources/Styles/Colors.xaml`, `Resources/Styles/Styles.xaml`.
- **Acceptance:** Toggling theme updates MAUI `UserAppTheme` and Syncfusion `VisualTheme`; all colors update at runtime.

#### T1.3 Project Structure & DI Wiring
- **Description:** Create folder skeleton (Models, Services, ViewModels, Views, Controls, Converters, Behaviors). Register all services and ViewModels in `MauiProgram.cs`.
- **Dependencies:** T1.1.
- **Deliverables:** Empty folders, registration block, `BaseViewModel`.
- **Acceptance:** `MauiProgram.CreateMauiApp()` resolves without errors.

#### T1.4 Mock Data Authoring
- **Description:** Create `movies.json`, `theaters.json`, `showtimes.json`, `cast.json`, `userprofile.json` per §13.
- **Dependencies:** T1.3.
- **Deliverables:** JSON files committed under `Resources/Raw/`.
- **Acceptance:** Each file deserializes into the model with no errors.

#### T1.5 Models & JSON Loading
- **Description:** Implement all models from §9 and the JSON loader in services.
- **Dependencies:** T1.4.
- **Deliverables:** All model files, `MovieService.GetNowShowingAsync()` returns ≥ 10 movies.
- **Acceptance:** Unit-testable (or sample load) returns expected counts.

### Phase 2 — Core UI Development

#### T2.1 App Shell & Navigation
- **Description:** Configure `AppShell.xaml` with tabs (Home, My Bookings) and routes for push pages. Implement `NavigationService` to support query params.
- **Dependencies:** T1.3.
- **Deliverables:** `AppShell.xaml(.cs)`, `Services/NavigationService.cs`.
- **Acceptance:** App launches to Home tab; routes resolve.

#### T2.2 Reusable UI Library
- **Description:** Build `MovieCardView`, `TheaterCardView`, `ShowtimeChip`, `RatingBadge`, `QrCodeView`, and all converters/behaviors.
- **Dependencies:** T1.2.
- **Deliverables:** Files under `Controls/`, `Converters/`, `Behaviors/`.
- **Acceptance:** All controls render with sample data in a test page.

#### T2.3 Home Screen
- **Description:** Implement `HomePage.xaml` and `HomeViewModel` per §6.1.
- **Dependencies:** T2.1, T2.2, T1.5.
- **Deliverables:** Working home with carousels, search, filters, shimmer loading.
- **Acceptance:** All cards render; search and filter update list; tap navigates to details.

#### T2.4 Movie Details Screen
- **Description:** Implement per §6.2.
- **Dependencies:** T2.3, T2.2.
- **Deliverables:** `MovieDetailsPage.xaml(.cs)`, `MovieDetailsViewModel.cs`.
- **Acceptance:** Hero, cast row, languages, and CTAs all work; trailer shows popup; "Book Tickets" navigates to theater.

### Phase 3 — Booking Workflow

#### T3.1 Theater Selection Screen
- **Description:** Implement per §6.3.
- **Dependencies:** T1.5, T2.2.
- **Deliverables:** `TheaterSelectionPage`, `TheaterSelectionViewModel`.
- **Acceptance:** Date changes update showtimes; tap showtime navigates to seat.

#### T3.2 Seat Selection Screen
- **Description:** Implement per §6.4, including `SeatControl` and `SeatStateToColorConverter`.
- **Dependencies:** T3.1, T2.2.
- **Deliverables:** `SeatSelectionPage`, `SeatSelectionViewModel`, `SeatControl`.
- **Acceptance:** Seats render in correct state; selection updates pricing; proceed disabled at 0 seats.

#### T3.3 Booking Summary Screen
- **Description:** Implement per §6.5 with coupon logic.
- **Dependencies:** T3.2, T2.2.
- **Deliverables:** `BookingSummaryPage`, `BookingSummaryViewModel`.
- **Acceptance:** WELCOME10 works; total recomputes; proceed navigates to payment.

#### T3.4 Payment Screen
- **Description:** Implement per §6.6 with method selection, forms, processing sheet.
- **Dependencies:** T3.3.
- **Deliverables:** `PaymentPage`, `PaymentViewModel`, `PaymentService`.
- **Acceptance:** Processing animates; success routes to confirmation; failure shows popup with retry.

#### T3.5 Booking Confirmation Screen
- **Description:** Implement per §6.7 with QR code, download/share/back actions.
- **Dependencies:** T3.4.
- **Deliverables:** `BookingConfirmationPage`, `BookingConfirmationViewModel`, `QrCodeView`, `BookingRepository.Add(...)`.
- **Acceptance:** Confirmation displays booking; download popup works; share uses system share sheet; back resets session.

#### T3.6 My Bookings Screen
- **Description:** Implement per §6.8 with Upcoming/Past tabs and empty state.
- **Dependencies:** T3.5, T2.2.
- **Deliverables:** `MyBookingsPage`, `MyBookingsViewModel`.
- **Acceptance:** A completed booking appears in Upcoming; after showtime, in Past.

### Phase 4 — AI Movie Assistant

#### T4.1 Intent Engine
- **Description:** Implement `AiAssistantService.ProcessAsync` with intent classification, keyword scoring, and data lookups.
- **Dependencies:** T1.5.
- **Deliverables:** `AiAssistantService.cs`, `AiContext.cs`, message templates.
- **Acceptance:** All 6 example prompts return non-empty, on-topic results.

#### T4.2 AI AssistView UI
- **Description:** Implement `AiAssistantSheet` and `AiAssistantViewModel`. Wire suggested prompts, input bar, history.
- **Dependencies:** T4.1, T2.2.
- **Deliverables:** `Views/AiAssistantSheet.xaml(.cs)`, `ViewModels/AiAssistantViewModel.cs`.
- **Acceptance:** Sheet opens from Home; suggestions work; messages render with correct templates.

#### T4.3 Global AI Entry Points
- **Description:** Add AI FAB on Home and toolbar entries on every primary page. Wire to `BookingSession`-aware context.
- **Dependencies:** T4.2, all primary pages.
- **Deliverables:** Updated `*Page.xaml` files.
- **Acceptance:** AI sheet opens from every primary screen with appropriate context greeting.

### Phase 5 — Business Logic

#### T5.1 Booking Session & Repository
- **Description:** Implement `BookingSession` (singleton) and `IBookingRepository` with add/list events.
- **Dependencies:** T1.3.
- **Deliverables:** `BookingSession.cs`, `BookingRepository.cs`.
- **Acceptance:** Session clears on confirm; repository raises event on add.

#### T5.2 Pricing & Coupon Logic
- **Description:** Centralize seat-tier pricing, fee, tax, and coupon math in `PricingService`.
- **Dependencies:** T3.2, T3.3.
- **Deliverables:** `Services/PricingService.cs`.
- **Acceptance:** All screens agree on totals; coupon is applied only once.

#### T5.3 Validation Service
- **Description:** Centralize input validation (card, UPI, CVV, expiry).
- **Dependencies:** T3.4.
- **Deliverables:** `Services/ValidationService.cs`.
- **Acceptance:** Invalid inputs surface inline error messages and disable Pay.

### Phase 6 — Data Integration

#### T6.1 Image Asset Bundling
- **Description:** Bundle 20+ placeholder posters, cast avatars, and theater logos as MAUI assets.
- **Dependencies:** T1.4.
- **Deliverables:** `Resources/Images/Posters/*`, `Resources/Images/Cast/*`, `Resources/Images/Theaters/*`.
- **Acceptance:** All images load in app.

#### T6.2 Localized Strings
- **Description:** Move all user-facing text to `Resources/Strings/Strings.resx` (or simple constants file) for i18n-readiness.
- **Dependencies:** T3.5.
- **Deliverables:** Strings resource.
- **Acceptance:** No literal user-facing strings in XAML code-behind.

#### T6.3 Mock Realism Layer
- **Description:** Add artificial delays, randomized seat occupancy, and 5% payment failure to make the app feel real.
- **Dependencies:** T1.5, T3.4.
- **Deliverables:** `Services/MockLatency.cs` (or inline delays), seeded RNG.
- **Acceptance:** Loading states visible; seats differ between launches; payment failure reproducible.

### Phase 7 — Testing & Validation

#### T7.1 Manual Test Pass
- **Description:** Walk through every screen on every target framework; file bugs.
- **Dependencies:** T1–T6.
- **Deliverables:** Test report.
- **Acceptance:** All FRs pass.

#### T7.2 Theming Audit
- **Description:** Verify all screens in light and dark themes.
- **Dependencies:** T7.1.
- **Deliverables:** Theme audit checklist.
- **Acceptance:** No contrast or color regressions.

#### T7.3 Performance Pass
- **Description:** Verify 60 fps on carousels, seat grid, AI sheet.
- **Dependencies:** T7.1.
- **Deliverables:** Performance notes.
- **Acceptance:** No visible jank.

### Phase 8 — Final Sample Polish

#### T8.1 Empty States & Illustrations
- **Description:** Add illustrations to all empty states.
- **Dependencies:** T7.1.
- **Deliverables:** SVG / PNG illustrations.
- **Acceptance:** Every list has a designed empty state.

#### T8.2 Animations & Transitions
- **Description:** Add page-transition animations, success animation, processing animation.
- **Dependencies:** T3.5, T3.6.
- **Deliverables:** XAML animations / Lottie files.
- **Acceptance:** Smooth, no jank, theme-aware.

#### T8.3 README & Sample Documentation
- **Description:** Write a sample README that explains the app, architecture, AI assistant, and how to run.
- **Dependencies:** T7.1.
- **Deliverables:** `README.md` at project root.
- **Acceptance:** A new developer can run the app following only the README.

#### T8.4 Final Build & Smoke Test
- **Description:** Clean, rebuild, and smoke-test all four target frameworks.
- **Dependencies:** T8.1–T8.3.
- **Deliverables:** Build report.
- **Acceptance:** All targets build and launch.

---

## 16. Acceptance Criteria

### 16.1 Build & Run

- AC-B1: `dotnet build -f net10.0-android` succeeds.
- AC-B2: `dotnet build -f net10.0-ios` succeeds (on macOS).
- AC-B3: `dotnet build -f net10.0-maccatalyst` succeeds (on macOS).
- AC-B4: `dotnet build -f net10.0-windows10.0.19041.0` succeeds (on Windows).
- AC-B5: App launches to Home within 2 s on a clean build.

### 16.2 Functional

- AC-F1: A user can complete a full booking in under 60 seconds.
- AC-F2: All 9 features from §5 are accessible and functional.
- AC-F3: AI Assistant answers all 6 example prompts.
- AC-F4: A completed booking appears in My Bookings → Upcoming.
- AC-F5: Search and filters update the home list reactively.
- AC-F6: Coupon `WELCOME10` produces a 10% discount; others show invalid.

### 16.3 Visual / UX

- AC-V1: All screens follow the Stitch design language (card-based, generous spacing, brand color).
- AC-V2: Light and dark themes are supported and consistent.
- AC-V3: All async operations show a shimmer or spinner.
- AC-V4: Page transitions are smooth and themed.

### 16.4 Technical

- AC-T1: MVVM strictly enforced — no business logic in code-behind.
- AC-T2: All services are DI-registered and stateless.
- AC-T3: `Nullable` enabled, no build warnings.
- AC-T4: All JSON models round-trip through deserialization.
- AC-T5: All errors are caught and surfaced via `SfPopup` or inline message.

### 16.5 Content

- AC-C1: ≥ 20 movies (10 Now Showing, 10 Coming Soon).
- AC-C2: ≥ 8 theaters across ≥ 2 cities.
- AC-C3: ≥ 100 showtimes across 7 days.
- AC-C4: All movies have ≥ 4 cast members and a synopsis.

---

## 17. User Stories

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

## 18. Milestones

| Milestone | Tasks | Definition of Done |
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

*End of Specification. Ready for AI-assisted implementation.*
