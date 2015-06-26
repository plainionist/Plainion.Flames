using System.ComponentModel.Composition;
using Plainion.Flames.Viewer.Views;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;

namespace Plainion.Flames.Viewer
{
    [ModuleExport( typeof( CoreModule ) )]
    public class CoreModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion( RegionNames.LogView, typeof( LogView ) );
            RegionManager.RegisterViewWithRegion( RegionNames.BrowserView, typeof( FlamesBrowser ) );
            RegionManager.RegisterViewWithRegion( Plainion.Flames.Infrastructure.RegionNames.FlamesSettings, typeof( BookmarksView ) );
        }
    }
}
