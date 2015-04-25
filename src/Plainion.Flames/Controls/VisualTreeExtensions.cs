using System.Windows;
using System.Windows.Media;

namespace Plainion.Flames.Controls
{
    internal static class VisualTreeExtensions
    {
        public static T GetVisualChild<T>( this DependencyObject self ) where T : DependencyObject
        {
            if( self == null )
            {
                return null;
            }

            for( int i = 0; i < VisualTreeHelper.GetChildrenCount( self ); i++ )
            {
                var child = VisualTreeHelper.GetChild( self, i );

                var result = ( child as T ) ?? GetVisualChild<T>( child );
                if( result != null )
                {
                    return result;
                }
            }
            return null;
        }
    }
}
