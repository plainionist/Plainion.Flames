using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;

namespace Plainion.Flames.Presentation
{
    abstract class AbstractPresentationBuilder
    {
        protected const int EstimatedAverageCallstackDepth = 25;

        protected IColorLut myColorLut;

        public AbstractPresentationBuilder()
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
            var flame = new Flame( thread, presentation.TimelineViewport, presentation.ColorLut );

            var callstacks = flame.Model.Process.Log.GetCallstacks( flame.Model );
            var activities = CreateActivities( flame, callstacks );
            flame.SetActivities( activities );

            if( flame.Activities.Count == 0 && presentation.HideEmptyFlames )
            {
                flame.Visibility = ContentVisibility.Hidden;
            }

            return flame;
        }

        protected abstract IReadOnlyCollection<Activity> CreateActivities( Flame flame, IReadOnlyList<Call> callstacks );

        protected void CreateActivitiesFromStack( Flame flame, Call call, Activity parentActivity, IList<Activity> allActivitiesInFlame )
        {
            var activity = AddActivity( flame, call, parentActivity, allActivitiesInFlame );

            foreach( var child in call.Children )
            {
                CreateActivitiesFromStack( flame, child, activity, allActivitiesInFlame );
            }
        }

        protected Activity AddActivity( Flame flame, Call call, Activity parentActivity, IList<Activity> allActivitiesInFlame )
        {
            var activity = new Activity( flame, call );
            activity.Parent = parentActivity;

            allActivitiesInFlame.Add( activity );

            return activity;
        }
    }
}
