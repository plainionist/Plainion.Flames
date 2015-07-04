using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Infrastructure.ViewModels;
using Plainion.Flames.Model;

namespace Plainion.Flames.Modules.ETW
{
    [Export]
    class FlamesSettingsViewModel : ViewModelBase
    {
        private bool myInterpolateBrokenStackSamples;
        private bool myShowTab;

        [Import]
        public ITraceLoaderService TraceLoader { get; set; }

        protected override void OnTraceLogChanged( ITraceLog oldValue )
        {
            if( TraceLog != null )
            {
                ShowTab = ProjectService.Project.TraceFiles
                    .Select( f => Path.GetExtension( f ) )
                    .Any( ext => ext.Equals( ".etl", StringComparison.OrdinalIgnoreCase )
                        || ext.Equals( ".etlx", StringComparison.OrdinalIgnoreCase ) );
            }
            else
            {
                ShowTab = false;
            }
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
                    TraceLoader.ReloadCurrentTrace();
                }
            }
        }
    }
}
