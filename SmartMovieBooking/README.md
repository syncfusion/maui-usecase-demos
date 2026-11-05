# Smart Movie Booking

Cross-platform .NET MAUI movie ticket booking sample with a conversational movie assistant and Syncfusion-powered booking UI.

## Overview

Smart Movie Booking demonstrates a movie discovery and ticket reservation flow for users who want to browse films, pick seats, review payment details, and get AI-assisted movie recommendations and answers inside the app. It uses a local AI copilot service to improve the experience with movie suggestions and conversational guidance without requiring a live cloud AI dependency.

This sample is for teams building entertainment or booking experiences in .NET MAUI. It shows how AI can help users discover movies faster, navigate the booking journey, and complete a reservation with less friction.

## Key Features

- Movie browsing with posters, details, and showtime discovery
- Seat selection with booking summary and confirmation flow
- Conversational movie copilot for recommendations and FAQs
- Ticket/payment confirmation with barcode presentation
- Responsive layouts for phone and desktop form factors

## AI Flow

1. User opens the movie assistant or asks for help
2. The app prepares movie context and user intent
3. The AI service processes the request using local sample logic
4. The app receives the generated recommendation or reply
5. The result is rendered in the chat and booking UI
6. If the assistant has no strong match, it falls back to a helpful default response

## Controls Used

- SfCarousel — displays featured movies in a swipeable experience
- SfCardView — presents movie and booking content in structured cards
- SfButton — handles primary booking and action triggers
- SfPopup — shows booking confirmations and overlays
- SfBarcodeGenerator — renders the ticket barcode for confirmation
- SfAIAssistView — supports the conversational movie assistant experience

## Architecture

- View / Page — XAML pages for home, theaters, bookings, seat selection, payment confirmation, and the AI copilot
- ViewModel — handles navigation, selection state, and booking actions
- AI service — provides the sample movie assistant responses
- Models — represent movies, theaters, seats, bookings, and assistant messages
- Supporting UI components — Syncfusion cards, carousel, popup, buttons, and barcode controls

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload
- Visual Studio 2022 with .NET MAUI support, if using Visual Studio
- Syncfusion MAUI packages
- Syncfusion license key, if your environment requires one

## AI Configuration

This sample uses local AI-style logic in the repository and does not require an external AI endpoint at runtime.

### Default Behavior

- Mock AI
- Local/offline logic
- No cloud AI provider required

### Environment Variables / Secrets

| Variable | Required | Description |
| --- | --- | --- |
| SYNCFUSION_LICENSE_KEY | no | Syncfusion license key for environments that require registration |

**Notes**

- Do not commit keys to source control
- Use environment variables, user secrets, or secure storage
- If configuration is missing, the sample continues with its local behavior

## Run

```bash
dotnet build SmartMovieBooking/SmartMovieBooking/SmartMovieBooking/SmartMovieBooking.csproj -t:Run -f net10.0-android
```

If you target another platform, adjust the `-f` value to match the supported framework on your machine.

## Output

[.NET MAUI TicketBooking Sample Demo](TicketBookingDemo.gif)

## Project Structure

- Views/ — UI pages and layouts
- ViewModels/ — presentation logic
- Models/ — data structures
- Services/ — AI and helper services
- Resources/ — styles, images, and assets

## Troubleshooting

**Issue 1: Missing AI configuration**

Cause: The sample is using local logic and no external AI configuration is available.

Fix: Confirm the app is using the built-in assistant service or replace it with your own AI provider.

**Issue 2: Build or workload errors**

Cause: Missing SDK, workload, or incompatible package version.

Fix: Verify the required .NET version and install the needed MAUI workload.

**Issue 3: AI response is unexpected**

Cause: The sample assistant uses deterministic local responses, so the prompt or context may be limited.

Fix: Review the assistant service and adjust the sample data it uses.

## Security and Privacy Notes

- Do not hard-code API keys
- Avoid sending unnecessary sensitive data to any AI service
- Minimize prompt content to only what is needed
- Prefer secure storage for credentials
- Review any external API calls for data exposure

## Supported Platforms

- Android
- iOS
- Windows
- macOS

## License

Add the repository license or vendor disclaimer here.
