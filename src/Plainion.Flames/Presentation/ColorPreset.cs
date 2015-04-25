using System.Windows.Media;

namespace Plainion.Flames.Presentation
{
    class ColorPreset : IColorPreset
    {
        public ColorPreset( Brush background, Brush foreground, Pen pen )
        {
            Background = background;
            Foreground = foreground;
            Pen = pen;
        }

        public Pen Pen { get; private set; }

        public Brush Foreground { get; private set; }

        public Brush Background { get; private set; }
    }
}
