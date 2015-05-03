using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Plainion.Flames.Infrastructure.Services;

namespace Plainion.Flames.Viewer.Services
{
    // TODO: this is an ugly workaround to allow ETW module viewmodels to reload a trace
    [Export( typeof( ITraceLoaderService ) )]
    [Export( typeof( TraceLoaderService ) )]
    class TraceLoaderService : ITraceLoaderService
    {
        public IEnumerable<string> LoadedTraceFiles { get; set; }

        public event EventHandler LoadingCompleted;

        public Action<string[]> UILoadAction { get; set; }

        public void ReloadCurrentTrace()
        {
            UILoadAction( LoadedTraceFiles.ToArray() );
        }

        public void LoadCompleted( IEnumerable<string> traceFiles )
        {
            LoadedTraceFiles = traceFiles.ToList();

            if( LoadingCompleted != null )
            {
                LoadingCompleted( this, EventArgs.Empty );
            }
        }
    }
}
