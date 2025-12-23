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

        public MainWindow()
        {
            this.InitializeComponent();
            StartMonitoring();
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
            ProcessGrid.ItemsSource =
                _processService.GetProcesses(1);
        }

        private void OnCpuClicked(object sender, RoutedEventArgs e)
        {
            Title = "CPU";
        }

        private void OnMemoryClicked(object sender, RoutedEventArgs e)
        {
            Title = "Memória";
        }
    }
}
