using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ActivityMonitor.Models
{
    public class ProcessInfo : INotifyPropertyChanged
    {
        private int _pid;
        private string _name;
        private double _cpuUsage;
        private double _memoryUsage;

        public int PID
        {
            get => _pid;
            set { _pid = value; OnPropertyChanged(nameof(PID)); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public double CpuUsage
        {
            get => _cpuUsage;
            set { _cpuUsage = value; OnPropertyChanged(nameof(CpuUsage)); }
        }

        public double MemoryUsage
        {
            get => _memoryUsage;
            set { _memoryUsage = value; OnPropertyChanged(nameof(MemoryUsage)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
