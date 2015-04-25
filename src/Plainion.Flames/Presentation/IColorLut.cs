using System.Windows.Media;
using Plainion.Flames.Model;

namespace Plainion.Flames.Presentation
{
    public interface IColorLut
    {
        IColorPreset GetPreset( Method method );

        Pen GetBookmarkPreset( string bookmarkName );
    }
}
