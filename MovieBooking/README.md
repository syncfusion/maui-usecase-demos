# Movie Booking: .NET MAUI Use Case Sample

![Movie Booking Demo](TicketBookingDemo.gif)

A cross-platform movie-discovery and ticket-booking application built with [Syncfusion .NET MAUI controls](https://help.syncfusion.com/maui/overview), demonstrating movie browsing, theater selection, seat selection, booking confirmation, and a local movie assistant.

---

## Overview

Movie Booking demonstrates a complete local booking flow using fictional movies and theaters. Users browse the catalog, select a theater/date/showtime, choose seats, review a simulated payment confirmation, and view the generated booking.

The app does not call a payment provider or AI service. The movie assistant is a deterministic suggestion service backed by the local movie catalog.

---

## Key Features

### Movie Discovery

- Browse featured, popular, and coming-soon movies.
- View movie catalog entries from `AppSeed`.
- Open the movie assistant for local movie suggestions.

### Theater and Showtime Selection

- Select a theater and date.
- Generate showtimes locally through `ShowtimeService`.
- Continue to seat selection from a selected showtime.

### Seat Selection

- Render a sample seat layout through `SeatLayoutFactory`.
- Select only available seats.
- Calculate subtotal, convenience fee, and total in `BookingSession`.

### Booking Confirmation

- Simulate processing through a short delay.
- Generate a booking ID.
- Store the booking in `BookingRepository`.
- Display the booking barcode with `SfBarcodeGenerator`.
- Reset the current selection session after confirmation.

### In-App Bookings

- Display confirmed bookings for the current app process.
- View booking details and generated barcodes.
- Store no bookings in an external database or account.

---

## Syncfusion Controls Used

| Control | Purpose |
|---|---|
| `SfRotator` | Featured-movie carousel |
| `SfListView` | Movie, theater, showtime, booking, and assistant collections |
| `SfCardView` | Movie, booking, and assistant cards |
| `SfPopup` | Seat and confirmation overlays |
| `SfBarcodeGenerator` | Booking barcodes |
| `SfButton` | Booking and navigation actions |

### Additional Technologies

- CommunityToolkit.Mvvm
- .NET MAUI Shell
- Dependency Injection
- MVVM architecture pattern
- Local deterministic assistant suggestions
- In-memory booking repository

---

## Supported Platforms

| Platform | Target Framework |
|---|---|
| Android | `net10.0-android` |
| iOS | `net10.0-ios` |
| macOS (Mac Catalyst) | `net10.0-maccatalyst` |
| Windows | `net10.0-windows10.0.19041.0` |

> iOS and Mac Catalyst targets require a compatible non-Linux host. The Windows target is available on Windows hosts.

---

## Prerequisites

- .NET 10 SDK or later
- .NET MAUI workloads
- Visual Studio 2022 (17.14+), Visual Studio Code with the .NET MAUI extension, or JetBrains Rider
- A platform emulator, simulator, or physical device
- Syncfusion .NET MAUI NuGet packages restored from NuGet

---

## Getting Started

### Open the Solution

```text
MovieBooking/MovieBooking/MovieBooking.slnx
```

### Restore Packages

```bash
dotnet restore MovieBooking/MovieBooking/MovieBooking/MovieBooking.csproj
```

### Build and Run

Example for Android:

```bash
dotnet build MovieBooking/MovieBooking/MovieBooking/MovieBooking.csproj -f net10.0-android
```

---

## Architecture

### Core Layers

- **Views** (`Views/`) - movie, theater, seat, confirmation, bookings, and assistant surfaces.
- **ViewModels** (`ViewModels/`) - catalog, theater, seat, confirmation, booking, and assistant state.
- **Models** (`Models/`) - `Movie`, `Theater`, `Showtime`, `SelectableDate`, `Seat`, `CastMember`, and `Booking`.
- **Services** (`Services/`) - catalog and theater/showtime services, seat layout, booking session/repository, and assistant suggestions.
- **Seed Data** - `AppSeed.cs` supplies fictional catalog data.

### Runtime Flow

1. `MauiProgram.CreateMauiApp()` registers movie, theater, showtime, seat, booking, and assistant services.
2. `AppSeed` provides local movie and theater data.
3. `BookingSession` retains the selected movie, theater, date, showtime, and seats.
4. `SeatLayoutFactory` builds the sample seat layout.
5. Confirmation creates a booking record and adds it to `BookingRepository`.
6. `BookingRepository.All` exposes confirmed bookings through a read-only collection.
7. `AiCopilotService.SuggestAsync()` returns the highest-rated local movies; it does not call an AI provider.

---

## Project Structure

```text
MovieBooking/
├── MovieBooking/
│   ├── MovieBooking.slnx
│   └── MovieBooking/
│       ├── Views/
│       ├── ViewModels/
│       ├── Models/
│       ├── Services/
│       ├── Behaviors/
│       ├── Converters/
│       ├── Resources/
│       └── Platforms/
└── TicketBookingDemo.gif
```

---

## Configuration

Set `SYNCFUSION_LICENSE_KEY` to register a Syncfusion license key when an environment value is available.

- No payment account or payment API is used.
- No AI provider or network credential is used.
- The movie, theater, showtime, seat, and booking state is local.

---

## Syncfusion Licensing

Set `SYNCFUSION_LICENSE_KEY` before starting the app or register a valid license through the app startup configuration.

Do not commit license keys to source control. For details, refer to [Syncfusion Licensing Documentation](https://help.syncfusion.com/common/licensing/how-to-register-maui-license-key).

---

## Limitations

- Confirmed bookings exist only for the current app process.
- Payment confirmation is simulated and takes no payment.
- Showtimes and seat layouts are generated sample data.
- The movie assistant only suggests the highest-rated local movies.
- The app does not integrate with a ticketing or theater system.

---

## Troubleshooting

### Bookings Disappear After Restart

Bookings are held in `BookingRepository`, an in-memory store.

### No Seats Can Be Selected

Only seats marked available by `SeatLayoutFactory` can be selected.

### A Payment or Card Flow Is Required

No real payment flow exists. The confirmation page is a local demonstration.

### Syncfusion Evaluation Warning

Set `SYNCFUSION_LICENSE_KEY` or register a valid license in application startup.

---

## Related Documentation

- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [.NET MAUI Shell](https://learn.microsoft.com/dotnet/maui/fundamentals/shell/)
- [.NET MAUI MVVM Guidance](https://learn.microsoft.com/dotnet/architecture/maui/mvvm)
- [Syncfusion .NET MAUI Controls](https://help.syncfusion.com/maui/overview)
- [Syncfusion Licensing Documentation](https://help.syncfusion.com/common/licensing/how-to-register-maui-license-key)

---

## License

This sample is provided for demonstration and learning purposes. Syncfusion controls used in this sample require a valid Syncfusion license for production use.
