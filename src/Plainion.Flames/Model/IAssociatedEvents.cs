using System.Collections.Generic;

namespace Plainion.Flames.Model
{
    /// <summary>
    /// Identifies additional events happend in the trace which are no call.
    /// </summary>
    public interface IAssociatedEvents
    {
        ModelReference ReferencedModel { get; }

        string Name { get; }
    }
}
