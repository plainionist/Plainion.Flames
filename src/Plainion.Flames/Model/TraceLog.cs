using System;
using System.Collections.Generic;
using Plainion.Collections;

namespace Plainion.Flames.Model
{
    public class TraceLog : ITraceLog, IDisposable
    {
        private CollectionReadonlyCollectionAdapter<TraceProcess> myProcessesView;
        private Dictionary<TraceProcess, List<TraceThread>> myThreads;
        private Dictionary<TraceThread, List<Call>> myCallstacks;
        private AssociatedEventsCollection myAssociatedEvents;

        internal TraceLog( SymbolRepository symbols )
        {
            Symbols = symbols;

            myThreads = new Dictionary<TraceProcess, List<TraceThread>>();
            myCallstacks = new Dictionary<TraceThread, List<Call>>();

            myProcessesView = new CollectionReadonlyCollectionAdapter<TraceProcess>( myThreads.Keys );

            myAssociatedEvents = new AssociatedEventsCollection();
        }

        public DateTime CreationTime { get; internal set; }

        public long TraceDuration { get; internal set; }

        public SymbolRepository Symbols { get; private set; }

        public IReadOnlyCollection<Method> Methods { get; internal set; }

        public IReadOnlyCollection<TraceProcess> Processes { get { return myProcessesView; } }

        public IReadOnlyCollection<TraceThread> GetThreads( TraceProcess process )
        {
            return myThreads[ process ];
        }

        internal void Add( TraceThread thread )
        {
            List<TraceThread> threads;
            if( !myThreads.TryGetValue( thread.Process, out threads ) )
            {
                threads = new List<TraceThread>();
                myThreads[ thread.Process ] = threads;
            }

            threads.Add( thread );
        }

        public IReadOnlyList<Call> GetCallstacks( TraceThread thread )
        {
            List<Call> calls;
            if( !myCallstacks.TryGetValue( thread, out calls ) )
            {
                calls = new List<Call>();
                myCallstacks.Add( thread, calls );
            }

            return calls;
        }

        internal void Add( Call call )
        {
            List<Call> stacks;
            if( !myCallstacks.TryGetValue( call.Thread, out stacks ) )
            {
                stacks = new List<Call>();
                myCallstacks[ call.Thread ] = stacks;
            }

            stacks.Add( call );
        }

        internal void Add( TraceThread thread, IReadOnlyCollection<Call> calls )
        {
            Add( thread );

            if( calls.Count == 0 )
            {
                return;
            }

            List<Call> stacks;
            if( !myCallstacks.TryGetValue( thread, out stacks ) )
            {
                stacks = new List<Call>();
                myCallstacks[ thread ] = stacks;
            }

            stacks.AddRange( calls );
        }

        public IAssociatedEventsCollection AssociatedEvents { get { return myAssociatedEvents; } }

        public void Add( IAssociatedEvents events )
        {
            myAssociatedEvents.Add( events );
        }

        public void Dispose()
        {
            if( myProcessesView == null )
            {
                return;
            }

            myProcessesView = null;

            myThreads.Clear();
            myThreads = null;

            myCallstacks.Clear();
            myCallstacks = null;

            Symbols.Clear();
        }
    }
}
