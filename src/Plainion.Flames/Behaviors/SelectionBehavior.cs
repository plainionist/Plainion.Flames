using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Plainion.Flames.Presentation;
using Plainion.Flames.Controls;

namespace Plainion.Flames.Behaviors
{
    public class SelectionBehavior : Behavior<FlameView>
    {
        private SelectionCreationAdorner mySelectionCreationAdorner;

        /// <summary>
        /// The control to overlay with the selection adorners
        /// </summary>
        public FrameworkElement Content
        {
            get { return ( FrameworkElement )GetValue( ContentProperty ); }
            set { SetValue( ContentProperty, value ); }
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register( "Content", typeof( FrameworkElement ),
            typeof( SelectionBehavior ), new FrameworkPropertyMetadata( null ) );

        public TimelineViewport TimelineViewport
        {
            get { return ( TimelineViewport )GetValue( TimelineViewportProperty ); }
            set { SetValue( TimelineViewportProperty, value ); }
        }

        public static readonly DependencyProperty TimelineViewportProperty = DependencyProperty.Register( "TimelineViewport", typeof( TimelineViewport ),
            typeof( SelectionBehavior ), new FrameworkPropertyMetadata( null ) );

        public SelectionModule Selections
        {
            get { return ( SelectionModule )GetValue( SelectionsProperty ); }
            set { SetValue( SelectionsProperty, value ); }
        }

        public static readonly DependencyProperty SelectionsProperty = DependencyProperty.Register( "Selections", typeof( SelectionModule ),
            typeof( SelectionBehavior ), new FrameworkPropertyMetadata( null , OnSelectionsChanged) );

        private static void OnSelectionsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( ( SelectionBehavior )d ).OnSelectionsChanged((SelectionModule)e.OldValue);
        }

        private void OnSelectionsChanged(SelectionModule oldValue)
        {
            if( oldValue != null )
            {
                RemoveSelectionAdorners( AdornerLayer.GetAdornerLayer( Content ) );
                oldValue.Cleared -= OnSelectionsCleared;
            }

            if( Selections != null )
            {
                Selections.Cleared += OnSelectionsCleared;
            }
        }

        private void OnSelectionsCleared( object sender, EventArgs e )
        {
            RemoveSelectionAdorners( AdornerLayer.GetAdornerLayer( Content ) );
        }

        public Typeface Font
        {
            get { return ( Typeface )GetValue( FontProperty ); }
            set { SetValue( FontProperty, value ); }
        }

        public static DependencyProperty FontProperty = DependencyProperty.Register( "Font", typeof( Typeface ), typeof( SelectionBehavior ),
             new FrameworkPropertyMetadata(
                 new Typeface( new FontFamily( "Tahoma" ), FontStyles.Normal, FontWeights.DemiBold, FontStretches.Normal ),
                 null ) );

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewMouseDown += OnMouseDown;
        }

        private void OnMouseDown( object sender, MouseButtonEventArgs e )
        {
            if( TimelineViewport == null )
            {
                return;
            }

            if( e.LeftButton != MouseButtonState.Pressed )
            {
                return;
            }

            var x = ( int )e.GetPosition( Content ).X;
            Selections.Start = TimelineViewport.CalculateTime( Content.ActualWidth, x );

            mySelectionCreationAdorner = new SelectionCreationAdorner( Content, AssociatedObject, TimelineViewport, x );
            mySelectionCreationAdorner.Font = Font;
            mySelectionCreationAdorner.Finished += OnSelectionClosed;
            mySelectionCreationAdorner.Changed += OnSelectionChanged;

            var adornerLayer = AdornerLayer.GetAdornerLayer( Content );
            adornerLayer.Add( mySelectionCreationAdorner );

            e.Handled = true;
        }

        private void OnSelectionChanged( object sender, EventArgs e )
        {
            Selections.End = TimelineViewport.CalculateTime( Content.ActualWidth, ( int )Mouse.GetPosition( Content ).X );
        }

        private void OnSelectionClosed( object sender, EventArgs e )
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer( Content );
            adornerLayer.Remove( mySelectionCreationAdorner );

            mySelectionCreationAdorner.Finished -= OnSelectionClosed;
            mySelectionCreationAdorner.Changed -= OnSelectionChanged;
            mySelectionCreationAdorner = null;

            if( Selections.Start > Selections.End )
            {
                var t = Selections.End;
                Selections.End = Selections.Start;
                Selections.Start = t;
            }

            if( Selections.End - Selections.Start < 10 )
            {
                return;
            }

            if( Keyboard.Modifiers != ModifierKeys.Control )
            {
                TimelineViewport.Set( Selections.Start.Value, Selections.End.Value );

                Selections.Start = null;
                Selections.End = null;
            }
            else
            {
                RemoveSelectionAdorners( adornerLayer );

                var selection = new SelectionVisualizationAdorner( Content, AssociatedObject, TimelineViewport,
                    Selections.Start.Value, Selections.End.Value );
                selection.Font = Font;

                adornerLayer.Add( selection );
            }
        }

        private void RemoveSelectionAdorners( AdornerLayer adornerLayer )
        {
            var adorners = adornerLayer.GetAdorners( Content );
            if( adorners == null )
            {
                return;
            }

            var selections = adorners
                .OfType<SelectionVisualizationAdorner>()
                .ToList();

            if( selections.Count > 0 )
            {
                foreach( var sel in selections )
                {
                    adornerLayer.Remove( sel );
                }
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseDown -= OnMouseDown;
            RemoveSelectionAdorners( AdornerLayer.GetAdornerLayer( Content ) );

            base.OnDetaching();
        }
    }
}
