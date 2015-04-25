using System.Diagnostics.Tracing;
using System.Windows;
using Plainion.Flames.Presentation;

namespace Plainion.Flames
{
    // https://github.com/Microsoft/dotnetsamples/blob/master/Microsoft.Diagnostics.Tracing/EventSource/docs/EventSource.md
    [EventSource( Name = "Plainion-Flames" )]
    public sealed class FlamesEventSource : EventSource
    {
        public static FlamesEventSource Log = new FlamesEventSource();

        public class Keywords
        {
            public const EventKeywords Rendering = ( EventKeywords )0x0001;
            public const EventKeywords Presentation = ( EventKeywords )0x0002;
        }

        public class Tasks
        {
            public const EventTask Render = ( EventTask )0x1;
            public const EventTask UpdateCallVisibility = ( EventTask )0x2;
        }

        [NonEvent]
        internal void RenderingStarted( int ObjectId, double RenderAreaWidth, double RenderAreaHeight )
        {
            if( IsEnabled() )
            {
                RenderingStarted( ObjectId, ( int )RenderAreaWidth, ( int )RenderAreaHeight );
            }
        }

        [Event( 1, Keywords = Keywords.Rendering, Task = Tasks.Render, Opcode = EventOpcode.Start )]
        private void RenderingStarted( int ObjectId, int RenderAreaWidth, int RenderAreaHeight )
        {
            WriteEvent( 1, ObjectId, RenderAreaWidth, RenderAreaHeight );
        }

        [Event( 2, Keywords = Keywords.Rendering, Task = Tasks.Render, Opcode = EventOpcode.Stop )]
        internal void RenderingFinished( int ObjectId, int RenderingCandidatesCount, int RenderedCallsCount )
        {
            if( IsEnabled() )
            {
                WriteEvent( 2, ObjectId, RenderingCandidatesCount, RenderedCallsCount );
            }
        }

        [Event( 3, Keywords = Keywords.Presentation, Task = Tasks.UpdateCallVisibility, Opcode = EventOpcode.Start )]
        public void CallVisibilityModifying( int ObjectId )
        {
            if( IsEnabled() )
            {
                WriteEvent( 3, ObjectId );
            }
        }

        [Event( 4, Keywords = Keywords.Presentation, Task = Tasks.UpdateCallVisibility, Opcode = EventOpcode.Stop )]
        public void CallVisibilityModified( int ObjectId )
        {
            if( IsEnabled() )
            {
                WriteEvent( 4, ObjectId );
            }
        }
    }
}
