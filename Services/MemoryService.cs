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

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        private MEMORYSTATUSEX _mem;

        public MemoryService()
        {
            _mem = new MEMORYSTATUSEX
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX))
            };
        }

        public void Update()
        {
            GlobalMemoryStatusEx(ref _mem);
        }

        public double TotalMemoryGB =>
            _mem.ullTotalPhys / 1024.0 / 1024 / 1024;

        public double UsedMemoryGB =>
            (_mem.ullTotalPhys - _mem.ullAvailPhys)
            / 1024.0 / 1024 / 1024;
    }
}
