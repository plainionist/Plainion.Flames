using System.Collections.Generic;
using Plainion;

namespace Plainion.Flames.Model
{
    public class Call
    {
        private List<Call> myChildren;
        private int? myDepth;

        internal Call( TraceThread trace, long startTime,  Method method )
        {
            Contract.RequiresNotNull( trace, "trace" );

            Thread = trace;

            Start = startTime;
            End = Start;
            Duration = 0;

            Method = method;

            myChildren = new List<Call>();
        }

        public TraceThread Thread { get; private set; }

        public Method Method { get; private set; }

        public int Depth
        {
            get
            {
                if( myDepth == null )
                {
                    myDepth = Parent == null ? 0 : Parent.Depth + 1;
                }

                return myDepth.Value;
            }
        }

        public long Start { get; private set; }

        public long End { get; private set; }

        public long Duration { get; private set; }
        
        /// <summary>
        /// Sets the end time of the call allowing to specify additional duration if start and end times from trace source
        /// are not precise enough
        /// </summary>
        public void SetEnd( long time, long duration = -1 )
        {
            End = time;
            Duration = duration > 0 ? duration : End - Start;

            if( Start == End )
            {
                End += Duration;
            }
        }

        public Call Parent { get; private set; }

        public IReadOnlyList<Call> Children { get { return myChildren; } }

        public void AddChild( Call child )
        {
            myChildren.Add( child );
            child.Parent = this;
        }
    }
}
