using ActivityMonitor.ViewModels;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WinRT.Interop;

namespace ActivityMonitor
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new();

        private ElementTheme _currentTheme = ElementTheme.Dark;

        public MainWindow()
        {
            InitializeComponent();
            RootGrid.DataContext = ViewModel;

            this.Activated += MainWindow_Activated;

            ApplyTheme(_currentTheme);

            SetWindowsSize(1200, 600, 700, 450);
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            appWindow.SetIcon("Assets/AppIcon_Logo.ico");

            this.Activated -= MainWindow_Activated;
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

        private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggle)
            {
                _currentTheme = toggle.IsOn
                    ? ElementTheme.Dark
                    : ElementTheme.Light;

                ApplyTheme(_currentTheme);
            }
        }

        private void ApplyTheme(ElementTheme theme)
        {
            RootGrid.RequestedTheme = ElementTheme.Default;
            RootGrid.RequestedTheme = theme;
        }
    }
}
