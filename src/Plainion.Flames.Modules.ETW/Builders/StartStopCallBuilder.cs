using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
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
        private ProcessThreadIndex<ThreadCallBuilder> myBuilders;
        private CallstackBuilder myCallstackBuilder;

        public StartStopCallBuilder(TraceModelBuilder traceLogBuilder, IReadOnlyCollection<int> processesToLoad)
        {
            myBuilder = traceLogBuilder;
            myProcessesToLoad = processesToLoad;

            myProcesses = new Index<int, TraceProcess>(pid => myBuilder.CreateProcess(pid));

            myCallstackBuilder = new CallstackBuilder(myBuilder);

            myBuilders = new ProcessThreadIndex<ThreadCallBuilder>((pid, tid) =>
                new ThreadCallBuilder(myBuilder, myBuilder.CreateThread(myProcesses[pid], tid), myCallstackBuilder));
        }

        public void Consume(TraceEvent e)
        {
            var profileSample = e as SampledProfileTraceData;
            if (profileSample != null)
            {
                if (myProcessesToLoad.Contains(e.ProcessID))
                {
                    myBuilders[e.ProcessID][e.ThreadID].CpuSampled(profileSample);
                }
            }

            var cswitch = e as CSwitchTraceData;
            if (cswitch != null)
            {
                if (myProcessesToLoad.Contains(cswitch.OldProcessID))
                {
                    myBuilders[cswitch.OldProcessID][cswitch.OldThreadID].SwitchedOut(cswitch);
                }
                if (myProcessesToLoad.Contains(cswitch.ProcessID))
                {
                    myBuilders[cswitch.ProcessID][cswitch.ThreadID].SwitchedIn(cswitch);
                }
            }
        }

        public void Complete()
        {
            foreach (var builder in myBuilders.Values)
            {
                builder.Complete();
            }
        }
    }
}
