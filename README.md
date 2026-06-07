# Activity Monitor App

A **Windows desktop application** inspired by **macOS Activity Monitor** and **Windows Task Manager**, developed in **C# with WinUI 3**.

---

## Features

### CPU Monitoring
- Per-process **CPU usage (%)** (formatted with 1 decimal)
- **CPU Time** formatted in Activity Monitor style (`mm:ss.ms`)
- Thread count per process
- Process type classification:
  - **Application** – processes that have at least one foreground instance
  - **Background** – processes running only in background
- Global CPU usage:
  - **Used (%)**
  - **Free (%)**
- **Real-time performance graph** with 60-second rolling history
- Dynamic column sorting with visual indicators:
  - Ascending (▲)
  - Descending (▼)
  - Default sorting resets when switching modes
- Sorting preserved during real-time refresh

---

### Memory Monitoring
- Per-process **memory usage with adaptive units**:
  - Displays **MB** for values below 1 GB
  - Automatically switches to **GB** for values ≥ 1 GB
- Thread count
- Handle count
- Process type classification (Application / Background)
- Global memory usage:
  - **Used (GB)**
  - **Total memory (GB)**
- **Real-time performance graph** with 60-second rolling history
- Independent sorting logic for Memory mode

---

### Process Search & Filtering
- Real-time **process filtering** via search bar
- Case-insensitive search
- Filters processes by:
  - Process name (friendly name and process key)
- Fully compatible with:
  - Sorting
  - Selection
  - Real-time updates
- Selected process is preserved whenever possible during list refresh

---

### Process Display
- **Friendly process names** extracted from executable metadata:
  - Uses `FileDescription` when available
  - Falls back to `ProductName` if needed
  - Shows original process name as final fallback
- **Process icons** displayed alongside names:
  - Extracted from executable files
  - Fallback generic icon for protected/system processes
  - Uses `QueryFullProcessImageName` API for better access
- **Process grouping** by name with instance count:
  - Example: "Google Chrome (18)"
  - Aggregates CPU, memory, threads, and handles across all instances
  - Shows icon from first instance in group

---

### User Interface
- Modern **WinUI 3** desktop UI
- CPU and Memory modes with dynamic column switching
- CommunityToolkit **DataGrid** with custom styling:
  - Removed focus border for cleaner look
  - Custom column templates for icon + text display
  - Right-aligned numeric columns (Task Manager–style)
- **Sorting indicators** with arrows (▲/▼) in column headers
- Real-time updates **without losing**:
  - Scroll position
  - Selected process
  - Sorting order
- Custom UI components:
  - Performance graphs with smooth animations
  - Theme-aware colors and brushes
- Clean, native-like Windows design

---

### Process Control
- Process selection directly from the DataGrid
- **End Task** functionality:
  - Terminates all instances in selected process group
  - Safe handling of protected processes
  - Graceful error handling for access denied scenarios
- Robust exception handling to prevent UI freezes

---

## Architecture

- **MVVM pattern** with clear separation of concerns
- **Models**:
  - `ProcessInfo` – individual process data
  - `GroupedProcessInfo` – aggregated process group data
- **ViewModel**:
  - `MainViewModel` – orchestrates UI state and data flow
- **View**:
  - XAML (WinUI 3) – declarative UI definition
- **Service-oriented design**:
  - `ProcessService` – process enumeration, metrics calculation, and icon extraction
  - `CpuService` – global CPU usage monitoring
  - `MemoryService` – global memory usage monitoring
- **Converters**:
  - `TimeSpanConverter` – formats CPU time display
  - `ButtonConverter` – dynamic button styling
  - `NullToVisibilityConverter` – conditional icon display
- Incremental updates using:
  - `ObservableCollection<T>` for reactive UI
  - PID-based caching for efficiency
- Efficient UI synchronization with `INotifyPropertyChanged`
- Command handling via custom `RelayCommand`

---

## Performance & Stability

- Real-time refresh using `DispatcherTimer` (1-second interval)
- Incremental updates (no full collection rebuilds)
- Collection reconciliation instead of `Clear()` operations
- Filtering and sorting applied on a derived collection (`FilteredProcesses`)
- Stable UI even under frequent updates
- Sorting implemented via collection reordering (`Move()`) for minimal UI overhead
- Icon extraction cached per process to avoid repeated file I/O
- Graceful degradation for inaccessible processes

---

## Theming System

- Centralized theming using `ResourceDictionary`
- Theme-aware brushes via `ThemeResource`
- Separate `Colors.xaml` for Light/Dark theme definitions
- Custom styles for:
  - Buttons (header, delete)
  - DataGrid cells and headers
  - Performance indicators (Used/Free)
- Clean separation between UI logic and visual resources
- Ready for future theme expansion

---

## Technologies

- **C# .NET** – Core application logic
- **WinUI 3** – Modern Windows UI framework
- **CommunityToolkit WinUI** – DataGrid component
- **System.Drawing.Common** – Icon extraction
- **Win32 APIs** – Enhanced process access (`QueryFullProcessImageName`)
- **MVVM Architecture** – Clean code organization

---

## Project Status

**Actively under development**

### Recent Updates
- ✅ Friendly process names from executable metadata
- ✅ Process icons with fallback for protected processes
- ✅ Visual sorting indicators in column headers
- ✅ Improved process grouping with instance counts
- ✅ Enhanced process access via Win32 APIs
