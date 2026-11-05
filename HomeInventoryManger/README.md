# Home Inventory Manager

Cross-platform .NET MAUI inventory management sample that demonstrates AI-assisted household tracking, item scanning, and maintenance insights with Syncfusion controls.

## Overview

Home Inventory Manager helps users catalog household assets, track warranty and maintenance details, and surface AI-style insights for inventory health and next actions. The sample uses a mock AI service in the repository so the full workflow can be explored without connecting to an external cloud AI provider.

This sample is for users and teams who want a practical home asset manager with AI-guided recommendations. It shows how AI can improve item classification, highlight warranty status, and recommend follow-up actions for household goods.

## Key Features

- Household inventory tracking and item management
- Invoice and warranty scanning workflow
- Warranty status and asset health dashboards
- AI-style recommendations backed by local mock data
- Shell-based navigation across the sample pages

## AI Flow

1. User scans an invoice, warranty document, or product label
2. The app prepares extracted context and item details
3. The AI service processes the request using local mock logic
4. The app receives the inferred item type, warranty details, or recommendation
5. The result is rendered in the scan, dashboard, and inventory UI
6. If the input is incomplete, the app falls back to editable sample values

## Controls Used

- SfTabView — organizes dashboard sections and inventory workflows
- SfListView — displays household items and related lists
- SfButton — triggers scan, save, and action commands
- SfCardView — presents dashboard KPIs and recommendation cards
- SfCircularProgressBar — shows health or completion indicators
- SfLinearProgressBar — displays progress and status states
- SfGauge — visualizes summary metrics and asset health trends
- SfPicker — supports structured selection where needed

## Architecture

- View / Page — XAML pages for dashboard, inventory, scan, warranty, and sidebar navigation
- ViewModel — manages page state, commands, and AI-driven summaries
- AI service — provides mock recommendations and classification responses
- Models — represent inventory items, recommendations, warranty data, and dashboard summaries
- Supporting UI components — Syncfusion charts, gauges, progress bars, lists, and tabs

## Prerequisites

- .NET 10 SDK
- .NET MAUI workload
- Visual Studio 2022 with .NET MAUI support, if using Visual Studio
- Syncfusion MAUI packages
- Syncfusion license key, if your environment requires one

## AI Configuration

This sample uses a mock AI service in the current codebase and does not require Azure OpenAI at runtime.

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
- If configuration is missing, the sample continues with its mock behavior

## Run

```bash
dotnet build HomeInventoryManger/HomeInventoryManager/HomeInventoryManager.csproj -t:Run -f net10.0-android
```

If you target another platform, adjust the `-f` value to match the supported framework on your machine.

## Output

[.NET MAUI HomeInventory Sample Demo](HomeInventoryDemo.gif)

## Project Structure

- Views/ — UI pages and layouts
- ViewModels/ — presentation logic
- Models/ — data structures
- Services/ — AI and helper services
- Resources/ — styles, images, and assets

## Troubleshooting

**Issue 1: Missing AI configuration**

Cause: Required environment variables or secrets are not set.

Fix: Use the built-in mock AI service or replace it with your own provider configuration.

**Issue 2: Build or workload errors**

Cause: Missing SDK, workload, or incompatible package version.

Fix: Verify the required .NET version and install the needed MAUI workload.

**Issue 3: AI response is unexpected**

Cause: Prompt, input, or service configuration may be incomplete.

Fix: Check the prompt/context being sent and confirm the AI settings.

## Security and Privacy Notes

- Do not hard-code API keys
- Avoid sending unnecessary sensitive data to the AI service
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
