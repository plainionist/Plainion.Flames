using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using Plainion.Flames.Presentation;
using Plainion.Flames.Controls;
using Plainion;

namespace Plainion.Flames.Behaviors
{
    public class CallTooltipBehavior : Behavior<FlameView>
    {
        private Activity myLastHooveredCall = null;
        private ToolTip myTooltip;
        private Lazy<IList<Activity>> myModel;

        public object ToolTipContent
        {
            get { return ( object )GetValue( ToolTipContentProperty ); }
            set { SetValue( ToolTipContentProperty, value ); }
        }

        public static DependencyProperty ToolTipContentProperty = DependencyProperty.Register( "ToolTipContent", typeof( object ), typeof( CallTooltipBehavior ),
             new FrameworkPropertyMetadata() );

        protected override void OnAttached()
        {
            base.OnAttached();

            myTooltip = new ToolTip();
            myTooltip.Placement = PlacementMode.Relative;
            myTooltip.PlacementTarget = AssociatedObject;

            var renderedCallsDescriptor = DependencyPropertyDescriptor.FromProperty( FlameView.RenderedActivitiesProperty, typeof( FlameView ) );
            renderedCallsDescriptor.AddValueChanged( AssociatedObject, OnRenderedCallsChanged );

            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;

            AssociatedObject.Unloaded += AssociatedObject_Unloaded;

            OnRenderedCallsChanged( null, null );
        }

        private void AssociatedObject_Unloaded( object sender, RoutedEventArgs e )
        {
            Cleanup();
        }

        private void OnRenderedCallsChanged( object sender, EventArgs e )
        {
            if( AssociatedObject.RenderedActivities == null )
            {
                return;
            }

            myModel = new Lazy<IList<Activity>>( () =>
                AssociatedObject.RenderedActivities
                    .OrderByDescending( a => a.VisibleDepth )
                    .ToList() );
        }

        private void AssociatedObject_MouseMove( object sender, MouseEventArgs e )
        {
            Contract.Invariant( ToolTipContent != null, "ToolTipContent not set" );

            if( AssociatedObject.RenderedActivities == null || AssociatedObject.RenderedActivities.Count == 0 )
            {
                return;
            }

            var mousePos = e.GetPosition( AssociatedObject );

            if( mousePos.Y < 0 || mousePos.Y > AssociatedObject.ActualHeight )
            {
                // not inside associated object
                myTooltip.IsOpen = false;
                return;
            }

            int yPos = ( int )( mousePos.Y );

            Activity hit = null;

            foreach( var call in myModel.Value )
            {
                int y1;
                int height;
                call.CalculateYandHeight( out y1, out height );

                if( yPos >= y1 && yPos <= y1 + height && mousePos.X >= call.X1 && mousePos.X <= call.X2 )
                {
                    hit = call;
                    break;
                }
            }

            if( hit == null )
            {
                myTooltip.IsOpen = false;
                myLastHooveredCall = null;
            }
            else if( hit != myLastHooveredCall )
            {
                var frameworkElement = ToolTipContent as FrameworkElement;
                if( frameworkElement != null )
                {
                    frameworkElement.DataContext = hit;
                }

                // setting "null" seems to be necessary so that visual tree gets fully updated
                myTooltip.Content = null;
                myTooltip.Content = ToolTipContent;

                myTooltip.IsOpen = true;
                myTooltip.VerticalOffset = mousePos.Y;
                myTooltip.HorizontalOffset = mousePos.X;

                myLastHooveredCall = hit;
            }
        }

        private void AssociatedObject_MouseDown( object sender, MouseButtonEventArgs e )
        {
            e.Handled = false;
            myTooltip.IsOpen = false;
        }

        private void AssociatedObject_MouseLeave( object sender, MouseEventArgs e )
        {
            myTooltip.IsOpen = false;
        }

        protected override void OnDetaching()
        {
            Cleanup();

            base.OnDetaching();
        }

        private void Cleanup()
        {
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;

            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;

            var renderedCallsDescriptor = DependencyPropertyDescriptor.FromProperty( FlameView.RenderedActivitiesProperty, typeof( FlameView ) );
            renderedCallsDescriptor.RemoveValueChanged( AssociatedObject, OnRenderedCallsChanged );
        }
    }
}
