using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;
using Plainion.Flames.Viewer.Services;
using Plainion.Logging;
using Microsoft.Practices.Prism.Interactivity;
using Plainion.Prism.Interactivity;
using Plainion.AppFw.Wpf;

namespace Plainion.Flames.Viewer
{
    public class Bootstrapper : BootstrapperBase<Shell>
    {
        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();

            // Prism automatically loads the module with that line
            AggregateCatalog.Catalogs.Add( new AssemblyCatalog( GetType().Assembly ) );
            AggregateCatalog.Catalogs.Add( new AssemblyCatalog( typeof( PopupWindowActionRegionAdapter ).Assembly ) );

            var moduleRoot = Path.GetDirectoryName( GetType().Assembly.Location );
            foreach( var moduleFile in Directory.GetFiles( moduleRoot, "Plainion.Flames.Modules.*.dll" ) )
            {
                AggregateCatalog.Catalogs.Add( new AssemblyCatalog( moduleFile ) );
            }
        }

        protected override Microsoft.Practices.Prism.Regions.RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            var mappings = base.ConfigureRegionAdapterMappings();
            mappings.RegisterMapping( typeof( PopupWindowAction ), Container.GetExportedValue<PopupWindowActionRegionAdapter>() );
            return mappings;
        }

        public override void Run( bool runWithDefaultConfiguration )
        {
            LoggerFactory.Implementation = new LoggingSinkLoggerFactory();
            LoggerFactory.LogLevel = LogLevel.Info;

            base.Run( runWithDefaultConfiguration );

            LoggerFactory.AddGuiAppender( Container.GetExportedValue<LoggingSink>() );
            Container.GetExportedValue<ProfilingService>().Start();
        }

        protected override void OnShutdown( object sender, ExitEventArgs e )
        {
            Container.GetExportedValue<ProfilingService>().Stop();

            base.OnShutdown( sender, e );
        }
    }
}
