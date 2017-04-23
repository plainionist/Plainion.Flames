using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Tracing;
using Plainion.Collections;
using Plainion.Diagnostics;
using Plainion.Flames.Model;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class StartStopCallBuilder : IEventConsumer
    {
        private TraceModelBuilder myBuilder;
        private IReadOnlyCollection<int> myProcessesToLoad;
        private Index<int, TraceProcess> myProcesses;
        private ProcessThreadIndex<StartStopThreadCallBuilder> myBuilders;

        public StartStopCallBuilder( TraceModelBuilder traceLogBuilder, IReadOnlyCollection<int> processesToLoad )
        {
            myBuilder = traceLogBuilder;
            myProcessesToLoad = processesToLoad;

            myProcesses = new Index<int, TraceProcess>( pid => myBuilder.CreateProcess( pid ) );

            myBuilders = new ProcessThreadIndex<StartStopThreadCallBuilder>( ( pid, tid ) =>
                new StartStopThreadCallBuilder( myBuilder, myBuilder.CreateThread( myProcesses[ pid ], tid ) ) );
        }

        public void Consume( TraceEvent e )
        {
            if( e.Opcode == TraceEventOpcode.Start || e.Opcode == TraceEventOpcode.DataCollectionStart ||
                e.Opcode == TraceEventOpcode.Stop || e.Opcode == TraceEventOpcode.DataCollectionStop )
            {
                if( myProcessesToLoad.Contains( e.ProcessID ) )
                {
                    myBuilders[ e.ProcessID ][ e.ThreadID ].Process( e );
                }
            }
        }

        public void Complete()
        {
            foreach( var builder in myBuilders.Values )
            {
                builder.Complete();
            }
        }
    }
}
