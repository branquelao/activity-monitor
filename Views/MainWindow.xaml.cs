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
        // Main ViewModel instance used by the window
        public MainViewModel ViewModel { get; } = new();

        public MainWindow()
        {
            InitializeComponent();

            // Bind ViewModel to the root layout
            RootGrid.DataContext = ViewModel;

            // Use system default theme (Light/Dark)
            RootGrid.RequestedTheme = ElementTheme.Default;

            // Delay window configuration until activated
            this.Activated += MainWindow_Activated;

            // Set initial and minimum window size
            SetWindowsSize(900, 600, 700, 450);

            // Handle sorting events for the DataGrid
            ProcessGrid.Loaded += (s, e) =>
            {
                UpdateSortIndicators("Process");
            };

            // Subscribe to sorting event
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.CurrentMode))
                {
                    UpdateSortIndicators("Process");
                }
            };
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            // Get native window handle
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            // Set application window icon
            appWindow.SetIcon("Assets/AppIcon_Logo.ico");

            // Run only once
            this.Activated -= MainWindow_Activated;
        }

        private void SetWindowsSize(int width, int height, int minWidth, int minHeight)
        {
            // Resolve AppWindow from Win32 handle
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            // Apply initial window size
            appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));

            // Configure minimum resize constraints
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.PreferredMinimumWidth = minWidth;
                presenter.PreferredMinimumHeight = minHeight;
            }
        }

        // Handle DataGrid column sorting indicators
        private void ProcessSorting(object sender, DataGridColumnEventArgs e)
        {
            string columnName = e.Column.Header?.ToString() ?? "";
            columnName = columnName.Replace(" ▲", "").Replace(" ▼", "");

            ViewModel.ApplyColumnSort(columnName);
            UpdateSortIndicators(columnName);
        }

        // Update sort indicators on DataGrid columns
        private void UpdateSortIndicators(string sortedColumn)
        {
            if (ProcessGrid.Columns == null || ProcessGrid.Columns.Count == 0)
                return;

            foreach (var column in ProcessGrid.Columns)
            {
                string headerText = column.Header?.ToString() ?? "";

                headerText = headerText.Replace(" ▲", "").Replace(" ▼", "");

                if (headerText == sortedColumn)
                {
                    var sortState = ViewModel.GetCurrentSortState();
                    string arrow = sortState == SortState.Ascending ? " ▲" : " ▼";
                    column.Header = headerText + arrow;
                }
                else
                {
                    column.Header = headerText;
                }
            }
        }
    }
}
