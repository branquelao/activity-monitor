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

        public MainWindow()
        {
            this.InitializeComponent();
            LoadMemory();
        }

        private void LoadMemory()
        {
            ProcessGrid.ItemsSource = _processService.GetProcesses();
        }

        private void OnCpuClicked(object sender, RoutedEventArgs e)
        {
            // Por enquanto só troca o título
            Title = "CPU";
        }

        private void OnMemoryClicked(object sender, RoutedEventArgs e)
        {
            LoadMemory();
            Title = "Memory";
        }
    }
}
