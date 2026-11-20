# Smart Expense Tracker

A cross-platform .NET MAUI personal finance and expense tracking application built with Syncfusion controls to manage daily spending, monitor budgets, and visualize financial trends.

## Overview

Managing personal finances across multiple categories, tracking income versus expenses, and understanding monthly spending habits can quickly become disorganized without a centralized dashboard. Smart Expense Tracker solves this problem by providing a clean, responsive, and cross-platform experience that helps users record transactions, monitor cash flow, and review categorized spending insights.

- **What the sample does**: Provides an intuitive dashboard with financial insight cards (balance, income, expense, savings), circular doughnut breakdown charts, spline area income/expense monthly trend charts, a paginated transaction history grid, and a streamlined transaction recording workflow.
- **Who it is for**: Developers looking to build personal finance, budgeting, or line-of-business data visualization applications in .NET MAUI using Syncfusion controls.
- **What AI capability it uses**: The sample operates deterministically with local, in-memory repository services and mock financial logic; it does not require an external AI endpoint.
- **What outcome the user gets**: A ready-to-run, cross-platform personal finance manager that works offline with zero configuration, responsive desktop and mobile views, and rich interactive charts and data grids.

## Key Features

- **Financial Overview Dashboard**: Visual summary cards for total balance, income, expenses, and savings, paired with recent transaction feeds.
- **Category Spending Breakdown**: Interactive doughnut charts (`SfCircularChart`) displaying expense distributions across categories with center labels and customizable period filters.
- **Monthly Income & Expense Trends**: Smooth spline area chart (`SfCartesianChart`) comparing monthly cash inflows and outflows over time.
- **Comprehensive Transaction Management**: Tabular transaction history powered by `SfDataGrid` with status styling, type-based color formatting, and numeric pagination via `SfDataPager`.
- **Add & Edit Transactions**: Modal and page workflows utilizing `SfComboBox`, `SfNumericEntry`, `SfCalendar`, and `SfTextInputLayout` for fast transaction entry.
- **Adaptive Desktop & Mobile Layouts**: Responsive navigation using `SfTabView` on mobile devices and an adaptive sidebar list layout on desktop platforms.

## AI Flow

1. User enters financial transactions or navigates dashboard views.
2. The app prepares local category, period, and transaction context through the repository services.
3. The local store processes calculations, aggregates summaries, and computes trend points.
4. The app receives the computed data models and updates observable collections.
5. The result is rendered in the UI across charts, cards, and data grids.
6. If no records match the selected period, fallback "No Records Found" views and empty states are displayed.

## Controls Used

- `SfCircularChart` — renders the financial category expense breakdown as an interactive doughnut chart with center text and legends.
- `SfCartesianChart` — visualizes multi-month income and expense trends using spline area series with gradient fills.
- `SfDataGrid` — displays tabular transaction history with custom column formatting, sorting, and styling.
- `SfDataPager` — provides smooth numeric page navigation across paginated transaction lists.
- `SfTabView` — provides mobile bottom tab navigation between Dashboard and Transactions views.
- `SfListView` — displays recent transaction items and desktop sidebar navigation items.
- `SfSegmentedControl` — filters transactions and chart data by transaction type (All, Income, Expense).
- `SfComboBox` — provides dropdown selection for time periods, categories, and payment types.
- `SfCalendar` — allows interactive date selection for transactions and profile setups.
- `SfNumericEntry` — provides validated numeric input for transaction amounts and salary values.
- `SfTextInputLayout` — wraps entry fields and combo boxes with assistive labels, hints, and container outlines.
- `SfPopup` — hosts modal popups for adding and editing transactions on desktop layouts.
- `SfButton` — triggers primary user actions such as saving transactions, opening date pickers, and toggling views.

## Architecture

The sample is built using the Model-View-ViewModel (MVVM) architecture with `CommunityToolkit.Mvvm` and dependency injection:

