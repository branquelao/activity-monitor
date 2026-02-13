using ActivityMonitor.Models;
using System;
using System.Linq;

namespace ActivityMonitor.ViewModels
{
    public partial class MainViewModel
    {
        // Refreshes processes, CPU, memory, disk, and GPU data
        private void UpdateProcesses()
        {
            string? selectedProcessName = SelectedProcess?.Name;
            var rawProcesses = _service.GetProcesses(1);

            // Group processes by name
            var grouped = rawProcesses
                .GroupBy(p => p.Name)
                .Select(g => new GroupedProcessInfo
                {
                    BaseName = g.Key,
                    Name = $"{g.Key} ({g.Count()})",
                    Pids = g.Select(p => p.Id).ToList(),
                    Cpu = g.Sum(p => p.Cpu),
                    Memory = g.Sum(p => p.Memory),
                    CpuTime = TimeSpan.FromTicks(g.Sum(p => p.CpuTime.Ticks)),
                    ThreadCount = g.Sum(p => p.ThreadCount),
                    HandleCount = g.Sum(p => p.HandleCount),
                    DiskReadRate = g.Sum(p => p.DiskReadRate),
                    DiskWriteRate = g.Sum(p => p.DiskWriteRate),
                    GpuUsage = g.Sum(p => p.GpuUsage),
                    GpuEngine = g.FirstOrDefault(p => !string.IsNullOrEmpty(p.GpuEngine))?.GpuEngine ?? "N/A",
                    Icon = g.First().Icon,

                    ExecutionType = g.Any(p => p.ExecutionType == "Application")
                        ? "Application"
                        : "Background"
                })
                .ToList();

            // Remove old entries
            RemoveOldProcesses(grouped);

            // Update or add grouped processes
            UpdateProcessList(grouped);

            // Update mode-specific data
            UpdateModeData(grouped);

            // Apply filtering and sorting
            ApplyFilterAndSorting();

            // Restore selection
            RestoreSelection(selectedProcessName);
        }

        private void RemoveOldProcesses(System.Collections.Generic.List<GroupedProcessInfo> grouped)
        {
            for (int i = Processes.Count - 1; i >= 0; i--)
            {
                if (!grouped.Any(p => p.Name == Processes[i].Name))
                    Processes.RemoveAt(i);
            }
        }

        private void UpdateProcessList(System.Collections.Generic.List<GroupedProcessInfo> grouped)
        {
            foreach (var p in grouped)
            {
                var existing = Processes.FirstOrDefault(x => x.Name == p.Name);

                if (existing == null)
                {
                    Processes.Add(p);
                }
                else
                {
                    existing.Cpu = p.Cpu;
                    existing.Memory = p.Memory;
                    existing.CpuTime = p.CpuTime;
                    existing.ThreadCount = p.ThreadCount;
                    existing.HandleCount = p.HandleCount;
                    existing.DiskReadRate = p.DiskReadRate;
                    existing.DiskWriteRate = p.DiskWriteRate;
                    existing.GpuUsage = p.GpuUsage;
                    existing.GpuEngine = p.GpuEngine;
                    existing.Pids = p.Pids;
                }
            }
        }

        private void UpdateModeData(System.Collections.Generic.List<GroupedProcessInfo> grouped)
        {
            if (IsCpuMode)
            {
                CpuUsed = _cpuService.CpuUsage();
                AddPoint(CpuHistory, CpuUsed);
            }

            if (IsMemoryMode)
            {
                _memoryService.Update();
                MemoryTotalGB = _memoryService.TotalMemoryGB;
                MemoryUsedGB = _memoryService.UsedMemoryGB;
                AddPoint(MemoryHistory, (MemoryUsedGB / MemoryTotalGB) * 100);
            }

            if (IsDiskMode)
            {
                DiskReadTotal = grouped.Sum(p => p.DiskReadRate);
                DiskWriteTotal = grouped.Sum(p => p.DiskWriteRate);
                AddPoint(DiskHistory, DiskReadTotal + DiskWriteTotal);
            }

            if (IsGpuMode)
            {
                GpuUsedTotal = grouped.Sum(p => p.GpuUsage);
                AddPoint(GpuHistory, GpuUsedTotal);
            }
        }

        private void RestoreSelection(string? selectedProcessName)
        {
            if (selectedProcessName != null)
            {
                SelectedProcess = FilteredProcesses
                    .FirstOrDefault(p => p.Name == selectedProcessName);
            }
        }
    }
}