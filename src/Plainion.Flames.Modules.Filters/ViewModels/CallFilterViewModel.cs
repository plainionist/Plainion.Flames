using System.ComponentModel.Composition;
using Plainion.Flames.Presentation;
using Microsoft.Practices.Prism.Mvvm;

namespace Plainion.Flames.Modules.Filters.ViewModels
{
    [Export]
    class CallFilterViewModel : BindableBase
    {
        private FlameSetPresentation myPresentation;
        private CallFilterModule myModules;

        public CallFilterViewModel()
        {
            NameFilterViewModel = new NameFilterViewModel();
            DurationFilterViewModel = new DurationFilterViewModel();
        }

        public string Description { get { return "Method call filters"; } }

        public bool ShowTab { get { return true; } }

        public NameFilterViewModel NameFilterViewModel { get; private set; }

        public DurationFilterViewModel DurationFilterViewModel { get; private set; }

        public FlameSetPresentation Presentation
        {
            get { return myPresentation; }
            set
            {
                if( SetProperty( ref myPresentation, value ) )
                {
                    if( myModules != null )
                    {
                        myModules.Dispose();
                    }

                    myModules = new CallFilterModule( myPresentation );

                    NameFilterViewModel.Module = myModules;
                    DurationFilterViewModel.Module = myModules;
                }
            }
        }
    }
}
