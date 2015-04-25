
namespace Plainion.Flames.Modules.StringTracing
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
