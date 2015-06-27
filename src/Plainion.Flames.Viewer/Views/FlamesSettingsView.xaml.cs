using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.Flames.Viewer.ViewModels;

namespace Plainion.Flames.Viewer.Views
{
    [Export, PartCreationPolicy( CreationPolicy.NonShared )]
    public partial class FlamesSettingsView : UserControl
    {
        [ImportingConstructor]
        internal FlamesSettingsView( FlamesSettingsViewModel viewModel )
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}
