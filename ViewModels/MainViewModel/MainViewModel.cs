using ActivityMonitor.Models;
using ActivityMonitor.Services;
using ActivityMonitor.ViewModels.Base;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ActivityMonitor.ViewModels.Commands;

namespace ActivityMonitor.ViewModels
{
    // Defines the current visualization mode
    public enum Viewmode
    {
        Cpu,
        Memory,
        Disk,
        Gpu
    }

    // Represents the current sorting state
    public enum SortState
    {
        None,
        Ascending,
        Descending
    }

    public partial class MainViewModel : ViewModelBase
    {
        // Services
        private readonly ProcessService _service = new();
        private readonly CpuService _cpuService = new();
        private readonly MemoryService _memoryService = new();

        // UI refresh timer
        private readonly DispatcherTimer _timer;

        // Constants
        private const int MaxHistoryPoints = 60;

        // Collections
        public ObservableCollection<GroupedProcessInfo> Processes { get; } = new();
        public ObservableCollection<GroupedProcessInfo> FilteredProcesses { get; } = new();
        public ObservableCollection<double> CpuHistory { get; } = new();
        public ObservableCollection<double> MemoryHistory { get; } = new();
        public ObservableCollection<double> DiskHistory { get; } = new();
        public ObservableCollection<double> GpuHistory { get; } = new();

        // Commands
        public ICommand CpuCommand { get; }
        public ICommand MemoryCommand { get; }
        public ICommand DiskCommand { get; }
        public ICommand GpuCommand { get; }
        public ICommand EndTaskCommand { get; }

        public MainViewModel()
        {
            // Initialize commands
            EndTaskCommand = new RelayCommand(EndTask);
            CpuCommand = new RelayCommand(() => CurrentMode = Viewmode.Cpu);
            MemoryCommand = new RelayCommand(() => CurrentMode = Viewmode.Memory);
            DiskCommand = new RelayCommand(() => CurrentMode = Viewmode.Disk);
            GpuCommand = new RelayCommand(() => CurrentMode = Viewmode.Gpu);

            // Setup update timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (_, _) => UpdateProcesses();
            _timer.Start();
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

        // Adds a rolling value to a graph history
        private void AddPoint(ObservableCollection<double> collection, double value)
        {
            if (collection.Count >= MaxHistoryPoints)
                collection.RemoveAt(0);

            collection.Add(value);
        }
    }
}