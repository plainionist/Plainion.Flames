
namespace Plainion.Flames.Modules.Streams
{
    public interface IParserContext
    {
        /// <summary>
        /// Time is relative time to trace start.
        /// </summary>
        EnteringTraceLine CreateEnteringLine( long time, int processId, int threadId, string module, string callNamespace, string callClass, string methodName );

        /// <summary>
        /// Time is relative time to trace start.
        /// </summary>
        LeavingTraceLine CreateLeavingLine( long time, int processId, int threadId, string module, string callNamespace, string callClass, string methodName );

        /// <summary>
        /// To be called by the parser to emit a new trace line.
        /// </summary>
        void Emit( TraceLineBase line );

        /// <summary>
        /// To be called by the parser to emit additional trace file infos.
        /// </summary>
        void Emit( TraceInfo info );
    }
}
