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

            control.Draw();
        }

        public PerformanceGraph()
        {
            InitializeComponent();
            SizeChanged += (_, _) => Draw();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Draw();
        }

        private void Draw()
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
                Stroke = new SolidColorBrush(Color.FromArgb(255, 90, 200, 90)),
                StrokeThickness = 2
            };

            for (int i = 0; i < Values.Count; i++)
            {
                double x = i * (width / (Values.Count - 1));
                double y = height - (Values[i] / 100.0 * height);

                polyline.Points.Add(new Windows.Foundation.Point(x, y));
            }

            GraphCanvas.Children.Add(polyline);
        }
    }
}
