using ActivityMonitor.Models;
using ActivityMonitor.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ActivityMonitor.Views
{
    public sealed partial class ProcessDetailsWindow : Window
    {
        // Win32 API to bring window to front
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public ProcessDetailsWindow(GroupedProcessInfo processInfo)
        {
            this.InitializeComponent();

            // Set window size and bring to front
            SetWindowSizeAndBringToFront(600, 800);

            LoadProcessDetails(processInfo);
        }

        private void SetWindowSizeAndBringToFront(int width, int height)
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            // Resize
            appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));

            // Make sure it's on top
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsAlwaysOnTop = true;
                // Disable after a short moment so it's not permanently on top
                var timer = new System.Threading.Timer(_ =>
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        presenter.IsAlwaysOnTop = false;
                    });
                }, null, 100, System.Threading.Timeout.Infinite);
            }

            // Also use Win32 API
            SetForegroundWindow(hWnd);
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
    }
}