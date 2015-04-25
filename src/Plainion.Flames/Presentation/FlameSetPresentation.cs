using System.Collections.Generic;
using Plainion.Flames.Model;
using Microsoft.Practices.Prism.Mvvm;
using Plainion;

namespace Plainion.Flames.Presentation
{
    public class FlameSetPresentation : BindableBase
    {
        private bool myHideEmptyFlames;

        internal FlameSetPresentation( TraceLog traceLog, IColorLut colorLut )
        {
            Contract.RequiresNotNull( traceLog, "traceLog" );
            Contract.RequiresNotNull( colorLut, "colorLut" );

            Model = traceLog;
            TimelineViewport = new TimelineViewport( traceLog );
            ColorLut = colorLut;

            Flames = new List<Flame>();
            HideEmptyFlames = true;
            Selections = new SelectionModule();
        }

        public TraceLog Model { get; private set; }

        public TimelineViewport TimelineViewport { get; private set; }

        public IReadOnlyList<Flame> Flames { get; internal set; }

        public SelectionModule Selections { get; private set; }

        public IColorLut ColorLut { get; private set; }

        public bool HideEmptyFlames
        {
            get { return myHideEmptyFlames; }
            set { SetProperty( ref myHideEmptyFlames, value ); }
        }
    }
}
