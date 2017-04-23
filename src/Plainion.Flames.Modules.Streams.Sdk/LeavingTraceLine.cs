
namespace Plainion.Flames.Modules.Streams
{
    public class LeavingTraceLine : TraceLineBase
    {
        internal LeavingTraceLine( long time )
            : base( time )
        {
        }

        public long Duration { get; set; }
    }
}
