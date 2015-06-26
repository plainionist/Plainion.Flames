using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.Flames.Viewer.ViewModels;

namespace Plainion.Flames.Viewer.Views
{
    [Export]
    public partial class FlamesBrowser : UserControl
    {
        [ImportingConstructor]
        internal FlamesBrowser( FlamesBrowserViewModel viewModel )
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}
