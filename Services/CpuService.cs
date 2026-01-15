using System;
using System.Diagnostics;

namespace ActivityMonitor.Services
{
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

            _cpuCounter.NextValue();
        }

        public double CpuUsage()
        {
            double raw = _cpuCounter.NextValue();

            if (!_initialized)
            {
                _lastValue = raw;
                _initialized = true;
                return Math.Round(raw, 2);
            }

            double smoothed = (_lastValue * 0.7) + (raw * 0.3);
            _lastValue = smoothed;

            return Math.Round(Math.Clamp(smoothed, 0, 100), 2);
        }
    }
}
