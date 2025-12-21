using ActivityMonitor.Models;
using ActivityMonitor.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace ActivityMonitor.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public CpuViewModel Cpu { get; } = new();
    public MemoryViewModel Memory { get; } = new();
    public ObservableCollection<ProcessInfo> Processes { get; } = new();

    private readonly DispatcherQueueTimer _timer;
    private readonly ProcessCpuTracker _cpuTracker = new();
    private readonly CpuInfo _cpuInfo = new();
    private readonly MemoryInfo _memoryInfo = new();

    public MainViewModel()
    {
        _timer = DispatcherQueue
            .GetForCurrentThread()
            .CreateTimer();

        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += (_, _) => Update();
        _timer.Start();
    }

    private void Update()
    {
        UpdateCpu();
        UpdateMemory();
        UpdateProcesses();

        OnPropertyChanged(nameof(IsCpuHigh));
        OnPropertyChanged(nameof(IsMemoryHigh));
    }

    private void UpdateCpu()
    {
        Cpu.Usage = _cpuInfo.GetCpuUsage();
    }

    private void UpdateMemory()
    {
        Memory.Update(_memoryInfo.GetStatus());
    }

    public void UpdateProcesses()
    {
        var running = Process.GetProcesses();

        foreach (var p in Processes.Where(p => running.All(r => r.Id != p.PID)).ToList())
            Processes.Remove(p);

        foreach (var p in running)
        {
            try
            {
                double cpu = _cpuTracker.GetCpu(p);
                double memory = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 2);

                var existing = Processes.FirstOrDefault(x => x.PID == p.Id);

                if (existing == null)
                {
                    Processes.Add(new ProcessInfo
                    {
                        PID = p.Id,
                        Name = p.ProcessName,
                        CpuUsage = cpu,
                        MemoryUsage = memory
                    });
                }
                else
                {
                    existing.CpuUsage = cpu;
                    existing.MemoryUsage = memory;
                }
            }
            catch { }
        }

        SortProcesses();
    }

    private void SortProcesses()
    {
        var sorted = Processes
            .OrderByDescending(p => p.CpuUsage)
            .ThenByDescending(p => p.MemoryUsage)
            .ThenBy(p => p.Name)
            .ToList();

        Processes.Clear();
        foreach (var p in sorted)
            Processes.Add(p);
    }

    public bool IsCpuHigh => Cpu.Usage >= 80;
    public bool IsMemoryHigh => Memory.Percent >= 80;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
