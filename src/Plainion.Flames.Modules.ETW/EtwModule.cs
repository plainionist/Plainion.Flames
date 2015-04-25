using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;

namespace Plainion.Flames.Modules.ETW
{
    [ModuleExport( typeof( EtwModule ) )]
    public class EtwModule : IModule
    {
        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            RegionManager.RegisterViewWithRegion( RegionNames.FlamesSettings, typeof( FlamesSettingsView ) );
        }
    }
}
