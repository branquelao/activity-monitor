using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ActivityMonitor.ViewModels
{
    public class CpuViewModel : INotifyPropertyChanged
    {
        private double _usage;

        public double Usage
        {
            get => Usage;
            set
            {
                if(_usage != value)
                {
                    _usage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        public string Text => $"CPU: {Usage:F2}%";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
