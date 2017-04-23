using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Plainion.Flames.Model;
using Prism.Mvvm;
using Plainion;

namespace Plainion.Flames.Presentation
{
    public class Flame : BindableBase
    {
        public const int CollapsedHeight = 40;
        public const int MaxYIndentDepth = 7;
        public const int ExpandedActivityHeight = 16;

        private int myHeight;
        private bool myIsExpanded;
        private ContentVisibility myEffectiveVisibility;
        private ContentVisibility myExplicitVisibility;
        private bool myIsRenderingEnabled;

        internal Flame( TraceThread trace, TimelineViewport viewport, IColorLut colorLut )
        {
            Contract.RequiresNotNull( trace, "trace" );
            Contract.RequiresNotNull( viewport, "viewport" );
            Contract.RequiresNotNull( colorLut, "colorLut" );

            Model = trace;
            TimelineViewport = viewport;
            ColorLut = colorLut;
            
            Visibility = ContentVisibility.Visible;
            IsExpanded = false;
            Height = CollapsedHeight;
            MaxVisibleDepth = 0;
            IsRenderingEnabled = true;

            Bookmarks = new BookmarksModule( this );

            Model.PropertyChanged += OnModelPropertyChanged;
            Model.Process.PropertyChanged += OnModelPropertyChanged;
        }

        private void OnModelPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "Name" )
            {
                OnPropertyChanged( "Name" );
            }
        }

        public TraceThread Model { get; private set; }

        public TimelineViewport TimelineViewport { get; private set; }
        
        public BookmarksModule Bookmarks { get; private set; }

        public IColorLut ColorLut { get; private set; }
        
        public IReadOnlyList<Activity> Activities { get; private set; }

        internal void SetActivities( IReadOnlyCollection<Activity> activities )
        {
            var orderedActivities = new List<Activity>( activities.Count );

            // ordering is very important because certain rendering optimizations are based on this fact
            orderedActivities.AddRange( activities.OrderBy( a => a.Model.Depth ) );

            Activities = orderedActivities;

            // after ordering and as there is no visibility mask applied yet we can
            // safely just set the highest level as MaxVisibleLevel
            if( activities.Count == 0 )
            {
                MaxVisibleDepth = 0;
            }
            else
            {
                MaxVisibleDepth = Activities[ Activities.Count - 1 ].Model.Depth;
            }
        }

        public int ProcessId { get { return Model.Process.ProcessId; } }

        public int ThreadId { get { return Model.ThreadId; } }

        public string Name
        {
            get
            {
                if( Model.Name != null )
                {
                    return Model.Name;
                }

                return Model.Process.Name;
            }
            set { Model.Name = value; }
        }

        public int Height
        {
            get { return myHeight; }
            private set { SetProperty( ref myHeight, value ); }
        }

        public ContentVisibility Visibility
        {
            get { return myEffectiveVisibility; }
            set
            {
                if( myEffectiveVisibility != ContentVisibility.Hidden && value == ContentVisibility.Hidden )
                {
                    myExplicitVisibility = myEffectiveVisibility;
                }

                if( SetProperty( ref myEffectiveVisibility, value ) )
                {
                    if( myEffectiveVisibility != ContentVisibility.Visible )
                    {
                        IsExpanded = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sets visibility back from "implicitly hidden" to explict visibility as set before it got hidden.
        /// </summary>
        public void Unhide()
        {
            Visibility = myExplicitVisibility;
        }

        public bool IsExpanded
        {
            get { return myIsExpanded; }
            set
            {
                if( myIsExpanded == value )
                {
                    return;
                }

                // first adjust height - then notify "IsExpanded"
                if( value )
                {
                    Height = CollapsedHeight + ExpandedActivityHeight * MaxVisibleDepth;
                }
                else
                {
                    Height = CollapsedHeight;
                }

                SetProperty( ref myIsExpanded, value );
            }
        }

        public bool IsRenderingEnabled
        {
            get { return myIsRenderingEnabled; }
            set { SetProperty( ref myIsRenderingEnabled, value ); }
        }

        public int MaxVisibleDepth { get; private set; }

        private void UpdateMaxVisibleDepth()
        {
            MaxVisibleDepth = -1;

            foreach( var a in Activities )
            {
                // we have to do it from outside here because we esp. have to update the once for which the visibility mask did not toggle!
                // AND: their parents
                a.ResetVisibleDepth();
            }

            foreach( var a in Activities.Where( c => c.VisiblityMask == 0 ) )
            {
                MaxVisibleDepth = Math.Max( a.VisibleDepth, MaxVisibleDepth );
            }

            if( Visibility == ContentVisibility.Visible && IsExpanded )
            {
                Height = CollapsedHeight + ExpandedActivityHeight * MaxVisibleDepth;
            }
        }

        public IFlameModifier CreateModifier()
        {
            return new FlameModifier( this );
        }

        private class FlameModifier : IFlameModifier
        {
            private Flame myFlame;

            public FlameModifier( Flame flame )
            {
                myFlame = flame;

                HasVisibilityMaskChanged = false;
            }

            public bool HasVisibilityMaskChanged { get; private set; }

            public void SetVisibilityMask( Activity activity, uint mask )
            {
                if( !HasVisibilityMaskChanged && mask != activity.VisiblityMask )
                {
                    HasVisibilityMaskChanged = true;
                }

                activity.VisiblityMask = mask;
            }

            public void Dispose()
            {
                if( HasVisibilityMaskChanged )
                {
                    myFlame.UpdateMaxVisibleDepth();
                    myFlame.OnPropertyChanged( "VisiblityMask" );
                }
            }
        }
    }
}
