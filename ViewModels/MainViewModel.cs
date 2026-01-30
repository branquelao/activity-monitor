using ActivityMonitor.Models;
using ActivityMonitor.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace ActivityMonitor.ViewModels
{
    // Defines the current visualization mode
    public enum Viewmode
    {
        Cpu,
        Memory
    }

    // Represents the current sorting state
    public enum SortState
    {
        None,
        Ascending,
        Descending
    }

    public class MainViewModel : ViewModelBase
    {
        // Services responsible for system data
        private readonly ProcessService _service = new();
        private readonly CpuService _cpuService = new();
        private readonly MemoryService _memoryService = new();

        // UI refresh timer
        private readonly DispatcherTimer _timer;

        // Currently selected process
        private GroupedProcessInfo? _selectedProcess;
        public GroupedProcessInfo? SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                if (_selectedProcess == value)
                    return;

                _selectedProcess = value;
                OnPropertyChanged();
            }
        }

        // Mode helpers for XAML bindings
        public bool IsCpuMode => CurrentMode == Viewmode.Cpu;
        public bool IsMemoryMode => CurrentMode == Viewmode.Memory;

        // Process list shown in the grid
        public ObservableCollection<GroupedProcessInfo> Processes { get; } = new();

        // Current view mode
        private Viewmode _currentMode = Viewmode.Cpu;
        public Viewmode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode == value)
                    return;

                _currentMode = value;

                // Reset sorting on mode change
                _sortedColumn = "Process";
                _sortState = SortState.Ascending;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCpuMode));
                OnPropertyChanged(nameof(IsMemoryMode));

                UpdateProcesses();
                ApplySorting();
            }
        }

        // UI commands
        public ICommand CpuCommand { get; }
        public ICommand MemoryCommand { get; }
        public ICommand EndTaskCommand { get; }

        // CPU usage state
        private double _cpuUsed;
        public double CpuUsed
        {
            get => _cpuUsed;
            set
            {
                if (Math.Abs(_cpuUsed - value) < 0.01)
                    return;

                _cpuUsed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CpuUsedText));
                OnPropertyChanged(nameof(CpuFree));
                OnPropertyChanged(nameof(CpuFreeText));
            }
        }

        public double CpuFree => 100 - CpuUsed;
        public string CpuUsedText => $"{CpuUsed:F2}%";
        public string CpuFreeText => $"{CpuFree:F2}%";

        // Memory usage state
        private double _memoryUsedGB;
        private double _memoryTotalGB;

        public double MemoryUsedGB
        {
            get => _memoryUsedGB;
            private set
            {
                if (Math.Abs(_memoryUsedGB - value) < 0.01)
                    return;

                _memoryUsedGB = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryUsedText));
            }
        }

        public double MemoryTotalGB
        {
            get => _memoryTotalGB;
            private set
            {
                if (Math.Abs(_memoryTotalGB - value) < 0.01)
                    return;

                _memoryTotalGB = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryTotalText));
            }
        }

        public string MemoryUsedText => $"{MemoryUsedGB:F1} GB";
        public string MemoryTotalText => $"{MemoryTotalGB:F1} GB";

        // Sorting state
        private string? _sortedColumn = "Process";
        private SortState _sortState = SortState.Ascending;

        public SortState GetCurrentSortState()
        {
            return _sortState;
        }

        public string? GetSortedColumn()
        {
            return _sortedColumn;
        }

        // Rolling history for graphs
        public ObservableCollection<double> CpuHistory { get; } = new();
        public ObservableCollection<double> MemoryHistory { get; } = new();

        private const int MaxHistoryPoints = 60;

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                    return;

                _searchText = value;
                OnPropertyChanged();
                ApplyFilterAndSorting();
            }
        }

        public ObservableCollection<GroupedProcessInfo> FilteredProcesses { get; } = new();

        public MainViewModel()
        {
            EndTaskCommand = new RelayCommand(EndTask);
            CpuCommand = new RelayCommand(() => CurrentMode = Viewmode.Cpu);
            MemoryCommand = new RelayCommand(() => CurrentMode = Viewmode.Memory);

            // Updates data every second
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (_, _) => UpdateProcesses();
            _timer.Start();
        }

        // Refreshes processes, CPU and memory data
        private void UpdateProcesses()
        {
            string? selectedProcessName = SelectedProcess?.Name;
            var rawProcesses = _service.GetProcesses(1);

            // Group processes by name
            var grouped = rawProcesses
                .GroupBy(p => p.Name)
                .Select(g => new GroupedProcessInfo
                {
                    BaseName = g.Key,
                    Name = $"{g.Key} ({g.Count()})",
                    Pids = g.Select(p => p.Id).ToList(),
                    Cpu = g.Sum(p => p.Cpu),
                    Memory = g.Sum(p => p.Memory),
                    CpuTime = TimeSpan.FromTicks(g.Sum(p => p.CpuTime.Ticks)),
                    ThreadCount = g.Sum(p => p.ThreadCount),
                    HandleCount = g.Sum(p => p.HandleCount),

                    ExecutionType = g.Any(p => p.ExecutionType == "Application")
                        ? "Application"
                        : "Background"
                })
                .ToList();

            // Remove old entries
            for (int i = Processes.Count - 1; i >= 0; i--)
            {
                if (!grouped.Any(p => p.Name == Processes[i].Name))
                    Processes.RemoveAt(i);
            }

            // Update or add grouped processes
            foreach (var p in grouped)
            {
                var existing = Processes.FirstOrDefault(x => x.Name == p.Name);

                if (existing == null)
                {
                    Processes.Add(p);
                }
                else
                {
                    existing.Cpu = p.Cpu;
                    existing.Memory = p.Memory;
                    existing.CpuTime = p.CpuTime;
                    existing.ThreadCount = p.ThreadCount;
                    existing.HandleCount = p.HandleCount;
                    existing.Pids = p.Pids;
                }
            }

            // CPU mode update
            if (IsCpuMode)
            {
                CpuUsed = _cpuService.CpuUsage();
                AddPoint(CpuHistory, CpuUsed);
            }

            // Memory mode update
            if (IsMemoryMode)
            {
                _memoryService.Update();

                MemoryTotalGB = _memoryService.TotalMemoryGB;
                MemoryUsedGB = _memoryService.UsedMemoryGB;

                AddPoint(MemoryHistory, (MemoryUsedGB / MemoryTotalGB) * 100);
            }
            
            ApplyFilterAndSorting();

            if (selectedProcessName != null)
            {
                SelectedProcess = FilteredProcesses
                    .FirstOrDefault(p => p.Name == selectedProcessName);
            }
        }

        // Adds a rolling value to a graph history
        private void AddPoint(ObservableCollection<double> collection, double value)
        {
            if (collection.Count >= MaxHistoryPoints)
                collection.RemoveAt(0);

            collection.Add(value);
        }

        // Terminates all processes in the selected group
        private void EndTask()
        {
            if (SelectedProcess is null)
                return;

            foreach (var pid in SelectedProcess.Pids)
            {
                try
                {
                    var process = System.Diagnostics.Process.GetProcessById(pid);
                    process.Kill();
                }
                catch
                {
                    // Access denied or process already exited
                }
            }

            UpdateProcesses();
        }


        // Handles column sorting state
        public void ApplyColumnSort(string column)
        {
            if (_sortedColumn != column)
            {
                _sortedColumn = column;
                _sortState = SortState.Descending;
            }
            else
            {
                _sortState = _sortState == SortState.Descending
                    ? SortState.Ascending
                    : SortState.Descending;
            }

            ApplySorting();
        }

        // Applies filtering and sorting to the process list
        private void ApplyFilterAndSorting()
        {
            IEnumerable<GroupedProcessInfo> query = Processes;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(p =>
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            Func<GroupedProcessInfo, object> selector = _sortedColumn switch
            {
                "Process" => p => p.Name,
                "CPU (%)" => p => p.Cpu,
                "CPU Time" => p => p.CpuTime,
                "Threads" => p => p.ThreadCount,
                "Memory (MB)" => p => p.Memory,
                "Handles" => p => p.HandleCount,
                "Type" => p => p.ExecutionType,
                _ => p => p.Name
            };

            query = _sortState == SortState.Ascending
                ? query.OrderBy(selector)
                : query.OrderByDescending(selector);

            var list = query.ToList();

            FilteredProcesses.Clear();
            foreach (var item in list)
                FilteredProcesses.Add(item);
        }

        // Applies sorting to the process list
        private void ApplySorting()
        {
            IEnumerable<GroupedProcessInfo> ordered;

            Func<GroupedProcessInfo, object> selector = _sortedColumn switch
            {
                "Process" => p => p.Name,
                "CPU (%)" => p => p.Cpu,
                "CPU Time" => p => p.CpuTime,
                "Threads" => p => p.ThreadCount,
                "Memory (MB)" => p => p.Memory,
                "Handles" => p => p.HandleCount,
                "Type" => p => p.ExecutionType,
                _ => p => p.Name
            };

            ordered = _sortState == SortState.Ascending
                ? Processes.OrderBy(selector)
                : Processes.OrderByDescending(selector);

            var list = ordered.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                int oldIndex = Processes.IndexOf(list[i]);
                if (oldIndex != i)
                    Processes.Move(oldIndex, i);
            }
        }
    }
}
