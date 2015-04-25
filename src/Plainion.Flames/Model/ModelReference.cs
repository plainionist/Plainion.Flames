using System;
using Plainion;

namespace Plainion.Flames.Model
{
    public class ModelReference : IEquatable<ModelReference>
    {
        public static int Undefined = -1;

        public ModelReference()
            : this( Undefined )
        {
        }

        public ModelReference( int processId )
            : this( processId, Undefined )
        {
        }

        public ModelReference( int processId, int threadId )
        {
            Contract.Requires( processId != Undefined || threadId == Undefined, "if ProcessId=Undefined then ThreadId must be undefined as well" );

            ProcessId = processId;
            ThreadId = threadId;
        }

        /// <summary>
        /// If "Undefined" the model is entire trace (system wide)
        /// </summary>
        public int ProcessId { get; private set; }

        /// <summary>
        /// If "Undefined" the model is the process itself.
        /// </summary>
        public int ThreadId { get; private set; }

        public bool Equals( ModelReference other )
        {
            return ProcessId == other.ProcessId && ThreadId == other.ThreadId;
        }

        public override bool Equals( object obj )
        {
            var other = obj as ModelReference;
            if( other == null )
            {
                return false;
            }

            return Equals( other );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ProcessId ^ ThreadId;
            }
        }
    }
}
