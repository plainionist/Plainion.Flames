using System;
using System.Globalization;
using System.Windows.Data;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Controls
{
    public class BoolToContentVisibilityConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo cultureInfo )
        {
            if( !( value is ContentVisibility ) )
            {
                return false;
            }

            var visibility = ( ContentVisibility )value;
            return visibility == ContentVisibility.Visible ? true : false;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo cultureInfo )
        {
            if( value is bool )
            {
                var boolean = ( bool )value;
                return boolean ? ContentVisibility.Visible : ContentVisibility.Invisible;
            }

            throw new NotSupportedException( value.GetType().ToString() );
        }
    }
}
