using System;
using System.Globalization;
using System.Windows.Data;

namespace Plainion.Flames.Controls
{
    public class DurationToTimeStringConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo cultureInfo )
        {
            var rawTime = ( long )value;
            double time = rawTime / 1000.0d;
            var unit = "ms";

            if( time > 1100 )
            {
                time /= 1000;
                unit = "s";
            }

            return string.Format( "{0:F3} {1}", time, unit );
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo cultureInfo )
        {
            throw new NotImplementedException();
        }
    }
}
