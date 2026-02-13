using Microsoft.UI.Xaml.Media.Imaging;
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

        // Disk I/O rates in MB/s
        private long _diskReadBytes;
        public long DiskReadBytes
        {
            get => _diskReadBytes;
            set
            {
                if (_diskReadBytes != value)
                {
                    _diskReadBytes = value;
                    OnPropertyChanged();
                }
            }
        }

        private long _diskWriteBytes;
        public long DiskWriteBytes
        {
            get => _diskWriteBytes;
            set
            {
                if (_diskWriteBytes != value)
                {
                    _diskWriteBytes = value;
                    OnPropertyChanged();
                }
            }
        }

        // For calculating rate (delta)
        public long PreviousDiskReadBytes { get; set; }
        public long PreviousDiskWriteBytes { get; set; }

        // Rate in MB/s
        private double _diskReadRate;
        public double DiskReadRate
        {
            get => _diskReadRate;
            set
            {
                if (Math.Abs(_diskReadRate - value) > 0.01)
                {
                    _diskReadRate = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _diskWriteRate;
        public double DiskWriteRate
        {
            get => _diskWriteRate;
            set
            {
                if (Math.Abs(_diskWriteRate - value) > 0.01)
                {
                    _diskWriteRate = value;
                    OnPropertyChanged();
                }
            }
        }

        public double TotalDiskIO => DiskReadRate + DiskWriteRate;

        // GPU usage percentage
        private double _gpuUsage;
        public double GpuUsage
        {
            get => _gpuUsage;
            set
            {
                if (Math.Abs(_gpuUsage - value) > 0.01)
                {
                    _gpuUsage = value;
                    OnPropertyChanged();
                }
            }
        }

        // GPU Engine (which GPU engine is being used)
        private string _gpuEngine = "";
        public string GpuEngine
        {
            get => _gpuEngine;
            set
            {
                if (_gpuEngine != value)
                {
                    _gpuEngine = value;
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

        // Process icon
        private BitmapImage? _icon;
        public BitmapImage? Icon
        {
            get => _icon;
            set
            {
                if (_icon == value)
                    return;

                _icon = value;
                OnPropertyChanged();
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
