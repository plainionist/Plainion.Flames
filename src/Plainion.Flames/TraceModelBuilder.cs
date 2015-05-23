using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Collections;
using Plainion.Flames.Model;

namespace Plainion.Flames
{
    public class TraceModelBuilder
    {
        private Dictionary<int, Method> myMethodIndex;
        private TraceLog myTraceLog;

        public TraceModelBuilder()
        {
            myMethodIndex = new Dictionary<int, Method>();

            myTraceLog = new TraceLog( new SymbolRepository() );
            myTraceLog.CreationTime = DateTime.MinValue;
            myTraceLog.TraceDuration = 0;

            ReaderContextHints = new List<object>();
        }

        public SymbolRepository Symbols { get { return myTraceLog.Symbols; } }

        public void SetCreationTime( DateTime timestamp )
        {
            if( myTraceLog.CreationTime == DateTime.MinValue )
            {
                myTraceLog.CreationTime = timestamp.ToUniversalTime();
            }
            else if( timestamp < myTraceLog.CreationTime )
            {
                myTraceLog.CreationTime = timestamp.ToUniversalTime();
            }
        }

        public void SetTraceDuration( long duration )
        {
            myTraceLog.TraceDuration = Math.Max( myTraceLog.TraceDuration, duration );
        }

        public TraceProcess CreateProcess( int processId )
        {
            return new TraceProcess( myTraceLog, processId );
        }

        public TraceThread CreateThread( TraceProcess process, int threadId )
        {
            Contract.Requires( process.Log == myTraceLog, "Process already attached to different TraceLog" );

            var thread = new TraceThread( process, threadId );

            myTraceLog.Add( thread );

            return thread;
        }

        public Method CreateMethod( string module, string callNamespace, string callClass, string methodName )
        {
            module = string.IsNullOrEmpty( module ) ? null : module;
            callNamespace = string.IsNullOrEmpty( callNamespace ) ? null : callNamespace;
            callClass = string.IsNullOrEmpty( callClass ) ? null : callClass;

            var hashCode = Method.GetHashCode( module, callNamespace, callClass, methodName );
            Method method = null;
            if( !myMethodIndex.TryGetValue( hashCode, out method ) )
            {
                method = new Method(
                   myTraceLog.Symbols.Modules.Intern( module ),
                   myTraceLog.Symbols.Namespaces.Intern( callNamespace ),
                   myTraceLog.Symbols.Classes.Intern( callClass ), myTraceLog.Symbols.Methods.Intern( methodName ) );
                myMethodIndex[ hashCode ] = method;
            }

            return method;
        }

        // TODO: have to be added manually
        public Call CreateCall( TraceThread thread, long startTime, Method method )
        {
            Contract.Requires( myTraceLog.GetThreads( thread.Process ).Any( t => t == thread ), "Thread not found in TraceLog" );

            return new Call( thread, startTime, method );
        }

        public void AddAssociatedEvents( IAssociatedEvents events )
        {
            myTraceLog.Add( events );
        }

        public void AddCallstackRoot( Call call )
        {
            myTraceLog.Add( call );
        }

        public TraceLog Complete()
        {
            try
            {
                myTraceLog.Methods = new CollectionReadonlyCollectionAdapter<Method>( myMethodIndex.Values );

                myTraceLog.Symbols.Freeze();
                return myTraceLog;
            }
            finally
            {
                myMethodIndex = null;
                myTraceLog = null;
            }
        }

        /// <summary>
        /// Allows the ITraceReader add additional context information which will then be given
        /// back to the reader for the next read of the same trace (e.g. after re-open)
        /// </summary>
        public IList<object> ReaderContextHints { get; private set; }
    }
}
