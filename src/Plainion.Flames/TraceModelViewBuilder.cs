using System;
using Plainion.Flames.Model;

namespace Plainion.Flames
{
    public class TraceModelViewBuilder
    {
        private TraceLog myView;

        public TraceModelViewBuilder( TraceLog model )
        {
            myView = new TraceLog( model.Symbols );
        }

        public ITraceLog Complete()
        {
            return myView;
        }

        public void SetCreationTime( DateTime timestamp )
        {
            myView.CreationTime = timestamp.ToUniversalTime();
        }

        public void SetTraceDuration( long duration )
        {
            myView.TraceDuration = duration;
        }

        public void Add( TraceThread traceThread )
        {
            myView.Add( traceThread, traceThread.Process.Log.GetCallstacks( traceThread ) );
        }

        public void Add( IAssociatedEvents events )
        {
            myView.Add( events );
        }
    }
}
