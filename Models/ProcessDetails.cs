using System;

namespace ActivityMonitor.Models
{
    // Holds detailed information about a process
    public class ProcessDetails
    {
        public string Name { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public string ExecutablePath { get; set; } = string.Empty;
        public string CommandLine { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string FileVersion { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public int ThreadCount { get; set; }
        public long HandleCount { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryMB { get; set; }
    }
}