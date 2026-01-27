using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ActivityMonitor.Models
{
    // Represents a grouped process (multiple PIDs under the same name)
    public class GroupedProcessInfo : INotifyPropertyChanged
    {
        // Process name
        public string Name { get; set; } = string.Empty;

        // List of process IDs that belong to this group
        public List<int> Pids { get; set; } = new();

        // Identifier key for the grouped process
        public string ProcessKey { get; set; } = string.Empty;

        // CPU usage percentage (summed)
        private double _cpu;
        public double Cpu
        {
            get => _cpu;
            set
            {
                if (_cpu != value)
                {
                    _cpu = value;
                    OnPropertyChanged();
                }
            }
        }

        // Memory usage in MB (summed)
        private double _memory;
        public double Memory
        {
            get => _memory;
            set
            {
                if (_memory != value)
                {
                    _memory = value;
                    OnPropertyChanged();
                }
            }
        }

        // Total CPU time used by all processes
        private TimeSpan _cpuTime;
        public TimeSpan CpuTime
        {
            get => _cpuTime;
            set
            {
                if (_cpuTime != value)
                {
                    _cpuTime = value;
                    OnPropertyChanged();
                }
            } 
        }

        // Total number of active threads
        private int _threadCount;
        public int ThreadCount
        {
            get => _threadCount;
            set
            {
                if (_threadCount != value)
                {
                    _threadCount = value;
                    OnPropertyChanged();
                }
            }
        }

        // Total number of open handles
        private long _handleCount;
        public long HandleCount
        {
            get => _handleCount;
            set
            {
                if (_handleCount != value)
                {
                    _handleCount = value;
                    OnPropertyChanged();
                }
            }
        }

        // Indicates whether the grouped process is application or background
        private string _executionType = "Background";
        public string ExecutionType
        {
            get => _executionType;
            set
            {
                if (_executionType != value)
                {
                    _executionType = value;
                    OnPropertyChanged();
                }
            }
        }

        // Number of processes in this group
        public int ProcessCount => Pids.Count;

        public event PropertyChangedEventHandler? PropertyChanged;

        // Notifies UI about property changes
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
