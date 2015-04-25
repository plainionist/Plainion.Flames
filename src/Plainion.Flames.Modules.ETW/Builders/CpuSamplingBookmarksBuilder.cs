using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Plainion.Diagnostics;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class CpuSamplingBookmarksBuilder : IEventConsumer
    {
        private TraceModelBuilder myBuilder;
        private IReadOnlyCollection<int> myProcessesToLoad;
        private ProcessThreadIndex<Bookmarks> myBookmarks;

        public CpuSamplingBookmarksBuilder( TraceModelBuilder builder, IReadOnlyCollection<int> processesToLoad )
        {
            myBuilder = builder;
            myProcessesToLoad = processesToLoad;

            myBookmarks = new ProcessThreadIndex<Bookmarks>( ( pid, tid ) => new Bookmarks( new ModelReference( pid, tid ), "CPU sampled" ) );
        }

        public void Consume( TraceEvent evt )
        {
            var cpuSample = evt as SampledProfileTraceData;
            if( cpuSample == null )
            {
                return;
            }

            if( myProcessesToLoad.Contains( evt.ProcessID ) )
            {
                myBookmarks[  evt.ProcessID ][ evt.ThreadID  ].Add( BuilderUtils.GetTime( evt ) );
            }
        }

        public void Complete()
        {
            foreach( var bookmarks in myBookmarks.Values )
            {
                myBuilder.AddAssociatedEvents( bookmarks );
            }
        }
    }
}
