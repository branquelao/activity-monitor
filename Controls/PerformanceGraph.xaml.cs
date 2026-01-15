using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Windows.UI;

namespace ActivityMonitor.Controls
{
    public sealed partial class PerformanceGraph : UserControl
    {
        public ObservableCollection<double>? Values
        {
            get => (ObservableCollection<double>?)GetValue(ValuesProperty);
            set => SetValue(ValuesProperty, value);
        }

        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register(
                nameof(Values),
                typeof(ObservableCollection<double>),
                typeof(PerformanceGraph),
                new PropertyMetadata(null, OnValuesChanged));

        private static void OnValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PerformanceGraph)d;

            if (e.OldValue is ObservableCollection<double> old)
                old.CollectionChanged -= control.OnCollectionChanged;

            if (e.NewValue is ObservableCollection<double> collection)
                collection.CollectionChanged += control.OnCollectionChanged;

            control.DrawLine();
        }

        public PerformanceGraph()
        {
            InitializeComponent();
            SizeChanged += (_, _) =>
            {
                DrawGrid();
                DrawLine();
            };
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            DrawLine();
        }

        private void DrawLine()
        {
            GraphCanvas.Children.Clear();

            if (Values == null || Values.Count < 2)
                return;

            double width = ActualWidth;
            double height = ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            var polyline = new Polyline
            {
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 215)), 
                StrokeThickness = 2
            };

            int count = Values.Count;

            for (int i = 0; i < count; i++)
            {
                double x = width - ((count - 1 - i) * (width / (count - 1)));
                double y = height - (Values[i] / 100.0 * height);

                polyline.Points.Add(new Windows.Foundation.Point(x, y));
            }

            GraphCanvas.Children.Add(polyline);
        }

        private void DrawGrid()
        {
            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;

            GridCanvas.Children.Clear();

            int verticalLines = 10;
            int horizontalLines = 5;

            double width = ActualWidth;
            double height = ActualHeight;

            var brush = new SolidColorBrush(
                Windows.UI.Color.FromArgb(50, 100, 100, 100));

            for (int i = 1; i < verticalLines; i++)
            {
                double x = i * width / verticalLines;
                GridCanvas.Children.Add(new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = 0,
                    Y2 = height,
                    Stroke = brush,
                    StrokeThickness = 1
                });
            }

            for (int i = 1; i < horizontalLines; i++)
            {
                double y = i * height / horizontalLines;
                GridCanvas.Children.Add(new Line
                {
                    X1 = 0,
                    X2 = width,
                    Y1 = y,
                    Y2 = y,
                    Stroke = brush,
                    StrokeThickness = 1
                });
            }
        }
    }
}
