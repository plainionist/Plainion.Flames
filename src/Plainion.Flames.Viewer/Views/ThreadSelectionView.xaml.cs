using System.ComponentModel.Composition;
using System.Windows.Controls;
using Prism.Regions;
using Plainion.Flames.Viewer.ViewModels;

namespace Plainion.Flames.Viewer.Views
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    [ViewSortHint("0100")]
    public partial class ThreadSelectionView : UserControl
    {
        [ImportingConstructor]
        internal ThreadSelectionView(ThreadSelectionViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
