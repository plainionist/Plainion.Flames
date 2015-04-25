using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Controls
{
    // http://msdn.microsoft.com/en-us/magazine/dd483292.aspx
    public class TimeScaleView : FrameworkElement
    {
        private DrawingVisual myCanvas;

        public TimeScaleView()
        {
            UseLayoutRounding = true;

            myCanvas = new DrawingVisual();

            AddVisualChild( myCanvas );
            AddLogicalChild( myCanvas );
        }

        public Brush Background
        {
            set { SetValue( BackgroundProperty, value ); }
            get { return ( Brush )GetValue( BackgroundProperty ); }
        }

        public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner( typeof( TimeScaleView ) );
        
        public Typeface Font
        {
            get { return ( Typeface )GetValue( FontProperty ); }
            set { SetValue( FontProperty, value ); }
        }

        public static DependencyProperty FontProperty = DependencyProperty.Register( "Font", typeof( Typeface ), typeof( TimeScaleView ),
             new FrameworkPropertyMetadata(
                 new Typeface( "Tahoma" ),
                 new PropertyChangedCallback( OnFontChanged ) ) );

        private static void OnFontChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( ( TimeScaleView )d ).Invalidate();
        }

        public TimelineViewport TimelineViewport
        {
            get { return ( TimelineViewport )GetValue( TimelineViewportProperty ); }
            set { SetValue( TimelineViewportProperty, value ); }
        }

        public static DependencyProperty TimelineViewportProperty = DependencyProperty.Register( "TimelineViewport", typeof( TimelineViewport ), typeof( TimeScaleView ),
             new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnTimelineViewportChanged ) ) );

        private static void OnTimelineViewportChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( ( TimeScaleView )d ).OnTimelineViewportChanged( ( TimelineViewport )e.OldValue );
        }

        private void OnTimelineViewportChanged( TimelineViewport oldValue )
        {
            if( oldValue != null )
            {
                oldValue.Changed -= OnTimelineViewportChanged;
                oldValue.PropertyChanged -= OnTimelineViewportPropertyChanged;
            }

            if( TimelineViewport == null )
            {
                return;
            }

            TimelineViewport.Changed += OnTimelineViewportChanged;
            TimelineViewport.PropertyChanged += OnTimelineViewportPropertyChanged;

            Invalidate();
        }

        private void OnTimelineViewportChanged( object sender, EventArgs e )
        {
            Invalidate();
        }

        private void OnTimelineViewportPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "ShowAbsoluteTimestamps" )
            {
                Invalidate();
            }
        }

        protected override int VisualChildrenCount { get { return 1; } }

        protected override Visual GetVisualChild( int index ) { return myCanvas; }

        protected override void OnRender( DrawingContext dc )
        {
            dc.DrawRectangle( Background, null, new Rect( RenderSize ) );
        }

        protected override void OnRenderSizeChanged( SizeChangedInfo sizeInfo )
        {
            Invalidate();

            base.OnRenderSizeChanged( sizeInfo );
        }

        // called to indicate that the view is no longer up to date
        private void Invalidate()
        {
            myCanvas.Children.Clear();

            var child = new DrawingVisual();

            if( TimelineViewport != null )
            {
                Render( child );
            }

            myCanvas.Children.Add( child );

            InvalidateVisual();
            UpdateLayout();
        }

        // does the actual rendering
        private void Render( DrawingVisual canvas )
        {
            using( var dc = canvas.RenderOpen() )
            {
                dc.PushClip( new RectangleGeometry( new Rect( RenderSize ) ) );

                var viewport = TimelineViewport;
                foreach( var time in viewport.GetTimeScaleSteps( ActualWidth ).Where( ( e, i ) => i % 2 == 0 ) )
                {
                    string timeString = viewport.GetTimeString( time );

                    var tx = new FormattedText( timeString, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                        Font, 12, Brushes.Black );

                    int timeX = viewport.CalculateX( ActualWidth, time );
                    int textPos = timeX - ( int )Math.Round( tx.Width / 2 );
                    dc.DrawText( tx, new Point( textPos, ActualHeight - 18 ) );
                }

                dc.Pop();
            }
        }
    }
}
