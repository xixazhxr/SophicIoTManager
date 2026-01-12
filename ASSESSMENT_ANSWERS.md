# Sophic IoT Manager - Detailed Assessment Report

> **Project**: Sophic IoT Device Manager  
> **Technology Stack**: WPF (.NET 6.0), C#, MVVM Pattern  
> **Author**: Sophic Automation © 2026  
> **Assessment Date**: January 12, 2026

---

## 📖 Introduction: Project Overview
**What is this project?**  
This is a Windows Desktop application designed to simulate the management of Internet of Things (IoT) devices. It mimics a real-world industrial dashboard used to monitor sensors (like temperature, vibration) in factories or warehouses.

**Why was it built?**  
To demonstrate proficiency in **C#** and **WPF (Windows Presentation Foundation)** while implementing specific architectural patterns like **MVVM (Model-View-ViewModel)** ensuring code maintainability and separation of concerns.

**Who is it for?**  
It is designed for system administrators or facility managers who need a central "Command Center" to see the health and data of hundreds of remote sensors at a glance.

---

## 📋 Part 1: Windows-Based IoT Tool (Core Coding Task)

### 1.1 Technology Choice & Structure
**Why use WPF and MVVM?**
*   **WPF (Windows Presentation Foundation)**: Chosen because it provides a powerful styling engine (XAML) that allows for the creation of proper "Dashboard" style UIs which are difficult to build in older generic Windows Forms.
*   **MVVM (Model-View-ViewModel)**:
    *   **What**: A design pattern that separates the *visualization* (XAML) from the *logic* (C#).
    *   **Why**: It allows the UI to update automatically when data changes without writing complex "event handling" code. If a sensor value changes in the code, the screen updates instantly via "Data Binding".

### 1.2 Data Handling (Seed & Mock Data)
**What data is used?**
We use "Mock Data" (fake simulation data). Since we don't have physical sensors connected to this computer, we create C# objects that *pretend* to be sensors.

**Where does the data come from?**
The data originates in the `MockDeviceService.cs` class. When the app starts (`App.xaml.cs`), it asks this service to "InitializeSampleData()".

**How is it structured?**
We use a **Hierarchical Structure**:
1.  **Projects** (Root): e.g., "Smart Factory"
2.  **Gateways** (Middle): e.g., "Gateway-North". In the real world, sensors are low-power and can't talk to the internet directly; they talk to a "Gateway" box first.
3.  **Devices** (Leaf): The actual sensors (Temperature, Pressure).

> **WH-Check:**
> *   **Who creates data?** The `MockDeviceService`.
> *   **When?** Immediately upon application startup.
> *   **Why mock data?** To allow testing and demonstration of UI features without needing expensive hardware.

### 1.3 CRUD Operations (Create, Read, Update, Delete)
**How does "Adding a Device" actually work?**
1.  **User Action**: The user fills in the "Add Device" form and clicks the button.
2.  **Command Execution**: The button triggers a command (`AddDeviceAsync`) in the `MainViewModel`.
3.  **Service Call**: The ViewModel asks the Service (`_deviceService.AddDeviceAsync`) to save the new device.
4.  **Validation**: The Service checks: "Does this device have a name?", "Does the parent Project exist?". If yes, it adds it to the list.
5.  **UI Update**: Because we use `ObservableCollection`, the moment the item is added to the list in code, the UI "List View" on the screen automatically draws a new row.

### 1.4 Connectivity Simulation
**What is being simulated?**
We simulate the "Status" of a device: **Online** (Green), **Offline** (Gray), or **Error** (Red).

**How is "Toggle Status" implemented?**
*   **What**: When a user right-clicks and selects "Toggle Status".
*   **When**: Immediate user interaction.
*   **How**:
    1.  The app introduces a fake artificial delay (0.5 to 1.5 seconds) using `await Task.Delay(...)`.
    2.  **Why delay?** To make it feel like a real network request is travelling to a remote device.
    3.  The status property is flipped (Online <-> Offline).
    4.  The change triggers a `PropertyChanged` event, causing the status indicator color in the UI to switch from Green to Gray instantly.

---

## 🏗️ Part 2: System Architecture & Data Flow

### 2.1 detailed Architecture Diagram

```
[ USER ] 👀 Checks Screen
    │
    ▼
[ VIEW LAYER (The "Face") ] 
(MainWindow.xaml)
"I only know how to draw buttons and text."
    │
    │  Data Binding (The "Glue")
    │  "Hey View, here represents the state of the system."
    ▼
[ VIEWMODEL LAYER (The "Brain") ]
(MainViewModel.cs)
"I handle the logic. User clicked add? I'll organize that."
    │
    │  Calls Methods
    ▼
[ SERVICE LAYER (The "Worker") ]
(MockDeviceService.cs)
"I actually manipulate the data."
    │
    │  Reads/Writes
    ▼
[ MODEL LAYER (The "Data") ]
(Device.cs)
"I am just a container for data: Name, ID, Value."
```

### 2.2 Data Flow Scenario: "A Temperature Update"
**Scenario**: A simulated temperature sensor updates its value from 23.5°C to 24.1°C.

1.  **Where (Origin)**: `MockDeviceService.cs` inside the `SimulationCallback` timer.
2.  **When**: Every 2 seconds (the tick interval).
3.  **What happens**:
    *   The `Random` number generator calculates a new value.
    *   `device.Value = 24.1;` is executed.
4.  **How the UI knows**:
    *   The `Device.cs` class inherits from `ObservableObject`.
    *   Setting the value triggers an event: `"Hey! The property 'Value' just changed!"`.
5.  **Who reacts**:
    *   The **Chart** on the dashboard hears this event and draws a new point.
    *   The **Text** in the tree view hears this event and updates the text to "24.1 °C".

---

## 📡 Part 3: Device Communication Handling

### 3.1 Simulation Mechanics
**How does the application "talk" to devices?**
Since there are no real devices, we use a **Timer**:
*   **What**: `System.Threading.Timer`
*   **Why**: To create a "Heartbeat". In real IoT, devices send data periodically. The timer mimics this exact behavior.
*   **When**: Every 2000 milliseconds (2 seconds).
*   **How**:
    1.  Timer ticks.
    2.  Loop through all `Online` devices.
    3.  Modify their values slightly (random walk).
    4.  **Random Errors**: There is a 2% chance (`_random.NextDouble() < 0.02`) the code forces a device into `Error` state.
    5.  **Why implement errors?** To prove the system can handle failure gracefully (logging the error, turning the icon red) rather than crashing.

### 3.2 Real-World Implementation (Comparison)
**If we built this for real, what changes?**

| Feature | Simulation (Current) | Real World (Future) | Why the difference? |
| :--- | :--- | :--- | :--- |
| **Transport** | Internal Memory | **MQTT Protocol** | IoT devices are remote; they need a lightweight network protocol to send data over the internet. |
| **Identity** | C# GUID | **DevEUI (Hex)** | LoRaWAN networks identify hardware using a unique, burned-in 16-character hex code, not a software GUID. |
| **Updates** | Timer Loop | **Event Subscription** | In reality, we don't "ask" devices for data; they "push" data when they wake up. We would subscribe (listen) to a topic. |

---

## 📊 Part 4: Logic & Data Documentation

### 4.1 Database Logic (ERD Explanation)
**What are the relationships between data?**

1.  **Project (1) ──has──> (N) Gateways**
    *   **Translation**: One Project (e.g., "Factory A") can contain Many Gateways (e.g., "Floor 1 Gateway", "Floor 2 Gateway").
    *   **Why**: Logical grouping. You don't want to see a flat list of 1000 devices; you want them organized by location.

2.  **Gateway (1) ──has──> (N) Devices**
    *   **Translation**: One physical Gateway box connects wirelessly to Many nearby Sensors.
    *   **Why**: This matches physical reality. One expensive gateway connects to hundreds of cheap sensors.

### 4.2 Sequence of Operations (Flowchart Explanation)
**Example: What exactly happens when a user deletes a project?**

1.  **Start**: User selects "Smart Factory" and clicks Delete.
2.  **Logic Step 1 (Safety Check)**: The app pauses. "Are you sure?" (MessageBox).
    *   **Why**: Accidental deletion causes massive data loss.
3.  **Logic Step 2 (Cascading Delete)**:
    *   **What**: The system doesn't just delete the Project. It looks inside.
    *   **Action**: It finds all **Gateways** inside that project.
    *   **Action**: It finds all **Devices** connected to those gateways.
    *   **Result**: It deletes the Children first, then the Parent.
    *   **Why**: To prevent "Orphaned Data" (Devices that belong to a project that no longer exists).
4.  **End**: The UI removes the entire branch from the TreeView and Logs the action: *"Success: Project Deleted"*.

---

## 💡 Summary
This documentation provides a "White Box" view of the system.
*   **Users** understand *Why* the layout is hierarchical (Project > Gateway).
*   **Developers** understand *How* the data updates (Observables & Timers).
*   **Architects** understand *Where* the data flows (Service -> View).

**Sophic Automation © 2026**
