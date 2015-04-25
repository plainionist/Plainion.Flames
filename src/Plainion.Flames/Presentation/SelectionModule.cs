using System;

namespace Plainion.Flames.Presentation
{
    public class SelectionModule
    {
        public long? Start { get; set; }

        public long? End { get; set; }

        public event EventHandler Cleared;

        public void Clear()
        {
            Start = null;
            End = null;

            if( Cleared != null )
            {
                Cleared( this, EventArgs.Empty );
            }
        }
    }
}
