using System;
using System.Collections.Generic;

namespace Plainion.Flames.Model
{
    public interface ITraceLog
    {
        /// <summary>
        /// Absolute UTC timestamp the trace log was taken at.
        /// </summary>
        DateTime CreationTime { get; }

        /// <summary>
        /// Duration of the trace in micro seconds.
        /// </summary>
        long TraceDuration { get; }

        IReadOnlyCollection<TraceProcess> Processes { get; }

        IReadOnlyCollection<TraceThread> GetThreads( TraceProcess process );

        IReadOnlyCollection<Call> GetCallstacks( TraceThread thread );

        SymbolRepository Symbols { get; }

        IAssociatedEventsCollection AssociatedEvents { get; }
    }
}
