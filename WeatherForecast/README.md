# Weather Forecast: .NET MAUI Use Case Sample

A cross-platform weather dashboard that looks up a city and displays current conditions, hourly details, and a seven-day forecast. It uses Syncfusion controls for charts, city autocomplete, refresh gestures, and responsive presentation.

## Overview

The app searches for a city using Open-Meteo and displays its current weather and forecast:

- Current temperature, feels-like temperature, today's high/low, humidity, and wind.
- Up to eight upcoming hourly forecast entries.
- Daily high/low conditions over a seven-day forecast.
- City search with popular-city suggestions and pull-to-refresh.
- A desktop page on Windows/Mac Catalyst and a mobile page on Android/iOS; mobile separates Today and Next 6 Days into tabs.

## Key Features

- London is loaded the first time the dashboard appears.
- City names are resolved to coordinates using Open-Meteo's geocoding API.
- Current, hourly, and daily weather data are mapped from WMO condition codes to display descriptions and icons.
- Loading state, pull-to-refresh state, invalid-city responses, network errors, and timeouts are surfaced in the UI.
- Platform-specific layouts share the same `WeatherViewModel` and service.

## Data Flow

1. The dashboard calls `WeatherViewModel.InitializeAsync()` on first appearance.
2. `WeatherService` sends the selected city name to Open-Meteo's geocoding endpoint and reads the first result.
3. The service requests current, hourly, and daily forecast data for the returned coordinates, with the API's automatic timezone and seven forecast days.
4. The response is mapped to current conditions, up to eight upcoming hourly entries, and daily forecast entries; WMO codes become readable conditions and icons.
5. The view model updates its bound properties and forecast collections. Search selection or pull-to-refresh starts another request.

The app needs an internet connection and has no offline forecast cache or local weather seed.

## Controls Used

- `SfAutocomplete`: city entry and suggestions.
- `SfCartesianChart`: hourly temperature trend and daily high/low chart.
- `SfListView`: hourly and daily forecast collections.
- `SfTabView`: Today and Next 6 Days tabs on mobile.
- `SfPullToRefresh`: refresh gesture.
- `SfLinearProgressBar`, `SfBadgeView`, and `SfBusyIndicator`: temperature range, hottest-day badge, and loading state.

## Architecture

- **Views** (`WeatherForecast/Views/`): `WeatherDashboardPageMobile` and `WeatherDashboardPageDesktop`.
- **View model** (`WeatherForecast/ViewModels/WeatherViewModel.cs`): selected city, loading/error state, commands, and presentation-ready weather collections.
- **Service** (`WeatherForecast/Services/WeatherService.cs`): typed `HttpClient`, Open-Meteo geocoding and forecast requests, response mapping, and WMO condition descriptions.
- **Models** (`WeatherForecast/Models/`): API response, current result, hourly forecast, and daily forecast data.
- **Converter** (`WeatherForecast/Converters/`): temperature-progress visualization converter.
- **Startup** (`WeatherForecast/MauiProgram.cs`): HTTP client and transient view model/page registration.

## Prerequisites

- .NET 10 SDK and the .NET MAUI workload.
- A platform workload/device for Android, iOS, Mac Catalyst, or Windows.
- An internet connection at runtime.
- Visual Studio with MAUI support or VS Code with the .NET MAUI extension.
- Syncfusion MAUI packages restored from NuGet. A valid Syncfusion license may be needed to remove evaluation watermarks.

## Configuration

The sample does not require an Open-Meteo API key, account, or user configuration. Weather lookup does require network access.

### Default Behavior

- The initial city is London.
- Temperatures are displayed in Celsius and wind speed in meters per second using the service's default units.
- Weather data is requested from Open-Meteo when the page loads and when the user searches or refreshes.

### Environment Variables / Secrets

None required. The app does not read an Open-Meteo API key or other runtime secret.

### Notes

- The project currently contains a placeholder Syncfusion license registration string in `MauiProgram.cs`. Replace it locally with a valid key if required; do not commit the key.
- Network and city-lookup failures are shown in the dashboard; no cached response is substituted.
- Review Open-Meteo's terms and attribution requirements before redistributing or extending the sample with production use.
- Open-Meteo documentation: [Geocoding API](https://open-meteo.com/en/docs/geocoding-api) and [Forecast API](https://open-meteo.com/en/docs).

## Run

From the repository root, build for a platform workload installed on your machine:

```bash
dotnet build WeatherForecast/WeatherForecast/WeatherForecast.csproj -f net10.0-android
```

Other supported target frameworks are `net10.0-ios`, `net10.0-maccatalyst`, and `net10.0-windows10.0.19041.0`. You can also open [`WeatherForecast.sln`](WeatherForecast.sln) in Visual Studio or VS Code, choose a target, and deploy to a device or emulator.

## Output

![Weather Forecast Demo](WeatherForecastDemo.gif)

## Project Structure

- `WeatherForecast.sln`: solution file.
- `WeatherForecast/`: MAUI app project.
- `WeatherForecast/Views/`: mobile and desktop dashboard XAML/code-behind.
- `WeatherForecast/ViewModels/`: city search, weather state, and forecast presentation.
- `WeatherForecast/Services/`: Open-Meteo HTTP integration and response mapping.
- `WeatherForecast/Models/`: API response and display models.
- `WeatherForecast/Resources/`: app icon, images, fonts, and styles.

## Troubleshooting

**The forecast does not load**  
Check the device's internet connection and retry. The service needs network access to both Open-Meteo geocoding and forecast endpoints.

**The city cannot be found**  
Check the spelling or choose a city from the autocomplete suggestions. The service displays a not-found error when geocoding returns no result.

**The dashboard shows a timeout or network error**  
Retry when the connection is stable. Requests use a 20-second HTTP timeout, and the dashboard displays service errors rather than using offline data.

**Syncfusion evaluation watermark appears**  
Replace the placeholder license registration value with a valid Syncfusion key; see Configuration → Notes.

## Security and Privacy Notes

- The app sends the searched city name to Open-Meteo's geocoding endpoint and requests weather for the returned coordinates.
- No account, API key, or personally identifying information is required by the sample.
- Avoid entering personal information into the city search field.
- Keep any Syncfusion license key out of source control.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)
