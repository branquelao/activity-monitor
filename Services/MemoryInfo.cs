using ActivityMonitor.Models;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ActivityMonitor.Services
{
    public class MemoryInfo
    {
        [StructLayout(LayoutKind.Sequential)]
        struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll")]
        static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        public MemoryStatus GetStatus()
        {
            var mem = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
            GlobalMemoryStatusEx(ref mem);

            ulong used = mem.ullTotalPhys - mem.ullAvailPhys;
            double percent = (double)used / mem.ullTotalPhys * 100;

            return new MemoryStatus(mem.ullTotalPhys, used, percent);
        }
    }
}
