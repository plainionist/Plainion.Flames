using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;
using Plainion.Flames.Controls;

namespace Plainion.Flames
{
    public class PresentationFactory
    {
        private IColorLut myColorLut;

        public PresentationFactory()
        {
            myColorLut = new DefaultColorLut();
        }

        public FlameSetPresentation CreateFlameSetPresentation( TraceLog traceLog )
        {
            var presentation = new FlameSetPresentation( traceLog, myColorLut );

            presentation.Flames = traceLog.Processes
                .SelectMany( p => traceLog.GetThreads( p ) )
                .OrderBy( c => c.Process.ProcessId )
                .ThenBy( c => c.ThreadId )
                .Select( t => CreateFlame( t, presentation ) )
                .ToList();

            return presentation;
        }

        private Flame CreateFlame( TraceThread thread, FlameSetPresentation presentation )
        {
            var flamePresentation = CreateFlame( thread, presentation.TimelineViewport, presentation.ColorLut );

            if( flamePresentation.Activities.Count == 0 && presentation.HideEmptyFlames )
            {
                flamePresentation.Visibility = ContentVisibility.Hidden;
            }

            return flamePresentation;
        }

        public Flame CreateFlame( TraceThread thread, TimelineViewport timelineViewport, IColorLut colorLut )
        {
            var flame = new Flame( thread, timelineViewport, colorLut );

            var callstacks = flame.Model.Process.Log.GetCallstacks( flame.Model );

            // TODO: can we calculate the size more presice?
            var calls = new List<Activity>( callstacks.Count * 25 );

            foreach( var stack in callstacks )
            {
                Process( flame, stack, null, calls );
            }

            flame.SetActivities( calls );

            return flame;
        }

        private void Process( Flame flame, Call call, Activity parentActivity, IList<Activity> allActivitiesInFlame )
        {
            var activity = new Activity( flame, call );
            activity.Parent = parentActivity;

            allActivitiesInFlame.Add( activity );

            foreach( var child in call.Children )
            {
                Process( flame, child, activity, allActivitiesInFlame );
            }
        }
    }
}
