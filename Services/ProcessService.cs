using ActivityMonitor.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Storage.Streams;

namespace ActivityMonitor.Services
{
    // Collects and updates running process information
    public class ProcessService
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(
            IntPtr hProcess,
            int dwFlags,
            [Out] System.Text.StringBuilder lpExeName,
            ref int lpdwSize);

        // ADD THIS: Win32 API for I/O counters
        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public long ReadTransferCount;
            public long WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetProcessIoCounters(
            IntPtr processHandle,
            out IO_COUNTERS ioCounters);

        private readonly Dictionary<int, ProcessInfo> _cache = new();
        private readonly int _processorCount = Environment.ProcessorCount;
        private readonly GpuService _gpuService = new();

        public List<ProcessInfo> GetProcesses(double intervalSeconds)
        {
            var result = new List<ProcessInfo>();
            var processes = Process.GetProcesses();

            foreach (var p in processes)
            {
                try
                {
                    var cpuTime = p.TotalProcessorTime;
                    var memory = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 2);

                    // Get disk I/O counters
                    long diskReadBytes = 0;
                    long diskWriteBytes = 0;

                    try
                    {
                        if (GetProcessIoCounters(p.Handle, out IO_COUNTERS counters))
                        {
                            diskReadBytes = counters.ReadTransferCount;
                            diskWriteBytes = counters.WriteTransferCount;
                        }
                    }
                    catch
                    {
                        // Access denied or not available
                    }

                    // Get GPU Usage
                    double gpuUsage = _gpuService.GetProcessGpuUsage(p.Id);
                    string gpuEngine = _gpuService.GetProcessGpuEngine(p.Id);

                    // Create cache entry if missing
                    if (!_cache.TryGetValue(p.Id, out var info))
                    {
                        info = new ProcessInfo
                        {
                            Id = p.Id,
                            Name = GetFriendlyProcessName(p),
                            Icon = GetProcessIcon(p),
                            PreviousCpuTime = cpuTime,
                            PreviousDiskReadBytes = diskReadBytes,   
                            PreviousDiskWriteBytes = diskWriteBytes  
                        };
                        ClassifyProcess(info, p);
                        _cache[p.Id] = info;
                    }

                    // Calculate CPU delta
                    var deltaCpu = cpuTime - info.PreviousCpuTime;
                    info.Cpu = Math.Round(
                        (deltaCpu.TotalMilliseconds /
                        (intervalSeconds * 1000 * _processorCount)) * 100,
                        2);

                    // Calculate Disk I/O rate (bytes per second -> MB/s)
                    var deltaRead = diskReadBytes - info.PreviousDiskReadBytes;
                    var deltaWrite = diskWriteBytes - info.PreviousDiskWriteBytes;

                    info.DiskReadRate = Math.Round((deltaRead / intervalSeconds) / (1024.0 * 1024.0), 2);
                    info.DiskWriteRate = Math.Round((deltaWrite / intervalSeconds) / (1024.0 * 1024.0), 2);

                    info.GpuUsage = Math.Round(gpuUsage, 1);
                    info.GpuEngine = gpuEngine;

                    info.Memory = memory;
                    info.CpuTime = cpuTime;
                    info.ThreadCount = p.Threads.Count;
                    info.HandleCount = p.HandleCount;

                    // Update disk bytes
                    info.DiskReadBytes = diskReadBytes;
                    info.DiskWriteBytes = diskWriteBytes;

                    info.PreviousCpuTime = cpuTime;
                    info.PreviousDiskReadBytes = diskReadBytes;   
                    info.PreviousDiskWriteBytes = diskWriteBytes;

                    result.Add(info);
                }
                catch
                {
                    // Ignore protected or inaccessible processes
                }
            }

