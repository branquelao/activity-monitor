using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace ActivityMonitor.Services
{
    public class HighValueToBoolConverter : IValueConverter
    {
        public double Threshold { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is double v && v >= Threshold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
