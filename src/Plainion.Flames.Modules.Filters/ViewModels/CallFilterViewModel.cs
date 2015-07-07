using System.ComponentModel.Composition;
using System.Linq;
using Plainion.Flames.Infrastructure.Services;
using Plainion.Flames.Infrastructure.ViewModels;
using Plainion.Flames.Modules.Filters.Model;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Modules.Filters.ViewModels
{
    [Export]
    class CallFilterViewModel : ViewModelBase
    {
        private CallFilterModule myModule;

        public CallFilterViewModel()
        {
            NameFilterViewModel = new NameFilterViewModel();
            DurationFilterViewModel = new DurationFilterViewModel();
        }

        public string Description { get { return "Method call filters"; } }

        public bool ShowTab { get { return true; } }

        public NameFilterViewModel NameFilterViewModel { get; private set; }

        public DurationFilterViewModel DurationFilterViewModel { get; private set; }

        protected override void OnProjectChanging()
        {
            if (ProjectService.Project == null)
            {
                return;
            }

            var document = new FiltersDocument();
            document.DurationFilter = myModule.DurationFilter;
            document.NameFilters.AddRange(myModule.NameFilters.Where(f => !(f is AllCallsFilter)));

            ProjectService.Project.Items.Add(document);
        }

        protected override void OnProjectChanged()
        {
            // new project - reset all user settings
            myModule = null;
        }

        protected override void OnPresentationChanged()
        {
            // lets preserve the module itself to preserve the user settings across presentations
            if (myModule != null)
            {
                myModule.Presentation = Presentation;
                NameFilterViewModel.Presentation = Presentation;
                return;
            }

            // take the filters from serialized project only initially
            var document = ProjectService.Project.Items.OfType<FiltersDocument>().SingleOrDefault();
            if (document == null)
            {
                myModule = CallFilterModule.CreateEmpty();
            }
            else
            {
                myModule = CallFilterModule.CreateFromDocument(document);
                ProjectService.Project.Items.Remove(document);
            }

            myModule.Presentation = Presentation;

            // TODO: as a workaround let us add the entire CallFilterModule to the Project.Items
            ProjectService.Project.Items.Add(myModule);

            NameFilterViewModel.Module = myModule;
            DurationFilterViewModel.Module = myModule;
        }
    }
}
