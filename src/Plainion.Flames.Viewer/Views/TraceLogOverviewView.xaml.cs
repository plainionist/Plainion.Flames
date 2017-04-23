using System.ComponentModel.Composition;
using System.Windows.Controls;
using Prism.Regions;
using Plainion.Flames.Viewer.ViewModels;

namespace Plainion.Flames.Viewer.Views
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    [ViewSortHint("0010")]
    public partial class TraceLogOverviewView : UserControl
    {
        [ImportingConstructor]
        internal TraceLogOverviewView(TraceLogOverviewViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
