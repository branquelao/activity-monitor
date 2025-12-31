# Activity Monitor (Windows)

A desktop application inspired by **macOS Activity Monitor** and **Windows Task Manager**, developed in **C# with WinUI 3**, focused on real-time process monitoring with emphasis on performance, UI stability, and user experience.

## ✨ Features

### 📊 CPU Monitoring
- Per-process CPU usage (%)
- **CPU Time** formatted in an Activity Monitor–like style (mm:ss.ms)
- Thread count per process
- Process ID (PID)
- Process classification:
  - `Application` – user-launched processes
  - `System` – core Windows system processes
  - `Service` – Windows services

### 🧠 Memory Monitoring
- Per-process memory usage (MB)
- Active thread count
- Handle count
- Process ID (PID)
- Process classification (Application / System / Service)

### 🧩 User Interface
- Dynamic DataGrid with **CPU** and **Memory** modes
- Continuous updates **without losing scroll position or selection**
- Stable sorting during real-time refresh
- Custom converters (e.g., `TimeSpan → string`)
- Clean layout inspired by native system tools

## ⚙️ Architecture

- **MVVM pattern**
- `ObservableCollection<ProcessInfo>` for incremental updates
- `INotifyPropertyChanged` for efficient UI refresh
- PID-based updates to avoid item recreation
- Clear separation of concerns:
  - Model (`ProcessInfo`)
  - ViewModel
  - View (XAML)

## 🚀 Performance

- Incremental property updates
- No full list resets (`Clear()`)
- Scroll and selection preserved
- Low UI and CPU overhead even with frequent refreshes

## 🛠️ Technologies

- C#
- WinUI 3
- CommunityToolkit WinUI DataGrid
- .NET

## 📌 Project Status

🚧 **Actively under development**
