using ActivityMonitor.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

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

        private readonly Dictionary<int, ProcessInfo> _cache = new();
        private readonly int _processorCount = Environment.ProcessorCount;

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

                    // Create cache entry if missing
                    if (!_cache.TryGetValue(p.Id, out var info))
                    {
                        info = new ProcessInfo
                        {
                            Id = p.Id,
                            Name = GetFriendlyProcessName(p),
                            Icon = GetProcessIcon(p),
                            PreviousCpuTime = cpuTime
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

                    info.Memory = memory;
                    info.CpuTime = cpuTime;
                    info.ThreadCount = p.Threads.Count;
                    info.HandleCount = p.HandleCount;
                    info.PreviousCpuTime = cpuTime;

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

        // Assigns execution type and user
        private static void ClassifyProcess(ProcessInfo info, Process process)
        {
            // Kernel process
            if (process.Id == 4)
            {
                info.ExecutionType = "Background";
                info.User = "SYSTEM";
                return;
            }

            // Well-known system and service processes
            if (process.ProcessName.Equals("svchost", StringComparison.OrdinalIgnoreCase) ||
                process.ProcessName.Equals("services", StringComparison.OrdinalIgnoreCase) ||
                process.ProcessName.Equals("lsass", StringComparison.OrdinalIgnoreCase))
            {
                info.ExecutionType = "Background";
                info.User = "SYSTEM";
                return;
            }

            // Processes with a visible main window are considered applications
            if (process.MainWindowHandle != IntPtr.Zero)
            {
                info.ExecutionType = "Application";
                info.User = Environment.UserName;
                return;
            }

            // Default fallback
            info.ExecutionType = "Background";
            info.User = Environment.UserName;
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
    }
}