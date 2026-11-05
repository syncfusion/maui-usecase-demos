# Stock Chart — .NET MAUI Use Case Sample

A realistic stock-portfolio viewer built with **Syncfusion .NET MAUI controls**, demonstrating how to build a production-style financial charting experience — watchlists, multiple chart types, trendlines, and interactive range navigation — using self-contained local market data.

## Overview

Investors want to track a watchlist of stocks, switch between chart types, spot trends, and zoom into specific date ranges. This sample shows how to build that experience with .NET MAUI and Syncfusion controls:

- **What the sample does** — Displays daily OHLCV price data for 7 stocks across 10 chart types with 6 trendline options, time-range filtering (including a custom date range), and full watchlist management (create, edit, delete, favorite, add/remove stocks).
- **Who it is for** — Developers building financial dashboards, analytics viewers, or any data-visualization-heavy MAUI app.
- **What it demonstrates** — Advanced use of `SfCartesianChart` (series, trendlines, tooltip, trackball, zoom & pan), `SfRangeSelector`, `SfPopup`, `SfTabView`, `SfComboBox`, `SfCalendar`, and `SfButton` in a single MVVM app.
- **What outcome the user gets** — A complete, cross-platform stock chart viewer that runs locally with no backend, network calls, or credentials required.

## Key Features

- 7 stocks (AAPL, META, GOOGL, MSFT, NDSN, NFLX, TSLA) with daily OHLCV data loaded from bundled JSON.
- 10 chart types switchable at runtime, including candlestick and hollow-candle variants.
- 6 trendline types (linear, exponential, logarithmic, polynomial, power) plus a moving average.
- Time-range filtering (1M, 3M, 5M, 1Y, All) and a custom start/end date picker via `SfCalendar`.
- Watchlists: create, edit, delete, add/remove stocks, and favorites.
- Chart settings: tooltip toggle, logarithmic axis, inverted axis, opposed axis, and range-control toggle.
- Adaptive layout with desktop and mobile breakpoints, plus a busy indicator while data loads.

## Data Flow

1. User launches the app; the stock service loads and validates the bundled JSON files from `Resources/Raw/Stocks/`.
2. The ViewModel exposes the loaded stocks, watchlists, and chart settings as observable state.
3. The user selects a stock or watchlist, switches chart type/trendline, or applies a time range (preset or custom dates).
4. The ViewModel recomputes the filtered series and the chart, trendlines, and range selector re-render.
5. Popup dialogs (settings, add-stock, watchlist CRUD, disclaimers) collect further input and update the ViewModel.
6. All data stays local — no network calls, retries, or fallbacks are needed.

## Controls Used

- `SfCartesianChart` (Charts) — Main price chart: candle/OHLC/HiLo/area/line/column/step series, tooltip, trackball, zoom & pan.
- Trendlines (Charts) — Linear, exponential, logarithmic, polynomial, power, and moving-average trendlines.
- `SfRangeSelector` (Sliders) — Interactive chart range selection below the chart.
- `SfPopup` (Popup) — Settings, disclaimer, add-stock, watchlist CRUD, and confirmation dialogs.
- `SfTabView` (TabView) — Stocks / watchlist view switching.
- `SfComboBox` (Inputs) — Watchlist selector and stock search.
- `SfCalendar` (Calendar) — Custom start/end date selection.
- `SfButton` / `SfRadioButton` (Buttons) — Dialog actions, toolbar, menu, and settings options.
- `SfBusyIndicator` (Core) — Loading feedback while data is prepared.
- `SfChipGroup` / `SfTextInputLayout` (Core) — Quick filters and styled text input.

## Architecture

The sample follows the MVVM pattern using the **CommunityToolkit.Mvvm** library:

- **View** — `MainPage.xaml` plus partial code-behind files (`MainPage.ChartSurface.cs`, `MainPage.MobileLayout.cs`, `MainPage.PopupWorkflows.cs`); code-behind contains only view concerns (popup show/hide, breakpoint layout, gesture handling).
- **ViewModels** — `StockChartViewModel*.cs` files with commands (`[RelayCommand]`) and observable state (`[ObservableProperty]`); all business logic (filtering, watchlist operations, series creation, range math) lives here, along with `ChartSettingsViewModel` and `TrendlineViewModel`.
- **Services** — `StockDataService.cs` loads and validates stock data from bundled JSON (no network calls).
- **Models** — `StockModel`, `CandleDataModel`, `WatchlistModel`, and selection models.
- **Supporting components** — `Helpers/PopupPresentation.cs` (popup sizing/positioning/theming shared by all `SfPopup` instances) and `Converters/`.

