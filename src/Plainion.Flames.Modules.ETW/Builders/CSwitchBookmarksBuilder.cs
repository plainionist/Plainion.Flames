using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Plainion.Diagnostics;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class CSwitchBookmarksBuilder : IEventConsumer
    {
        private TraceModelBuilder myBuilder;
        private IReadOnlyCollection<int> myProcessesToLoad;
        private ProcessThreadIndex<Bookmarks> mySwitchInBookmarks;
        private ProcessThreadIndex<Bookmarks> mySwitchOutBookmarks;

        public CSwitchBookmarksBuilder( TraceModelBuilder builder, IReadOnlyCollection<int> processesToLoad )
        {
            myBuilder = builder;
            myProcessesToLoad = processesToLoad;

            mySwitchInBookmarks = new ProcessThreadIndex<Bookmarks>( ( pid, tid ) => new Bookmarks( new ModelReference( pid, tid ), "Switched in" ) );
            mySwitchOutBookmarks = new ProcessThreadIndex<Bookmarks>( ( pid, tid ) => new Bookmarks( new ModelReference( pid, tid ), "Switched out" ) );
        }

        public void Consume( TraceEvent evt )
        {
            var cswitch = evt as CSwitchTraceData;
            if( cswitch == null )
            {
                return;
            }

            if( myProcessesToLoad.Contains( cswitch.OldProcessID ) )
            {
                mySwitchOutBookmarks[ cswitch.OldProcessID ][ cswitch.OldThreadID ].Add( BuilderUtils.GetTime( cswitch ) );
            }

            if( myProcessesToLoad.Contains( cswitch.ProcessID ) )
            {
                mySwitchInBookmarks[ evt.ProcessID ][ evt.ThreadID ].Add( BuilderUtils.GetTime( evt ) );
            }
        }

        public void Complete()
        {
            foreach( var bookmarks in mySwitchInBookmarks.Values )
            {
                myBuilder.AddAssociatedEvents( bookmarks );
            }
            foreach( var bookmarks in mySwitchOutBookmarks.Values )
            {
                myBuilder.AddAssociatedEvents( bookmarks );
            }
        }
    }
}
