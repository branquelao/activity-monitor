using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ActivityMonitor.Models
{
    // Holds runtime information about a system process
    public class ProcessInfo : INotifyPropertyChanged
    {
        // Process ID
        public int Id { get; set; }

        // Process name
        public string Name { get; set; } = string.Empty;

        // CPU usage percentage
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

        // Memory usage in MB
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

        // Total CPU time used by the process
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

        // Number of active threads
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

        // Number of open handles
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

        // User running the process
        private string _user = "-";
        public string User
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged();
                }
            }
        }

        // Indicates whether the process runs in foreground or background
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

        // Used for CPU delta calculation
        public TimeSpan PreviousCpuTime { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        // Notifies UI about property changes
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
