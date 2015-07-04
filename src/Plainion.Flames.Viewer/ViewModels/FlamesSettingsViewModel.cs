using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure.ViewModels;

namespace Plainion.Flames.Viewer.ViewModels
{
    [Export]
    class FlamesSettingsViewModel : ViewModelBase
    {
        private int mySelectedTabIndex;

        public int SelectedTabIndex
        {
            get { return mySelectedTabIndex; }
            set { SetProperty( ref mySelectedTabIndex, value ); }
        }

        protected override void OnProjectChanged()
        {
            if( ProjectService.Project != null && ProjectService.Project.WasDeserialized )
            {
                // we loaded user settings from disk which might filter out certain threads or calls.
                // lets jump to process and threads view
                // TODO: setting selected tab index actually is a workaround - we should better use Prism navigation
                // to explicitly specify what we want
                SelectedTabIndex = 1;
            }
        }
    }
}
