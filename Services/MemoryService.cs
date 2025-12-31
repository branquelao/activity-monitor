using System;
using System.Runtime.InteropServices;

namespace ActivityMonitor.Services
{
    public class MemoryService
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MEMORYSTATUSEX
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

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        public double MemoryUsedPercent()
        {
            var mem = new MEMORYSTATUSEX();
            mem.dwLength = (uint)Marshal.SizeOf(mem);

            if (!GlobalMemoryStatusEx(ref mem))
                return 0;

            double used = mem.ullTotalPhys - mem.ullAvailPhys;
            return (used / mem.ullTotalPhys) * 100;
        }

        public double MemoryFreePercent()
        {
            var mem = new MEMORYSTATUSEX();
            mem.dwLength = (uint)Marshal.SizeOf(mem);

            if (!GlobalMemoryStatusEx(ref mem))
                return 0;

            return (mem.ullAvailPhys / (double)mem.ullTotalPhys) * 100;
        }
    }
}
