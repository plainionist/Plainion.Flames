using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;

namespace Plainion.Flames.Modules.ETW
{
    [Export, PartCreationPolicy( CreationPolicy.NonShared )]
    [ViewSortHint( "1000" )]
    public partial class FlamesSettingsView : UserControl
    {
        [ImportingConstructor]
        internal FlamesSettingsView( FlamesSettingsViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
