using System.Collections.Generic;
using System.ComponentModel;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Infrastructure.Model
{
    public interface IProject : INotifyPropertyChanged
    {
        IReadOnlyCollection<string> TraceFiles { get; }

        ITraceLog TraceLog { get; }

        FlameSetPresentation Presentation { get; }

        IList<object> Items { get; }

        bool WasDeserialized { get; }
    }
}
