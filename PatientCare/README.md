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
4. Add your Syncfusion license key in `MauiProgram.cs`:
   ```csharp
   SyncfusionLicenseProvider.RegisterLicense("YOUR_LICENSE_KEY");
   ```
5. Build and run the application

## Application Structure
- **Views**: XAML pages for each feature area
- **ViewModels**: MVVM pattern implementation
- **Models**: Data models for patients, appointments, etc.
- **Services**: Business logic and data services
- **Controls**: Custom reusable UI controls

## License Registration
Before running the application, you must register your Syncfusion license:
1. Obtain a Syncfusion license key from https://www.syncfusion.com/account/license
2. Replace "YOUR_LICENSE_KEY" in `MauiProgram.cs` with your actual license key

## Supported Platforms
- Android
- iOS
- Windows
- macOS