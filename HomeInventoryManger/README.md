# Home Inventory Manager

This sample shows a .NET MAUI inventory app built with Syncfusion controls. It uses a mock AI service in the current codebase to demonstrate the user flow without requiring an Azure OpenAI resource.

## What the sample demonstrates

- Household inventory tracking
- Invoice and warranty scanning
- Warranty status and asset health dashboards
- AI-style recommendations backed by local mock data
- Shell-based navigation across the sample pages

## Prerequisites

- Visual Studio 2022 with the .NET MAUI workload
- .NET 10 SDK or the SDK version used by your MAUI environment
- Syncfusion MAUI packages

## Notes

- The sample does not call Azure OpenAI at runtime.
- If you want to use a real AI service, replace the mock implementation in `HomeInventoryManager/Services` with your own provider.
- The app now starts through `AppShell` so the shell navigation used by the pages is consistent.

## Troubleshooting

If you see build issues after switching branches or SDK versions, delete the `bin` and `obj` folders and rebuild.
