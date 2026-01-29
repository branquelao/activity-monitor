# 🖥️ Activity Monitor (Windows)

A **Windows desktop application** inspired by **macOS Activity Monitor** and **Windows Task Manager**, developed in **C# with WinUI 3**.

---

## ✨ Features

### 📊 CPU Monitoring
- Per-process **CPU usage (%)** (formatted with 1 decimal)
- **CPU Time** formatted in Activity Monitor style (`mm:ss.ms`)
- Thread count per process
- Process type classification:
  - **Application** – processes that have at least one foreground instance
  - **Background** – processes running only in background
- Global CPU usage:
  - **Used (%)**
  - **Free (%)**
- Dynamic column sorting:
  - Ascending
  - Descending
  - Reset to default
- Sorting preserved during real-time refresh

---

### 🧠 Memory Monitoring
- Per-process **memory usage with adaptive units**:
  - Displays **MB** for values below 1 GB
  - Automatically switches to **GB** for values ≥ 1 GB
- Thread count
- Handle count
- Process type classification (Application / Background)
- Global memory usage:
  - **Used (GB)**
  - **Total memory (GB)**
- Independent sorting logic for Memory mode

---

### 🔍 Process Search & Filtering
- Real-time **process filtering** via search bar
- Case-insensitive search
- Filters processes by:
  - Process name
- Fully compatible with:
  - Sorting
  - Selection
  - Real-time updates
- Selected process is preserved whenever possible during list refresh

---

### 🎨 User Interface
- Modern **WinUI 3** desktop UI
- CPU and Memory modes with dynamic column switching
- CommunityToolkit **DataGrid**
- Numeric columns aligned to the **right** (Task Manager–style)
- Real-time updates **without losing**:
  - Scroll position
  - Selected process
  - Sorting order
- Custom UI components:
  - Performance graphs
  - Theme-aware colors and brushes
- Clean, native-like Windows design

---

### ⚙️ Process Control
- Process selection directly from the DataGrid
- **End Task** functionality
- Safe handling of:
  - Protected processes
  - Access denied scenarios
- Robust exception handling to prevent UI freezes

---

## 🧩 Architecture

- **MVVM pattern**
- Clear separation of concerns:
  - **Model** → `GroupedProcessInfo`
  - **ViewModel** → `MainViewModel`
  - **View** → XAML (WinUI 3)
- Service-oriented design:
  - `ProcessService` – process enumeration and metrics calculation
  - `CpuService` – global CPU usage
  - `MemoryService` – global memory usage
- Incremental updates using:
  - `ObservableCollection<T>`
  - PID-based caching
- Efficient UI synchronization with `INotifyPropertyChanged`
- Command handling via custom `RelayCommand`

---

## 🚀 Performance & Stability

- Real-time refresh using `DispatcherTimer` (1-second interval)
- Incremental updates (no full collection rebuilds)
- Collection reconciliation instead of `Clear()` operations
- Filtering and sorting applied on a derived collection
- Stable UI even under frequent updates
- Sorting implemented via collection reordering (`Move`) for minimal UI overhead

---

## 🎨 Theming System

- Centralized theming using `ResourceDictionary`
- Theme-aware brushes via `ThemeResource`
- Clean separation between UI logic and visual resources
- Ready for future Light/Dark theme expansion

---

## 🛠️ Technologies

- C#
- .NET
- WinUI 3
- CommunityToolkit WinUI (DataGrid)
- MVVM Architecture

---

## 📌 Project Status

🚧 **Actively under development**
