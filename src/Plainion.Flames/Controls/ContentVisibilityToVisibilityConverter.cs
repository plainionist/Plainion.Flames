using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Controls
{
    public class ContentVisibilityToVisibilityConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo cultureInfo )
        {
            if( !( value is ContentVisibility ) )
            {
                return Visibility.Collapsed;
            }

            var visibility = ( ContentVisibility )value;
            if( visibility == ContentVisibility.Visible )
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo cultureInfo )
        {
            throw new NotSupportedException( value.GetType().ToString() );
        }
    }
}
