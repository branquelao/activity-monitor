using System;
using System.Diagnostics;

namespace ActivityMonitor.Services
{
    // Provides smoothed total CPU usage
    public class CpuService
    {
        private readonly PerformanceCounter _cpuCounter;
        private double _lastValue;
        private bool _initialized;

        public CpuService()
        {
            _cpuCounter = new PerformanceCounter(
                "Processor",
                "% Processor Time",
                "_Total"
            );

            // Prime counter to avoid first invalid read
            _cpuCounter.NextValue();
        }

        public double CpuUsage()
        {
            double raw = _cpuCounter.NextValue();

            // Skip smoothing on first read
            if (!_initialized)
            {
                _lastValue = raw;
                _initialized = true;
                return Math.Round(raw, 2);
            }

            // Apply simple exponential smoothing
            double smoothed = (_lastValue * 0.7) + (raw * 0.3);
            _lastValue = smoothed;

            return Math.Round(Math.Clamp(smoothed, 0, 100), 2);
        }
    }
}
