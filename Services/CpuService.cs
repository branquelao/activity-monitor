using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ActivityMonitor.Services
{
    public class CpuService
    {
        private readonly PerformanceCounter _cpuCounter;

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
            return Math.Round(_cpuCounter.NextValue(), 2);
        }
    }
}
