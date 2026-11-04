# Smart Movie Booking

This sample shows a cross-platform .NET MAUI movie ticket booking app built with Syncfusion controls.

## What the sample demonstrates

- Browsing movies, theaters, and showtimes
- Selecting seats with MVVM-bound UI state
- Confirming a booking and showing a payment summary
- Reusable Syncfusion cards, buttons, barcode, and popup controls
- Responsive layouts for phone and desktop form factors

## Prerequisites

- Visual Studio 2022 with the .NET MAUI workload
- The .NET SDK used by your MAUI environment
- Syncfusion MAUI packages

## Configuration

The sample reads a Syncfusion license key from the `SYNCFUSION_LICENSE_KEY` environment variable when one is available. If the variable is not set, the sample still runs with the current project setup.

## Notes

- The booking flow uses local in-memory sample services.
- Seat selection is driven from the view model and bound to the XAML view.
- The confirmation screen uses adaptive sizing so it fits smaller screens better.
