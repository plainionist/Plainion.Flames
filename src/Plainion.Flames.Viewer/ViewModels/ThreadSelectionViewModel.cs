using System.ComponentModel.Composition;
using System.Linq;
using Plainion.Flames.Infrastructure.Controls;
using Plainion.Flames.Infrastructure.ViewModels;
using Plainion.Flames.Presentation;

namespace Plainion.Flames.Viewer.ViewModels
{
    [Export]
    class ThreadSelectionViewModel : ViewModelBase
    {
        internal ThreadSelectionViewModel()
        {
            TracesTreeSource = new TracesTree();
        }

        public string Description { get { return "Processes & Threads"; } }

        public bool ShowTab { get { return true; } }

        public TracesTree TracesTreeSource { get; private set; }

        protected override void OnPresentationChanged(FlameSetPresentation oldValue)
        {
            TracesTreeSource.Processes = Presentation.Flames
                .GroupBy(x => x.Model.Process)
                .OrderBy(x => x.Key.Name)
                .Select(x => new SelectableProcessAdapter(x.Key, x.AsEnumerable()))
                .ToList();
        }
    }
}
