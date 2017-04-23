
namespace Plainion.Flames.Modules.Streams
{
    public interface ITraceLineFactory
    {
        /// <summary>
        /// Time is relative time to trace start.
        /// </summary>
        EnteringTraceLine CreateEnteringLine( long time, int processId, int threadId, string module, string callNamespace, string callClass, string methodName );

        /// <summary>
        /// Time is relative time to trace start.
        /// </summary>
        LeavingTraceLine CreateLeavingLine( long time, int processId, int threadId, string module, string callNamespace, string callClass, string methodName );
    }
}
