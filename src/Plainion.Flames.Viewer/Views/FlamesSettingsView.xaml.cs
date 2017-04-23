using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Prism.Regions;
using Plainion.Flames.Viewer.ViewModels;

namespace Plainion.Flames.Viewer.Views
{
    [Export, PartCreationPolicy( CreationPolicy.NonShared )]
    public partial class FlamesSettingsView : UserControl
    {
        private IRegionManager myRegionManager;
        private IRegion myRegion;
        private FlamesSettingsViewModel myViewModel;

        [ImportingConstructor]
        internal FlamesSettingsView(IRegionManager regionManager, FlamesSettingsViewModel viewModel )
        {
            myRegionManager = regionManager;
            myViewModel = viewModel;

            InitializeComponent();

            // DataContext will be set "delayed" because when prism adds views to TabControl the
            // selected tab index will be subsequently changed finally to the last tab. We want to have the
            // actually wanted selected tab index defined in view model 
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // trigger prism to load all views into the region.
            // use member to ensure that JIT does not optimize this line away
            myRegion = myRegionManager.Regions.Single(r => r.Name == Plainion.Flames.Infrastructure.RegionNames.FlamesSettings);

            // now we can set the DataContext to have the correct selected tab index
            DataContext = myViewModel;
        }
    }
}
