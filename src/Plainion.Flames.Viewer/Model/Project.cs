using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Viewer.Model
{
    /// <summary>
    /// Root entity for one loaded trace log from a single trace session including 
    /// associated data, results or context information.
    /// </summary>
    class Project : IProject
    {
        public Project( IEnumerable<string> traceFiles )
        {
            TraceFiles = traceFiles.ToList();
            Items = new List<object>();
        }

        public IReadOnlyCollection<string> TraceFiles { get; private set; }

        public TraceLog TraceLog { get; set; }

        public FlameSetPresentation Presentation { get; internal set; }

        public IList<object> Items { get; private set; }

        ITraceLog IProject.TraceLog
        {
            get { return TraceLog; }
        }
    }
}
