using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Plainion.Flames.Model;

namespace Plainion.Flames.Controls
{
    public class MethodToFullnameConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo cultureInfo )
        {
            var method = ( Method )value;

            var sb = new StringBuilder();

            if( method.Module != null )
            {
                sb.Append( method.Module );

                if( method.Namespace != null || method.Class != null )
                {
                    sb.Append( "!" );
                }
            }

            if( method.Namespace != null )
            {
                sb.Append( method.Namespace );

                if( method.Class != null )
                {
                    sb.Append( "." );
                }
            }

            if( method.Class != null )
            {
                sb.Append( method.Class );
            }

            return sb.ToString();
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo cultureInfo )
        {
            throw new NotImplementedException();
        }
    }
}
