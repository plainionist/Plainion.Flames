using System;
using System.Collections.Generic;
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
    public class FlameView : FrameworkElement
    {
        private DrawingVisual myCanvas;
        private Flame myFlame;
        private Pen myGridPen;
        private Brush myLegendBrush;

        public FlameView()
        {
            UseLayoutRounding = true;
            RenderOptions.SetEdgeMode( this, EdgeMode.Aliased );

            myCanvas = new DrawingVisual();
            AddVisualChild( myCanvas );
            AddLogicalChild( myCanvas );

            var brush = new SolidColorBrush( Colors.Blue );
            brush.Freeze();

            myGridPen = new Pen( brush, 1 );
            myGridPen.DashStyle = DashStyles.Dot;
            myGridPen.Freeze();

            myLegendBrush = new SolidColorBrush( Colors.LightGray );
            myLegendBrush.Opacity = 0.75;
            myLegendBrush.Freeze();
        }

        public bool ShowScaleLines
        {
            get { return ( bool )GetValue( ShowScaleLinesProperty ); }
            set { SetValue( ShowScaleLinesProperty, value ); }
        }

        public static DependencyProperty ShowScaleLinesProperty = DependencyProperty.Register( "ShowScaleLines", typeof( bool ), typeof( FlameView ),
             new FrameworkPropertyMetadata( true, new PropertyChangedCallback( OnShowScaleLines ) ) );

        private static void OnShowScaleLines( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( ( FlameView )d ).Invalidate();
        }

        public Typeface Font
        {
            get { return ( Typeface )GetValue( FontProperty ); }
            set { SetValue( FontProperty, value ); }
        }

        public static DependencyProperty FontProperty = DependencyProperty.Register( "Font", typeof( Typeface ), typeof( FlameView ),
             new FrameworkPropertyMetadata(
                 new Typeface( new FontFamily( "GenericSansSerif" ), FontStyles.Normal, FontWeights.DemiBold, FontStretches.Normal ),
                 new PropertyChangedCallback( OnFontChanged ) ) );

        private static void OnFontChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( ( FlameView )d ).Invalidate();
        }

        public Brush Background
        {
            set { SetValue( BackgroundProperty, value ); }
            get { return ( Brush )GetValue( BackgroundProperty ); }
        }

        public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner( typeof( FlameView ) );

        public IReadOnlyCollection<Activity> RenderedActivities
        {
            get { return ( IReadOnlyCollection<Activity> )GetValue( RenderedActivitiesProperty ); }
            set { SetValue( RenderedActivitiesProperty, value ); }
        }

        public static readonly DependencyProperty RenderedActivitiesProperty = DependencyProperty.Register( "RenderedActivities", typeof( IReadOnlyCollection<Activity> ),
            typeof( FlameView ), new FrameworkPropertyMetadata( null ) );

        public Flame Flame
        {
            get { return ( Flame )GetValue( FlameProperty ); }
            set { SetValue( FlameProperty, value ); }
        }

        public static DependencyProperty FlameProperty = DependencyProperty.Register( "Flame", typeof( Flame ), typeof( FlameView ),
             new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnFlameChanged ) ) );

        private static void OnFlameChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( ( FlameView )d ).OnFlameChanged();
        }

        private void OnFlameChanged()
        {
            if( myFlame != null )
            {
                myFlame.PropertyChanged -= OnModelPropertyChanged;
                myFlame.Bookmarks.SelectedItems.CollectionChanged -= OnSelectedBookmarksChanged;
                myFlame.TimelineViewport.Changed -= OnViewportChanged;
            }

            if( Flame == null )
            {
                return;
            }

            myFlame = Flame;

            myFlame.PropertyChanged += OnModelPropertyChanged;
            myFlame.Bookmarks.SelectedItems.CollectionChanged += OnSelectedBookmarksChanged;
            myFlame.TimelineViewport.Changed += OnViewportChanged;

            Invalidate();
        }

        private void OnModelPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "IsRenderingEnabled" )
            {
                if( myFlame.IsRenderingEnabled && RenderedActivities == null )
                {
                    // only invalidate IF rendering got enabled and latest rendering output is no longer valid
                    Invalidate();
                }
            }
            else if( e.PropertyName == "VisiblityMask" )
            {
                Invalidate();
            }
            else if( e.PropertyName == "Visibility" )
            {
                Invalidate();
            }
        }

        private void OnSelectedBookmarksChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            Invalidate();
        }

        private void OnViewportChanged( object sender, EventArgs e )
        {
            Invalidate();
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
            // mark the latest rendering output as invalid
            RenderedActivities = null;

            // Clear the view
            myCanvas.Children.Clear();

            // do rendering only if we have to update the views content now
            if( myFlame != null && ActualWidth > 0 && myFlame.IsRenderingEnabled
                && myFlame.Visibility == ContentVisibility.Visible )
            {
                var child = new DrawingVisual();
                Render( child );
                myCanvas.Children.Add( child );
            }

            InvalidateVisual();
            //UpdateLayout();
        }

        // does the actual rendering
        private void Render( DrawingVisual canvas )
        {
            int numRenderingCandidates = -1;
            try
            {
                FlamesEventSource.Log.RenderingStarted( GetHashCode(), ActualWidth, ActualHeight );

                using( var dc = canvas.RenderOpen() )
                {
                    dc.PushClip( new RectangleGeometry( new Rect( RenderSize ) ) );

                    RenderScaleLine( dc );
                    numRenderingCandidates = RenderActivities( dc );
                    RenderBookmarks( dc );
                    dc.Pop();
                }
            }
            finally
            {
                FlamesEventSource.Log.RenderingFinished( GetHashCode(), numRenderingCandidates, RenderedActivities.Count );
            }
        }

        private void RenderScaleLine( DrawingContext dc )
        {
            if( !ShowScaleLines )
            {
                return;
            }

            var steps = myFlame.TimelineViewport.GetTimeScaleSteps( ActualWidth ).ToList();
            foreach( var time in steps )
            {
                int x = myFlame.TimelineViewport.CalculateX( ActualWidth, time );

                dc.DrawLine( myGridPen, new Point( x, 0 ), new Point( x, ActualHeight ) );
            }
        }

        private int RenderActivities( DrawingContext dc )
        {
            var renderCandidates = myFlame.Activities
                .Where( c => c.IsVisible )
                .Where( c => c.StartTime < myFlame.TimelineViewport.End && c.EndTime > myFlame.TimelineViewport.Start )
                .ToList();

            using( var modifier = myFlame.CreateModifier() )
            {
                foreach( var activitiy in renderCandidates )
                {
                    ArrangeActivity( modifier, activitiy );
                }
            }

            var activitiesToRender = renderCandidates
                .Where( a => a.IsRenderingRelevant )
                .ToList();

            foreach( var a in activitiesToRender )
            {
                RenderActivity( dc, a );
            }

            RenderedActivities = activitiesToRender;

            return renderCandidates.Count;
        }

        private void ArrangeActivity( IFlameModifier modifier, Activity activity )
        {
            activity.X1 = myFlame.TimelineViewport.CalculateX( ActualWidth, activity.StartTime );
            activity.X2 = myFlame.TimelineViewport.CalculateX( ActualWidth, activity.EndTime );

            if( activity.X1 < 0 )
            {
                activity.X1 = 0;
            }

            int width = activity.X2 - activity.X1;

            if( width == 0 && activity.VisibleDepth > 0 )
            {
                activity.IsRenderingRelevant = false;
                return;
            }

            activity.IsRenderingRelevant = true;

            if( myFlame.IsExpanded || activity.VisibleDepth == 0 )
            {
                return;
            }

            if( activity.VisibleDepth > Flame.MaxYIndentDepth && width == activity.Parent.X2 - activity.Parent.X1 )
            {
                activity.Parent.IsRenderingRelevant = false;
            }
        }

        // TODO: should we avoid rendering calls which are outside of visible area?
        private bool RenderActivity( DrawingContext dc, Activity activity )
        {
            int y1;
            int height;
            activity.CalculateYandHeight( out y1, out height );

            int width = activity.X2 - activity.X1;

            var preset = Flame.ColorLut.GetPreset( activity.Model.Method );

            if( ( width < 10 && activity.VisibleDepth > 0 ) || width < 3 )
            {
                dc.DrawLine( preset.Pen, new Point( activity.X1, y1 ), new Point( activity.X1, y1 + height ) );
                return true;
            }

            dc.DrawRectangle( preset.Background, preset.Pen, new Rect( activity.X1, y1, width, height ) );

            if( width > 30 )
            {
                var tx = new FormattedText( activity.Name, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    Font, 11, preset.Foreground );

                dc.PushClip( new RectangleGeometry( new Rect( activity.X1, y1, width, height ) ) );

                bool x1Outside = activity.StartTime < myFlame.TimelineViewport.Start;
                if( height > 20 )
                {
                    dc.DrawText( tx, new Point( x1Outside ? activity.X1 - 4 : activity.X1 + 4, y1 + 2 ) );
                }
                else
                {
                    dc.DrawText( tx, new Point( x1Outside ? activity.X1 - 4 : activity.X1 + 2, y1 ) );
                }
                dc.Pop();
            }

            return true;
        }

        // TODO: can we separate it from the core impl somehow?
        private void RenderBookmarks( DrawingContext dc )
        {
            if( myFlame.Bookmarks.SelectedItems.Count == 0 )
            {
                return;
            }

            var startTime = myFlame.TimelineViewport.Start;
            var endTime = myFlame.TimelineViewport.End;

            foreach( var bookmarks in myFlame.Bookmarks.SelectedItems )
            {
                var pen = myFlame.ColorLut.GetBookmarkPreset( bookmarks.Name );
                foreach( var bTime in bookmarks.Timestamps )
                {
                    if( startTime <= bTime && bTime <= endTime )
                    {
                        var x = myFlame.TimelineViewport.CalculateX( ActualWidth, bTime );
                        dc.DrawLine( pen, new Point( x, 0 ), new Point( x, ActualHeight ) );
                    }
                }
            }
        }
    }
}
