using System.Collections.Generic;
using Plainion.Flames.Model;

namespace Plainion.Flames.Presentation
{
    class DefaultPresentationBuilder : AbstractPresentationBuilder
    {
        protected override IReadOnlyCollection<Activity> CreateActivities( Flame flame, IReadOnlyList<Call> callstacks )
        {
            var activities = new List<Activity>( callstacks.Count * EstimatedAverageCallstackDepth );

            foreach( var stack in callstacks )
            {
                CreateActivitiesFromStack( flame, stack, null, activities );
            }

            return activities;
        }
    }
}
