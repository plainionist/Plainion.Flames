using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.Flames.Modules.Filters.ViewModels;
using Prism.Regions;

namespace Plainion.Flames.Modules.Filters.Views
{
    [Export, PartCreationPolicy( CreationPolicy.NonShared )]
    [ViewSortHint( "0100" )]
    partial class CallFilterView : UserControl
    {
        [ImportingConstructor]
        public CallFilterView( CallFilterViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
