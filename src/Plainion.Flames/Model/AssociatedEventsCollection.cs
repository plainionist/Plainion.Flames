using System.Collections.Generic;
using System.Linq;
using Plainion.Diagnostics;

namespace Plainion.Flames.Model
{
    public class AssociatedEventsCollection : IAssociatedEventsCollection
    {
        private ProcessThreadIndex<IList<IAssociatedEvents>> myIndex;

        internal AssociatedEventsCollection()
        {
            myIndex = new ProcessThreadIndex<IList<IAssociatedEvents>>( ( pid, tid ) => new List<IAssociatedEvents>() );
        }

        public void Add( IAssociatedEvents events )
        {
            myIndex[ events.ReferencedModel.ProcessId ][ events.ReferencedModel.ThreadId ].Add( events );
        }

        public IEnumerable<T> Get<T>( int processId, int threadId ) where T : IAssociatedEvents
        {
            return myIndex[ processId ][ threadId ].OfType<T>();
        }

        public IEnumerable<T> All<T>() where T : IAssociatedEvents
        {
            return myIndex.Values
                .SelectMany( v => v )
                .OfType<T>();
        }
    }
}
