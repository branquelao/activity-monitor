using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ActivityMonitor.Services
{
    public class CpuInfo
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetSystemTimes(
            out FILETIME idleTime,
            out FILETIME kernelTime,
            out FILETIME userTime);

        struct FILETIME
        {
            public uint Low;
            public uint High;
        }

        private ulong _lastIdle, _lastKernel, _lastUser;

        public double GetCpuUsage()
        {
            GetSystemTimes(out var idle, out var kernel, out var user);

            ulong idleNow = ToUInt64(idle);
            ulong kernelNow = ToUInt64(kernel);
            ulong userNow = ToUInt64(user);

            ulong idleDelta = idleNow - _lastIdle;
            ulong kernelDelta = kernelNow - _lastKernel;
            ulong userDelta = userNow - _lastUser;

            _lastIdle = idleNow;
            _lastKernel = kernelNow;
            _lastUser = userNow;

            ulong total = kernelDelta + userDelta;
            double usage = total == 0
                ? 0
                : (1.0 - ((double)idleDelta / total)) * 100.0;

            return Math.Round(usage, 2);
        }

        static ulong ToUInt64(FILETIME time)
            => ((ulong)time.High << 32) | time.Low;
    }
}