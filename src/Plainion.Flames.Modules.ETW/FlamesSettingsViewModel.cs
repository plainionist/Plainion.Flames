using System;
using System.Linq;
using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure.Services;
using Microsoft.Practices.Prism.Mvvm;
using System.IO;

namespace Plainion.Flames.Modules.ETW
{
    [Export]
    class FlamesSettingsViewModel : BindableBase
    {
        private ITraceLoaderService myTraceLoader;
        private bool myInterpolateBrokenStackSamples;
        private bool myShowTab;

        [ImportingConstructor]
        public FlamesSettingsViewModel( ITraceLoaderService traceLoader )
        {
            myTraceLoader = traceLoader;
            myTraceLoader.LoadingCompleted += OnLoadingCompleted;
        }

        private void OnLoadingCompleted( object sender, EventArgs e )
        {
            ShowTab = myTraceLoader.LoadedTraceFiles
                .Select( f => Path.GetExtension( f ) )
                .Any( ext => ext.Equals( ".etl", StringComparison.OrdinalIgnoreCase )
                    || ext.Equals( ".etlx", StringComparison.OrdinalIgnoreCase ) );
        }

        public string Description { get { return "ETW settings"; } }

        public bool ShowTab
        {
            get { return myShowTab; }
            set { SetProperty( ref myShowTab, value ); }
        }

        public bool InterpolateBrokenStackSamples
        {
            get { return myInterpolateBrokenStackSamples; }
            set
            {
                if( SetProperty( ref myInterpolateBrokenStackSamples, value ) )
                {
                    myTraceLoader.ReloadCurrentTrace();
                }
            }
        }
    }
}
