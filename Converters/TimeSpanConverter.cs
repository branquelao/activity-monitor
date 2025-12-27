using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace ActivityMonitor.Converters
{
    public class TimeSpanConverter : IValueConverter
    {
        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not TimeSpan time)
                return "0.00";

            double totalSeconds = time.TotalSeconds;

            // >= 1 hour → H:MM:SS.ss
            if (totalSeconds >= 3600)
            {
                int hours = (int)time.TotalHours;
                return string.Format(
                    Invariant,
                    "{0}:{1:D2}:{2:D2}.{3:D2}",
                    hours,
                    time.Minutes,
                    time.Seconds,
                    time.Milliseconds / 10);
            }

            // >= 1 minute → M:SS.ss
            if (totalSeconds >= 60)
            {
                return string.Format(
                    Invariant,
                    "{0}:{1:D2}.{2:D2}",
                    time.Minutes,
                    time.Seconds,
                    time.Milliseconds / 10);
            }

            // < 1 minute → SS.ss
            return totalSeconds.ToString("F2", Invariant);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
