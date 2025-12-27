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

            var list = _service.GetProcesses(1);

            var ordered = _currentMode == Viewmode.Cpu
                ? list.OrderByDescending(p => p.Cpu)
                : list.OrderByDescending(p => p.Memory);

            Processes.Clear();
            ProcessInfo? newSelection = null;

            foreach (var p in ordered)
            {
                Processes.Add(p);

                if (p.Id == selectedId)
                    newSelection = p;
            }
            
            SelectedProcess = newSelection;
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