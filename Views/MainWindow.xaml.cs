using ActivityMonitor.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using WinRT.Interop;

namespace ActivityMonitor
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new();

        public MainWindow()
        {
            InitializeComponent();
            RootGrid.DataContext = ViewModel;

            SetWindowsSize(1200, 600, 700, 450);
        }

        private void SetWindowsSize(int width, int height, int minWidth, int minHeight)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.PreferredMinimumWidth = minWidth;
                presenter.PreferredMinimumHeight = minHeight;
            }
        }

        private void ProcessSorting(object sender, DataGridColumnEventArgs e)
        {
            string column = e.Column.Header?.ToString() ?? string.Empty;

            ViewModel.ApplyColumnSort(column);
        }
    }
}
