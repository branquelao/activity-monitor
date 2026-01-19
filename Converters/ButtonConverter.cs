using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace ActivityMonitor.Converters
{
    // Converts a boolean value into an active or inactive button style
    public class ButtonConverter : IValueConverter
    {
        // Style used when value is true
        public Style ActiveStyle { get; set; } = default!;

        // Style used when value is false
        public Style InactiveStyle { get; set; } = default!;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is true ? ActiveStyle : InactiveStyle;
        }

        // One-way converter only
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotSupportedException();
    }
}
