using System;
using System.Collections.Generic;
using Plainion.Flames.Model;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class BuilderUtils
    {
        public static bool IsBroken( IReadOnlyList<Method> callstack )
        {
            var firstMethod = callstack[ 0 ];

            if( firstMethod.Module == "ntoskrnl" || firstMethod.Module.EndsWith( ".sys" ) )
            {
                // no broken stack detection for kernel or kernel drivers
                return false;
            }

            if( firstMethod.Module == "ntdll" && ( firstMethod.Name == "RtlUserThreadStart" || firstMethod.Name == "_RtlUserThreadStart" ) )
            {
                return false;
            }

            return true;
        }

        public static long GetTime( TraceEvent evt )
        {
            return ( long )( evt.TimeStampRelativeMSec * 1000 );
        }
    }
}
