# Stock Chart — .NET MAUI Use Case Sample

A realistic stock-portfolio viewer built with **Syncfusion .NET MAUI controls**, demonstrating how to build a production-style financial charting experience — watchlists, multiple chart types, trendlines, and interactive range navigation — using self-contained local market data.

## Syncfusion Controls Demonstrated

| Control | Package | Used For |
|---|---|---|
| `SfCartesianChart` | Syncfusion.Maui.Charts | Main price chart: candle/OHLC/HiLo/area/line/column/step series, tooltip, trackball, zoom & pan |
| Trendlines | Syncfusion.Maui.Charts | Linear, exponential, logarithmic, polynomial, power, and moving-average trendlines |
| `SfRangeSelector` | Syncfusion.Maui.Sliders | Interactive chart range selection below the chart |
| `SfPopup` | Syncfusion.Maui.Popup | Settings, disclaimer, add-stock, watchlist CRUD, and confirmation dialogs |
| `SfTabView` | Syncfusion.Maui.TabView | Stocks / watchlist view switching |
| `SfComboBox` | Syncfusion.Maui.Inputs | Watchlist selector and stock search |
| `SfCalendar` | Syncfusion.Maui.Calendar | Custom start/end date selection |
| `SfButton` | Syncfusion.Maui.Buttons | Dialog actions, toolbar, and menu buttons |

## Features

- 7 stocks (AAPL, META, GOOGL, MSFT, NDSN, NFLX, TSLA) with daily OHLCV data loaded from bundled JSON.
- 10 chart types switchable at runtime, including candlestick and hollow-candle variants.
- 6 trendline types with a moving-average option.
- Time-range filtering (1M, 3M, 5M, 1Y, All) and a custom start/end date picker.
- Watchlists: create, edit, delete, add/remove stocks, and favorites.
- Chart settings: tooltip toggle, logarithmic axis, inverted axis, opposed axis, and range-control toggle.
- Adaptive layout with desktop and mobile breakpoints.

## Architecture

The sample follows the MVVM pattern using the **CommunityToolkit.Mvvm** library:

- `ViewModels/StockChartViewModel*.cs` — commands (`[RelayCommand]`) and observable state (`[ObservableProperty]`); all business logic (filtering, watchlist operations, series creation, range math) lives here.
- `Services/StockDataService.cs` — loads and validates stock data from `Resources/Raw/Stocks/*.json` (no network calls).
- `Helpers/PopupPresentation.cs` — popup sizing/positioning/theming rules shared by all `SfPopup` instances.
- `MainPage.xaml` / `MainPage.xaml.cs` — view; code-behind contains only view concerns (popup show/hide, breakpoint layout, gesture handling).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) with the .NET MAUI workload installed
- Platform workloads for your target OS (Android, iOS, Windows, or Mac Catalyst)
- A Syncfusion license — register it in `MauiProgram.cs`:

```csharp
SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
```

Replace `YOUR_LICENSE_KEY` with your license key before running (see [Syncfusion licensing](https://help.syncfusion.com/common/licensing/how-to-register-maui-license-key)).

## Build and Run

Open `StockChart/StockChart.slnx` in Visual Studio or VS Code, restore packages, and deploy to your target platform (Android, iOS, Windows, or Mac Catalyst). Alternatively, from the command line:

```bash
dotnet build StockChart/StockChart/StockChart.csproj -f net10.0-android   # or your target framework
dotnet run --project StockChart/StockChart/StockChart.csproj -f net10.0-android
```

## Data

All market data is fictional sample data bundled with the app under `Resources/Raw/Stocks/`. No authentication, network services, or API keys are required.
