using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Controls
{
    public class BookmarkToBrushConverter : Freezable, IValueConverter
    {
        public static readonly DependencyProperty ColorLutProperty =
           DependencyProperty.Register( "ColorLut", typeof( IColorLut ), typeof( BookmarkToBrushConverter ) );

        public IColorLut ColorLut
        {
            get { return ( IColorLut )GetValue( ColorLutProperty ); }
            set { SetValue( ColorLutProperty, value ); }
        }

        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }
        
        public object Convert( object value, Type targetType, object parameter, CultureInfo cultureInfo )
        {
            if( value is IBookmarks )
            {
                return ColorLut.GetBookmarkPreset( ( ( IBookmarks )value ).Name ).Brush;
            }
            else if( value is string )
            {
                return ColorLut.GetBookmarkPreset( ( string )value ).Brush;
            }

            throw new NotSupportedException();
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo cultureInfo )
        {
            throw new NotImplementedException();
        }
    }
}
