using System.Collections.Generic;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Infrastructure
{
    public interface IProject
    {
        IReadOnlyCollection<string> TraceFiles { get; }

        ITraceLog TraceLog { get; }

        FlameSetPresentation Presentation { get; }

        IList<object> Items { get; }
    }
}
