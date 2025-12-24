using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityMonitor.Converters
{
    public class ButtonConverter : IValueConverter
    {
        public Style ActiveStyle { get; set; } = default!;
        public Style InactiveStyle { get; set; } = default!;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is true ? ActiveStyle : InactiveStyle;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotSupportedException();
    }
}
