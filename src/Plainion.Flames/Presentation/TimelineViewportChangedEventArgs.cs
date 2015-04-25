using System;

namespace Plainion.Flames.Presentation
{
    public class TimelineViewportChangedEventArgs : EventArgs
    {
        public TimelineViewportChangedEventArgs( long oldStart, long oldEnd, long newStart, long newEnd )
        {
            OldStart = oldStart;
            OldEnd = oldEnd;
            NewStart = newStart;
            NewEnd = newEnd;
        }

        public long OldStart { get; private set; }

        public long OldEnd { get; private set; }

        public long NewStart { get; private set; }

        public long NewEnd { get; private set; }

        public bool PositionChanged { get { return NewStart != OldStart || NewEnd != OldEnd; } }

        public bool WidthChanged { get { return ( NewEnd - NewStart ) != ( OldEnd - OldStart ); } }
    }
}
