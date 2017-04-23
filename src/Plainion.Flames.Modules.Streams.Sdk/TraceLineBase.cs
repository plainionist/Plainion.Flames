using System.Diagnostics;
using Plainion.Flames.Model;

namespace Plainion.Flames.Modules.Streams
{
    public abstract class TraceLineBase
    {
        internal TraceLineBase( long time )
        {
            Time = time;
        }

        public long Time { get; private set; }

        public int ProcessId { get; internal set; }

        public int ThreadId { get; internal set; }

        public Method Method { get; internal set; }
    }
}
