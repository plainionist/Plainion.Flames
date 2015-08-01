using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;

namespace Plainion.Flames.Presentation
{
    class InterpolatingBrokenStacksPresentationBuilder : AbstractPresentationBuilder
    {
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

        private Flame CreateFlame( TraceThread thread, TimelineViewport timelineViewport, IColorLut colorLut )
        {
            var flame = new Flame( thread, timelineViewport, colorLut );

            var callstacks = flame.Model.Process.Log.GetCallstacks( flame.Model );

            var activities = CreateInterpolatedActivities( flame, callstacks );

            flame.SetActivities( activities );

            return flame;
        }

        private IReadOnlyCollection<Activity> CreateInterpolatedActivities( Flame flame, IReadOnlyList<Call> callstacks )
        {
            var nonBrokenCallstacks = callstacks
                .Where( c => !c.Method.IsBrokenCallstack() )
                .Select( c => new[] { c } )
                .ToList();

            var activities = new List<Activity>( nonBrokenCallstacks.Count * EstimatedAverageCallstackDepth );

            CreateActivitiesFromStacksAndTryMerge( flame, nonBrokenCallstacks, null, activities );

            return activities;
        }

        // recursive merge algorithm (for every callstack level):
        // - for two groups: merge most right call of LHS group with the most left cal of the RHS group (if both call same method)
        // - if one group consists only of a single call it will be merged with LHS group and RHS group
        private void CreateActivitiesFromStacksAndTryMerge( Flame flame, IReadOnlyList<IReadOnlyList<Call>> callGroups, Activity parentActivity, IList<Activity> allActivitiesInFlame )
        {
            var callsToMerge = new List<Call>();
            foreach( var calls in callGroups )
            {
                if( calls.Count == 0 )
                {
                    // nothing to merge -> just ignore
                    continue;
                }

                if( callsToMerge.Count == 0 )
                {
                    // merge start -> just take the most right call
                    callsToMerge.Add( calls[ calls.Count - 1 ] );

                    // build activities for remaining calls in this group (recursively)
                    for( int i = 0; i < calls.Count - 1; ++i )
                    {
                        CreateActivitiesFromStack( flame, calls[ i ], parentActivity, allActivitiesInFlame );
                    }

                    continue;
                }

                // now we have already s.th. considered for merging

                int idx = 0;

                // does the most left one fit?
                if( callsToMerge[ 0 ].Method.Equals( calls[ 0 ].Method ) )
                {
                    callsToMerge.Add( calls[ 0 ] );
                    idx++;

                    if( calls.Count == 1 )
                    {
                        // only one call in this group
                        // -> continue searching for merge candidates in the other groups
                        continue;
                    }
                }

                // either current call group does not match to merge candidates 
                // OR there is more than 1 call in current call group

                // stop collecting merge candidates - we do not merge within groups
                CreateMergedActivityRecursively( flame, callsToMerge, parentActivity, allActivitiesInFlame );

                // start new collection of merge candidates with the most right call
                callsToMerge.Clear();
                callsToMerge.Add( calls[ calls.Count - 1 ] );

                // build activities for remaining calls in this group (recursively)
                for( ; idx < calls.Count - 1; ++idx )
                {
                    CreateActivitiesFromStack( flame, calls[ idx ], parentActivity, allActivitiesInFlame );
                }
            }

            if( callsToMerge.Count > 0 )
            {
                // unmerged merge candidates left -> merge now
                CreateMergedActivityRecursively( flame, callsToMerge, parentActivity, allActivitiesInFlame );
            }
        }

        private void CreateMergedActivityRecursively( Flame flame, List<Call> callsToMerge, Activity parentActivity, IList<Activity> allActivitiesInFlame )
        {
            // merge all calls if necessary
            var call = MergeCallsOnDemand( callsToMerge );

            // new activity
            var activity = AddActivity( flame, call, parentActivity, allActivitiesInFlame );

            // continue with children
            // (candidate list should have exactly one call per group passed into this method)
            var childGropus = callsToMerge
                .Select( c => c.Children )
                .ToList();

            CreateActivitiesFromStacksAndTryMerge( flame, childGropus, activity, allActivitiesInFlame );
        }

        private static Call MergeCallsOnDemand( IReadOnlyList<Call> callsToMerge )
        {
            if( callsToMerge.Count == 1 )
            {
                return callsToMerge[ 0 ];
            }

            var first = callsToMerge[ 0 ];
            var last = callsToMerge[ callsToMerge.Count - 1 ];

            var mergedCall = new Call( first.Thread, first.Start, first.Method );
            mergedCall.SetEnd( last.End );

            var allChildren = callsToMerge
                .SelectMany( call => call.Children )
                .ToList();

            foreach( var child in allChildren )
            {
                mergedCall.AddChild( child );
            }

            return mergedCall;
        }
    }
}
