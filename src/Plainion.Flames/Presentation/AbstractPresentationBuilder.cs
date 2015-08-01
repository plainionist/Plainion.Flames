using System.Collections.Generic;
using Plainion.Flames.Model;

namespace Plainion.Flames.Presentation
{
    class AbstractPresentationBuilder
    {
        protected const int EstimatedAverageCallstackDepth = 25;

        protected IColorLut myColorLut;

        public AbstractPresentationBuilder()
        {
            myColorLut = new DefaultColorLut();
        }

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
