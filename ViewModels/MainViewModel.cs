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
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCpuMode));
                OnPropertyChanged(nameof(IsMemoryMode));

                UpdateProcesses();
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

        private double _memoryUsed;

        public double MemoryUsed
        {
            get => _memoryUsed;
            private set
            {
                if (Math.Abs(_memoryUsed - value) < 0.01)
                    return;

                _memoryUsed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryUsedText));
                OnPropertyChanged(nameof(MemoryFree));
                OnPropertyChanged(nameof(MemoryFreeText));
            }
        }

        public double MemoryFree => 100 - MemoryUsed;
        public string MemoryUsedText => $"{MemoryUsed:F2}%";
        public string MemoryFreeText => $"{MemoryFree:F2}%";

        private readonly MemoryService _memoryService = new();

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

            var ordered = _currentMode == Viewmode.Cpu
                ? Processes.OrderByDescending(p => p.Cpu).ToList()
                : Processes.OrderByDescending(p => p.Memory).ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                var currentIndex = Processes.IndexOf(ordered[i]);
                if (currentIndex != i)
                    Processes.Move(currentIndex, i);
            }

            if (selectedId.HasValue)
                SelectedProcess = Processes.FirstOrDefault(p => p.Id == selectedId);

            if(IsCpuMode)
            {
                CpuUsed = _cpuService.CpuUsage();
            }

            if (IsMemoryMode)
            {
                MemoryUsed = _memoryService.MemoryUsedPercent();
            }
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
    }
}