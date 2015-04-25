using System.Collections.Generic;

namespace Plainion.Flames.Model
{
    public interface IBookmarks : IAssociatedEvents
    {
        IReadOnlyCollection<long> Timestamps { get; }
    }
}
