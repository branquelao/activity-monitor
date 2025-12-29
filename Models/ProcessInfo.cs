using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ActivityMonitor.Models
{
    public class ProcessInfo : INotifyPropertyChanged
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

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

        private string _ownerType = "Application";
        public string OwnerType
        {
            get => _ownerType;
            set
            {
                if (_ownerType != value)
                {
                    _ownerType = value;
                    OnPropertyChanged();
                }
            }
        }
        public TimeSpan PreviousCpuTime { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
