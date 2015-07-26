using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames
{
    public class PresentationFactory
    {
        private const int EstimatedAverageCallstackDepth = 25;

        private IColorLut myColorLut;

        public PresentationFactory()
        {
            myColorLut = new DefaultColorLut();
        }

        public bool InterpolateBrokenStackCalls { get; set; }

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

            var activities = InterpolateBrokenStackCalls ?
                CreateInterpolatedActivities( flame, callstacks ) :
                CreateActivities( flame, callstacks );

            flame.SetActivities( activities );

            return flame;
        }

        private IReadOnlyCollection<Activity> CreateInterpolatedActivities( Flame flame, IReadOnlyList<Call> callstacks )
        {
            var nonBrokenCallstacks = callstacks
                .Where( c => !c.Method.IsBrokenCallstack() )
                .ToList();

            var activities = new List<Activity>( nonBrokenCallstacks.Count * EstimatedAverageCallstackDepth );

            CreateActivitiesFromStacksAndTryMerge( flame, nonBrokenCallstacks, null, activities );

            return activities;
        }

        private IReadOnlyCollection<Activity> CreateActivities( Flame flame, IReadOnlyList<Call> callstacks )
        {
            var activities = new List<Activity>( callstacks.Count * EstimatedAverageCallstackDepth );

            foreach( var stack in callstacks )
            {
                CreateActivitiesFromStack( flame, stack, null, activities );
            }

            return activities;
        }

        private void CreateActivitiesFromStack( Flame flame, Call call, Activity parentActivity, IList<Activity> allActivitiesInFlame )
        {
            var activity = new Activity( flame, call );
            activity.Parent = parentActivity;

            allActivitiesInFlame.Add( activity );

            foreach( var child in call.Children )
            {
                CreateActivitiesFromStack( flame, child, activity, allActivitiesInFlame );
            }
        }

        private void CreateActivitiesFromStacksAndTryMerge( Flame flame, IReadOnlyList<Call> calls, Activity parentActivity, IList<Activity> allActivitiesInFlame )
        {
            var callsToMerge = new List<Call>();
            foreach( var call in calls )
            {
                if( callsToMerge.Count == 0 || callsToMerge[ 0 ].Method.Equals( call.Method ) )
                {
                    callsToMerge.Add( call );
                    continue;
                }

                MergeCalls( flame, callsToMerge, parentActivity, allActivitiesInFlame );

                callsToMerge.Clear();
                callsToMerge.Add( call );
            }

            if( callsToMerge.Count > 0 )
            {
                MergeCalls( flame, callsToMerge, parentActivity, allActivitiesInFlame );
            }
        }

        private void MergeCalls( Flame flame, IReadOnlyList<Call> callsToMerge, Activity parentActivity, IList<Activity> allActivitiesInFlame )
        {
            if( callsToMerge.Count == 1 )
            {
                CreateActivitiesFromStack( flame, callsToMerge[ 0 ], parentActivity, allActivitiesInFlame );
                return;
            }

            var first = callsToMerge[ 0 ];
            var last = callsToMerge[ callsToMerge.Count - 1 ];

            var mergedCall = new Call( first.Thread, first.Start, first.Method );
            mergedCall.SetEnd( last.End );

            var activity = new Activity( flame, mergedCall );
            activity.Parent = parentActivity;

            allActivitiesInFlame.Add( activity );

            // TODO: we do not want to merge everything - we want to merge only those callstacks which
            // were previously splitted because of broken stacks in between
            
            var allChildren = callsToMerge
                .SelectMany( call => call.Children )
                .ToList();

            foreach( var child in allChildren )
            {
                mergedCall.AddChild( child );
            }

            CreateActivitiesFromStacksAndTryMerge( flame, allChildren, activity, allActivitiesInFlame );
        }
    }
}
