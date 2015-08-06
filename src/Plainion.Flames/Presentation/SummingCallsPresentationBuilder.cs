using System.Collections.Generic;
using System.Linq;
using Plainion.Flames.Model;

namespace Plainion.Flames.Presentation
{
    class SummingCallsPresentationBuilder : AbstractPresentationBuilder
    {
        protected override IReadOnlyCollection<Activity> CreateActivities(Flame flame, IReadOnlyList<Call> callstacks)
        {
            var activities = new List<Activity>(callstacks.Count * EstimatedAverageCallstackDepth);

            CreateActivitiesFromStacksAndSumUp(flame, callstacks, null, activities);

            return activities;
        }

        private void CreateActivitiesFromStacksAndSumUp(Flame flame, IReadOnlyList<Call> calls, Activity parentActivity, IList<Activity> allActivitiesInFlame)
        {
            var groupedCalls = calls
                .GroupBy(c => c.Method)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in groupedCalls)
            {

                //var sumCall = new Call(
            }

        }
  }
}
