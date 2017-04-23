using System.Collections.Generic;
using System.Linq;
using Prism.Mvvm;
using Plainion.Flames.Infrastructure.Model;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Viewer.Model
{
    /// <summary>
    /// Root entity for one loaded trace log from a single trace session including 
    /// associated data, results or context information.
    /// </summary>
    class Project : BindableBase, IProject
    {
        private TraceLog myTraceLog;
        private FlameSetPresentation myPresentation;

        public Project( IEnumerable<string> traceFiles )
        {
            Contract.RequiresNotNullNotEmpty( traceFiles, "traceFiles" );

            TraceFiles = traceFiles.ToList();
            Items = new List<object>();
        }

        public IReadOnlyCollection<string> TraceFiles { get; private set; }

        public TraceLog TraceLog
        {
            get { return myTraceLog; }
            set { SetProperty( ref myTraceLog, value ); }
        }

        public FlameSetPresentation Presentation
        {
            get { return myPresentation; }
            set { SetProperty( ref myPresentation, value ); }
        }

        public IList<object> Items { get; private set; }

        public bool WasDeserialized { get; set; }
    }
}
