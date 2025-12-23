using ActivityMonitor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityMonitor.Services
{
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

                    if (!_cache.TryGetValue(p.Id, out var info))
                    {
                        info = new ProcessInfo
                        {
                            Id = p.Id,
                            Name = p.ProcessName,
                            PreviousCpuTime = cpuTime
                        };
                        _cache[p.Id] = info;
                    }

                    var deltaCpu = cpuTime - info.PreviousCpuTime;

                    info.Cpu = Math.Round(
                        (deltaCpu.TotalMilliseconds /
                         (intervalSeconds * 1000 * _processorCount)) * 100,
                        2);

                    info.Memory = memory;
                    info.PreviousCpuTime = cpuTime;

                    result.Add(info);
                }
                catch
                {
                    // protected processes
                }
            }

            return result;
        }
    }
}