            return result;
        }

        // Gets the friendly display name from the executable metadata
        private string GetFriendlyProcessName(Process process)
        {
            try
            {
                // Try to get the file description
                if (!string.IsNullOrEmpty(process.MainModule?.FileName))
                {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(process.MainModule.FileName);

                    // Use FileDescription if available, otherwise use ProductName
                    if (!string.IsNullOrEmpty(fileVersionInfo.FileDescription))
                        return fileVersionInfo.FileDescription;

                    if (!string.IsNullOrEmpty(fileVersionInfo.ProductName))
                        return fileVersionInfo.ProductName;
                }
            }
            catch
            {
                // Access denied or protected process
            }

            // Fallback: use the process name
            return process.ProcessName;
        }

        // Assigns execution type
        private static void ClassifyProcess(ProcessInfo info, Process process)
        {
            // Kernel process
            if (process.Id == 4)
            {
                info.ExecutionType = "Background";
                return;
            }

            // Well-known system and service processes
            if (process.ProcessName.Equals("svchost", StringComparison.OrdinalIgnoreCase) ||
                process.ProcessName.Equals("services", StringComparison.OrdinalIgnoreCase) ||
                process.ProcessName.Equals("lsass", StringComparison.OrdinalIgnoreCase))
            {
                info.ExecutionType = "Background";
                return;
            }

            // Processes with a visible main window are considered applications
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                info.ExecutionType = "Application";
                return;
            }

            // Default fallback
            info.ExecutionType = "Background";
        }

        // Helper Method for Icon Extraction
        private string? GetProcessFilePath(Process process)
        {
            try
            {
                // First Attempt: MainModule.FileName
                if (!string.IsNullOrEmpty(process.MainModule?.FileName))
                    return process.MainModule.FileName;
            }
            catch { }

            try
            {
                // Second Attempt: QueryFullProcessImageName
                var buffer = new System.Text.StringBuilder(1024);
                int size = buffer.Capacity;

                if (QueryFullProcessImageName(process.Handle, 0, buffer, ref size))
                {
                    return buffer.ToString();
                }
            }
            catch { }

            return null;
        }

        // Extracts the icon from the process executable
        private BitmapImage? GetProcessIcon(Process process)
        {
            try
            {
                string? filePath = GetProcessFilePath(process);

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    // Extract the icon
                    using var icon = Icon.ExtractAssociatedIcon(filePath);

                    if (icon != null)
                    {
                        using var bitmap = icon.ToBitmap();
                        using var memory = new MemoryStream();

                        bitmap.Save(memory, ImageFormat.Png);
                        memory.Position = 0;

                        var bitmapImage = new BitmapImage();
                        var raStream = memory.AsRandomAccessStream();
                        bitmapImage.SetSource(raStream);

                        return bitmapImage;
                    }
                }
            }
            catch
            {
                // Ignore extraction failures
            }

            return null;
        }

        // Gets detailed information about a specific process
        public ProcessDetails? GetProcessDetails(int processId)
        {
            try
            {
                var process = System.Diagnostics.Process.GetProcessById(processId);

                var details = new ProcessDetails
                {
                    Name = GetFriendlyProcessName(process),
                    ProcessId = processId,
                    StartTime = process.StartTime,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    CpuUsage = 0, // Will be updated from cache if available
                    MemoryMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2)
                };

                // Try to get executable path and related info
                try
                {
                    string? filePath = GetProcessFilePath(process);

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        details.ExecutablePath = filePath;
                        details.WorkingDirectory = System.IO.Path.GetDirectoryName(filePath) ?? "";

                        // Get file version info
                        var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(filePath);
                        details.FileVersion = versionInfo.FileVersion ?? "";
                        details.ProductName = versionInfo.ProductName ?? "";
                        details.Company = versionInfo.CompanyName ?? "";
                        details.Description = versionInfo.FileDescription ?? "";
                    }
                }
                catch
                {
                    // Access denied or unavailable
                    details.ExecutablePath = "Access Denied";
                }

                // Try to get command line
                try
                {
                    details.CommandLine = GetProcessCommandLine(processId);
                }
                catch
                {
                    details.CommandLine = "Access Denied";
                }

                // Try to get username
                try
                {
                    details.UserName = GetProcessOwner(process);
                }
                catch
                {
                    details.UserName = "Unknown";
                }

                // Get CPU from cache if available
                if (_cache.TryGetValue(processId, out var cachedInfo))
                {
                    details.CpuUsage = cachedInfo.Cpu;
                }

                return details;
            }
            catch
            {
                return null;
            }
        }

        // Helper: Get command line arguments
        private string GetProcessCommandLine(int processId)
        {
            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}");

                foreach (System.Management.ManagementObject obj in searcher.Get())
                {
                    return obj["CommandLine"]?.ToString() ?? "";
                }
            }
            catch { }

            return "";
        }

        // Helper: Get process owner
        private string GetProcessOwner(System.Diagnostics.Process process)
        {
            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    $"SELECT * FROM Win32_Process WHERE ProcessId = {process.Id}");

                foreach (System.Management.ManagementObject obj in searcher.Get())
                {
                    string[] owner = new string[2];
                    obj.InvokeMethod("GetOwner", owner);
                    return $"{owner[1]}\\{owner[0]}"; // Domain\Username
                }
            }
            catch { }

            return Environment.UserName;
        }
    }
}