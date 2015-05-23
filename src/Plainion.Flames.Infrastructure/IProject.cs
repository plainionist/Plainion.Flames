using System.Collections.Generic;
using Plainion.Flames.Model;

namespace Plainion.Flames.Infrastructure
{
    public interface IProject
    {
        IReadOnlyCollection<string> TraceFiles { get;  }
        
        ITraceLog TraceLog { get; }

        IList<object> Items { get; }
    }
}
