using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.Collections;
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
            var firstNonBrokenStackIdx = callstacks.IndexOf( s => !s.Method.IsBrokenCallstack() );
            if( firstNonBrokenStackIdx == -1 )
            {
                // everyting broken?
                return new List<Activity>();
            }

            // TODO: can we calculate the size more presice?
            var activities = new List<Activity>( ( callstacks.Count - firstNonBrokenStackIdx ) * 25 );

            // ignore the broken stacks at the beginning
            for( int i = firstNonBrokenStackIdx; i < callstacks.Count; ++i )
            {
                int nextNonBrokenStack = firstNonBrokenStackIdx + 1;
                for( ; nextNonBrokenStack < callstacks.Count; ++nextNonBrokenStack )
                {
                    if( !callstacks[ nextNonBrokenStack ].Method.IsBrokenCallstack() )
                    {
                        break;
                    }
                }

                if( nextNonBrokenStack == callstacks.Count )
                {
                    // all subsequent callstacks are broken
                    // -> just process the current one and stop
                    CreateActivitiesFromStack( flame, callstacks[ i ], null, activities );
                    break;
                }

                if( i == nextNonBrokenStack )
                {
                    // no broken stacks between current stack and next stack
                    // -> just process the current one and continue
                    CreateActivitiesFromStack( flame, callstacks[ i ], null, activities );
                    continue;
                }

                // at least one broken stack in between
                // -> merge
                CreateActivitiesFromStacksAndMerge( flame, callstacks[ i ], callstacks[ nextNonBrokenStack ], null, activities );
            }

            return activities;
        }

        private IReadOnlyCollection<Activity> CreateActivities( Flame flame, IReadOnlyList<Call> callstacks )
        {
            // TODO: can we calculate the size more presice?
            var activities = new List<Activity>( callstacks.Count * 25 );

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

        private void CreateActivitiesFromStacksAndMerge( Flame flame, Call lhsCal, Call rhsCal, Activity parentActivity, IList<Activity> allActivitiesInFlame )
        {
            if( lhsCal.Method.Equals( rhsCal.Method ) )
            {
                var mergedCall = new Call( lhsCal.Thread, lhsCal.Start, lhsCal.Method );
                mergedCall.SetEnd( rhsCal.End );

                var activity = new Activity( flame, mergedCall );
                activity.Parent = parentActivity;

                allActivitiesInFlame.Add( activity );

                var children = rhsCal.Children.Any() ? lhsCal.Children.Take( lhsCal.Children.Count - 1 ) : lhsCal.Children;
                foreach( var child in children )
                {
                    mergedCall.AddChild( child );
                    CreateActivitiesFromStack( flame, child, activity, allActivitiesInFlame );
                }

                if( lhsCal.Children.Any() && rhsCal.Children.Any() )
                {
                    mergedCall.AddChild( lhsCal.Children.Last() );
                    mergedCall.AddChild( rhsCal.Children.First() );
                    CreateActivitiesFromStacksAndMerge( flame, lhsCal.Children.Last(), rhsCal.Children.First(), activity, allActivitiesInFlame );
                }

                children = lhsCal.Children.Any() ? rhsCal.Children.Skip( 1 ) : rhsCal.Children;
                foreach( var child in rhsCal.Children.Skip( 1 ) )
                {
                    mergedCall.AddChild( child );
                    CreateActivitiesFromStack( flame, child, activity, allActivitiesInFlame );
                }
            }
            else
            {
                CreateActivitiesFromStack( flame, lhsCal, parentActivity, allActivitiesInFlame );
                CreateActivitiesFromStack( flame, rhsCal, parentActivity, allActivitiesInFlame );
            }
        }
    }
}
