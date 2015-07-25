using System.Collections.Generic;
using System.ComponentModel;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Infrastructure.Model
{
    public interface IProject : INotifyPropertyChanged
    {
        IReadOnlyCollection<string> TraceFiles { get; }

        TraceLog TraceLog { get; }

        FlameSetPresentation Presentation { get; set; }

        IList<object> Items { get; }

        bool WasDeserialized { get; }
    }
}
