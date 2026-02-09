using ActivityMonitor.Models;
using ActivityMonitor.Services;
using Microsoft.UI.Xaml;
using System;
using System.Globalization;

namespace ActivityMonitor.Views
{
    public sealed partial class ProcessDetailsWindow : Window
    {
        public ProcessDetailsWindow(GroupedProcessInfo processInfo)
        {
            this.InitializeComponent();

            // Set window size
            SetWindowSize(600, 800);

            // Bring Window to front
            BringToFront();

            LoadProcessDetails(processInfo);
        }

        private void SetWindowSize(int width, int height)
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
        }

        private void LoadProcessDetails(GroupedProcessInfo processInfo)
        {
            // Get the first PID from the group
            if (processInfo.Pids.Count == 0)
                return;

            int pid = processInfo.Pids[0];

            var service = new ProcessService();
            var details = service.GetProcessDetails(pid);

            if (details == null)
            {
                ProcessNameText.Text = "Process not found or access denied";
                return;
            }

            // Populate UI
            ProcessNameText.Text = details.Name;
            ProcessIdText.Text = details.ProcessId.ToString();
            ExecutablePathText.Text = details.ExecutablePath;
            CommandLineText.Text = string.IsNullOrEmpty(details.CommandLine)
                ? "N/A"
                : details.CommandLine;
            WorkingDirectoryText.Text = string.IsNullOrEmpty(details.WorkingDirectory)
                ? "N/A"
                : details.WorkingDirectory;
            UserNameText.Text = details.UserName;
            StartTimeText.Text = details.StartTime.ToString("G", CultureInfo.CurrentCulture);

            // File information
            ProductNameText.Text = string.IsNullOrEmpty(details.ProductName)
                ? "N/A"
                : details.ProductName;
            CompanyText.Text = string.IsNullOrEmpty(details.Company)
                ? "N/A"
                : details.Company;
            DescriptionText.Text = string.IsNullOrEmpty(details.Description)
                ? "N/A"
                : details.Description;
            FileVersionText.Text = string.IsNullOrEmpty(details.FileVersion)
                ? "N/A"
                : details.FileVersion;

            // Resource usage
            CpuUsageText.Text = $"{details.CpuUsage:F1}%";
            MemoryText.Text = $"{details.MemoryMB:F2} MB";
            ThreadsText.Text = details.ThreadCount.ToString();
            HandlesText.Text = details.HandleCount.ToString();
        }

        private void BringToFront()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            // Move to foreground
            appWindow.MoveInZOrderAtTop();
        }
    }
}