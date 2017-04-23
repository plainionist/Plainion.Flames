using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;

namespace Plainion.Flames.Modules.Streams
{
    class TraceThreadBuilder
    {
        private TraceModelBuilder myBuilder;
        private IEnumerable<TraceLineBase> myLines;

        public TraceThreadBuilder( TraceModelBuilder builder, TraceProcess process, int threadId, IEnumerable<TraceLineBase> lines )
        {
            myBuilder = builder;
            myLines = lines;

            Thread = myBuilder.CreateThread( process, threadId );
        }

        public TraceThread Thread { get; private set; }

        internal void Build()
        {
            foreach( var callstackRoot in BuildCallstacks() )
            {
                myBuilder.AddCallstackRoot( callstackRoot );
            }

            myLines = null;
        }

        private IEnumerable<Call> BuildCallstacks()
        {
            var stack = new List<Call>();

            foreach( var line in myLines )
            {
                var enteringLine = line as EnteringTraceLine;
                if( enteringLine != null )
                {
                    var call = myBuilder.CreateCall( Thread, enteringLine.Time, enteringLine.Method );

                    if( stack.Count > 0 )
                    {
                        var parent = stack[ stack.Count - 1 ];
                        parent.AddChild( call );
                    }

                    if( stack.Count == 0 )
                    {
                        // only yield root of stack
                        yield return call;
                    }

                    stack.Add( call );

                    continue;
                }

                var leavingLine = line as LeavingTraceLine;
                if( leavingLine != null )
                {
                    if( stack.Count > 0 )
                    {
                        var call = stack[ stack.Count - 1 ];

                        // close open calls
                        while( line.Method != call.Method )
                        {
                            call.SetEnd( leavingLine.Time, leavingLine.Duration );
                            stack.Remove( call );
                            call = stack[ stack.Count - 1 ];
                        }

                        call.SetEnd( leavingLine.Time, leavingLine.Duration );

                        stack.Remove( call );
                    }
                    else
                    {
                        // trace starts with leaving -> ignore
                        //myLinesInvalidForCallstack.Add( line );
                    }

                    continue;
                }

                throw new NotSupportedException( line.GetType().ToString() );
            }

            if( stack.Count > 0 )
            {
                // yield left-overs
                yield return stack.First();
            }
        }
    }
}
