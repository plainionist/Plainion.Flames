using System;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.Flames.Presentation;
using Plainion.Flames.Controls;

namespace Plainion.Flames.Behaviors
{
    internal class SelectionCreationAdorner : Adorner
    {
        private int myStart;
        private int? myEnd;
        private Brush myBrush;
        private Pen myPen;
        private TimelineViewport myViewport;
        // control to do time measurements on
        private FrameworkElement myContent;

        public SelectionCreationAdorner( FrameworkElement adornedElement, FlameView content, TimelineViewport viewport, int start )
            : base( adornedElement )
        {
            myContent = content;
            myViewport = viewport;
            myStart = start;

            myBrush = new SolidColorBrush( Colors.LightBlue );
            myBrush.Opacity = .5d;
            myBrush.Freeze();

            myPen = new Pen();
            myPen.Freeze();

            Font = content.Font;
        }

        public Typeface Font { get; set; }

        protected override void OnMouseMove( MouseEventArgs e )
        {
            if( e.LeftButton == MouseButtonState.Pressed )
            {
                if( !IsMouseCaptured )
                {
                    CaptureMouse();
                }

                myEnd = ( int )e.GetPosition( this ).X;
                InvalidateVisual();

                if( Changed != null )
                {
                    Changed( this, EventArgs.Empty );
                }
            }
            else
            {
                if( IsMouseCaptured )
                {
                    ReleaseMouseCapture();
                }
            }

            e.Handled = true;
        }

        protected override void OnMouseUp( MouseButtonEventArgs e )
        {
            if( IsMouseCaptured )
            {
                ReleaseMouseCapture();
            }

            if( Finished != null )
            {
                Finished( this, EventArgs.Empty );
            }
        }

        protected override void OnRender( DrawingContext dc )
        {
            base.OnRender( dc );

            dc.DrawRectangle( Brushes.Transparent, null, new Rect( RenderSize ) );

            if( myEnd.HasValue )
            {
                dc.DrawRectangle( myBrush, myPen, new Rect( new Point( myStart, 0 ), new Point( myEnd.Value, RenderSize.Height ) ) );

                var x = Math.Min( myStart, myEnd.Value ) + 5;
                var startTime = myViewport.CalculateTime( myContent.ActualWidth, myStart );
                var endTime = myViewport.CalculateTime( myContent.ActualWidth, myEnd.Value );

                var tx = new FormattedText(
                    string.Format( "{0}{1}{2}{3}{4}",
                        myViewport.GetTimeString( startTime ),
                        Environment.NewLine,
                        myViewport.GetTimeString( endTime ),
                        Environment.NewLine,
                        myViewport.GetTimeString( endTime - startTime ) ),
                    CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    Font, 11, Brushes.Black );

                dc.DrawText( tx, new Point( x, 5 ) );
            }
        }

        public event EventHandler Finished;
        public event EventHandler Changed;
    }
}
