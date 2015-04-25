using System.Windows.Media;

namespace Plainion.Flames.Presentation
{
    public interface IColorPreset
    {
        Pen Pen { get; }

        Brush Foreground { get; }

        Brush Background { get; }
    }
}
