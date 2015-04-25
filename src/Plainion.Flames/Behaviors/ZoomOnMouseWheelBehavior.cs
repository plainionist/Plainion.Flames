using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Plainion.Flames.Presentation;
using Plainion.Flames.Controls;

namespace Plainion.Flames.Behaviors
{
    public class ZoomOnMouseWheelBehavior : Behavior<FlameView>
    {
        public TimelineViewport TimelineViewport
        {
            get { return ( TimelineViewport )GetValue( TimelineViewportProperty ); }
            set { SetValue( TimelineViewportProperty, value ); }
        }

        public static readonly DependencyProperty TimelineViewportProperty = DependencyProperty.Register( "TimelineViewport", typeof( TimelineViewport ),
            typeof( ZoomOnMouseWheelBehavior ), new FrameworkPropertyMetadata( null ) );
        
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewMouseWheel += OnMouseWheel;
        }

        private void OnMouseWheel( object sender, MouseWheelEventArgs e )
        {
            var timePos = TimelineViewport.CalculateTime(AssociatedObject.ActualWidth, ( int )e.GetPosition( AssociatedObject ).X );

            var scale = Math.Sign( e.Delta ) * 0.1;
            var deltaLeft = ( timePos - TimelineViewport.Start ) * scale;
            var deltaRight = ( TimelineViewport.End - timePos ) * scale;

            var min = ( long )Math.Max( TimelineViewport.Min, TimelineViewport.Start + deltaLeft );
            var max = ( long )Math.Min( TimelineViewport.Max, TimelineViewport.End - deltaRight );
            TimelineViewport.Set( min, max );

            e.Handled = true;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseWheel -= OnMouseWheel;

            base.OnDetaching();
        }
    }
}
