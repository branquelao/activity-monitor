using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ActivityMonitor.Services
{
    // Monitors GPU usage using Performance Counters
    public class GpuService
    {
        private readonly Dictionary<int, GpuProcessInfo> _gpuCache = new();
        private PerformanceCounter? _totalGpuCounter;
        private List<PerformanceCounter> _engineCounters = new();

        public GpuService()
        {
            try
            {
                InitializeCounters();
            }
            catch
            {
                // GPU counters not available (no dedicated GPU or driver issue)
            }
        }

        private void InitializeCounters()
        {
            try
            {
                // Try to get GPU performance counters
                var category = new PerformanceCounterCategory("GPU Engine");
                var instanceNames = category.GetInstanceNames();

                // Get all GPU engine counters
                foreach (var instance in instanceNames)
                {
                    try
                    {
                        var counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instance);
                        counter.NextValue(); // Initialize
                        _engineCounters.Add(counter);
                    }
                    catch
                    {
                        // Skip counters that can't be initialized
                    }
                }
            }
            catch
            {
                // GPU counters not available
            }
        }

        // Get GPU usage for a specific process
        public double GetProcessGpuUsage(int processId)
        {
            if (_engineCounters.Count == 0)
                return 0;

            try
            {
                double totalUsage = 0;
                int validCounters = 0;

                foreach (var counter in _engineCounters)
                {
                    try
                    {
                        // Check if this counter belongs to the process
                        string instanceName = counter.InstanceName;

                        // Instance name format: "pid_XXXXX_luid_0x..."
                        if (instanceName.Contains($"pid_{processId}_"))
                        {
                            float value = counter.NextValue();
                            totalUsage += value;
                            validCounters++;
                        }
                    }
                    catch
                    {
                        // Skip invalid counters
                    }
                }

                return validCounters > 0 ? totalUsage / validCounters : 0;
            }
            catch
            {
                return 0;
            }
        }

        // Get total GPU usage across all processes
        public double GetTotalGpuUsage()
        {
            if (_engineCounters.Count == 0)
                return 0;

            try
            {
                double totalUsage = 0;
                int validCounters = 0;

                foreach (var counter in _engineCounters)
                {
                    try
                    {
                        float value = counter.NextValue();
                        totalUsage += value;
                        validCounters++;
                    }
                    catch
                    {
                        // Skip invalid counters
                    }
                }

                return validCounters > 0 ? Math.Min(100, totalUsage / validCounters) : 0;
            }
            catch
            {
                return 0;
            }
        }

        // Get GPU engine name for a process
        public string GetProcessGpuEngine(int processId)
        {
            if (_engineCounters.Count == 0)
                return "N/A";

            try
            {
                foreach (var counter in _engineCounters)
                {
                    string instanceName = counter.InstanceName;

                    if (instanceName.Contains($"pid_{processId}_"))
                    {
                        // Extract engine type from instance name
                        // Format: "pid_XXXXX_luid_0x..._phys_0_eng_X_engtype_YYYY"
                        if (instanceName.Contains("engtype_"))
                        {
                            var parts = instanceName.Split('_');
                            var engineIndex = Array.IndexOf(parts, "engtype");
                            if (engineIndex >= 0 && engineIndex + 1 < parts.Length)
                            {
                                string engineType = parts[engineIndex + 1];
                                return GetEngineName(engineType);
                            }
                        }

                        return "GPU";
                    }
                }
            }
            catch
            {
                // Error getting engine name
            }

            return "N/A";
        }

        private string GetEngineName(string engineType)
        {
            // Map engine type numbers to names
            return engineType switch
            {
                "0" => "3D",
                "1" => "Video Decode",
                "2" => "Video Encode",
                "3" => "Video Processing",
                "4" => "Scene Assembly",
                _ => "GPU"
            };
        }

        public void Dispose()
        {
            _totalGpuCounter?.Dispose();
            foreach (var counter in _engineCounters)
            {
                counter?.Dispose();
            }
            _engineCounters.Clear();
        }
    }

    internal class GpuProcessInfo
    {
        public double Usage { get; set; }
        public string Engine { get; set; } = "";
    }
}