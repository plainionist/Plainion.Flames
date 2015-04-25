using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Controls
{
    public partial class FlameSetView : UserControl
    {
        public FlameSetView()
        {
            InitializeComponent();

            myListView.Loaded += myListView_Loaded;
            myListView.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        public Typeface CallNameFont
        {
            get { return ( Typeface )GetValue( CallNameFontProperty ); }
            set { SetValue( CallNameFontProperty, value ); }
        }

        public static DependencyProperty CallNameFontProperty = DependencyProperty.Register( "CallNameFont", typeof( Typeface ), typeof( FlameSetView ),
             new FrameworkPropertyMetadata( new Typeface( new FontFamily( "GenericSansSerif" ), FontStyles.Normal, FontWeights.DemiBold, FontStretches.Normal ) ) );

        public Typeface TimeScaleFont
        {
            get { return ( Typeface )GetValue( TimeScaleFontProperty ); }
            set { SetValue( TimeScaleFontProperty, value ); }
        }

        public static DependencyProperty TimeScaleFontProperty = DependencyProperty.Register( "TimeScaleFont", typeof( Typeface ), typeof( FlameSetView ),
             new FrameworkPropertyMetadata( new Typeface( "Tahoma" ) ) );

        public Typeface SelectionFont
        {
            get { return ( Typeface )GetValue( SelectionFontProperty ); }
            set { SetValue( SelectionFontProperty, value ); }
        }

        public static DependencyProperty SelectionFontProperty = DependencyProperty.Register( "SelectionFont", typeof( Typeface ), typeof( FlameSetView ),
             new FrameworkPropertyMetadata( new Typeface( new FontFamily( "Tahoma" ), FontStyles.Normal, FontWeights.DemiBold, FontStretches.Normal ) ) );

        public object CallToolTip
        {
            get { return ( object )GetValue( CallToolTipProperty ); }
            set { SetValue( CallToolTipProperty, value ); }
        }

        public static DependencyProperty CallToolTipProperty = DependencyProperty.Register( "CallToolTip", typeof( object ), typeof( FlameSetView ),
             new FrameworkPropertyMetadata() );

        public DataTemplate FlameHeaderTemplate
        {
            get { return ( DataTemplate )GetValue( FlameHeaderTemplateProperty ); }
            set { SetValue( FlameHeaderTemplateProperty, value ); }
        }

        public static DependencyProperty FlameHeaderTemplateProperty = DependencyProperty.Register( "FlameHeaderTemplate", typeof( DataTemplate ),
            typeof( FlameSetView ), new FrameworkPropertyMetadata() );

        public FlameSetPresentation Presentation
        {
            get { return ( FlameSetPresentation )GetValue( PresentationProperty ); }
            set { SetValue( PresentationProperty, value ); }
        }

        public static DependencyProperty PresentationProperty = DependencyProperty.Register( "Presentation", typeof( FlameSetPresentation ), typeof( FlameSetView ),
             new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnPresentationChanged ) ) );

        private static void OnPresentationChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( ( FlameSetView )d ).OnPresentationChanged( e.OldValue );
        }

        private void OnPresentationChanged( object oldValue )
        {
            // TODO: actually we want to trigger all behaviors (selection behavior) to remove adorners but unclear how
            var adornerLayer = AdornerLayer.GetAdornerLayer( FlameResizer );
            var adorners = adornerLayer.GetAdorners( FlameResizer );
            if( adorners != null )
            {
                foreach( var adorner in adorners )
                {
                    adornerLayer.Remove( adorner );
                }
            }

            if( Presentation != null )
            {
                foreach( var item in Presentation.Flames )
                {
                    item.IsRenderingEnabled = false;
                }
            }
        }

        public ContextMenu FlameHeaderContextMenu
        {
            get { return ( ContextMenu )GetValue( FlameHeaderContextMenuProperty ); }
            set { SetValue( FlameHeaderContextMenuProperty, value ); }
        }

        public static DependencyProperty FlameHeaderContextMenuProperty = DependencyProperty.Register( "FlameHeaderContextMenu",
            typeof( ContextMenu ), typeof( FlameSetView ) );

        private void ItemContainerGenerator_StatusChanged( object sender, EventArgs e )
        {
            Dispatcher.BeginInvoke( new Action( SetOnScreen ) );
        }

        private void myListView_Loaded( object sender, RoutedEventArgs e )
        {
            var scrollViewer = myListView.GetVisualChild<ScrollViewer>();
            var scrollBar = scrollViewer.Template.FindName( "PART_VerticalScrollBar", scrollViewer ) as ScrollBar;
            scrollBar.ValueChanged += delegate
            {
                SetOnScreen();
            };
        }

        private void SetOnScreen()
        {
            foreach( Flame trace in myListView.Items )
            {
                var container = ( ListViewItem )myListView.ItemContainerGenerator.ContainerFromItem( trace );
                if( container != null )
                {
                    var topLeft = container.TransformToVisual( myListView ).Transform( new Point() ).Y;
                    trace.IsRenderingEnabled = 0 - trace.Height <= topLeft && topLeft <= myListView.ActualHeight;
                }
            }
        }
    }
}
