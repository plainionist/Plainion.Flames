using System.Collections.Generic;
using System.Collections.ObjectModel;
using Prism.Mvvm;
using Plainion.Flames.Model;

namespace Plainion.Flames.Presentation
{
    public class FlameSetPresentation : BindableBase
    {
        private bool myHideEmptyFlames;
        // http://referencesource.microsoft.com/#PresentationFramework/Framework/MS/Internal/Data/ViewManager.cs
        // we use ObservableCollection here to avoid that ViewManager holds a strong ref to our collection.
        // this will be removed in some time so it is not a real leak (see comments in linked source code) but as
        // we hold havy data here (flames) we want to avoid holding it longer than necessary
        private ObservableCollection<Flame> myFlames;

        internal FlameSetPresentation( TraceLog traceLog, IColorLut colorLut )
        {
            Contract.RequiresNotNull( traceLog, "traceLog" );
            Contract.RequiresNotNull( colorLut, "colorLut" );

            Model = traceLog;
            TimelineViewport = new TimelineViewport( traceLog );
            ColorLut = colorLut;

            Flames = new ObservableCollection<Flame>();

            HideEmptyFlames = true;
            Selections = new SelectionModule();
        }

        public TraceLog Model { get; private set; }

        public TimelineViewport TimelineViewport { get; private set; }

        /// <summary>
        /// Creates a copy.
        /// Does not notify changes.
        /// </summary>
        public IEnumerable<Flame> Flames
        {
            get { return myFlames; }
            internal set
            {
                myFlames = value == null ? null : new ObservableCollection<Flame>( value );
            }
        }

        public SelectionModule Selections { get; private set; }

        public IColorLut ColorLut { get; private set; }

        public bool HideEmptyFlames
        {
            get { return myHideEmptyFlames; }
            set
            {
                if( SetProperty( ref myHideEmptyFlames, value ) )
                {
                    foreach( var flame in myFlames )
                    {
                        ApplyEmptyBehavior( flame, flame.IsEmpty() );
                    }
                }
            }
        }

        public void ApplyEmptyBehavior( Flame flame, bool isEmpty )
        {
            if( isEmpty && myHideEmptyFlames )
            {
                flame.Visibility = ContentVisibility.Hidden;
            }
            else if( isEmpty && !myHideEmptyFlames )
            {
                flame.Unhide();
            }
            else if( !isEmpty && flame.Visibility == ContentVisibility.Hidden )
            {
                flame.Unhide();
            }
        }
    }
}
