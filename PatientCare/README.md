# Patient Care Sample Application

## Overview
The Patient Care sample is a healthcare management application built with .NET MAUI that demonstrates Syncfusion controls in a realistic patient care scenario. The application includes dashboard metrics, appointment scheduling, patient management, and care plan tracking.

## Features
- Dashboard with health metrics visualization
- Patient management system
- Appointment scheduling with clinician assignment
- Care plan tracking and task management
- Notification system for alerts

## Setup Instructions
1. Ensure you have Visual Studio 2022 or later with .NET MAUI workload installed
2. Open the `PatientCare.slnx` solution file
3. Restore NuGet packages (this will automatically restore Syncfusion packages)
4. Build and run the application

Note: Syncfusion components may require a license key. Register it in `MauiProgram.cs` via `Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY")` if your usage requires it.

## Application Structure
- **Views**: XAML pages for each feature area
- **ViewModels**: MVVM pattern implementation
- **Models**: Data models for patients, appointments, etc.
- **Services**: Business logic and data services
- **Controls**: Custom reusable UI controls

## License Registration
Syncfusion controls run in evaluation mode unless a valid license key is registered. To register one:
1. Obtain a Syncfusion license key from https://www.syncfusion.com/account/license
2. Add the registration call at the top of `MauiProgram.cs` in `CreateMauiApp()`:
   ```csharp
   Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
   ```

## Supported Platforms
- Android
- iOS
- Windows
- macOS
