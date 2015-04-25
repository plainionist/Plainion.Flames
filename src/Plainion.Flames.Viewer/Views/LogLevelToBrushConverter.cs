using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Plainion.Logging;

namespace Plainion.Flames.Viewer.Views
{
    public class LogLevelToBrushConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            var level = ( LogLevel )value;

            if( level == LogLevel.Error )
            {
                return Brushes.Red;
            }
            else if( level == LogLevel.Warning )
            {
                return Brushes.DarkOrange;
            }
            else
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotImplementedException();
        }
    }
}