Data flows from the bundled JSON → `StockDataService` → ViewModel observable state → bound chart/list/popup views.

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload (plus platform workloads for Android, iOS, Mac Catalyst, or Windows)
- Visual Studio 2022 (17.14+) or VS Code with the MAUI extension
- Syncfusion .NET MAUI packages (v34.2.7) — restored automatically from NuGet; a Syncfusion license is required (see below)

## Configuration

No configuration is required. All market data is fictional sample data bundled with the app under `Resources/Raw/Stocks/`. There are no API keys, environment variables, or secrets.

### Default Behavior

- **Local/offline logic** — the sample is fully self-contained and runs without any network access. No authentication, backend services, or market data feeds are used.

### Environment Variables / Secrets

None required. The sample does not call any external service.

### Notes

- Do not commit license keys to source control. Keep your Syncfusion license key out of committed code when sharing changes.
- Syncfusion controls run in evaluation mode unless a valid license key is registered:

  ```csharp
  SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
  ```

  Add this in `MauiProgram.cs`, replacing `YOUR_LICENSE_KEY` with your key (see [Syncfusion licensing](https://help.syncfusion.com/common/licensing/how-to-register-maui-license-key)).

## Run

```bash
dotnet build StockChart/StockChart/StockChart.csproj -t:Run -f net10.0-android
```

Other supported platforms:

```bash
dotnet build StockChart/StockChart/StockChart.csproj -t:Run -f net10.0-ios          # iOS
dotnet build StockChart/StockChart/StockChart.csproj -t:Run -f net10.0-maccatalyst  # macOS
dotnet build StockChart/StockChart/StockChart.csproj -t:Run -f net10.0-windows10.0.19041.0  # Windows
```

Or open `StockChart/StockChart.slnx` in Visual Studio or VS Code, restore packages, and deploy to your target platform.

## Output

A demo recording of the sample is included in the repository: `stock-chart-demo.mp4` — it walks through stock selection, chart type and trendline switching, time-range filtering with the range selector, watchlist management, and the settings dialogs.

## Project Structure

- `Views` (MainPage + partials) — UI, chart surface, mobile layout, and popup workflows
- `ViewModels/` — presentation logic and commands
- `Models/` — stock, candle, and watchlist data structures
- `Services/` — stock data loading from bundled JSON
- `Helpers/` — shared popup presentation rules
- `Converters/` — XAML value converters
- `Resources/` — styles, fonts, images, and bundled raw stock data (`Resources/Raw/Stocks/`)

## Troubleshooting

**Issue 1: Charts render with no data**
Cause: Bundled JSON assets missing or the raw asset was renamed/moved.
Fix: Confirm the seven `*.json` files exist under `Resources/Raw/Stocks/` and are marked as `MauiAsset`.

**Issue 2: Build or workload errors**
Cause: Missing .NET 10 SDK, MAUI workload, or platform workload.
Fix: Verify the installed .NET version (`dotnet --version`) and install the needed MAUI workload (`dotnet workload install maui`).

**Issue 3: Trial/evaluation banner shown**
Cause: No Syncfusion license key registered.
Fix: Register a valid license key in `MauiProgram.cs` (see the Notes section above).

## Security and Privacy Notes

- No API keys or credentials are used anywhere in the sample.
- All market data is fictional; no external API calls are made and no data leaves the device.
- If you extend the sample to consume a live market data feed, keep credentials in secure storage (never hard-coded) and review what data the provider receives.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)

## License

This sample is provided by Syncfusion under the repository's license. Syncfusion .NET MAUI components require a valid Syncfusion license for production use; see <https://www.syncfusion.com/products/maui> for details. All market data shown is fictional sample data and is not investment advice.

This sample is part of the Syncfusion .NET MAUI sample browser examples. The Stock Chart sample and all other examples are for demonstrative purposes only. Syncfusion has no liability for any damage or consequence that may arise by the use or viewing of the Stock Chart sample or any other example. If you choose to use or access the examples, you agree to not hold Syncfusion liable, in any form, for any damage that is related to the use, accessing, or otherwise viewing of the Stock Chart sample or any other example. By accessing, viewing, or otherwise seeing the examples you acknowledge and agree that Syncfusion's examples, including the Stock Chart sample, will not allow you to seek injunctive relief in any form for any claim related to the sample. If you do not agree to this, do not view, access, utilize, or otherwise do anything with Syncfusion's examples.
