using System.Collections.Generic;
using Plainion.Flames.Model;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Plainion.Diagnostics;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class ThreadLifecycleBookmarksBuilder : IEventConsumer
    {
        private TraceModelBuilder myBuilder;
        private IReadOnlyCollection<int> myProcessesToLoad;
        private ProcessThreadIndex<Bookmarks> myBookmarks;

        public ThreadLifecycleBookmarksBuilder( TraceModelBuilder builder, IReadOnlyCollection<int> processesToLoad )
        {
            myBuilder = builder;
            myProcessesToLoad = processesToLoad;

            myBookmarks = new ProcessThreadIndex<Bookmarks>( ( pid, tid ) => new Bookmarks( new ModelReference( pid, tid ), "Thread start/stop" ) );
        }

        public void Consume( TraceEvent evt )
        {
            var thread = evt as ThreadTraceData;
            if( thread == null )
            {
                return;
            }

            if( thread.Opcode == TraceEventOpcode.Start || thread.Opcode == TraceEventOpcode.DataCollectionStart )
            {
                myBookmarks[ thread.ProcessID ][ thread.ThreadID ].Add( BuilderUtils.GetTime( evt ) );
            }
            else if( thread.Opcode == TraceEventOpcode.Stop || thread.Opcode == TraceEventOpcode.DataCollectionStop )
            {
                myBookmarks[ thread.ProcessID ][ thread.ThreadID ].Add( BuilderUtils.GetTime( evt ) );
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
