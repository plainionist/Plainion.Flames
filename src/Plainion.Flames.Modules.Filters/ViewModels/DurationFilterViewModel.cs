using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Flames.Modules.Filters.ViewModels
{
    class DurationFilterViewModel : BindableBase
    {
        private CallFilterModule myModule;

        public CallFilterModule Module
        {
            get { return myModule; }
            set { SetProperty( ref myModule, value ); }
        }
    }
}
