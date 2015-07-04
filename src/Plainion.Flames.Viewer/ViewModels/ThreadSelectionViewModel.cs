using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Mvvm;
using Plainion.Flames.Infrastructure.Controls;
using Plainion.Flames.Infrastructure.ViewModels;
using Plainion.Flames.Model;
using Plainion.Flames.Presentation;
using Plainion.Flames.Viewer.Model;

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

        protected override void OnProjectChanged()
        {
            // new project - reset all user settings
            TracesTreeSource.Processes = null;
        }

        protected override void OnTraceLogChanged( ITraceLog oldValue )
        {
            // new tracelog - as process and thread instances are our model we have reset all user settings
            TracesTreeSource.Processes = null;
        }

        protected override void OnPresentationChanged(FlameSetPresentation oldValue)
        {
            if (Presentation == null)
            {
                return;
            }

            // lets preserve the user settings across presentations.
            // if we get here with a new project/tracelog we have reseted the TracesTreeSource already
            if (TracesTreeSource.Processes != null)
            {
                return;
            }

            TracesTreeSource.Processes = Presentation.Flames
                .GroupBy(x => x.Model.Process)
                .OrderBy(x => x.Key.Name)
                .Select(x => new SelectableProcessAdapter(x.Key, x.AsEnumerable()))
                .ToList();

            var selectedThreads = ProjectService.Project.Items.OfType<SelectedThreadsDocument>().SingleOrDefault();
            if (selectedThreads != null)
            {
                foreach (var flame in Presentation.Flames)
                {
                    if (!selectedThreads.IsVisible(flame.ProcessId, flame.ThreadId))
                    {
                        flame.Visibility = ContentVisibility.Invisible;
                    }
                }
            }
        }
    }
}
