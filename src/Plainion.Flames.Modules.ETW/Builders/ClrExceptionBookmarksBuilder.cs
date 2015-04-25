using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Plainion.Diagnostics;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class ClrExceptionBookmarksBuilder : IEventConsumer
    {
        private TraceModelBuilder myBuilder;
        private IReadOnlyCollection<int> myProcessesToLoad;
        private ProcessThreadIndex<Bookmarks> myBookmarks;

        public ClrExceptionBookmarksBuilder( TraceModelBuilder builder, IReadOnlyCollection<int> processesToLoad )
        {
            myBuilder = builder;
            myProcessesToLoad = processesToLoad;

            myBookmarks = new ProcessThreadIndex<Bookmarks>( ( pid, tid ) => new Bookmarks( new ModelReference( pid, tid ), "Clr exception" ) );
        }

        public void Consume( TraceEvent evt )
        {
            var exception = evt as ExceptionTraceData;
            if( exception == null )
            {
                return;
            }

            if( myProcessesToLoad.Contains( evt.ProcessID ) )
            {
                myBookmarks[ evt.ProcessID ][ evt.ThreadID ].Add( BuilderUtils.GetTime( evt ) );
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
