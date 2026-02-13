using ActivityMonitor.Models;
using System;

namespace ActivityMonitor.ViewModels
{
    public partial class MainViewModel
    {
        #region View Mode

        private Viewmode _currentMode = Viewmode.Cpu;
        public Viewmode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (_currentMode == value)
                    return;

                _currentMode = value;

                // Reset sorting on mode change
                _sortedColumn = "Process";
                _sortState = SortState.Ascending;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCpuMode));
                OnPropertyChanged(nameof(IsMemoryMode));
                OnPropertyChanged(nameof(IsDiskMode));
                OnPropertyChanged(nameof(IsGpuMode));

                UpdateProcesses();
                ApplySorting();
            }
        }

        public bool IsCpuMode => CurrentMode == Viewmode.Cpu;
        public bool IsMemoryMode => CurrentMode == Viewmode.Memory;
        public bool IsDiskMode => CurrentMode == Viewmode.Disk;
        public bool IsGpuMode => CurrentMode == Viewmode.Gpu;

        #endregion

        #region Selected Process

        private GroupedProcessInfo? _selectedProcess;
        public GroupedProcessInfo? SelectedProcess
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

        #endregion

        #region Search

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                    return;

                _searchText = value;
                OnPropertyChanged();
                ApplyFilterAndSorting();
            }
        }

        #endregion

        #region CPU Properties

        private double _cpuUsed;
        public double CpuUsed
        {
            get => _cpuUsed;
            set
            {
                if (Math.Abs(_cpuUsed - value) < 0.01)
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

        #endregion

        #region Memory Properties

        private double _memoryUsedGB;
        private double _memoryTotalGB;

        public double MemoryUsedGB
        {
            get => _memoryUsedGB;
            private set
            {
                if (Math.Abs(_memoryUsedGB - value) < 0.01)
                    return;

                _memoryUsedGB = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryUsedText));
            }
        }

        public double MemoryTotalGB
        {
            get => _memoryTotalGB;
            private set
            {
                if (Math.Abs(_memoryTotalGB - value) < 0.01)
                    return;

                _memoryTotalGB = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryTotalText));
            }
        }

        public string MemoryUsedText => $"{MemoryUsedGB:F1} GB";
        public string MemoryTotalText => $"{MemoryTotalGB:F1} GB";

        #endregion

        #region Disk Properties

        private double _diskReadTotal;
        private double _diskWriteTotal;

        public double DiskReadTotal
        {
            get => _diskReadTotal;
            set
            {
                if (Math.Abs(_diskReadTotal - value) < 0.01)
                    return;

                _diskReadTotal = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DiskReadTotalText));
            }
        }

        public double DiskWriteTotal
        {
            get => _diskWriteTotal;
            set
            {
                if (Math.Abs(_diskWriteTotal - value) < 0.01)
                    return;

                _diskWriteTotal = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DiskWriteTotalText));
            }
        }

        public string DiskReadTotalText => $"{DiskReadTotal:F2} MB/s";
        public string DiskWriteTotalText => $"{DiskWriteTotal:F2} MB/s";

        #endregion

        #region GPU Properties

        private double _gpuUsedTotal;

        public double GpuUsedTotal
        {
            get => _gpuUsedTotal;
            set
            {
                if (Math.Abs(_gpuUsedTotal - value) < 0.01)
                    return;

                _gpuUsedTotal = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GpuUsedTotalText));
                OnPropertyChanged(nameof(GpuFree));
                OnPropertyChanged(nameof(GpuFreeText));
            }
        }

        public double GpuFree => 100 - GpuUsedTotal;
        public string GpuUsedTotalText => $"{GpuUsedTotal:F1}%";
        public string GpuFreeText => $"{GpuFree:F1}%";

        #endregion
    }
}