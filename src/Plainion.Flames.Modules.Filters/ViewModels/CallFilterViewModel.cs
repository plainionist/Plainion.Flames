using System.ComponentModel.Composition;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Infrastructure.ViewModels;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Modules.Filters.ViewModels
{
    [Export]
    class CallFilterViewModel : ViewModelBase
    {
        private CallFilterModule myModule;

        [ImportingConstructor]
        public CallFilterViewModel(IProjectService projectService)
            : base(projectService)
        {
            NameFilterViewModel = new NameFilterViewModel();
            DurationFilterViewModel = new DurationFilterViewModel();
        }

        public string Description { get { return "Method call filters"; } }

        public bool ShowTab { get { return true; } }

        public NameFilterViewModel NameFilterViewModel { get; private set; }

        public DurationFilterViewModel DurationFilterViewModel { get; private set; }

        protected override void OnPresentationChanged(FlameSetPresentation oldValue)
        {
            if (myModule != null)
            {
                myModule.Dispose();
            }

            myModule = new CallFilterModule(Presentation);

            NameFilterViewModel.Module = myModule;
            DurationFilterViewModel.Module = myModule;
        }
    }
}
