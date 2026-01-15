using ActivityMonitor.Models;
using ActivityMonitor.Services;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ActivityMonitor.ViewModels
{
    public enum Viewmode
    {
        Cpu,
        Memory
    }

    public enum SortState
    {
        None,
        Ascending,
        Descending
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly ProcessService _service = new();
        private readonly DispatcherTimer _timer;
        private ProcessInfo? _selectedProcess;
        public ProcessInfo? SelectedProcess
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

        public bool IsCpuMode => CurrentMode == Viewmode.Cpu;
        public bool IsMemoryMode => CurrentMode == Viewmode.Memory;

        public ObservableCollection<ProcessInfo> Processes { get; } = new();

        private Viewmode _currentMode = Viewmode.Cpu;
        public Viewmode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode == value)
                    return;

                _currentMode = value;

                _sortedColumn = null;
                _sortState = SortState.None;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCpuMode));
                OnPropertyChanged(nameof(IsMemoryMode));

                UpdateProcesses();
                ApplySorting();
            }
        }

        public ICommand CpuCommand { get; }
        public ICommand MemoryCommand { get; }
        public ICommand EndTaskCommand { get; }

        private readonly CpuService _cpuService = new();

        private double _cpuUsed;
        public double CpuUsed
        {
            get => _cpuUsed;
            set
            {
                if(Math.Abs(_cpuUsed - value) < 0.01)
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

        private readonly MemoryService _memoryService = new();

        private string? _sortedColumn;
        private SortState _sortState = SortState.None;

        public ObservableCollection<double> CpuHistory { get; } = new();
        public ObservableCollection<double> MemoryHistory { get; } = new();

        private const int MaxHistoryPoints = 60;

        public MainViewModel()
        {
            EndTaskCommand = new RelayCommand(EndTask);
            CpuCommand = new RelayCommand(() => CurrentMode = Viewmode.Cpu);
            MemoryCommand = new RelayCommand(() => CurrentMode = Viewmode.Memory);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (_, _) => UpdateProcesses();
            _timer.Start();
        }

        private void UpdateProcesses()
        {
            int? selectedId = SelectedProcess?.Id;

            var list = _service.GetProcesses(1)
                               .ToDictionary(p => p.Id);

            for (int i = Processes.Count - 1; i >= 0; i--)
            {
                if (!list.ContainsKey(Processes[i].Id))
                    Processes.RemoveAt(i);
            }

            foreach (var p in list.Values)
            {
                var existing = Processes.FirstOrDefault(x => x.Id == p.Id);

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
                }
            }

            if (selectedId.HasValue)
                SelectedProcess = Processes.FirstOrDefault(p => p.Id == selectedId);

            if(IsCpuMode)
            {
                CpuUsed = _cpuService.CpuUsage();
                AddPoint(CpuHistory, CpuUsed);
            }

            if (IsMemoryMode)
            {
                _memoryService.Update();

                MemoryUsedGB = _memoryService.UsedMemoryGB;
                MemoryTotalGB = _memoryService.TotalMemoryGB;
                AddPoint(MemoryHistory, MemoryUsedGB);
            }

            ApplySorting();
        }

        private void AddPoint(ObservableCollection<double> collection, double value)
        {
            if (collection.Count >= MaxHistoryPoints)
                collection.RemoveAt(0);

            collection.Add(value);
        }

        private void EndTask()
        {
            if (SelectedProcess is null)
                return;

            try
            {
                var process = System.Diagnostics.Process.GetProcessById(SelectedProcess.Id);
                process.Kill();
                process.WaitForExit();
            }
            catch
            {
                // Protected processes or acess denied
            }

            UpdateProcesses();
        }
        public void ApplyColumnSort(string column)
        {
            if (_sortedColumn != column)
            {
                _sortedColumn = column;
                _sortState = SortState.Descending;
            }
            else
            {
                _sortState = _sortState switch
                {
                    SortState.Descending => SortState.Ascending,
                    SortState.Ascending => SortState.None,
                    _ => SortState.Descending
                };

                if (_sortState == SortState.None)
                    _sortedColumn = null;
            }

            ApplySorting();
        }

        private void ApplySorting()
        {
            IEnumerable<ProcessInfo> ordered;

            if (_sortState == SortState.None || _sortedColumn == null)
            {
                ordered = CurrentMode == Viewmode.Cpu
                    ? Processes.OrderByDescending(p => p.Cpu)
                    : Processes.OrderByDescending(p => p.Memory);
            }
            else
            {
                Func<ProcessInfo, object> selector = _sortedColumn switch
                {
                    "Process" => p => p.Name,
                    "CPU (%)" => p => p.Cpu,
                    "CPU Time" => p => p.CpuTime,
                    "Threads" => p => p.ThreadCount,
                    "PID" => p => p.Id,
                    "Memory (MB)" => p => p.Memory,
                    "Handles" => p => p.HandleCount,
                    "Type" => p => p.OwnerType,
                    _ => p => p.Id
                };

                ordered = _sortState == SortState.Ascending
                    ? Processes.OrderBy(selector)
                    : Processes.OrderByDescending(selector);
            }

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