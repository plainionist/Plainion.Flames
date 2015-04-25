using System.Collections.Generic;

namespace Plainion.Flames.Model
{
    public interface IAssociatedEventsCollection
    {
        IEnumerable<T> Get<T>( int processId, int threadId ) where T : IAssociatedEvents;

        IEnumerable<T> All<T>() where T : IAssociatedEvents;
    }
}
