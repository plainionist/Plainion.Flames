using System.ComponentModel.Composition;
using System.Windows.Controls;
using Plainion.Flames.Viewer.ViewModels;
using Microsoft.Practices.Prism.Regions;

namespace Plainion.Flames.Viewer.Views
{
    [Export, PartCreationPolicy( CreationPolicy.NonShared )]
    [ViewSortHint( "0200" )]
    public partial class BookmarksView : UserControl
    {
        [ImportingConstructor]
        BookmarksView( BookmarksViewModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
