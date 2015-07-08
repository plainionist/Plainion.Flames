using System.ComponentModel.Composition;
using System.Linq;
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

        protected override void OnProjectChanging()
        {
            if (Presentation == null)
            {
                return;
            }

            var selectedThreads = new SelectedThreadsDocument();

            foreach (var flame in Presentation.Flames)
            {
                if (flame.Visibility != ContentVisibility.Invisible)
                {
                    selectedThreads.Add(flame.ProcessId, flame.ThreadId);
                }
            }

            ProjectService.Project.Items.Add(selectedThreads);

            var initialNames = ProjectService.Project.Items.OfType<FriendlyNamesDocument>().Single();
            var friendlyNames = new FriendlyNamesDocument();

            // only save what was really changed!
            string origName = null;
            foreach (var process in TraceLog.Processes)
            {
                if (initialNames.TryGetName(process.ProcessId, out origName) && origName != process.Name)
                {
                    friendlyNames.Add(process.ProcessId, process.Name);
                }

                foreach (var thread in TraceLog.GetThreads(process))
                {
                    if (initialNames.TryGetName(process.ProcessId, thread.ThreadId, out origName) && origName != thread.Name)
                    {
                        friendlyNames.Add(process.ProcessId, thread.ThreadId, thread.Name);
                    }
                }
            }

            ProjectService.Project.Items.Remove(initialNames);
            ProjectService.Project.Items.Add(friendlyNames);
        }

        protected override void OnProjectChanged()
        {
            // new project - reset all user settings
            TracesTreeSource.Processes = null;
        }

        protected override void OnTraceLogChanged()
        {
            // new tracelog - as process and thread instances are our model we have reset all user settings
            TracesTreeSource.Processes = null;

            if (TraceLog != null)
            {
                // first collect the "real" initial names -  as provided by the TraceLog itself
                var initialNames = new FriendlyNamesDocument();
                CollectInitialNames(initialNames);

                // apply friendly names if available
                var friendlyNames = ProjectService.Project.Items.OfType<FriendlyNamesDocument>().SingleOrDefault();
                if (friendlyNames != null)
                {
                    // friendly names deserialized - apply to TraceLog
                    ApplyFriendlyNames(friendlyNames);
                    ProjectService.Project.Items.Remove(friendlyNames);
                }

                // add initial names so that we can calculate the diff on shutdown
                ProjectService.Project.Items.Add(initialNames);
            }
        }

        private void ApplyFriendlyNames(FriendlyNamesDocument document)
        {
            string name = null;

            foreach (var process in TraceLog.Processes)
            {
                if (document.TryGetName(process.ProcessId, out name))
                {
                    process.Name = name;
                }

                foreach (var thread in TraceLog.GetThreads(process))
                {
                    if (document.TryGetName(process.ProcessId, thread.ThreadId, out name))
                    {
                        thread.Name = name;
                    }
                }
            }
        }

        /// <summary>
        /// Names of processes and threads can be changed directly in the model. Here we keep the initial 
        /// names so that we store only the user modified names on shutdown. This way we ensure that if we might later be
        /// able to detect proecess/thread names (better) the user can benefit from it automatically.
        /// </summary>
        private void CollectInitialNames(FriendlyNamesDocument document)
        {
            foreach (var process in TraceLog.Processes)
            {
                document.Add(process.ProcessId, process.Name);

                foreach (var thread in TraceLog.GetThreads(process))
                {
                    document.Add(process.ProcessId, thread.ThreadId, thread.Name);
                }
            }
        }

        protected override void OnPresentationChanged()
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

            var document = ProjectService.Project.Items.OfType<SelectedThreadsDocument>().SingleOrDefault();
            if (document != null)
            {
                foreach (var flame in Presentation.Flames)
                {
                    if (!document.IsVisible(flame.ProcessId, flame.ThreadId))
                    {
                        flame.Visibility = ContentVisibility.Invisible;
                    }
                }
                ProjectService.Project.Items.Remove(document);
            }
        }
    }
}
