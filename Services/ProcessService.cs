using ActivityMonitor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Management;

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
                    info.CpuTime = cpuTime;
                    info.ThreadCount = p.Threads.Count;
                    info.HandleCount = p.HandleCount;
                    GetProcessUser(info,p);
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

        private static void GetProcessUser(ProcessInfo info, Process process)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT * FROM Win32_Process WHERE ProcessId = {process.Id}");

                foreach (ManagementObject obj in searcher.Get())
                {
                    var ownerInfo = new string[2];
                    var result = (uint)obj.InvokeMethod("GetOwner", ownerInfo);

                    if (result == 0)
                    {
                        var user = ownerInfo[0];
                        var domain = ownerInfo[1];

                        info.User = $"{domain}\\{user}";

                        info.OwnerType =
                            user.Equals("SYSTEM", StringComparison.OrdinalIgnoreCase) ||
                            domain.Equals("NT AUTHORITY", StringComparison.OrdinalIgnoreCase)
                                ? "System"
                                : "User";

                        return;
                    }
                }

                info.OwnerType = "System";
                info.User = "SYSTEM";
            }
            catch
            {
                info.OwnerType = "System";
                info.User = "SYSTEM";
            }
        }
    }
}
