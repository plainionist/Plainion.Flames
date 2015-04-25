using System.Collections.Generic;
using System.Linq;

namespace Plainion.Flames.Model
{
    public static class AssociatedEventsCollectionExtensions
    {
        /// <summary>
        /// Gets all events for this given thread, its owning process and the system wide onces
        /// </summary>
        public static IEnumerable<T> GetAllFor<T>( this IAssociatedEventsCollection self, int processId, int threadId ) where T : IAssociatedEvents
        {
            return self.Get<T>( ModelReference.Undefined, ModelReference.Undefined )
                .Concat( self.Get<T>( processId, ModelReference.Undefined ) )
                .Concat( self.Get<T>( processId, threadId ) );
        }

        public static IEnumerable<T> GetAllFor<T>( this IAssociatedEventsCollection self, TraceThread thread ) where T : IAssociatedEvents
        {
            return self.GetAllFor<T>( thread.Process.ProcessId, thread.ThreadId );
        }
    }
}
