using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.Flames.Viewer.Services;

namespace Plainion.Flames.Viewer.Views
{
    [Export]
    public partial class LogView : UserControl
    {
        [ImportingConstructor]
        internal LogView( LoggingSink sink )
        {
            InitializeComponent();

            DataContext = sink;
        }
    }
}
