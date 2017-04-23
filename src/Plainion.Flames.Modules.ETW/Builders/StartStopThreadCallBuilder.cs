using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Microsoft.Diagnostics.Tracing;
using Plainion.Flames.Model;
using Plainion.Logging;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class StartStopThreadCallBuilder
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger( typeof( ThreadCallBuilder ) );

        private TraceModelBuilder myBuilder;
        private bool myFirstEvent;
        private Subject<TraceEvent> myProcessor;

        public StartStopThreadCallBuilder( TraceModelBuilder builder, TraceThread thread )
        {
            myBuilder = builder;
            Thread = thread;

            myFirstEvent = true;

            myProcessor = new Subject<TraceEvent>();

            var stack = new List<Call>();
            myProcessor.Subscribe(
                e => OnNext( stack, e ),
                () =>
                {
                    if( stack.Count > 0 )
                    {
                        // add left-overs
                        myBuilder.AddCallstackRoot( stack.First() );
                    }
                } );
        }

        public TraceThread Thread { get; private set; }

        public void Process( TraceEvent e )
        {
            if( myFirstEvent )
            {
                Thread.Process.Name = e.ProcessName;
                myFirstEvent = false;
            }

            myProcessor.OnNext( e );
        }

        public void Complete()
        {
            myProcessor.OnCompleted();
        }

        private void OnNext( List<Call> stack, TraceEvent e )
        {
            var time = BuilderUtils.GetTime( e );
            var method = myBuilder.CreateMethod( null, e.ProviderName, null, e.TaskName );

            if( e.Opcode == TraceEventOpcode.Start || e.Opcode == TraceEventOpcode.DataCollectionStart )
            {
                var call = myBuilder.CreateCall( Thread, time, method );

                if( stack.Count > 0 )
                {
                    var parent = stack[ stack.Count - 1 ];
                    parent.AddChild( call );
                }

                if( stack.Count == 0 )
                {
                    // only add root of stack
                    myBuilder.AddCallstackRoot( call );
                }

                stack.Add( call );
            }
            else if( e.Opcode == TraceEventOpcode.Stop || e.Opcode == TraceEventOpcode.DataCollectionStop )
            {
                if( stack.Count > 0 )
                {
                    var call = stack[ stack.Count - 1 ];

                    // ignore stop calls without start
                    if( method == call.Method )
                    {
                        call.SetEnd( time );

                        stack.Remove( call );
                    }
                }
                else
                {
                    // trace starts with leaving -> ignore
                    //myLinesInvalidForCallstack.Add( line );
                }
            }
        }
    }
}