- **Views** (`Views/`): XAML views for user onboarding (`SetupProfilePage`), main container (`MainPage`), financial overview (`DashboardPage`, `DashboardInsightsView`, `InsightCard`), transaction history (`TransactionsPage`, `TransactionsPage_Desktop`, `TransactionsPage_Mobile`), and transaction creation (`AddTransactionPage`, `AddTransactionPopupContent`).
- **ViewModels** (`ViewModels/`): Presentation logic and observable state (`DashboardViewModel`, `TransactionsViewModel`, `AddTransactionViewModel`, `SetupProfileViewModel`, `MainViewModel`, `InsightCardViewModel`).
- **Services** (`Services/`): Business logic and state coordination (`AppStateService`, `TransactionService`, `AuthService`).
- **Data & Repositories** (`Data/`): In-memory data store (`InMemoryExpenseStore`) and repositories (`TransactionRepository`, `CategoryRepository`, `UserRepository`).
- **Models** (`Models/`): Domain models representing `Transaction`, `Category`, `MonthlyTrendPoint`, `CategoryBreakdownItem`, `DashboardSummary`, and `UserProfile`.
- **Supporting UI components**: Converters in `Converters/` and visual helpers in `Helpers/`.

Data flows from the in-memory data store to repositories, which populate the central `AppStateService` observable collections, driving the ViewModels and automatically refreshing the bound UI controls.

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload (`dotnet workload install maui`)
- Visual Studio 2022 (17.14+) or VS Code with the .NET MAUI extension
- Syncfusion .NET MAUI packages (v34.2.7 or latest compatible, restored via NuGet)
- Syncfusion license key, if required by your environment

## AI Configuration

This sample does not require an external cloud AI endpoint. It uses local, deterministic financial computation and in-memory sample repositories.

### Default Behavior

- Local/offline logic
- In-memory data persistence
- No cloud AI provider required

### Environment Variables / Secrets

| Variable | Required | Description |
| --- | --- | --- |
| SYNCFUSION_LICENSE_KEY | no | Syncfusion license key for environments that require registration |

### Notes

- Do not commit keys to source control.
- Use environment variables, user secrets, or secure storage for license keys.
- If configuration is missing, the sample runs normally with local sample data and standard Syncfusion evaluation banners if unlicensed.

## Run

From the `ExpenseTracker/Expense Tracker` directory:

```bash
# Android
dotnet build SmartExpense.csproj -t:Run -f net10.0-android

# iOS / Mac Catalyst (macOS host)
dotnet build SmartExpense.csproj -t:Run -f net10.0-ios
dotnet build SmartExpense.csproj -t:Run -f net10.0-maccatalyst

# Windows
dotnet build SmartExpense.csproj -t:Run -f net10.0-windows10.0.19041.0
```

Alternatively, open `SmartExpense.sln` in Visual Studio or VS Code, select your target platform, and run the project.

## Output

![Smart Expense Tracker](https://github.com/user-attachments/assets/a952dc5e-f03d-4b9a-8fd4-a0acd7f8e8d9)

## Project Structure

- `Views/` — UI pages and platform-adaptive layouts (Dashboard, Transactions, Add Transaction, Setup Profile)
- `ViewModels/` — presentation logic and observable view models
- `Models/` — financial data structures (Transaction, Category, MonthlyTrendPoint, etc.)
- `Services/` — state coordination and transaction services
- `Data/` — in-memory storage and repositories
- `Converters/` — XAML value converters for amounts, colors, and visibility
- `Helpers/` — icons, color constants, and seed data
- `Resources/` — styles, fonts, vector icons, and splash assets

## Troubleshooting

**Issue 1: Missing AI configuration**

Cause: Required environment variables or secrets are not set.

Fix: This sample operates offline using in-memory mock data; no external AI configuration is required.

**Issue 2: Build or workload errors**

Cause: Missing SDK, MAUI workload, or incompatible package version.

Fix: Verify that the .NET 10 SDK is installed and run `dotnet workload install maui` to update workloads.

**Issue 3: AI response is unexpected**

Cause: Prompt, input, or service configuration may be incomplete.

Fix: Ensure transaction fields (amount, category, date) contain valid values for proper aggregation and chart rendering.

## Security and Privacy Notes

- Do not hard-code API or license keys in source code.
- Financial transactions are held in-memory for demonstration purposes and are not transmitted externally.
- Minimize prompt and data content to only what is needed when integrating external services.
- Prefer secure storage for user preferences and sensitive credentials.
- Review any external API calls or extensions for unintended data exposure.

## Supported Platforms

- Android
- iOS
- Windows
- macOS (Mac Catalyst)

## License

Refer to the repository license or Syncfusion licensing terms for details.

