using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure;
using Plainion.Flames.Modules.Filters.Views;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Plainion.Flames.Modules.Filters
{
    [ModuleExport( typeof( FiltersModule ) )]
    public class FiltersModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion( RegionNames.FlamesSettings, typeof( CallFilterView) );
        }
    }
}
