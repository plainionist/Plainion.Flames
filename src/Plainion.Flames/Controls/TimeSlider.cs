using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Controls
{
    // http://msdn.microsoft.com/en-us/library/system.windows.controls.primitives.track%28VS.90%29.aspx
    // http://stackoverflow.com/questions/3116287/setting-the-scrollbar-thumb-size
    public class TimeSlider : ScrollBar
    {
        private double myOldValue;

        public TimeSlider()
        {
            Minimum = 0;
            Maximum = 0;
            Value = 0;
            Orientation = Orientation.Horizontal;

            Scroll += OnScroll;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Track.Thumb.DragCompleted += OnThumbDragCompleted;
        }

        private void OnThumbDragCompleted( object sender, DragCompletedEventArgs e )
        {
            UpdateValue();
        }

        public TimelineViewport TimelineViewport
        {
            get { return ( TimelineViewport )GetValue( TimelineViewportProperty ); }
            set { SetValue( TimelineViewportProperty, value ); }
        }

        public static DependencyProperty TimelineViewportProperty = DependencyProperty.Register( "TimelineViewport", typeof( TimelineViewport ), typeof( TimeSlider ),
             new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnTimelineViewportChanged ) ) );

        private static void OnTimelineViewportChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( ( TimeSlider )d ).OnTimelineViewportChanged( ( TimelineViewport )e.OldValue );
        }

        private void OnTimelineViewportChanged( TimelineViewport oldValue )
        {
            if( oldValue != null )
            {
                oldValue.Changed -= OnTimelineViewportChanged;
            }

            if( TimelineViewport == null )
            {
                Minimum = 0;
                Maximum = 0;
                Value = 0;
                return;
            }

            TimelineViewport.Changed += OnTimelineViewportChanged;
            Invalidate();
        }

        private void OnTimelineViewportChanged( object sender, TimelineViewportChangedEventArgs e )
        {
            if( !e.WidthChanged )
            {
                return;
            }

            Invalidate();
        }

        private void Invalidate()
        {
            Minimum = 0;
            // we add a factor here so that we can scroll quite some time using the thumb until we have to release the mouse and
            // restart the scrolling
            Maximum = Math.Min( TimelineViewport.Width * 4, TimelineViewport.Max - TimelineViewport.Min );
            ViewportSize = Maximum / 10;

            LargeChange = Maximum * 0.10;
            SmallChange = Maximum * 0.05;

            UpdateValue();
        }

        // updates the thumb value so that we can continuously scroll - except we reached the begin or end with some tolerance 
        private void UpdateValue()
        {
            var tolerance = TimelineViewport.Width * 0.1;
            if( TimelineViewport.Min - tolerance < TimelineViewport.Start && TimelineViewport.End < TimelineViewport.Max + tolerance )
            {
                Value = Maximum / 2;
                myOldValue = Value;
            }
        }

        private void OnScroll( object sender, ScrollEventArgs e )
        {
            TimelineViewport.Move( e.NewValue - myOldValue );
            myOldValue = Value;

            if( !Track.Thumb.IsDragging )
            {
                UpdateValue();
            }
        }
    }
}
