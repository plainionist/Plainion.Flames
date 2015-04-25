using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Plainion.Flames.Model;

namespace Plainion.Flames.Presentation
{
    public class DefaultColorLut : IColorLut
    {
        private CachingRingBuffer<int, IColorPreset> myDomainPresets;
        private CachingRingBuffer<string, IColorPreset> myBookmarksPresets;

        public DefaultColorLut()
        {
            // Item1 == background brush, Item2 = Pen color
            var colors = new List<Tuple<string, string>>( 20 );
            colors.Add( new Tuple<string, string>( "#89bded", "#074e90" ) );
            colors.Add( new Tuple<string, string>( "#f36f6f", "#bc1919" ) );
            colors.Add( new Tuple<string, string>( "#8ddfe4", "#16747a" ) );
            colors.Add( new Tuple<string, string>( "#f1803c", "#ae4c11" ) );
            colors.Add( new Tuple<string, string>( "#838ddd", "#15206e" ) );
            colors.Add( new Tuple<string, string>( "#f8c276", "#cc8013" ) );
            colors.Add( new Tuple<string, string>( "#ecb6fa", "#9f0fc3" ) );
            colors.Add( new Tuple<string, string>( "#f1a2b4", "#cc123e" ) );
            colors.Add( new Tuple<string, string>( "#f2db81", "#b99a1c" ) );
            colors.Add( new Tuple<string, string>( "#baabea", "#2f1d67" ) );
            colors.Add( new Tuple<string, string>( "#e6ed71", "#b0ba0c" ) );
            colors.Add( new Tuple<string, string>( "#f1a6c6", "#a00d4b" ) );
            colors.Add( new Tuple<string, string>( "#b6e954", "#5e8b07" ) );
            colors.Add( new Tuple<string, string>( "#c6aae0", "#6919b3" ) );
            colors.Add( new Tuple<string, string>( "#86e779", "#29bd15" ) );
            colors.Add( new Tuple<string, string>( "#adebc0", "#4db66d" ) );
            colors.Add( new Tuple<string, string>( "#93edd7", "#157e64" ) );
            colors.Add( new Tuple<string, string>( "#ef7bc5", "#890d5c" ) );
            colors.Add( new Tuple<string, string>( "#f9a7f4", "#d40ec8" ) );
            colors.Add( new Tuple<string, string>( "#f1a3a3", "#dc1111" ) );

            var presets = colors
                .Select( tuple =>
                    {
                        var brush = new SolidColorBrush( ( Color )ColorConverter.ConvertFromString( tuple.Item1 ) );
                        brush.Freeze();

                        var pen = new Pen( new SolidColorBrush( ( Color )ColorConverter.ConvertFromString( tuple.Item2 ) ), 1 );
                        pen.Brush.Freeze();
                        pen.Freeze();

                        return new ColorPreset( brush, Brushes.Blue, pen );
                    } )
                .ToList();

            myDomainPresets = new CachingRingBuffer<int, IColorPreset>( presets );
            myBookmarksPresets = new CachingRingBuffer<string, IColorPreset>( presets );
        }

        public IColorPreset GetPreset( Method method )
        {
            var id = GetDomainId( method );

            return myDomainPresets.Get( id );
        }

        private int GetDomainId( Method method )
        {
            unchecked
            {
                return ( method.Module == null ? 0 : method.Module.GetHashCode() )
                    ^ ( method.Namespace == null ? 0 : method.Namespace.GetHashCode() )
                    ^ ( method.Class == null ? 0 : method.Class.GetHashCode() );
            }
        }

        public Pen GetBookmarkPreset( string bookmarkName )
        {
            return myBookmarksPresets.Get( bookmarkName ).Pen;
        }
    }
}
