using System.Windows;
using System.Windows.Controls;

namespace Plainion.Flames.Infrastructure.Controls
{
    public partial class TracesTreeView : UserControl
    {
        public TracesTreeView()
        {
            InitializeComponent();
        }

        public TracesTree TracesSource
        {
            get { return ( TracesTree )GetValue( TracesSourceProperty ); }
            set { SetValue( TracesSourceProperty, value ); }
        }

        public static DependencyProperty TracesSourceProperty = DependencyProperty.Register( "TracesSource", typeof( TracesTree ), typeof( TracesTreeView ),
             new FrameworkPropertyMetadata( null ) );
    }
}
