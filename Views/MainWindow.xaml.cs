using ActivityMonitor.Services;
using ActivityMonitor.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;
using static System.Net.WebRequestMethods;


namespace ActivityMonitor
{
    public sealed partial class MainWindow : Window
    {
        private readonly ProcessService _processService = new();
        private DispatcherTimer _timer;
        private ViewMode _currentMode = ViewMode.Cpu;

        public MainWindow()
        {
            this.InitializeComponent();

            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            appWindow.Resize(new Windows.Graphics.SizeInt32(800, 600));

            StartMonitoring();
            UpdateColumns();
        }

        private void StartMonitoring()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTick;
            _timer.Start();
        }

        private void OnTick(object sender, object e)
        {
            var processes = _processService.GetProcesses(1);

            if (_currentMode == ViewMode.Cpu)
            {
                ProcessGrid.ItemsSource = processes
                    .OrderByDescending(p => p.Cpu)
                    .ToList();
            } else
            {
                ProcessGrid.ItemsSource = processes
                    .OrderByDescending(p => p.Memory)
                    .ToList();
            }
        }

        private void OnCpuClicked(object sender, RoutedEventArgs e)
        {
            _currentMode = ViewMode.Cpu;
            Title = "CPU";
            UpdateColumns();
        }

        private void OnMemoryClicked(object sender, RoutedEventArgs e)
        {
            _currentMode = ViewMode.Memory;
            Title = "Memória";
            UpdateColumns();
        }

        public enum ViewMode
        {
            Cpu,
            Memory
        }

        private void UpdateColumns()
        {
            if (_currentMode == ViewMode.Cpu)
            {
                CpuColumn.Visibility = Visibility.Visible;
                MemoryColumn.Visibility = Visibility.Collapsed;

                CpuButton.Style = 
                    (Style)Application.Current.Resources["HeaderButtonActive"];
                MemoryButton.Style = 
                    (Style)Application.Current.Resources["HeaderButtonInactive"];
            }
            else
            {
                CpuColumn.Visibility = Visibility.Collapsed;
                MemoryColumn.Visibility = Visibility.Visible;

                CpuButton.Style =
                    (Style)Application.Current.Resources["HeaderButtonInactive"];
                MemoryButton.Style =
                    (Style)Application.Current.Resources["HeaderButtonActive"];
            }
        }
    }
}
