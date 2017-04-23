using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;

namespace Plainion.Flames.Presentation
{
    class SummingCallsPresentationBuilder : AbstractPresentationBuilder
    {
        protected override IReadOnlyCollection<Activity> CreateActivities( Flame flame, IReadOnlyList<Call> callstacks )
        {
            var activities = new List<Activity>( callstacks.Count * EstimatedAverageCallstackDepth );

            CreateActivitiesFromStacksAndSumUp( flame, 100, null, callstacks, null, activities );

            return activities;
        }

        private void CreateActivitiesFromStacksAndSumUp( Flame flame, long start, Call parentCall, IReadOnlyList<Call> calls, Activity parentActivity, IList<Activity> allActivitiesInFlame )
        {
            var groupedCalls = calls
                .GroupBy( c => c.Method )
                .OrderBy( g => g.Key )
                .ToList();

            foreach( var group in groupedCalls )
            {
                var callsOfGroup = group
                    .OrderBy( c => c.Start )
                    .ToList();

                // there is at least one call due to the grouping above
                var sumCall = new Call( callsOfGroup[ 0 ].Thread, start, group.Key );

                var duration = callsOfGroup.Sum( c => c.Duration );
                sumCall.SetEnd( sumCall.Start + duration );

                if( parentCall != null )
                {
                    parentCall.AddChild( sumCall );
                }

                var activity = AddActivity( flame, sumCall, parentActivity, allActivitiesInFlame );

                var childrenOfGroup = callsOfGroup
                    .SelectMany( c => c.Children )
                    .ToList();
                var childrenStart = start + ( ( duration - childrenOfGroup.Sum( c => c.Duration ) ) / 2 );
                CreateActivitiesFromStacksAndSumUp( flame, childrenStart, sumCall, childrenOfGroup, activity, allActivitiesInFlame );

                start += duration;
            }
        }
    }
}
