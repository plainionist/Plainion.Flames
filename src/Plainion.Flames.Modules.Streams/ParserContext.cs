
using System.Collections.Generic;
namespace Plainion.Flames.Modules.Streams
{
    class ParserContext : IParserContext
    {
        private TraceModelBuilder myBuilder;
        private List<TraceLineBase> myLines;

        public ParserContext( TraceModelBuilder builder, int lineBufferCapacity )
        {
            myBuilder = builder;
            myLines = new List<TraceLineBase>( lineBufferCapacity );
        }

        public IReadOnlyCollection<TraceLineBase> Lines
        {
            get { return myLines; }
        }

        public EnteringTraceLine CreateEnteringLine( long time, int processId, int threadId, string module, string callNamespace, string callClass, string methodName )
        {
            var method = myBuilder.CreateMethod( module, callNamespace, callClass, methodName );
            return new EnteringTraceLine( time )
            {
                ProcessId = processId,
                ThreadId = threadId,
                Method = method
            };
        }

        public LeavingTraceLine CreateLeavingLine( long time, int processId, int threadId, string module, string callNamespace, string callClass, string methodName )
        {
            var method = myBuilder.CreateMethod( module, callNamespace, callClass, methodName );
            return new LeavingTraceLine( time )
            {
                ProcessId = processId,
                ThreadId = threadId,
                Method = method
            };
        }

        public void Emit( TraceLineBase line )
        {
            myLines.Add( line );
        }

        public void Emit( TraceInfo info )
        {
            myBuilder.SetCreationTime( info.CreationTimestamp );
            myBuilder.SetTraceDuration( info.TraceDuration );
        }
    }
}
