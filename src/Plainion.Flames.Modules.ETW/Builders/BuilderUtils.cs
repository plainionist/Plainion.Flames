using Microsoft.Diagnostics.Tracing;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class BuilderUtils
    {
        public static long GetTime( TraceEvent evt )
        {
            return ( long )( evt.TimeStampRelativeMSec * 1000 );
        }
    }
}
