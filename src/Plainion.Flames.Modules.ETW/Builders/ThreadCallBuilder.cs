using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Plainion.Flames.Model;
using Plainion.Logging;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class ThreadCallBuilder
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(ThreadCallBuilder));

        private TraceModelBuilder myBuilder;
        private CallstackBuilder myCallstackBuilder;
        private List<Call> myCallstack;
        private IReadOnlyList<Method> myLastSample;
        private long myLastSampleTime;
        private long myLastSwitchOutTime = -1;

        public ThreadCallBuilder(TraceModelBuilder builder, TraceThread thread, CallstackBuilder callstackBuilder)
        {
            myBuilder = builder;
            Thread = thread;
            myCallstackBuilder = callstackBuilder;

            myCallstack = new List<Call>(100);
        }

        public bool InterpolateBrokenStackSamples { get; set; }

        public TraceThread Thread { get; private set; }

        public void CpuSampled(SampledProfileTraceData evt)
        {
            ProcessSample(BuilderUtils.GetTime(evt), evt);
        }

        public void SwitchedOut(CSwitchTraceData evt)
        {
            myLastSwitchOutTime = BuilderUtils.GetTime(evt);
        }

        public void SwitchedIn(CSwitchTraceData evt)
        {
            if (myLastSwitchOutTime != -1)
            {
                ProcessSample(myLastSwitchOutTime, evt);
                myLastSwitchOutTime = -1;
            }

            ProcessSample(BuilderUtils.GetTime(evt), evt);
        }

        public void Complete()
        {
            if (myLastSample == null)
            {
                myLogger.Notice("No samples found for Process={0}, Thread={1}", Thread.Process.ProcessId, Thread.ThreadId);
                return;
            }

            WalkSample(myLastSampleTime + 1000, myLastSample, new List<Method>());

            Contract.Invariant(myCallstack.Count == 0, "Stack processing failed");
        }

        private void ProcessSample(long time, TraceEvent evt)
        {
            if (myLastSample == null)
            {
                // only first time ...
                Thread.Process.Name = evt.ProcessName;
            }

            ProcessSample(time, myCallstackBuilder.GetCallstack(evt));
        }

        private void ProcessSample(long time, IReadOnlyList<Method> frames)
        {
            if (frames != null && frames.Count > 0)
            {
                if (!InterpolateBrokenStackSamples || !frames[0].IsBrokenCallstack())
                {
                    myLastSample = WalkSample(time, myLastSample ?? new List<Method>(), frames);
                    myLastSampleTime = time;
                }
            }
            else
            {
                myLogger.Warning("No callstack found for Process={0}, Thread={1}, Time={2}",
                    Thread.Process.ProcessId, Thread.ThreadId, TimeSpan.FromMilliseconds(time / 1000));
            }
        }

        private IReadOnlyList<Method> WalkSample(long time, IReadOnlyList<Method> last, IReadOnlyList<Method> current)
        {
            var depth = 0;

            for (; depth < last.Count; depth++)
            {
                if (depth >= current.Count)
                {
                    break;
                }

                if (!last[depth].Equals(current[depth]))
                {
                    break;
                }
            }

            var sameDepth = depth;

            for (depth = last.Count - 1; depth >= sameDepth; depth--)
            {
                CloseTopCall(time, last[depth]);
            }

            for (depth = sameDepth; depth < current.Count; depth++)
            {
                OpenNewCall(time, current[depth]);
            }

            return current;
        }

        private void OpenNewCall(long time, Method method)
        {
            var call = myBuilder.CreateCall(Thread, time, method);

            if (myCallstack.Count > 0)
            {
                var parent = myCallstack[myCallstack.Count - 1];
                parent.AddChild(call);
            }

            if (myCallstack.Count == 0)
            {
                myBuilder.AddCallstackRoot(call);
            }

            myCallstack.Add(call);
        }

        private void CloseTopCall(long time, Method method)
        {
            Contract.Invariant(myCallstack.Count > 0, "Invalid stack");

            var call = myCallstack[myCallstack.Count - 1];

            // close open calls
            while (method != call.Method)
            {
                call.SetEnd(time);
                myCallstack.Remove(call);
                call = myCallstack[myCallstack.Count - 1];
            }

            call.SetEnd(time);

            myCallstack.Remove(call);
        }
    }
}
