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
            set { SetProperty(ref mySelectedTabIndex, value); }
        }
    }
}
