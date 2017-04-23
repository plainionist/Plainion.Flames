
namespace Plainion.Flames.Modules.Streams
{
    class TraceLineFactory : ITraceLineFactory
    {
        private TraceModelBuilder myBuilder;

        public TraceLineFactory( TraceModelBuilder builder )
        {
            myBuilder = builder;
        }

        public EnteringTraceLine CreateEnteringLine( long time, int processId, int threadId, 
            string module, string callNamespace, string callClass, string methodName )
        {
            var method = myBuilder.CreateMethod( module, callNamespace, callClass, methodName );
            return new EnteringTraceLine( time )
            {
                ProcessId = processId,
                ThreadId = threadId,
                Method = method
            };
        }

        public LeavingTraceLine CreateLeavingLine( long time, int processId, int threadId, 
            string module, string callNamespace, string callClass, string methodName )
        {
            var method = myBuilder.CreateMethod( module, callNamespace, callClass, methodName );
            return new LeavingTraceLine( time )
            {
                ProcessId = processId,
                ThreadId = threadId,
                Method = method
            };
        }
    }
}
