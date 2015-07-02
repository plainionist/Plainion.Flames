using System.ComponentModel.Composition;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Regions;
using Plainion.Flames.Viewer.ViewModels;

namespace Plainion.Flames.Viewer.Views
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    [ViewSortHint("0100")]
    public partial class ThreadSelectionView : UserControl
    {
        [ImportingConstructor]
        ThreadSelectionView(ThreadSelectionViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
