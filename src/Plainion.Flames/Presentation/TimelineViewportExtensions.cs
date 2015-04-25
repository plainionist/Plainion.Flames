using System;

namespace Plainion.Flames.Presentation
{
    public static class TimelineViewportExtensions
    {
        public static void ZoomAtCenter(this TimelineViewport self, double scale )
        {
            var delta = ( self.End - self.Start ) * scale;
            var min = ( long )Math.Max( self.Min, self.Start + delta );
            var max = ( long )Math.Min( self.Max, self.End - delta );
            self.Set( min, max );
        }
    }
}
