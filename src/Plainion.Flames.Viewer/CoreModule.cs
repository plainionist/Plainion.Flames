using System.ComponentModel.Composition;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;
using Plainion.Flames.Viewer.Views;

namespace Plainion.Flames.Viewer
{
    [ModuleExport(typeof(CoreModule))]
    public class CoreModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.LogView, typeof(LogView));
            RegionManager.RegisterViewWithRegion(RegionNames.BrowserView, typeof(FlamesBrowser));
            RegionManager.RegisterViewWithRegion(RegionNames.SettingsView, typeof(FlamesSettingsView));

            RegionManager.RegisterViewWithRegion(Plainion.Flames.Infrastructure.RegionNames.FlamesSettings, typeof(BookmarksView));
            RegionManager.RegisterViewWithRegion(Plainion.Flames.Infrastructure.RegionNames.FlamesSettings, typeof(ThreadSelectionView));
            RegionManager.RegisterViewWithRegion(Plainion.Flames.Infrastructure.RegionNames.FlamesSettings, typeof(TraceLogOverviewView));
        }
    }
}
