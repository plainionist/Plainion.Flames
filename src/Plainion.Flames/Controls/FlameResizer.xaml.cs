using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Controls
{
    public partial class FlameResizer : UserControl
    {
        private double? myLastDragY;

        public FlameResizer()
        {
            InitializeComponent();

            myResizer.MouseDown += myResizer_MouseDown;
            myResizer.MouseUp += myResizer_MouseUp;
            myResizer.MouseMove += myResizer_MouseMove;
        }

        public Flame Flame
        {
            get { return ( Flame )GetValue( FlameProperty ); }
            set { SetValue( FlameProperty, value ); }
        }

        public static DependencyProperty FlameProperty = DependencyProperty.Register( "Flame", typeof( Flame ), typeof( FlameResizer ),
             new FrameworkPropertyMetadata( null ) );

        private void myResizer_MouseDown( object sender, MouseButtonEventArgs e )
        {
            myResizer.CaptureMouse();
            myLastDragY = e.GetPosition( this ).Y;
            e.Handled = true;
        }

        private void myResizer_MouseUp( object sender, MouseButtonEventArgs e )
        {
            myResizer.ReleaseMouseCapture();

            myLastDragY = null;
            e.Handled = true;
        }

        private void myResizer_MouseMove( object sender, MouseEventArgs e )
        {
            if( !myLastDragY.HasValue )
            {
                return;
            }

            var currentDragY = e.GetPosition( this ).Y;
            var delta = currentDragY - myLastDragY.Value;

            if( Math.Abs( delta ) < 1 )
            {
                return;
            }

            Debug.WriteLine( delta );

            var newHeight = ActualHeight + delta;
            if( newHeight < 3 )
            {
                newHeight = 3;
            }
            myLastDragY = currentDragY;

            Height = newHeight;
            Flame.Height += ( int )delta;

            e.Handled = true;
        }
    }
}
