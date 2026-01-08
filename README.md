# 🖥️ Activity Monitor (Windows)

A desktop application for Windows inspired by **macOS Activity Monitor** and **Windows Task Manager**, developed in **C# with WinUI 3**.

The project focuses on **real-time process monitoring**, **UI stability during frequent updates**, and a **clean, native-like user experience**, following solid MVVM practices.

---

## ✨ Features

### 📊 CPU Monitoring
- Per-process CPU usage (%)
- **CPU Time** formatted in Activity Monitor style (`mm:ss.ms`)
- Thread count per process
- Process ID (PID)
- Process type classification:
  - `Application` – user-launched processes
  - `System` – Windows system processes
  - `Service` – Windows services
- Global CPU usage:
  - **Used (%)**
  - **Free (%)**
- Dynamic sorting by any column (ascending / descending / reset)

---

### 🧠 Memory Monitoring
- Per-process memory usage (MB)
- Thread count
- Handle count
- Process ID (PID)
- Process type classification
- Global memory usage:
  - **Used (%)**
  - **Free (%)**
- Independent sorting logic for Memory mode

---

### 🎨 User Interface
- WinUI 3 modern desktop UI
- Light & Dark themes with dynamic switching
- Theme-aware colors using `ThemeDictionaries`
- Dynamic DataGrid:
  - CPU and Memory modes with column switching
  - Real-time updates **without losing scroll position or selection**
  - Stable sorting preserved during refresh
- Custom converters:
  - `TimeSpan → string` (CPU Time formatting)
  - Button state → style converter (active / inactive)
- Responsive layout with minimum window size constraints

---

### ⚙️ Process Control
- Select a process directly from the DataGrid
- **End Task** functionality
- Safe handling of protected processes and access denial

---

## 🧩 Architecture

- **MVVM pattern**
- Clear separation of responsibilities:
  - Model → `ProcessInfo`
  - ViewModel → `MainViewModel`
  - View → XAML
- Services-based architecture:
  - `ProcessService` – process enumeration and metrics
  - `CpuService` – global CPU usage calculation
  - `MemoryService` – global memory usage calculation
- Incremental updates using:
  - `ObservableCollection<ProcessInfo>`
  - PID-based reconciliation (no full list resets)
- Efficient UI updates with `INotifyPropertyChanged`
- Commands via custom `RelayCommand`

---

## 🚀 Performance & Stability

- Real-time refresh using `DispatcherTimer` (1-second interval)
- Incremental updates (no `Clear()` or full collection rebuilds)
- Selection and scroll position preserved during updates
- Low UI overhead even with frequent refresh cycles
- Sorting implemented via collection reordering (`Move`) instead of recreation

---

## 🎨 Theming System

- Centralized color management using `ResourceDictionary`
- Separate Light and Dark palettes
- Theme-aware brushes via `ThemeResource`
- Runtime theme switching without application restart

---

## 🛠️ Technologies

- C#
- .NET
- WinUI 3
- CommunityToolkit WinUI DataGrid
- MVVM architecture

---

## 📌 Project Status

🚧 **Actively under development**
