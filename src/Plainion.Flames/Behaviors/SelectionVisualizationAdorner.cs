using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Plainion.Flames.Presentation;
using Plainion.Flames.Controls;

namespace Plainion.Flames.Behaviors
{
    internal class SelectionVisualizationAdorner : Adorner
    {
        private TimelineViewport myViewport;
        private long myStart;
        private long myEnd;
        private Brush myBrush;
        private Pen myPen;
        private FrameworkElement myContent;

        public SelectionVisualizationAdorner( FrameworkElement adornedElement, FlameView content, TimelineViewport viewport, long start, long end )
            : base( adornedElement )
        {
            myContent = content;
            myViewport = viewport;
            myStart = start;
            myEnd = end;

            myBrush = new SolidColorBrush( Colors.Gold );
            myBrush.Opacity = .2d;
            myBrush.Freeze();

            myPen = new Pen();
            myPen.Freeze();

            IsHitTestVisible = false;

            myViewport.Changed += myViewport_Changed;

            var ownerVisibilityDescriptor = DependencyPropertyDescriptor.FromProperty( FrameworkElement.IsVisibleProperty, typeof( FrameworkElement ) );
            ownerVisibilityDescriptor.AddValueChanged( AdornedElement, OnOwnerVisibilityChanged );

            Font = content.Font;
        }

        private void OnOwnerVisibilityChanged( object sender, EventArgs e )
        {
            InvalidateVisual();
        }

        public Typeface Font { get; set; }

        private void myViewport_Changed( object sender, TimelineViewportChangedEventArgs e )
        {
            InvalidateVisual();
        }

        protected override void OnRender( DrawingContext dc )
        {
            base.OnRender( dc );

            if( !AdornedElement.IsVisible )
            {
                return;
            }

            var x1 = myViewport.CalculateX( myContent.ActualWidth, myStart );
            var x2 = myViewport.CalculateX( myContent.ActualWidth, myEnd );

            dc.DrawRectangle( myBrush, myPen, new Rect( new Point( x1, 0 ), new Point( x2, RenderSize.Height ) ) );

            var tx = new FormattedText(
                string.Format( "{0}{1}{2}{3}{4}",
                    myViewport.GetTimeString( myStart ),
                    Environment.NewLine,
                    myViewport.GetTimeString( myEnd ),
                    Environment.NewLine,
                    myViewport.GetTimeString( myEnd - myStart ) ),
                CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                Font, 11, Brushes.Black );

            dc.DrawText( tx, new Point( x1 + 5, 5 ) );
        }
    }
}
