using ActivityMonitor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ActivityMonitor.Services
{
    // Collects and updates running process information
    public class ProcessService
    {
        private readonly Dictionary<int, ProcessInfo> _cache = new();
        private readonly int _processorCount = Environment.ProcessorCount;

        public List<ProcessInfo> GetProcesses(double intervalSeconds)
        {
            var result = new List<ProcessInfo>();
            var processes = Process.GetProcesses();

            foreach (var p in processes)
            {
                try
                {
                    var cpuTime = p.TotalProcessorTime;
                    var memory = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 2);

                    // Create cache entry if missing
                    if (!_cache.TryGetValue(p.Id, out var info))
                    {
                        info = new ProcessInfo
                        {
                            Id = p.Id,
                            Name = p.ProcessName,
                            PreviousCpuTime = cpuTime
                        };

                        ClassifyProcess(info, p);
                        _cache[p.Id] = info;
                    }

                    // Calculate CPU delta
                    var deltaCpu = cpuTime - info.PreviousCpuTime;

                    info.Cpu = Math.Round(
                        (deltaCpu.TotalMilliseconds /
                        (intervalSeconds * 1000 * _processorCount)) * 100,
                        2);

                    info.Memory = memory;
                    info.CpuTime = cpuTime;
                    info.ThreadCount = p.Threads.Count;
                    info.HandleCount = p.HandleCount;
                    info.PreviousCpuTime = cpuTime;

                    result.Add(info);
                }
                catch
                {
                    // Ignore protected or inaccessible processes
                }
            }

            return result;
        }

        // Assigns process type and user
        private static void ClassifyProcess(ProcessInfo info, Process process)
        {
            if (process.Id == 4)
            {
                info.OwnerType = "Kernel";
                info.User = "SYSTEM";
                return;
            }

            if (process.ProcessName.Equals("svchost", StringComparison.OrdinalIgnoreCase) ||
                process.ProcessName.Equals("services", StringComparison.OrdinalIgnoreCase) ||
                process.ProcessName.Equals("lsass", StringComparison.OrdinalIgnoreCase))
            {
                info.OwnerType = "Service";
                info.User = "SYSTEM";
                return;
            }

            info.OwnerType = "Application";
            info.User = Environment.UserName;
        }
    }
}
