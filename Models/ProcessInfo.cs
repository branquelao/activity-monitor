using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ActivityMonitor.Models
{
    public class ProcessInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Cpu { get; set; }
        public double Memory { get; set; }
        public TimeSpan PreviousCpuTime { get; set; }
    }

}
