using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Plainion.Diagnostics;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class ReadyThreadBookmarksBuilder : IEventConsumer
    {
        private TraceModelBuilder myBuilder;
        private IReadOnlyCollection<int> myProcessesToLoad;
        private ProcessThreadIndex<Bookmarks> myBookmarks;

        public ReadyThreadBookmarksBuilder( TraceModelBuilder builder, IReadOnlyCollection<int> processesToLoad )
        {
            myBuilder = builder;
            myProcessesToLoad = processesToLoad;

            myBookmarks = new ProcessThreadIndex<Bookmarks>( ( pid, tid ) => new Bookmarks( new ModelReference( pid, tid ), "Readied" ) );
        }

        public void Consume( TraceEvent evt )
        {
            var readyThread = evt as DispatcherReadyThreadTraceData;
            if( readyThread == null )
            {
                return;
            }

            if( myProcessesToLoad.Contains( readyThread.AwakenedProcessID ) )
            {
                myBookmarks[ readyThread.AwakenedProcessID ][ readyThread.AwakenedThreadID ].Add( BuilderUtils.GetTime( evt ) );
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
