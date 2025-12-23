using ActivityMonitor.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ActivityMonitor.ViewModels
{
    public class MemoryViewModel : INotifyPropertyChanged
    {
        private MemoryStatus _status = new(0, 0, 0);

        public double Percent => _status.Percent;
        public ulong Total => _status.Total;
        public ulong Used => _status.Used;

        public string Text => $"Memory: " +
            $"{Used / 1024 / 1024:F0} / {Total / 1024 / 1024:F0} MB)";
    
        public bool IsHigh => Percent >= 80;

        public void Update(MemoryStatus status)
        {
            _status = status;
            OnPropertyChanged(nameof(Percent));
            OnPropertyChanged(nameof(Text));
            OnPropertyChanged(nameof(IsHigh));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
