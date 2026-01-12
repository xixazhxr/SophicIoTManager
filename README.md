# Sophic IoT Device Manager

A modern WPF Desktop Application for IoT device management with real-time dashboard, charts, and LoRaWAN-style configuration.

![.NET 6.0](https://img.shields.io/badge/.NET-6.0-512BD4)
![WPF](https://img.shields.io/badge/WPF-Desktop-blue)
![LiveCharts2](https://img.shields.io/badge/LiveCharts2-Charts-green)

---

## 📋 Table of Contents
- [Setup & Build Instructions](#setup--build-instructions)
- [Features Summary](#features-summary)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Tools & Libraries](#tools--libraries)

---

## Setup & Build Instructions

### Prerequisites
- **.NET 6.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **Visual Studio 2022** or **VS Code** with C# extension
- Windows 10/11 (WPF requirement)

### Quick Start
```bash
# Clone or navigate to the project
cd SophicIoTManager

# Restore the project
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run

# Stop the application (choose one):
# Option 1: Close the app window (click X)
# Option 2: Press Ctrl+C in the terminal
# Option 3: Use PowerShell:
Get-Process | Where-Object {$_.ProcessName -like "*SophicIoT*"} | Stop-Process -Force
```

### Visual Studio
1. Open `SophicIoTManager.sln` or `SophicIoTManager.csproj`
2. Press **F5** to build and run
3. NuGet packages restore automatically

---

## Features Summary

### ✅ Dashboard with Real-Time Charts
- **Stats Cards**: Projects, Gateways, Devices, Online, Offline, Errors counts
- **Pie Chart**: Device status distribution (Online/Offline/Error)
- **Bar Chart**: Devices per project comparison
- **Line Chart**: Real-time sensor values updating every 2 seconds
- **Collapsible Section**: Toggle dashboard visibility

### ✅ Device Management
- Hierarchical organization: **Projects → Gateways → Devices**
- Add, update, delete devices with validation
- LoRaWAN configuration fields (DevEUI, JoinEUI, AppKey)
- Real-time value simulation with realistic sensor data
- Status indicators with color coding

### ✅ Context Menus (Right-Click Actions)
| Item | Menu Options |
|------|--------------|
| Project | Add Gateway, Add Device, Edit, Delete |
| Gateway | Add Device, Edit, Toggle Status, Delete |
| Device | Edit, Toggle Status, Delete |

### ✅ Floating Edit Modal
- Dark overlay popup for editing items
- Dynamic form based on item type
- LoRaWAN settings for devices
- Save and Cancel with validation

### ✅ Action Logging
- All operations logged with timestamps
- Device Added/Updated/Deleted events
- Status Toggle events
- Simulation Start/Stop
- Error events

### ✅ Real-Time Simulation
- Timer-based value updates every **2 seconds**
- Realistic sensor value generation:
  - Temperature: 15-45°C
  - Humidity: 30-90%
  - Pressure: 90-120 PSI
  - Vibration: 0-100 Hz
- Random error simulation (2% chance)

---

## Architecture

### MVVM Pattern
```
┌─────────────────────────────────────────────────────────┐
│                        VIEW                             │
│  MainWindow.xaml, DashboardView.xaml, EditModal.xaml    │
├─────────────────────────────────────────────────────────┤
│                     VIEW MODEL                          │
│  MainViewModel, DashboardViewModel, EditModalViewModel  │
│  DeviceViewModel, GatewayViewModel, ProjectViewModel    │
├─────────────────────────────────────────────────────────┤
│                      MODEL                              │
│  Device.cs, Gateway.cs, Project.cs, LogEntry.cs         │
├─────────────────────────────────────────────────────────┤
│                     SERVICES                            │
│  IDeviceService.cs (Interface)                          │
│  MockDeviceService.cs (Simulation Implementation)       │
└─────────────────────────────────────────────────────────┘
```

### Dependency Injection
```csharp
// App.xaml.cs - Service Registration
services.AddSingleton<IDeviceService, MockDeviceService>();
services.AddTransient<MainViewModel>();
```

---

## Project Structure

```
SophicIoTManager/
├── Models/
│   ├── Device.cs              # Device entity with LoRaWAN fields
│   ├── Gateway.cs             # Gateway entity
│   ├── Project.cs             # Project container
│   └── LogEntry.cs            # Log entry model
├── ViewModels/
│   ├── MainViewModel.cs       # Main orchestration ViewModel
│   ├── DashboardViewModel.cs  # Chart data and stats
│   ├── EditModalViewModel.cs  # Edit modal form logic
│   ├── DeviceViewModel.cs     # Device wrapper
│   ├── GatewayViewModel.cs    # Gateway wrapper
│   └── ProjectViewModel.cs    # Project wrapper
├── Views/
│   ├── DashboardView.xaml     # Dashboard with charts
│   ├── DashboardView.xaml.cs  # Dashboard code-behind
│   ├── EditModal.xaml         # Floating edit modal
│   └── EditModal.xaml.cs      # Modal code-behind
├── Services/
│   ├── IDeviceService.cs      # Service interface
│   └── MockDeviceService.cs   # Mock implementation
├── MainWindow.xaml            # Main UI with sidebar & detail panel
├── MainWindow.xaml.cs         # Code-behind (minimal)
├── App.xaml                   # Application resources
├── App.xaml.cs                # DI configuration
└── SophicIoTManager.csproj    # Project file with dependencies
```

---

## Tools & Libraries

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 6.0 | Runtime Framework |
| **WPF** | - | UI Framework (Windows Presentation Foundation) |
| **C#** | 10.0 | Programming Language |
| **CommunityToolkit.Mvvm** | 8.2.2 | MVVM helpers (ObservableObject, RelayCommand, [ObservableProperty]) |
| **Microsoft.Extensions.DependencyInjection** | 8.0.0 | IoC Container for dependency injection |
| **LiveChartsCore.SkiaSharpView.WPF** | 2.0.0-rc2 | Real-time charting (Pie, Bar, Line charts) |
| **XAML** | - | UI Markup Language |

### NuGet Packages (from .csproj)
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc2" />
```

---

## UI Screenshots

### Main Application Layout
- **Left Sidebar**: TreeView with Projects → Gateways → Devices hierarchy
- **Right Panel**: 
  - Collapsible Dashboard section with charts
  - Collapsible Details section showing selected item properties
- **Bottom Panel**: System logs with timestamps
- **Header**: Online/Total device counts

### Dashboard Section
- Stats cards with consistent light blue (#00D9FF) count values
- Pie chart: Device status distribution
- Bar chart: Devices per project
- Line chart: Real-time sensor values

---

## Real-World Extension

This simulation can be extended to real IoT devices by implementing:

### MQTT (Recommended for IoT)
```csharp
public class MqttDeviceService : IDeviceService
{
    private readonly IMqttClient _mqttClient;
    // Subscribe to device topics
    await _mqttClient.SubscribeAsync("devices/+/telemetry");
}
```

### REST API
```csharp
public class RestDeviceService : IDeviceService
{
    private readonly HttpClient _httpClient;
    // GET /api/devices, PUT /api/devices/{id}
}
```

### Serial Port (COM)
```csharp
public class SerialDeviceService : IDeviceService
{
    private SerialPort _serialPort;
    // Direct hardware communication
}
```

---

## Error Handling

- **Device Status**: Online, Offline, Error, Maintenance states
- **Visual Indicators**: Color-coded status (green/gray/red)
- **Value Display**: Shows "---" when device is offline
- **Validation**: Required fields, confirmation dialogs for delete
- **Logging**: All errors logged with timestamps

---

## License

This project was created for assesment purposes demonstrating WPF, MVVM, and real-time data visualization best practices.

---

## Author

**Aidil Azhar © 2026**
